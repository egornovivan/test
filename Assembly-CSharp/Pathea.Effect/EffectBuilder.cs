using System.Collections;
using System.Collections.Generic;
using PETools;
using SkillSystem;
using UnityEngine;

namespace Pathea.Effect;

public class EffectBuilder : Singleton<EffectBuilder>
{
	public class EffectRequest
	{
		internal int m_ID;

		internal object m_Data;

		internal EffectData m_EffectData;

		public event OnEffectSpawned SpawnEvent;

		public EffectRequest(int argid, object argData)
		{
			m_ID = argid;
			m_Data = argData;
			m_EffectData = EffectData.GetEffCastData(m_ID);
		}

		protected void OnSpawned(GameObject obj)
		{
			if (this.SpawnEvent != null)
			{
				this.SpawnEvent(obj);
			}
		}

		public virtual bool IsValid()
		{
			return false;
		}

		public virtual GameObject Create()
		{
			return null;
		}
	}

	public class EffectRequestTransform : EffectRequest
	{
		public Transform _bone;

		public Transform _rootTrans;

		public EffectRequestTransform(int argid, object argData, Transform argTrans)
			: base(argid, argData)
		{
			_rootTrans = argTrans;
			if (m_EffectData != null)
			{
				if (m_EffectData.m_posStr == "0" || m_EffectData.m_posStr == "CenterPos")
				{
					_bone = _rootTrans;
				}
				else
				{
					_bone = PEUtil.GetChild(_rootTrans, m_EffectData.m_posStr);
				}
			}
		}

		public override bool IsValid()
		{
			return m_EffectData != null && _bone != null;
		}

		public override GameObject Create()
		{
			if (m_EffectData == null || _bone == null)
			{
				return null;
			}
			GameObject effect = Singleton<EffectBuilder>.Instance.GetEffect(m_EffectData.m_path);
			if (effect != null)
			{
				Quaternion rotation = Quaternion.identity;
				if (m_EffectData.m_Rot)
				{
					rotation = _bone.rotation;
				}
				GameObject gameObject = Object.Instantiate(effect, _bone.position, rotation) as GameObject;
				if (m_EffectData.m_liveTime > -1E-45f)
				{
					Object.DestroyObject(gameObject, m_EffectData.m_liveTime);
				}
				if (m_EffectData.m_bind)
				{
					EffectLateupdateHelper component = gameObject.GetComponent<EffectLateupdateHelper>();
					if (null != component)
					{
						component.Init(_bone);
						if (m_EffectData.m_posStr == "CenterPos")
						{
							PeEntity componentInParent = _rootTrans.GetComponentInParent<PeEntity>();
							if (componentInParent != null)
							{
								component.local = componentInParent.bounds.center;
								HitEffectScale component2 = gameObject.GetComponent<HitEffectScale>();
								if (component2 != null)
								{
									component2.Scale = componentInParent.maxRadius * 2f;
									if (componentInParent.peTrans != null)
									{
										component2.EmissionScale = componentInParent.peTrans.bound.size;
									}
								}
							}
						}
					}
				}
				if (m_Data != null)
				{
					MonoBehaviour[] componentsInChildren = gameObject.GetComponentsInChildren<MonoBehaviour>();
					MonoBehaviour[] array = componentsInChildren;
					foreach (MonoBehaviour monoBehaviour in array)
					{
						SkInst skInst = m_Data as SkInst;
						ISkEffectEntity skEffectEntity = monoBehaviour as ISkEffectEntity;
						if (skInst != null && skEffectEntity != null && !skEffectEntity.Equals(null))
						{
							skEffectEntity.Inst = skInst;
						}
					}
				}
				OnSpawned(gameObject);
				return gameObject;
			}
			return null;
		}
	}

	public class EffectRequestWorldPos : EffectRequest
	{
		private Vector3 position;

		private Quaternion rotation;

		private Transform parent;

