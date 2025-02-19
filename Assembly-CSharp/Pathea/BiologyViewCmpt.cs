using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PEIK;
using PETools;
using SkillSystem;
using UnityEngine;
using WhiteCat;
using WhiteCat.UnityExtension;

namespace Pathea;

public class BiologyViewCmpt : ViewCmpt, IRagdollHandler, IPeMsg
{
	[SerializeField]
	private string m_PrefabPath = string.Empty;

	private int mColorID = -1;

	private AssetReq mPrefabReq;

	private Transform mViewPrefab;

	public IKCmpt cmptIK;

	public CommonCmpt cmptCommon;

	public EquipmentCmpt cmptEquipment;

	public HumanPhyCtrl monoPhyCtrl;

	public IKAimCtrl monoIKAimCtrl;

	public IKAnimEffectCtrl monoIKAnimCtrl;

	public PEModelController monoModelCtrlr;

	public PERagdollController monoRagdollCtrlr;

	public PEDefenceTrigger monoDefenceTrigger;

	public BoneCollector monoBoneCollector;

	protected bool mTargetInjuredState = true;

	private List<Type> m_HideMask = new List<Type>();

	private static List<Collider> g_NPCModelColliders = new List<Collider>();

	private static List<Collider> g_NPCOutColliders = new List<Collider>();

	private Collider[] m_SelfCols;

	private Collider[] m_OutCols;

	private bool m_IsNPCOutCol;

	private bool m_PhyActive = true;

	protected bool m_MeshBuildEnd;

	protected Func<IEnumerator> _coroutineMeshProc;

	public bool canInjured => mTargetInjuredState;

	public PEDefenceTrigger defenceTrigger => monoDefenceTrigger;

	public BiologyViewRoot biologyViewRoot { get; set; }

	public override bool hasView => null != mViewPrefab;

	public override Transform centerTransform
	{
		get
		{
			Transform transform = null;
			if (defenceTrigger != null)
			{
				transform = defenceTrigger.centerBone;
			}
			if (transform == null)
			{
				transform = GetCenterBone();
			}
			return transform;
		}
	}

	public string prefabPath => m_PrefabPath;

	public Transform tView => mViewPrefab;

	public bool IsRagdoll => monoRagdollCtrlr != null && monoRagdollCtrlr.active;

	public Transform modelTrans => (!(monoModelCtrlr != null)) ? null : monoModelCtrlr.transform;

	void IRagdollHandler.OnRagdollBuild(GameObject obj)
	{
		base.Entity.SendMsg(EMsg.View_Ragdoll_Build, obj);
	}

	void IRagdollHandler.OnDetachJoint(GameObject boneRagdoll)
	{
		base.Entity.SendMsg(EMsg.View_Ragdoll_DeatchJoint, boneRagdoll);
	}

	void IRagdollHandler.OnReattachJoint(GameObject boneRagdoll)
	{
		base.Entity.SendMsg(EMsg.View_Ragdoll_ReattachJoint, boneRagdoll);
	}

	void IRagdollHandler.OnAttachJoint(GameObject boneRagdoll)
	{
		base.Entity.SendMsg(EMsg.View_Ragdoll_AttachJoint, boneRagdoll);
	}

	void IRagdollHandler.OnFallBegin(GameObject ragdoll)
	{
		if (cmptCommon != null && cmptCommon.entityProto.proto == EEntityProto.Monster)
		{
			ActivateCollider(value: false);
		}
		base.Entity.SendMsg(EMsg.View_Ragdoll_Fall_Begin, ragdoll);
	}

	void IRagdollHandler.OnFallFinished(GameObject ragdoll)
	{
		base.Entity.SendMsg(EMsg.View_Ragdoll_Fall_Finished, ragdoll);
	}

	void IRagdollHandler.OnGetupBegin(GameObject ragdoll)
	{
		base.Entity.SendMsg(EMsg.View_Ragdoll_Getup_Begin, ragdoll);
	}

	void IRagdollHandler.OnGetupFinished(GameObject ragdoll)
	{
		if (cmptCommon != null && cmptCommon.entityProto.proto == EEntityProto.Monster)
		{
			ActivateCollider(value: true);
		}
		base.Entity.SendMsg(EMsg.View_Ragdoll_Getup_Finished, ragdoll);
	}

	public void SetViewPath(string path)
	{
		m_PrefabPath = path;
	}

	public void SetColorID(int id)
	{
		mColorID = id;
	}

