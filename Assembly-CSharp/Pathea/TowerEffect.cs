using Pathea.Effect;
using UnityEngine;

namespace Pathea;

public class TowerEffect
{
	private float m_hpPercent;

	private int m_effectID;

	private int m_AudioID;

	private GameObject m_effect;

	private AudioController m_auCtrl;

	private EffectBuilder.EffectRequest m_Request;

	public float hpPercent => m_hpPercent;

	public int effectID => m_effectID;

	public GameObject effect => m_effect;

	public TowerEffect(float argHpPercent, int argEffectID, int argAudioID)
	{
		m_hpPercent = argHpPercent;
		m_effectID = argEffectID;
		m_AudioID = argAudioID;
		m_effect = null;
		m_Request = null;
		m_auCtrl = null;
	}

	public void ActivateEffect(PeEntity peEntity)
	{
		if (peEntity.HPPercent <= m_hpPercent && !peEntity.IsDeath())
		{
			if (m_effect == null && m_Request == null)
			{
				m_Request = Singleton<EffectBuilder>.Instance.Register(m_effectID, null, peEntity.tr);
				m_Request.SpawnEvent += OnSpawned;
				m_auCtrl = AudioManager.instance.Create(peEntity.position, m_AudioID, null, isPlay: true, isDelete: false);
			}
			if (m_effect != null && !m_effect.activeSelf)
			{
				m_effect.SetActive(value: true);
				if (m_auCtrl != null)
				{
					m_auCtrl.PlayAudio(2f);
				}
			}
		}
		else if (m_effect != null && m_effect.activeSelf)
		{
			m_effect.SetActive(value: false);
			if (m_auCtrl != null)
			{
				m_auCtrl.StopAudio(2f);
			}
		}
	}

	public void Destroy()
	{
		if (m_effect != null)
		{
			Object.Destroy(m_effect);
		}
		if (m_auCtrl != null)
		{
			m_auCtrl.Delete();
		}
		m_auCtrl = null;
		m_Request = null;
	}

	public void OnSpawned(GameObject obj)
	{
		m_effect = obj;
	}
}
