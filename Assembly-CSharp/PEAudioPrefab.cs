using UnityEngine;

public class PEAudioPrefab : MonoBehaviour
{
	public int audioID;

	private AudioController m_Audio;

	private void Start()
	{
		if (audioID > 0)
		{
			m_Audio = AudioManager.instance.Create(base.transform.position, audioID, base.transform, isPlay: true, isDelete: false);
		}
	}

	private void OnDestroy()
	{
		if (m_Audio != null)
		{
			m_Audio.Delete();
		}
	}
}