	private string GetModelName()
	{
		return MonsterProtoDb.Get(base.Entity.ProtoID)?.modelName;
	}

	private void SetupMaterials(int colorID, string modelName)
	{
		if (string.IsNullOrEmpty(modelName) || colorID < 0)
		{
			return;
		}
		if (!MonsterRandomDb.ContainsMaterials(colorID, modelName))
		{
			MonsterRandomDb.RegisterMaterials(colorID, modelName, monoModelCtrlr.GetMaterials());
		}
		Material[] materials = MonsterRandomDb.GetMaterials(colorID, modelName);
		if (materials != null && materials.Length > 0)
		{
			if (monoModelCtrlr != null)
			{
				monoModelCtrlr.SetMaterials(materials);
			}
			if (monoRagdollCtrlr != null)
			{
				monoRagdollCtrlr.SetMaterials(materials);
			}
		}
	}

	public void ActivatePhysics(bool value)
	{
		if (monoModelCtrlr != null)
		{
			monoModelCtrlr.ActivatePhysics(value);
		}
		if (base.Entity.Id == 9008)
		{
			m_PhyActive = value;
		}
	}

	public void ActivateRagdollPhysics(bool value)
	{
		if (monoRagdollCtrlr != null)
		{
			monoRagdollCtrlr.ActivatePhysics(value);
		}
	}

	public void ActivateCollider(bool value)
	{
		if (monoModelCtrlr != null)
		{
			monoModelCtrlr.ActivateColliders(value);
			ResetOutCollider(value);
		}
	}

	public void ActivateInjured(bool value, float delayTime = 0f)
	{
		mTargetInjuredState = value;
		CancelInvoke("DoActivateInjured");
		if (float.Epsilon > delayTime)
		{
			DoActivateInjured();
		}
		else
		{
			Invoke("DoActivateInjured", delayTime);
		}
	}

	private void DoActivateInjured()
	{
		if (monoDefenceTrigger != null)
		{
			monoDefenceTrigger.active = mTargetInjuredState;
		}
	}

	public void ActivateRenderer(Type type, bool value)
	{
		if (value)
		{
			m_HideMask.Remove(type);
		}
		else if (!m_HideMask.Contains(type))
		{
			m_HideMask.Add(type);
		}
		UpdateRender();
	}

	private void UpdateRender()
	{
		if (monoModelCtrlr != null)
		{
			monoModelCtrlr.ActivateRenderer(m_HideMask.Count == 0 && !IsRagdoll);
		}
		if (monoRagdollCtrlr != null)
		{
			monoRagdollCtrlr.ActivateRenderer(m_HideMask.Count == 0 && IsRagdoll);
		}
	}

	public void Fadein(float time = 2f)
	{
		if (monoModelCtrlr != null && !IsRagdoll)
		{
			monoModelCtrlr.FadeIn(time);
		}
		if (monoRagdollCtrlr != null && IsRagdoll)
		{
			monoRagdollCtrlr.FadeIn(time);
		}
	}

	public void Fadeout(float time = 2f)
	{
		if (monoModelCtrlr != null && !IsRagdoll)
		{
			monoModelCtrlr.FadeOut(time);
		}
		if (monoRagdollCtrlr != null && IsRagdoll)
		{
			monoRagdollCtrlr.FadeOut(time);
		}
	}

	public void HideView(float time)
	{
		if (monoModelCtrlr != null && !IsRagdoll)
		{
			monoModelCtrlr.HideView(time);
		}
		if (monoRagdollCtrlr != null && IsRagdoll)
		{
			monoRagdollCtrlr.HideView(time);
		}
	}

	private IEnumerator FadeInDelay(float delayTime, float time)
	{
		yield return new WaitForSeconds(delayTime);
		Fadein(time);
	}

	public Collider GetModelCollider(string name)
	{
		Transform transform = ((!(monoModelCtrlr != null)) ? null : PEUtil.GetChild(monoModelCtrlr.transform, name));
		return (!(transform != null)) ? null : transform.GetComponent<Collider>();
	}

	public Collider GetRagdollCollider(string name)
	{
		Transform transform = ((!(monoRagdollCtrlr != null)) ? null : PEUtil.GetChild(monoRagdollCtrlr.transform, name));
		return (!(transform != null)) ? null : transform.GetComponent<Collider>();
	}

	public Transform GetModelTransform(string name)
	{
		return (!(monoModelCtrlr != null)) ? null : PEUtil.GetChild(monoModelCtrlr.transform, name);
	}

