using UnityEngine;

namespace PeAudio;

public class SPOTInst : MonoBehaviour
{
	public FMODAudioSource audioSrc;

	private void Awake()
	{
		audioSrc = base.gameObject.AddComponent<FMODAudioSource>();
	}

	private void OnDestroy()
	{
		Object.Destroy(audioSrc);
	}

	private void Update()
	{
	}
}
