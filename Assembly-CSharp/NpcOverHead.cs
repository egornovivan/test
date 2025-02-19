using System.Collections;
using System.Collections.Generic;
using Pathea;
using PETools;
using SkillSystem;
using UnityEngine;

public class NpcOverHead : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer missionMark;

	[SerializeField]
	private MeshRenderer revivalMark;

	[SerializeField]
	private UILabel mLable;

	[SerializeField]
	private UISprite mIcon;

	[SerializeField]
	private Transform NameLb;

	[SerializeField]
	private Transform PlayerTrans;

	[SerializeField]
	private Transform NpcTrans;

	[SerializeField]
	private Transform MonsterTrans;

	[SerializeField]
	private Transform SpeakTrans;

	[SerializeField]
	private NpcSpeakSentenseItem mSpeakSentensePrefab;

	[SerializeField]
	private UISlider mBloodPlayer;

	[SerializeField]
	private Bloodcmpt mPlayerBloodcmpt;

	[SerializeField]
	private UISlider mBloodMonster;

	[SerializeField]
	private Bloodcmpt mMonsterBloodcmpt;

	[SerializeField]
	private UISlider mBloodNpc;

	[SerializeField]
	private Bloodcmpt mNpcBloodcmpt;

	public float minDis = 0.4f;

	public float maxDis = 12f;

	public float heightAdjust = 0.4f;

	public float scaleAdjust = 8f;

	[SerializeField]
	private bool m_CircleHpBar;

	[SerializeField]
	private bool m_LowHpBar;

	[SerializeField]
	private float m_LowHpBarFactor = 0.2f;

	private List<int> m_NeedCircleHpBarMonsterIDs = new List<int> { 95 };

	private List<int> m_NeedLowHpBarMonsterIDs = new List<int> { 96 };

	private NpcMissionState missionState = NpcMissionState.Max;

	private float mTimer;

	private Bounds m_localBounds;

	private bool InitBloodPos;

	private PeEntity m_Entity;

	private SkAliveEntity m_SkAlive;

	private Transform mCurBloodTrans;

	private UISlider mCurBloodSlider;

	private EEntityProto mCurEEntityProto = EEntityProto.Max;

	private PeEntity m_entity;

	private float HPchangeTime;

	private float BLOOD_MAX_TIME = 20f;

	private bool mInitBlood;

	private bool mDamge;

	private bool mInitNameLb;

	private bool mHideMainPlayerInfo;

	private List<NpcSpeakSentenseItem> mSpeakSentenseList = new List<NpcSpeakSentenseItem>();

	public PeEntity EntityOver
	{
		get
		{
			return m_Entity;
		}
		set
		{
			m_Entity = value;
		}
	}

	public bool Visiable
	{
		get
		{
			return base.gameObject.activeInHierarchy;
		}
		set
		{
			base.gameObject.SetActive(value);
		}
	}

	public EEntityProto CurEEntityProto
	{
		get
		{
			return mCurEEntityProto;
		}
		set
		{
			mCurEEntityProto = value;
			SwichBloodByProto(mCurEEntityProto);
		}
	}

	public SkAliveEntity SkAlive
	{
		get
		{
			if (m_SkAlive == null)
			{
				if (m_entity != null)
				{
					m_SkAlive = m_entity.GetCmpt<SkAliveEntity>();
				}
				return m_SkAlive;
			}
			return m_SkAlive;
		}
	}

	public float HpPercent => (!(SkAlive != null)) ? 1f : SkAlive.HPPercent;

	private void Awake()
	{
	}

	private void Start()
	{
		SetRevivalMark(show: false, 0f);
		StartCoroutine(UpdateBlood());
		CheckMainPlayerInfo();
	}

	private void Update()
	{
		if (null != m_Entity && null != m_Entity.NpcCmpt)
		{
			if (m_entity.NpcCmpt.IsFollower)
			{
				missionMark.gameObject.SetActive(value: false);
			}
			else
			{
				SetState(missionState);
			}
		}
		CheckMainPlayerInfo();
	}

	private void LateUpdate()
	{
		mTimer += Time.deltaTime;
		if (mTimer >= 0.25f)
		{
			mTimer = 0f;
			UpdateSpeakPos();
		}
		UpdateTransform();
	}

	public void UpdateTransform()
	{
		Transform mainCamTransform = PEUtil.MainCamTransform;
		if (mainCamTransform != null && null != m_Entity && m_Entity.peTrans != null)
		{
			Vector3 zero = Vector3.zero;
			float num = 0f;
			if (m_CircleHpBar)
			{
				zero = m_Entity.peTrans.center;
				num = Vector3.Distance(mainCamTransform.position, zero);
				num = Mathf.Clamp(num, minDis, maxDis);
				float num2 = Mathf.Max(m_Entity.peTrans.boundExtend.extents.x, m_Entity.peTrans.boundExtend.extents.z);
				Vector3 vector = Vector3.Normalize(mainCamTransform.position - zero);
				zero.z += vector.z * num2;
				zero.x += vector.x * num2;
				float num3 = num * heightAdjust;
				zero.y += num3;
			}
			else
			{
				zero = m_Entity.peTrans.uiHeadTop;
				num = Vector3.Distance(mainCamTransform.position, zero);
				num = Mathf.Clamp(num, minDis, maxDis);
				float num4 = num * heightAdjust;
				zero.y += num4;
			}
			base.transform.position = zero;
			float num5 = num / scaleAdjust;
			base.transform.localScale = new Vector3(num5, num5, 0f);
			base.transform.rotation = mainCamTransform.rotation;
			SetBouds(m_Entity.peTrans.bound);
		}
	}

	public void SetNameShow(bool showOrnot)
	{
		NameLbShow(showOrnot);
	}

	public bool SetProto(EEntityProto Proto)
	{
		return true;
	}

	public void SetTheEntity(PeEntity entity)
	{
		m_Entity = entity;
		m_CircleHpBar = m_NeedCircleHpBarMonsterIDs.Contains(m_Entity.ProtoID);
		m_LowHpBar = m_NeedLowHpBarMonsterIDs.Contains(m_Entity.ProtoID);
	}

	public void SetBouds(Bounds localBounds)
	{
		m_localBounds = localBounds;
		UpdateLocalPostion();
	}

	public void ShowMissionMark(bool show)
	{
		missionMark.gameObject.SetActive(show);
	}

	public void WillBeDestroyed()
	{
	}

	public void SetState(NpcMissionState state)
	{
		missionState = state;
		SetStateMark(state);
	}

	public void SetRevivalMark(EEntityProto Proto, bool show, float percent)
	{
		if (null == revivalMark)
		{
			Debug.LogWarning(string.Concat(this, "revivalMark is null"));
		}
		else if (Proto == EEntityProto.Npc)
		{
			if (!show)
			{
				revivalMark.gameObject.SetActive(value: false);
				SetStateMark(missionState);
			}
			else
			{
				revivalMark.gameObject.SetActive(value: true);
				revivalMark.material.SetFloat("_Percent", percent);
				SetStateMark(NpcMissionState.Max);
			}
		}
	}

	public void SetRevivalMark(bool show, float percent)
	{
		if (null == revivalMark)
		{
			Debug.LogWarning(string.Concat(this, "revivalMark is null"));
		}
		else if (!show)
		{
			revivalMark.gameObject.SetActive(value: false);
			SetStateMark(missionState);
		}
		else
		{
			revivalMark.gameObject.SetActive(value: true);
			revivalMark.material.SetFloat("_Percent", percent);
			SetStateMark(NpcMissionState.Max);
		}
	}

	public void SetNpcShowName(string npcName, bool colored = false)
	{
		HeadInfoMgr.Instance.SetText(base.transform, npcName);
		if (!(mLable == null))
		{
			mLable.text = npcName;
		}
	}

	public void SetNameColord(bool colored)
	{
		HeadInfoMgr.Instance.SetColor(base.transform, colored);
	}

	public void SetNpcShopIcon(string shopIcon)
	{
		HeadInfoMgr.Instance.SetIcon(base.transform, shopIcon);
	}

	public void SetShowIcon(string Icon)
	{
		switch (Icon)
		{
		case "shop_wuqi":
		case "shop_fangju":
		case "shop_buji":
		case "shop_jiaju":
		case "shop_zahuo":
		{
			mIcon.spriteName = Icon;
			mIcon.MakePixelPerfect();
			Vector3 localPosition = mLable.transform.localPosition;
			localPosition.x = localPosition.x - (mLable.relativeSize.x * (float)mLable.font.size + mIcon.transform.localScale.x) * 0.5f - 5f;
			mIcon.transform.localPosition = localPosition;
			break;
		}
		default:
			mIcon.spriteName = "null";
			break;
		}
	}

	public void Reset()
	{
		SetState(NpcMissionState.Max);
		SetRevivalMark(show: false, 0f);
		HeadInfoMgr.Instance.Remove(base.transform);
	}

	public void NameLbShow(bool show)
	{
		mInitNameLb = true;
		NameLb.gameObject.SetActive(show);
	}

	public void BloodShow(bool show)
	{
		if (!(mCurBloodTrans == null))
		{
			mInitBlood = true;
			mCurBloodTrans.gameObject.SetActive(show && !mHideMainPlayerInfo);
		}
	}

	public void HpChange(SkEntity caster, float hpChange)
	{
		if (!PeGameMgr.IsSingle || CurEEntityProto != 0)
		{
			SetBloodPercent(HpPercent, hpChange);
			mDamge = true;
			HPchangeTime = Time.time;
		}
	}

	public bool SetBloodPercent(float percent, float hpchange)
	{
		if (mCurBloodSlider == null)
		{
			return false;
		}
		if (null != mCurBloodTrans)
		{
			if (percent > 0f)
			{
				if (hpchange < 0f)
				{
					BloodShow(show: true);
				}
			}
			else if (m_entity.entityProto.proto != 0)
			{
				BloodShow(show: false);
			}
		}
		mCurBloodSlider.sliderValue = percent;
		return true;
	}

	public void InitTheentity(PeEntity entity)
	{
		m_entity = entity;
	}

	private void CheckMainPlayerInfo()
	{
		if (null != m_Entity && PeGameMgr.IsMulti && m_Entity.IsMainPlayer)
		{
			if (SystemSettingData.Instance.FirstPersonCtrl)
			{
				mHideMainPlayerInfo = true;
				UpdateMainPlayerInfoState();
			}
			else if (SystemSettingData.Instance.HidePlayerOverHeadInfo != mHideMainPlayerInfo)
			{
				mHideMainPlayerInfo = SystemSettingData.Instance.HidePlayerOverHeadInfo;
				UpdateMainPlayerInfoState();
			}
		}
	}

	private void UpdateMainPlayerInfoState()
	{
		SetNameShow(!mHideMainPlayerInfo);
		mCurBloodTrans.gameObject.SetActive(!mHideMainPlayerInfo);
	}

	private void DestroyUI()
	{
		if (PlayerTrans != null)
		{
			PlayerTrans.gameObject.SetActive(value: false);
		}
		if (NpcTrans != null)
		{
			NpcTrans.gameObject.SetActive(value: false);
		}
		if (MonsterTrans != null)
		{
			MonsterTrans.gameObject.SetActive(value: false);
		}
	}

	private void Destroy()
	{
		HeadInfoMgr.Instance.Remove(base.transform);
	}

	private void OnDestroy()
	{
		DestroyUI();
	}

	private void InitOverHead()
	{
		Transform transform = base.transform.FindChild("MissionMark");
		if (null == transform)
		{
			Debug.LogWarning("no text mesh to show mission mark");
			return;
		}
		missionMark = transform.GetComponent<MeshRenderer>();
		transform = base.transform.FindChild("Revival");
		if (null == transform)
		{
			Debug.LogWarning("no Revival found");
			return;
		}
		revivalMark = transform.GetComponent<MeshRenderer>();
		SetRevivalMark(show: false, 0f);
		transform = base.transform.FindChild("Namlb");
		if (null == transform)
		{
			Debug.LogWarning("no text show Namlb");
			return;
		}
		NameLb = transform;
		transform = transform.transform.FindChild("Label");
		if (null == transform)
		{
			Debug.LogWarning("no text show Namlb");
			return;
		}
		mLable = transform.GetComponent<UILabel>();
		transform = NameLb.transform.FindChild("Icon");
		if (null == transform)
		{
			Debug.LogWarning("no text show Icon");
			return;
		}
		mIcon = transform.GetComponent<UISprite>();
		transform = (PlayerTrans = base.transform.FindChild("Player"));
		if (PlayerTrans != null)
		{
			transform = PlayerTrans.FindChild("BloodItemPlayer");
		}
		if (transform != null)
		{
			mBloodPlayer = transform.GetComponent<UISlider>();
			mPlayerBloodcmpt = transform.GetComponent<Bloodcmpt>();
		}
		transform = (NpcTrans = base.transform.FindChild("NPc"));
		if (NpcTrans != null)
		{
			transform = NpcTrans.FindChild("BloodItemNpc");
		}
		if (transform != null)
		{
			mBloodNpc = transform.GetComponent<UISlider>();
			mNpcBloodcmpt = transform.GetComponent<Bloodcmpt>();
		}
		transform = (MonsterTrans = base.transform.FindChild("Monster"));
		if (MonsterTrans != null)
		{
			transform = MonsterTrans.FindChild("BloodItemMon");
		}
		if (transform != null)
		{
			mBloodMonster = transform.GetComponent<UISlider>();
			mMonsterBloodcmpt = transform.GetComponent<Bloodcmpt>();
		}
		transform = base.transform.FindChild("Speak");
		SpeakTrans = transform;
		if (SpeakTrans != null)
		{
			mSpeakSentensePrefab = SpeakTrans.FindChild("SentencePrefab").GetComponent<NpcSpeakSentenseItem>();
		}
	}

	private void setLocalScale(Vector2 Stepsize)
	{
		mBloodPlayer.fullSize = Stepsize;
		mPlayerBloodcmpt.setBackScale(Stepsize);
		mBloodNpc.fullSize = Stepsize;
		mNpcBloodcmpt.setBackScale(Stepsize);
		mBloodMonster.fullSize = Stepsize;
		mMonsterBloodcmpt.setBackScale(Stepsize);
	}

	private void setPostion()
	{
		Vector3 localPosition = new Vector3((0f - mBloodPlayer.fullSize.x) / 200f, -0.2f, 0f);
		float num = 1f;
		if (mBloodPlayer.fullSize.x >= 1900f)
		{
			num = 6.2f;
		}
		mBloodPlayer.transform.localPosition = localPosition;
		float num2 = 0f;
		num2 = (m_LowHpBar ? (localPosition.y + num * m_LowHpBarFactor) : ((!m_CircleHpBar) ? (localPosition.y + num) : localPosition.y));
		mBloodMonster.transform.localPosition = new Vector3(localPosition.x, num2, localPosition.z);
		mBloodNpc.transform.localPosition = localPosition;
	}

	private Vector2 calculateSteps(Vector3 size)
	{
		float x = size.x;
		Vector2 result = Vector3.zero;
		result.x = Mathf.Round(Mathf.Clamp(x * 20f, 80f, 300f));
		result.y = Mathf.Round(result.x / 20f);
		return result;
	}

	private void UpdateLocalPostion()
	{
		if (m_localBounds.size != Vector3.zero && !InitBloodPos)
		{
			setLocalScale(calculateSteps(m_localBounds.size));
			setPostion();
			InitBloodPos = true;
		}
	}

	public void SayOneWord(string _content, float _interval)
	{
		if (mSpeakSentenseList.Count > 0)
		{
			mSpeakSentenseList[0].AheadDisappear();
		}
		NpcSpeakSentenseItem npcSpeakSentenseItem = CreatOneSentense(mSpeakSentensePrefab, SpeakTrans);
		mSpeakSentenseList.Add(npcSpeakSentenseItem);
		npcSpeakSentenseItem.OnDestroySelfEvent += OnDestroySentenseSelf;
		npcSpeakSentenseItem.SayOneWord(_content, _interval);
		UpdateSentensePos();
	}

	private NpcSpeakSentenseItem CreatOneSentense(NpcSpeakSentenseItem _prefab, Transform _parent)
	{
		NpcSpeakSentenseItem npcSpeakSentenseItem = Object.Instantiate(_prefab);
		npcSpeakSentenseItem.gameObject.SetActive(value: true);
		npcSpeakSentenseItem.transform.parent = _parent;
		npcSpeakSentenseItem.transform.localPosition = Vector3.zero;
		npcSpeakSentenseItem.transform.localRotation = Quaternion.identity;
		npcSpeakSentenseItem.transform.localScale = Vector3.one;
		return npcSpeakSentenseItem;
	}

	private void OnDestroySentenseSelf(NpcSpeakSentenseItem _item)
	{
		mSpeakSentenseList.Remove(_item);
	}

	private void UpdateSentensePos()
	{
		if (mSpeakSentenseList.Count > 1)
		{
			for (int num = mSpeakSentenseList.Count - 2; num >= 0; num--)
			{
				Vector3 localPosition = mSpeakSentenseList[num].transform.localPosition;
				Vector3 localPosition2 = new Vector3(localPosition.x, localPosition.y + 35f, localPosition.z);
				mSpeakSentenseList[num].transform.localPosition = localPosition2;
			}
		}
	}

	private void UpdateSpeakPos()
	{
		if (missionMark.gameObject.activeInHierarchy)
		{
			SpeakTrans.localPosition = new Vector3(SpeakTrans.localPosition.x, 1.25f, SpeakTrans.localPosition.z);
		}
		else if (revivalMark.gameObject.activeInHierarchy)
		{
			SpeakTrans.localPosition = new Vector3(SpeakTrans.localPosition.x, 0.8f, SpeakTrans.localPosition.z);
		}
		else
		{
			SpeakTrans.localPosition = new Vector3(SpeakTrans.localPosition.x, 0.35f, SpeakTrans.localPosition.z);
		}
	}

	private Texture GetMissionMark(NpcMissionState mark)
	{
		return mark switch
		{
			NpcMissionState.CanGet => Resources.Load("Texture2d/BillBoard/map_an_3") as Texture, 
			NpcMissionState.HasGet => Resources.Load("Texture2d/BillBoard/map_an_1") as Texture, 
			NpcMissionState.CanSubmit => Resources.Load("Texture2d/BillBoard/map_an_2") as Texture, 
			NpcMissionState.MainCanGet => Resources.Load("Texture2d/BillBoard/map_an_3_1") as Texture, 
			NpcMissionState.MainHasGet => Resources.Load("Texture2d/BillBoard/map_an_1_1") as Texture, 
			NpcMissionState.MainCanSubmit => Resources.Load("Texture2d/BillBoard/map_an_2_1") as Texture, 
			_ => null, 
		};
	}

	private void SetStateMark(NpcMissionState state)
	{
		if (null == missionMark)
		{
			Debug.LogWarning(string.Concat(this, "missionMark is null"));
			return;
		}
		if (state == NpcMissionState.Max)
		{
			missionMark.gameObject.SetActive(value: false);
			return;
		}
		revivalMark.gameObject.SetActive(value: false);
		missionMark.gameObject.SetActive(value: true);
		missionMark.material.mainTexture = GetMissionMark(state);
	}

	private void SwichBloodByProto(EEntityProto proto)
	{
		if (mCurBloodTrans != null)
		{
			return;
		}
		switch (proto)
		{
		case EEntityProto.Player:
			if (PeGameMgr.IsMulti && null != m_Entity)
			{
				if (null == m_Entity.netCmpt || (PeSingleton<PeCreature>.Instance != null && m_Entity == PeSingleton<PeCreature>.Instance.mainPlayer))
				{
					mCurBloodTrans = PlayerTrans;
					mCurBloodSlider = mBloodPlayer;
				}
				else if (null != m_Entity.netCmpt.network && PeSingleton<PeCreature>.Instance != null && null != PeSingleton<PeCreature>.Instance.mainPlayer && null != PeSingleton<PeCreature>.Instance.mainPlayer.netCmpt && null != PeSingleton<PeCreature>.Instance.mainPlayer.netCmpt.network && m_Entity.netCmpt.network.TeamId == PeSingleton<PeCreature>.Instance.mainPlayer.netCmpt.network.TeamId)
				{
					mCurBloodTrans = NpcTrans;
					mCurBloodSlider = mBloodNpc;
				}
				else
				{
					mCurBloodTrans = PlayerTrans;
					mCurBloodSlider = mBloodPlayer;
				}
			}
			else
			{
				mCurBloodTrans = PlayerTrans;
				mCurBloodSlider = mBloodPlayer;
			}
			break;
		case EEntityProto.Npc:
			mCurBloodTrans = NpcTrans;
			mCurBloodSlider = mBloodNpc;
			break;
		case EEntityProto.Monster:
		case EEntityProto.Doodad:
			mCurBloodTrans = MonsterTrans;
			mCurBloodSlider = mBloodMonster;
			break;
		default:
			mCurBloodTrans = null;
			mCurBloodSlider = null;
			break;
		}
	}

	private void InitPlyerBlood()
	{
		if (!(mCurBloodTrans == null) && !mInitBlood)
		{
			if (PeGameMgr.IsSingle)
			{
				BloodShow(show: false);
			}
			else
			{
				BloodShow(show: true);
			}
		}
	}

	private void InitPlayerNameLb()
	{
		if (!(mCurBloodTrans == null) && !mInitNameLb)
		{
			if (PeGameMgr.IsSingle)
			{
				NameLbShow(show: false);
			}
			else
			{
				NameLbShow(show: true);
			}
		}
	}

	private void InitOtherBlood()
	{
		if (!(mCurBloodTrans == null) && !mInitBlood)
		{
			BloodShow(show: false);
		}
	}

	private void InitBlood(EEntityProto proto)
	{
		if (!mInitBlood)
		{
			switch (proto)
			{
			case EEntityProto.Player:
				InitPlyerBlood();
				break;
			case EEntityProto.Npc:
			case EEntityProto.Monster:
			case EEntityProto.Doodad:
				InitOtherBlood();
				break;
			default:
				BloodShow(show: false);
				break;
			}
		}
	}

	private void TimerCout(EEntityProto proto, float startTime)
	{
		if (proto != 0 && !(mCurBloodTrans == null) && mDamge && Time.time - startTime >= BLOOD_MAX_TIME && mDamge)
		{
			BloodShow(show: false);
			mDamge = false;
		}
	}

	private void InitNameLb(EEntityProto proto)
	{
		if (mInitNameLb)
		{
			return;
		}
		switch (proto)
		{
		case EEntityProto.Player:
			InitPlayerNameLb();
			if (PeGameMgr.IsMulti)
			{
				NameLbShow(show: true);
			}
			break;
		case EEntityProto.RandomNpc:
		case EEntityProto.Npc:
			NameLbShow(show: true);
			break;
		case EEntityProto.Monster:
		case EEntityProto.Doodad:
			NameLbShow(show: false);
			break;
		default:
			NameLbShow(show: false);
			break;
		}
	}

	private IEnumerator UpdateBlood()
	{
		while (true)
		{
			SwichBloodByProto(mCurEEntityProto);
			InitBlood(mCurEEntityProto);
			InitNameLb(mCurEEntityProto);
			TimerCout(mCurEEntityProto, HPchangeTime);
			yield return new WaitForSeconds(1f);
		}
	}
}