	public Rigidbody GetModelRigidbody()
	{
		return (!(monoModelCtrlr != null)) ? null : monoModelCtrlr.Rigid;
	}

	public bool IsReadyGetUp()
	{
		return !(monoRagdollCtrlr != null) || monoRagdollCtrlr.IsReadyGetUp();
	}

	public Transform GetRagdollTransform(string name)
	{
		return (!(monoRagdollCtrlr != null)) ? null : PEUtil.GetChild(monoRagdollCtrlr.transform, name);
	}

	public void IgnoreCollision(Collider collider, bool isIgnore = true)
	{
		if (!(null == collider) && collider.enabled)
		{
			if (null != monoModelCtrlr)
			{
				PEUtil.IgnoreCollision(monoModelCtrlr.colliders, collider);
			}
			if (null != monoRagdollCtrlr)
			{
				PEUtil.IgnoreCollision(monoRagdollCtrlr.colliders, collider);
			}
		}
	}

	protected IEnumerator ProcPostBoneLoad()
	{
		if (monoPhyCtrl == null)
		{
			monoPhyCtrl = biologyViewRoot.humanPhyCtrl;
		}
		if (monoIKAimCtrl == null)
		{
			monoIKAimCtrl = biologyViewRoot.ikAimCtrl;
		}
		if (monoIKAnimCtrl == null)
		{
			monoIKAnimCtrl = biologyViewRoot.ikAnimEffectCtrl;
		}
		if (monoModelCtrlr == null)
		{
			monoModelCtrlr = biologyViewRoot.modelController;
		}
		if (monoRagdollCtrlr == null)
		{
			monoRagdollCtrlr = biologyViewRoot.ragdollController;
		}
		if (monoDefenceTrigger == null)
		{
			monoDefenceTrigger = biologyViewRoot.defenceTrigger;
		}
		m_MeshBuildEnd = false;
		if (monoRagdollCtrlr != null)
		{
			monoRagdollCtrlr.SetHandler(this);
		}
		if (monoModelCtrlr != null)
		{
			SetupMaterials(mColorID, GetModelName());
			if (base.Entity.proto == EEntityProto.Monster && base.Entity.Race == ERace.Mankind)
			{
				monoModelCtrlr.gameObject.layer = 8;
			}
			PEUtil.IgnoreCollision(monoModelCtrlr.colliders, monoModelCtrlr.colliders);
			if (monoRagdollCtrlr != null)
			{
				PEUtil.IgnoreCollision(monoModelCtrlr.colliders, monoRagdollCtrlr.colliders);
			}
		}
		BuildOutCollider(biologyViewRoot.gameObject);
		if (base.Entity.IsDeath())
		{
			OnDeath(null, null);
		}
		else if (monoModelCtrlr != null)
		{
			monoModelCtrlr.ActivateDeathMode(isDeath: false);
		}
		HideView(0.01f);
		if (base.Entity.Id == 9008)
		{
			ActivatePhysics(m_PhyActive);
		}
		DoActivateInjured();
		yield return 0;
		if (null == biologyViewRoot)
		{
			yield break;
		}
		if (_coroutineMeshProc != null)
		{
			yield return biologyViewRoot.StartCoroutine(_coroutineMeshProc());
		}
		m_MeshBuildEnd = true;
		if (null == biologyViewRoot || null == base.Entity)
		{
			yield break;
		}
		if (monoModelCtrlr != null)
		{
			base.Entity.SendMsg(EMsg.View_Model_Build, monoModelCtrlr.gameObject, biologyViewRoot);
			yield return 0;
		}
		if (!(null == biologyViewRoot) && !(null == base.Entity))
		{
			base.Entity.SendMsg(EMsg.View_Prefab_Build, this, biologyViewRoot);
			yield return 0;
			if (!(null == biologyViewRoot))
			{
				biologyViewRoot.StartCoroutine(FadeInDelay(0.01f, 3f));
			}
		}
	}

	protected virtual void OnBoneLoad(GameObject obj)
	{
		mPrefabReq = null;
		mViewPrefab = obj.transform;
		mViewPrefab.parent = base.Entity.GetGameObject().transform;
		mViewPrefab.position = base.Entity.peTrans.position;
		mViewPrefab.rotation = base.Entity.peTrans.rotation;
		mViewPrefab.localScale = base.Entity.peTrans.scale;
		biologyViewRoot = mViewPrefab.GetComponent<BiologyViewRoot>();
		monoBoneCollector = mViewPrefab.GetComponent<BoneCollector>();
		if (null == biologyViewRoot)
		{
			biologyViewRoot = mViewPrefab.gameObject.AddComponent<BiologyViewRoot>();
		}
		biologyViewRoot.StartCoroutine(ProcPostBoneLoad());
	}