		public EffectRequestWorldPos(int argid, object argData, Vector3 worldPos, Quaternion worldRot, Transform parentTrans)
			: base(argid, argData)
		{
			position = worldPos;
			rotation = worldRot;
			parent = parentTrans;
		}

		public override bool IsValid()
		{
			return m_EffectData != null;
		}

		public override GameObject Create()
		{
			if (m_EffectData == null)
			{
				return null;
			}
			GameObject effect = Singleton<EffectBuilder>.Instance.GetEffect(m_EffectData.m_path);
			if (effect != null)
			{
				GameObject gameObject = Object.Instantiate(effect, position, rotation) as GameObject;
				if (m_EffectData.m_liveTime > -1E-45f)
				{
					Object.DestroyObject(gameObject, m_EffectData.m_liveTime);
				}
				if (m_Data != null)
				{
					MonoBehaviour[] componentsInChildren = gameObject.GetComponentsInChildren<MonoBehaviour>();
					MonoBehaviour[] array = componentsInChildren;
					foreach (MonoBehaviour monoBehaviour in array)
					{
						SkInst skInst = m_Data as SkInst;
						ISkEffectEntity skEffectEntity = monoBehaviour as ISkEffectEntity;
						if (skInst != null && skEffectEntity != null && !skEffectEntity.Equals(null))
						{
							skEffectEntity.Inst = skInst;
						}
					}
				}
				if (null != parent)
				{
					gameObject.transform.parent = parent;
				}
				OnSpawned(gameObject);
				return gameObject;
			}
			return null;
		}
	}

	public delegate void OnEffectSpawned(GameObject obj);

	private static Dictionary<string, GameObject> s_EffectPools = new Dictionary<string, GameObject>();

	private List<EffectRequest> m_ReqList = new List<EffectRequest>();

	public GameObject GetEffect(string path)
	{
		GameObject value = null;
		if (s_EffectPools.TryGetValue(path, out value))
		{
			return value;
		}
		Singleton<EffectBuilder>.Instance.StartCoroutine(LoadEffect(path));
		return null;
	}

	private IEnumerator LoadEffect(string path)
	{
		s_EffectPools[path] = null;
		ResourceRequest rr = Resources.LoadAsync<GameObject>(path);
		while (!rr.isDone)
		{
			yield return null;
		}
		s_EffectPools[path] = rr.asset as GameObject;
	}

	private void LateUpdate()
	{
		for (int num = m_ReqList.Count - 1; num >= 0; num--)
		{
			if (!m_ReqList[num].IsValid())
			{
				m_ReqList.RemoveAt(num);
			}
			else
			{
				GameObject gameObject = m_ReqList[num].Create();
				if (gameObject != null)
				{
					if (gameObject.transform.parent == null)
					{
						gameObject.transform.parent = base.transform;
					}
					m_ReqList.RemoveAt(num);
				}
			}
		}
	}

	public EffectRequest Register(int id, object data, Transform caster)
	{
		EffectRequest effectRequest = new EffectRequestTransform(id, data, caster);
		m_ReqList.Add(effectRequest);
		return effectRequest;
	}

	public EffectRequest Register(int id, object data, Vector3 position, Quaternion rotation, Transform parent = null)
	{
		EffectRequest effectRequest = new EffectRequestWorldPos(id, data, position, rotation, parent);
		m_ReqList.Add(effectRequest);
		return effectRequest;
	}

	public void RegisterEffectFromSkill(int id, SkRuntimeInfo info, Transform caster)
	{
		EffectData effCastData = EffectData.GetEffCastData(id);
		if (effCastData != null)
		{
			if (info is SkInst skInst && effCastData.m_posStr == "0")
			{
				Register(id, info, (skInst._colInfo == null) ? caster.position : skInst._colInfo.hitPos, Quaternion.identity);
			}
			else
			{
				Register(id, info, caster);
			}
		}
	}
}
