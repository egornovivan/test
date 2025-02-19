using UnityEngine;

public class EDRunner : MonoBehaviour
{
	private MovieTexture mMovie;

	private void Start()
	{
		mMovie = GetComponent<UITexture>().mainTexture as MovieTexture;
		if (mMovie != null)
		{
			mMovie.Stop();
			mMovie.Play();
			AudioSource audioSource = base.gameObject.AddComponent<AudioSource>();
			audioSource.clip = mMovie.audioClip;
			audioSource.Stop();
			audioSource.Play();
		}
	}

	private void Update()
	{
		if (PeInput.Get(PeInput.LogicFunction.OptionsUI) || !mMovie.isPlaying)
		{
			Application.LoadLevel("GameCredits");
		}
	}
}