	private void BuildOutCollider(GameObject obj)
	{
		if (null == monoModelCtrlr)
		{
			return;
		}
		List<Collider> list = new List<Collider>();
		List<Collider> list2 = new List<Collider>();
		m_IsNPCOutCol = false;
		if (base.Entity.commonCmpt.entityProto.proto == EEntityProto.Monster)
		{
			MonsterProtoDb.Item item = MonsterProtoDb.Get(base.Entity.commonCmpt.entityProto.protoId);
			if (item == null || item.canBePush)
			{
				return;
			}
		}
		else
		{
			if (base.Entity.commonCmpt.entityProto.proto != EEntityProto.Npc && base.Entity.commonCmpt.entityProto.proto != 0 && base.Entity.commonCmpt.entityProto.proto != EEntityProto.RandomNpc)
			{
				return;
			}
			m_IsNPCOutCol = true;
		}
		Collider[] array = ((monoModelCtrlr.colliders == null || monoModelCtrlr.colliders.Length <= 0) ? monoModelCtrlr.GetComponentsInChildren<Collider>(includeInactive: true) : monoModelCtrlr.colliders);
		if (array == null)
		{
			Debug.LogError(base.name + " don't has cols.");
			return;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == null)
			{
				Debug.LogError("[BView]Colliders have empty element:" + base.name);
			}
			else if (!array[i].isTrigger)
			{
				Collider item2 = null;
				GameObject gameObject = new GameObject("OutCollider" + i);
				gameObject.layer = 9;
				gameObject.transform.parent = obj.transform;
				gameObject.transform.localScale = Vector3.one;
				PEFollowSimple pEFollowSimple = gameObject.AddComponent<PEFollowSimple>();
				pEFollowSimple.master = array[i].transform;
				if (array[i] is BoxCollider)
				{
					BoxCollider boxCollider = array[i] as BoxCollider;
					BoxCollider boxCollider2 = gameObject.AddComponent<BoxCollider>();
					item2 = boxCollider2;
					boxCollider2.center = boxCollider.center;
					boxCollider2.size = boxCollider.size + 0.2f * Vector3.one;
				}
				else if (array[i] is SphereCollider)
				{
					SphereCollider sphereCollider = array[i] as SphereCollider;
					SphereCollider sphereCollider2 = gameObject.AddComponent<SphereCollider>();
					item2 = sphereCollider2;
					sphereCollider2.center = sphereCollider.center;
					sphereCollider2.radius = sphereCollider.radius + 0.1f;
				}
				else if (array[i] is CapsuleCollider)
				{
					CapsuleCollider capsuleCollider = array[i] as CapsuleCollider;
					CapsuleCollider capsuleCollider2 = gameObject.AddComponent<CapsuleCollider>();
					item2 = capsuleCollider2;
					capsuleCollider2.center = capsuleCollider.center;
					capsuleCollider2.radius = capsuleCollider.radius + 0.1f;
					capsuleCollider2.height = capsuleCollider.height + 0.2f;
					capsuleCollider2.direction = capsuleCollider.direction;
				}
				if (m_IsNPCOutCol)
				{
					g_NPCOutColliders.Add(item2);
					g_NPCModelColliders.Add(array[i]);
				}
				list.Add(array[i]);
				list2.Add(item2);
			}
		}
		m_SelfCols = list.ToArray();
		m_OutCols = list2.ToArray();
		list.Clear();
		list2.Clear();
		list = null;
		list2 = null;
		ResetOutCollider();
	}

	private void ResetOutCollider(bool active = true)
	{
		if (m_SelfCols == null || m_OutCols == null)
		{
			return;
		}
		if (m_IsNPCOutCol)
		{
			for (int num = g_NPCModelColliders.Count - 1; num >= 0; num--)
			{
				if (null == g_NPCModelColliders[num])
				{
					g_NPCModelColliders.RemoveAt(num);
				}
				else
				{
					PEUtil.IgnoreCollision(m_OutCols, g_NPCModelColliders[num]);
				}
			}
			for (int num2 = g_NPCOutColliders.Count - 1; num2 >= 0; num2--)
			{
				if (null == g_NPCOutColliders[num2])
				{
					g_NPCOutColliders.RemoveAt(num2);
				}
				else
				{
					PEUtil.IgnoreCollision(m_SelfCols, g_NPCOutColliders[num2]);
				}
			}
		}
		PEUtil.IgnoreCollision(monoModelCtrlr.colliders, m_OutCols);
		PEUtil.IgnoreCollision(monoRagdollCtrlr.colliders, m_OutCols);
		for (int i = 0; i < m_OutCols.Length; i++)
		{
			if (null != m_OutCols[i])
			{
				m_OutCols[i].enabled = active;
			}
		}
	}

	protected virtual void OnBoneLoadSync(GameObject modelObject)
	{
		if ((bool)cmptEquipment)
		{
			cmptEquipment.ResetModels();
		}
	}

	public override void Start()
	{
		base.Start();
		cmptIK = base.Entity.GetCmpt<IKCmpt>();
		cmptCommon = GetComponent<CommonCmpt>();
		cmptEquipment = GetComponent<EquipmentCmpt>();
		base.Entity.aliveEntity.deathEvent += OnDeath;
		base.Entity.aliveEntity.reviveEvent += OnRevive;
		if (SceneMan.self == null)
		{
			this.Invoke(0.1f, Build);
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		base.Entity.aliveEntity.deathEvent -= OnDeath;
		base.Entity.aliveEntity.reviveEvent -= OnRevive;
		if (mPrefabReq != null)
		{
			mPrefabReq.Deactivate();
		}
	}

	public override void Deserialize(BinaryReader r)
	{
		base.Deserialize(r);
		m_PrefabPath = r.ReadString();
	}

	public override void Serialize(BinaryWriter w)
	{
		base.Serialize(w);
		w.Write(m_PrefabPath);
	}

	private void OnDeath(SkEntity self, SkEntity injurer)
	{
		ActivateRagdoll(null, isGetupReady: false);
		ActivateInjured(value: false);
		if (monoModelCtrlr != null)
		{
			monoModelCtrlr.ActivateDeathEffect();
			monoModelCtrlr.ActivateDeathMode(isDeath: true);
		}
	}

	private void OnRevive(SkEntity self)
	{
	}

	private bool HasColliderUnder(Vector3 pos)
	{
		return Physics.Raycast(pos + Vector3.up * 2f, Vector3.down, 100f);
	}

	private Transform GetCenterBone()
	{
		string empty = string.Empty;
		if (monoRagdollCtrlr != null && monoRagdollCtrlr.ragdollRootBone != null)
		{
			empty = monoRagdollCtrlr.ragdollRootBone.name;
		}
		Transform transform = ((!(monoModelCtrlr != null)) ? null : PEUtil.GetChild(monoModelCtrlr.transform, empty));
		return (transform != null) ? transform : ((!(monoModelCtrlr != null)) ? null : monoModelCtrlr.transform);
	}

	public void ActivateRagdoll(RagdollHitInfo hitInfo = null, bool isGetupReady = true)
	{
		if (monoRagdollCtrlr != null)
		{
			monoRagdollCtrlr.Activate(hitInfo, isGetupReady);
		}
		if (null != cmptIK)
		{
			cmptIK.ikEnable = false;
		}
	}

	public void DeactivateRagdoll(bool immediately = false)
	{
		if (monoRagdollCtrlr != null)
		{
			monoRagdollCtrlr.Deactivate(immediately);
		}
		if (null != cmptIK)
		{
			cmptIK.ikEnable = true;
		}
	}

	public virtual void AddPart(int partMask, string path)
	{
	}

	public virtual void RemovePart(int partMask)
	{
	}

	public bool DetachObject(GameObject obj)
	{
		if (monoModelCtrlr != null && obj != null && (bool)monoBoneCollector)
		{
			monoBoneCollector.RemoveEquipment(obj.transform);
			monoModelCtrlr.Remodel();
			base.Entity.SendMsg(EMsg.View_Model_DeatchJoint, obj);
		}
		return true;
	}

	public void Reattach(GameObject obj, string boneName)
	{
		if (monoModelCtrlr != null && (bool)monoBoneCollector)
		{
			monoBoneCollector.SwitchBone(obj.transform, boneName);
			monoModelCtrlr.Remodel();
			base.Entity.SendMsg(EMsg.View_Model_ReattachJoint, obj);
		}
	}

	public bool IsAttached(GameObject obj)
	{
		if (monoModelCtrlr != null && (bool)monoBoneCollector)
		{
			return monoBoneCollector.FindEquipGroup(obj.transform) >= 0;
		}
		return false;
	}

	public void AttachObject(GameObject obj, string boneName)
	{
		Collider[] componentsInChildren = obj.GetComponentsInChildren<Collider>();
		if (monoModelCtrlr != null && (bool)monoBoneCollector)
		{
			monoBoneCollector.AddEquipment(obj.transform, boneName);
			PEUtil.IgnoreCollision(monoModelCtrlr.colliders, componentsInChildren);
			int layer = monoModelCtrlr.gameObject.layer;
			obj.transform.TraverseHierarchy(delegate(Transform t, int i)
			{
				t.gameObject.layer = layer;
			});
			monoModelCtrlr.Remodel();
			base.Entity.SendMsg(EMsg.View_Model_AttachJoint, obj);
		}
		if (monoRagdollCtrlr != null)
		{
			PEUtil.IgnoreCollision(monoRagdollCtrlr.colliders, componentsInChildren);
		}
	}

	public virtual void Build()
	{
		if (!string.IsNullOrEmpty(prefabPath) && !(null != mViewPrefab) && mPrefabReq == null)
		{
			mPrefabReq = AssetsLoader.Instance.AddReq(m_PrefabPath, base.Entity.peTrans.position, base.Entity.peTrans.rotation, Vector3.one);
			mPrefabReq.ReqFinishHandler += OnBoneLoad;
		}
	}

	private GameObject BuildModelSync()
	{
		if (string.IsNullOrEmpty(prefabPath))
		{
			return null;
		}
		GameObject gameObject = AssetsLoader.Instance.InstantiateAssetImm(m_PrefabPath, base.Entity.peTrans.position, base.Entity.peTrans.rotation, Vector3.one);
		if (gameObject == null)
		{
			return null;
		}
		BiologyViewRoot biologyViewRoot = this.biologyViewRoot;
		PEModelController pEModelController = monoModelCtrlr;
		BoneCollector boneCollector = monoBoneCollector;
		this.biologyViewRoot = gameObject.GetComponent<BiologyViewRoot>();
		monoModelCtrlr = this.biologyViewRoot.modelController;
		monoBoneCollector = gameObject.GetComponent<BoneCollector>();
		GameObject gameObject2 = monoModelCtrlr.gameObject;
		OnBoneLoadSync(gameObject2);
		this.biologyViewRoot = biologyViewRoot;
		monoModelCtrlr = pEModelController;
		monoBoneCollector = boneCollector;
		gameObject2.transform.SetParent(null, worldPositionStays: true);
		UnityEngine.Object.Destroy(gameObject);
		return gameObject2;
	}

	public virtual void Destroy()
	{
		if (mViewPrefab != null)
		{
			base.Entity.SendMsg(EMsg.View_Prefab_Destroy, mViewPrefab.gameObject);
			monoBoneCollector = null;
			if (monoModelCtrlr != null)
			{
				base.Entity.SendMsg(EMsg.View_Model_Destroy, monoModelCtrlr.gameObject);
			}
			if (monoRagdollCtrlr != null)
			{
				base.Entity.SendMsg(EMsg.View_Ragdoll_Destroy, monoRagdollCtrlr.gameObject);
			}
			if (mPrefabReq != null)
			{
				mPrefabReq.Deactivate();
			}
			UnityEngine.Object.Destroy(mViewPrefab.gameObject);
		}
	}

	public virtual GameObject CloneModel()
	{
		GameObject gameObject = null;
		if (null != monoModelCtrlr && monoBoneCollector != null && m_MeshBuildEnd)
		{
			bool isRagdoll = monoBoneCollector.isRagdoll;
			monoBoneCollector.isRagdoll = false;
			gameObject = UnityEngine.Object.Instantiate(monoModelCtrlr.gameObject);
			if (isRagdoll)
			{
				Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = true;
				}
			}
			monoBoneCollector.isRagdoll = isRagdoll;
		}
		else
		{
			try
			{
				gameObject = BuildModelSync();
			}
			catch
			{
				gameObject = null;
			}
		}
		if (gameObject != null)
		{
			gameObject.transform.rotation = Quaternion.identity;
		}
		return gameObject;
	}

	public void ActivateRagdollRenderer(bool isActive)
	{
		UpdateRender();
	}

	public void OnMsg(EMsg msg, params object[] args)
	{
		if (msg == EMsg.View_FirstPerson)
		{
			bool flag = (bool)args[0];
			ActivateRenderer((Type)args[1], !flag);
			if (flag)
			{
				Fadeout(0.2f);
			}
			else
			{
				Fadein(0.2f);
			}
		}
	}
}
