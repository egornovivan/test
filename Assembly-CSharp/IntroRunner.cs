using UnityEngine;
using UnityEngine.UI;

public class IntroRunner : MonoBehaviour
{
	public delegate void MovieEnd();

	private MovieTexture mMovie;

	public static MovieEnd movieEnd;

	private void Start()
	{
		mMovie = GetComponent<RawImage>().texture as MovieTexture;
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
		if (mMovie != null && !mMovie.isPlaying)
		{
			EndMovie();
		}
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
		{
			EndMovie();
		}
	}

	private void EndMovie()
	{
		if (movieEnd != null)
		{
			movieEnd();
		}
		base.enabled = false;
	}
}
