using Pathea.Effect;
using UnityEngine;

public class EquipmentActiveEffect : MonoBehaviour
{
	[SerializeField]
	private int m_SoundID;

	[SerializeField]
	private int m_EffectID;

	private AudioController m_Audio;

	private ControllableEffect m_Effect;

	public void SetActiveState(bool active)
	{
		if (active)
		{
			if (null == m_Audio)
			{
				m_Audio = AudioManager.instance.Create(base.transform.position, m_SoundID, base.transform, isPlay: true, isDelete: false);
				if (null != m_Audio)
				{
					m_Audio.SetVolume(0f);
					m_Audio.SetVolume(1f, 0.5f);
				}
			}
			if (m_Effect == null)
			{
				m_Effect = new ControllableEffect(m_EffectID, base.transform);
			}
		}
		else
		{
			if (null != m_Audio)
			{
				m_Audio.StopAudio(1f);
				m_Audio.Delete(1.1f);
				m_Audio = null;
			}
			if (m_Effect != null)
			{
				m_Effect.Destory();
				m_Effect = null;
			}
		}
	}

	private void OnDestroy()
	{
		if (m_Effect != null)
		{
			m_Effect.Destory();
			m_Effect = null;
		}
	}
}
