using System.Collections;
using UnityEngine;

public abstract class CSEntityObject : GLBehaviour
{
	public CSBuildingLogic csbl;

	[HideInInspector]
	public ColonyBase _ColonyObj;

	[HideInInspector]
	public int m_ObjectId;

	public float m_Power;

	public int m_ItemID;

	public Material m_EffectsMat;

	[HideInInspector]
	public CSCreator m_Creator;

	[HideInInspector]
	public CSEntity m_Entity;

	[HideInInspector]
	public int m_BoundState;

	public CSConst.ObjectType m_Type;

	private Animation[] workAnimation;

	private AudioController workSound;

	public Transform[] m_WorkTrans;

	private bool m_Start;

	private MeshRenderer[] m_MeshRenders;

	private SkinnedMeshRenderer[] m_SkinnerMeshRenders;

	private MeshFilter[] m_Meshfilters;

	private float m_StartTime;

	private Material m_LineMat;

	public bool HasStarted => m_Start;

	public virtual int Init(CSBuildingLogic csbl, CSCreator creator, bool bFight = true)
	{
		this.csbl = csbl;
		CSEntityAttr attr = default(CSEntityAttr);
		attr.m_InstanceId = csbl.InstanceId;
		attr.m_protoId = csbl.protoId;
		attr.m_Type = (int)csbl.m_Type;
		attr.m_Pos = csbl.transform.position;
		attr.m_LogicObj = csbl.gameObject;
		attr.m_Obj = base.gameObject;
		attr.m_Bound = GetObjectBounds();
		attr.m_Bound.center = base.transform.TransformPoint(attr.m_Bound.center);
		attr.m_ColonyBase = _ColonyObj;
		int num = creator.CreateEntity(attr, out m_Entity);
		if (num != 4)
		{
			return num;
		}
		m_Creator = creator;
		m_ObjectId = csbl.InstanceId;
		if (bFight)
		{
		}
		return num;
	}

	public virtual int Init(int id, CSCreator creator, bool bFight = true)
	{
		CSEntityAttr attr = default(CSEntityAttr);
		attr.m_InstanceId = id;
		attr.m_protoId = m_ItemID;
		attr.m_Type = (int)m_Type;
		attr.m_Pos = base.transform.position;
		attr.m_Obj = base.gameObject;
		attr.m_Bound = GetObjectBounds();
		attr.m_Bound.center = base.transform.TransformPoint(attr.m_Bound.center);
		attr.m_ColonyBase = _ColonyObj;
		int num = creator.CreateEntity(attr, out m_Entity);
		if (num != 4)
		{
			return num;
		}
		m_Creator = creator;
		m_ObjectId = id;
		if (bFight)
		{
		}
		return num;
	}

