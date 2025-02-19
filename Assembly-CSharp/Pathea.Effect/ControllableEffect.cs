using UnityEngine;

namespace Pathea.Effect;

public class ControllableEffect
{
	private GameObject m_EffectObj;

	public bool active
	{
		get
		{
			return null != m_EffectObj && m_EffectObj.activeInHierarchy;
		}
		set
		{
			if (null != m_EffectObj)
			{
				m_EffectObj.SetActive(value);
			}
		}
	}

	public ControllableEffect(int effectID, Transform trnas)
	{
		EffectBuilder.EffectRequest effectRequest = Singleton<EffectBuilder>.Instance.Register(effectID, null, trnas);
		effectRequest.SpawnEvent += OnEffectSpawn;
	}

	public void Destory()
	{
		if (null != m_EffectObj)
		{
			Object.Destroy(m_EffectObj);
		}
	}

	private void OnEffectSpawn(GameObject gameObj)
	{
		m_EffectObj = gameObj;
	}
}
