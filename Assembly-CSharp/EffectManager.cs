using System.Collections;
using System.Collections.Generic;
using SkillAsset;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
	private static EffectManager _Instance;

	public List<EffectInst> effectInstList;

	public List<Transform> effectList;

	public static EffectManager Instance
	{
		get
		{
			if (_Instance == null)
			{
				_Instance = new GameObject("EffectManager").AddComponent<EffectManager>();
			}
			return _Instance;
		}
	}

	private void Awake()
	{
		effectInstList = new List<EffectInst>();
		effectList = new List<Transform>();
	}

	private IEnumerator InstantiateEffect(EffectInst inst, Object obj, Vector3 position, Quaternion rotation, EffCastData data, Transform parent = null, EffectInstantiateCompleted OnInstantiated = null)
	{
		effectInstList.Add(inst);
		if (data.m_delaytime > float.Epsilon)
		{
			yield return new WaitForSeconds(data.m_delaytime);
		}
		GameObject effect = Object.Instantiate(obj, position + rotation * data.mOffsetPos, rotation) as GameObject;
		if (effect != null)
		{
			OnInstantiated?.Invoke(data, effect);
		}
		if (effect != null)
		{
			if (parent != null)
			{
				effect.transform.parent = parent;
			}
			else
			{
				effect.transform.parent = base.transform;
			}
		}
		if (effect != null && data.m_liveTime > float.Epsilon)
		{
			yield return new WaitForSeconds(data.m_liveTime);
			Object.Destroy(effect);
		}
		effectInstList.Remove(inst);
	}

	private IEnumerator InstantiateEffect(EffectInst inst, Object obj, Transform tr, EffCastData data, Transform parent = null, Transform target = null, EffectInstantiateCompleted OnInstantiated = null)
	{
		effectInstList.Add(inst);
		if (data.m_delaytime > float.Epsilon)
		{
			yield return new WaitForSeconds(data.m_delaytime);
		}
		if (tr == null)
		{
			yield break;
		}
		GameObject effect = Object.Instantiate(rotation: (data.m_direction == 0) ? Quaternion.identity : ((data.m_direction == 1) ? tr.rotation : ((!(target != null)) ? tr.rotation : Quaternion.LookRotation(target.position - tr.position))), original: obj, position: tr.position + tr.rotation * data.mOffsetPos) as GameObject;
		if (effect != null)
		{
			OnInstantiated?.Invoke(data, effect);
		}
		if (effect != null)
		{
			if (parent != null)
			{
				effect.transform.parent = parent;
			}
			else
			{
				effect.transform.parent = base.transform;
			}
		}
		if (effect != null && data.m_liveTime > float.Epsilon)
		{
			yield return new WaitForSeconds(data.m_liveTime);
			Object.Destroy(effect);
		}
		effectInstList.Remove(inst);
	}

	public void Instantiate(int id, Transform tr, Transform parent = null, Transform target = null, EffectInstantiateCompleted OnInstantiated = null)
	{
		EffCastData effCastData = EffCastData.GetEffCastData(id);
		if (effCastData != null)
		{
			EffectInst effectInst = new EffectInst();
			effectInst.coroutine = new CoroutineEffect(this, InstantiateEffect(effectInst, Resources.Load(effCastData.m_path), tr, effCastData, parent, target, OnInstantiated));
		}
	}

	public void Instantiate(int id, Vector3 position, Quaternion rotation, Transform parent = null, EffectInstantiateCompleted OnInstantiated = null)
	{
		EffCastData effCastData = EffCastData.GetEffCastData(id);
		if (effCastData != null)
		{
			EffectInst effectInst = new EffectInst();
			effectInst.coroutine = new CoroutineEffect(this, InstantiateEffect(effectInst, Resources.Load(effCastData.m_path), position, rotation, effCastData, parent, OnInstantiated));
		}
	}

	public void InstantiateEffect(int effId, Transform caster, Transform target = null, EffectInstantiateCompleted OnInstantiated = null)
	{
		EffCastData effCastData = EffCastData.GetEffCastData(effId);
		if (effCastData != null)
		{
			Transform transform = null;
			transform = ((!string.IsNullOrEmpty(effCastData.m_posStr) && !effCastData.m_posStr.Equals("0")) ? AiUtil.GetChild(caster, effCastData.m_posStr) : caster);
			if (!(transform == null))
			{
				Transform parent = ((!effCastData.m_bind) ? null : transform);
				Instantiate(effId, transform, parent, target, OnInstantiated);
			}
		}
	}
}