	public Bounds GetObjectBounds()
	{
		MeshFilter[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshFilter>(includeInactive: true);
		Bounds b_out = new Bounds(Vector3.zero, Vector3.zero);
		MeshFilter[] array = componentsInChildren;
		foreach (MeshFilter meshFilter in array)
		{
			if (meshFilter != null && meshFilter.gameObject.layer == CSMain.CSEntityLayerIndex)
			{
				Bounds bounds = meshFilter.mesh.bounds;
				ExtendBounds(bounds, meshFilter.transform, ref b_out);
			}
		}
		SkinnedMeshRenderer[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
		SkinnedMeshRenderer[] array2 = componentsInChildren2;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array2)
		{
			if (skinnedMeshRenderer != null && skinnedMeshRenderer.gameObject.layer == CSMain.CSEntityLayerIndex)
			{
				Bounds bounds2 = skinnedMeshRenderer.sharedMesh.bounds;
				ExtendBounds(bounds2, skinnedMeshRenderer.transform, ref b_out);
			}
		}
		return b_out;
	}

	private void ExtendBounds(Bounds b_in, Transform trans, ref Bounds b_out)
	{
		Vector3 direction = trans.right * trans.lossyScale.x;
		Vector3 direction2 = trans.up * trans.lossyScale.y;
		Vector3 direction3 = trans.forward * trans.lossyScale.z;
		direction = base.transform.InverseTransformDirection(direction);
		direction2 = base.transform.InverseTransformDirection(direction2);
		direction3 = base.transform.InverseTransformDirection(direction3);
		Vector3 vector = base.transform.InverseTransformPoint(trans.TransformPoint(b_in.center));
		Vector3[] array = new Vector3[8];
		for (int i = 0; i < 8; i++)
		{
			array[i] = vector;
			if ((i & 1) == 0)
			{
				array[i] -= b_in.extents.x * direction;
			}
			else
			{
				array[i] += b_in.extents.x * direction;
			}
			if ((i & 2) == 0)
			{
				array[i] -= b_in.extents.y * direction2;
			}
			else
			{
				array[i] += b_in.extents.y * direction2;
			}
			if ((i & 4) == 0)
			{
				array[i] -= b_in.extents.z * direction3;
			}
			else
			{
				array[i] += b_in.extents.z * direction3;
			}
		}
		if (object.Equals(b_out.extents, Vector3.zero))
		{
			b_out.center = array[0];
			for (int j = 1; j < 8; j++)
			{
				b_out.Encapsulate(array[j]);
			}
		}
		else
		{
			for (int k = 0; k < 8; k++)
			{
				b_out.Encapsulate(array[k]);
			}
		}
	}

	public void SetCollidersEnable(bool enable)
	{
		Collider[] componentsInChildren = base.gameObject.GetComponentsInChildren<Collider>(includeInactive: true);
		Collider[] array = componentsInChildren;
		foreach (Collider collider in array)
		{
			collider.enabled = enable;
		}
	}

	protected void OnDestroy()
	{
	}

	protected void Awake()
	{
	}

	protected void Start()
	{
		GlobalGLs.AddGL(this);
		if (m_EffectsMat != null)
		{
			m_Meshfilters = base.gameObject.GetComponentsInChildren<MeshFilter>(includeInactive: true);
			m_MeshRenders = base.gameObject.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
			MeshRenderer[] meshRenders = m_MeshRenders;
			foreach (MeshRenderer meshRenderer in meshRenders)
			{
				meshRenderer.enabled = false;
			}
			m_SkinnerMeshRenders = base.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
			SkinnedMeshRenderer[] skinnerMeshRenders = m_SkinnerMeshRenders;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnerMeshRenders)
			{
				skinnedMeshRenderer.enabled = false;
			}
			StartCoroutine(DelayToEndEffect());
		}
		m_StartTime = Time.time;
		workAnimation = base.gameObject.GetComponentsInChildren<Animation>(includeInactive: true);
		Animation[] array = workAnimation;
		foreach (Animation animation in array)
		{
			animation.Stop();
			animation.playAutomatically = false;
		}
		m_Start = false;
		if (m_Entity != null && m_Entity.m_Info.workSound >= 0)
		{
			workSound = AudioManager.instance.Create(base.gameObject.transform.position, m_Entity.m_Info.workSound, base.gameObject.transform, isPlay: false, isDelete: false);
		}
	}

	public void StartWork()
	{
		if (workAnimation != null)
		{
			Animation[] array = workAnimation;
			foreach (Animation animation in array)
			{
				if (!animation.isPlaying)
				{
					animation.wrapMode = WrapMode.Loop;
					animation.Play();
				}
			}
		}
		if (workSound != null && !workSound.isPlaying)
		{
			workSound.PlayAudio(0.5f);
		}
		m_Start = true;
	}

	public void StopWork()
	{
		if (workAnimation != null)
		{
			Animation[] array = workAnimation;
			foreach (Animation animation in array)
			{
				if (animation.isPlaying && animation.wrapMode != WrapMode.Once)
				{
					animation.wrapMode = WrapMode.Once;
				}
			}
		}
		if (workSound != null && workSound.isPlaying)
		{
			workSound.StopAudio(0.5f);
		}
		m_Start = false;
	}

	private IEnumerator DelayToEndEffect()
	{
		while (!(Time.time - m_StartTime > 0.5f))
		{
			MeshFilter[] meshfilters = m_Meshfilters;
			foreach (MeshFilter mf in meshfilters)
			{
				Graphics.DrawMesh(mf.mesh, mf.transform.position, mf.transform.rotation, m_EffectsMat, 0);
			}
			SkinnedMeshRenderer[] skinnerMeshRenders = m_SkinnerMeshRenders;
			foreach (SkinnedMeshRenderer mr in skinnerMeshRenders)
			{
				Graphics.DrawMesh(mr.sharedMesh, mr.transform.position, mr.transform.rotation, m_EffectsMat, 0);
			}
			yield return 0;
		}
		MeshRenderer[] meshRenders = m_MeshRenders;
		foreach (MeshRenderer mr2 in meshRenders)
		{
			mr2.enabled = true;
		}
		SkinnedMeshRenderer[] skinnerMeshRenders2 = m_SkinnerMeshRenders;
		foreach (SkinnedMeshRenderer mr3 in skinnerMeshRenders2)
		{
			mr3.enabled = true;
		}
	}

	protected void Update()
	{
		if (m_Entity != null && m_Entity.IsDoingJobOn)
		{
			if (!m_Start)
			{
				StartWork();
			}
		}
		else if (m_Start)
		{
			StopWork();
		}
	}

	public override void OnGL()
	{
		if (m_BoundState != 1 && m_BoundState != 2)
		{
			return;
		}
		Bounds objectBounds = GetObjectBounds();
		Vector3[] array = new Vector3[8];
		for (int i = 0; i < 8; i++)
		{
			ref Vector3 reference = ref array[i];
			reference = objectBounds.center;
			if ((i & 1) == 0)
			{
				array[i] -= objectBounds.extents.x * Vector3.right;
			}
			else
			{
				array[i] += objectBounds.extents.x * Vector3.right;
			}
			if ((i & 2) == 0)
			{
				array[i] -= objectBounds.extents.y * Vector3.up;
			}
			else
			{
				array[i] += objectBounds.extents.y * Vector3.up;
			}
			if ((i & 4) == 0)
			{
				array[i] -= objectBounds.extents.z * Vector3.forward;
			}
			else
			{
				array[i] += objectBounds.extents.z * Vector3.forward;
			}
			ref Vector3 reference2 = ref array[i];
			reference2 = base.transform.TransformPoint(array[i]);
		}
		if (m_LineMat == null)
		{
			m_LineMat = new Material(Shader.Find("Lines/Colored Blended"));
			m_LineMat.hideFlags = HideFlags.HideAndDontSave;
			m_LineMat.shader.hideFlags = HideFlags.HideAndDontSave;
		}
		GL.PushMatrix();
		m_LineMat.SetPass(0);
		GL.Begin(1);
		GL.Color((m_BoundState != 1) ? Color.red : Color.green);
		GL.Vertex3(array[0].x, array[0].y, array[0].z);
		GL.Vertex3(array[1].x, array[1].y, array[1].z);
		GL.Vertex3(array[2].x, array[2].y, array[2].z);
		GL.Vertex3(array[3].x, array[3].y, array[3].z);
		GL.Vertex3(array[4].x, array[4].y, array[4].z);
		GL.Vertex3(array[5].x, array[5].y, array[5].z);
		GL.Vertex3(array[6].x, array[6].y, array[6].z);
		GL.Vertex3(array[7].x, array[7].y, array[7].z);
		GL.Vertex3(array[0].x, array[0].y, array[0].z);
		GL.Vertex3(array[4].x, array[4].y, array[4].z);
		GL.Vertex3(array[1].x, array[1].y, array[1].z);
		GL.Vertex3(array[5].x, array[5].y, array[5].z);
		GL.Vertex3(array[2].x, array[2].y, array[2].z);
		GL.Vertex3(array[6].x, array[6].y, array[6].z);
		GL.Vertex3(array[3].x, array[3].y, array[3].z);
		GL.Vertex3(array[7].x, array[7].y, array[7].z);
		GL.Vertex3(array[0].x, array[0].y, array[0].z);
		GL.Vertex3(array[2].x, array[2].y, array[2].z);
		GL.Vertex3(array[1].x, array[1].y, array[1].z);
		GL.Vertex3(array[3].x, array[3].y, array[3].z);
		GL.Vertex3(array[4].x, array[4].y, array[4].z);
		GL.Vertex3(array[6].x, array[6].y, array[6].z);
		GL.Vertex3(array[5].x, array[5].y, array[5].z);
		GL.Vertex3(array[7].x, array[7].y, array[7].z);
		GL.End();
		GL.Begin(7);
		GL.Color((m_BoundState != 1) ? new Color(0.1f, 0f, 0f, 0.15f) : new Color(0f, 0.1f, 0f, 0.15f));
		GL.Vertex3(array[0].x, array[0].y, array[0].z);
		GL.Vertex3(array[1].x, array[1].y, array[1].z);
		GL.Vertex3(array[3].x, array[3].y, array[3].z);
		GL.Vertex3(array[2].x, array[2].y, array[2].z);
		GL.Vertex3(array[4].x, array[4].y, array[4].z);
		GL.Vertex3(array[5].x, array[5].y, array[5].z);
		GL.Vertex3(array[7].x, array[7].y, array[7].z);
		GL.Vertex3(array[6].x, array[6].y, array[6].z);
		GL.Vertex3(array[3].x, array[3].y, array[3].z);
		GL.Vertex3(array[2].x, array[2].y, array[2].z);
		GL.Vertex3(array[6].x, array[6].y, array[6].z);
		GL.Vertex3(array[7].x, array[7].y, array[7].z);
		GL.Vertex3(array[0].x, array[0].y, array[0].z);
		GL.Vertex3(array[1].x, array[1].y, array[1].z);
		GL.Vertex3(array[5].x, array[5].y, array[5].z);
		GL.Vertex3(array[4].x, array[4].y, array[4].z);
		GL.Vertex3(array[1].x, array[1].y, array[1].z);
		GL.Vertex3(array[5].x, array[5].y, array[5].z);
		GL.Vertex3(array[7].x, array[7].y, array[7].z);
		GL.Vertex3(array[3].x, array[3].y, array[3].z);
		GL.Vertex3(array[0].x, array[0].y, array[0].z);
		GL.Vertex3(array[4].x, array[4].y, array[4].z);
		GL.Vertex3(array[6].x, array[6].y, array[6].z);
		GL.Vertex3(array[2].x, array[2].y, array[2].z);
		GL.End();
		GL.PopMatrix();
	}
}
