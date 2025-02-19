using UnityEngine;

namespace PeUIEffect;

public class UIGridEffect : UIEffect
{
	public float Speed = 1f;

	private float FadeTime;

	private void Start()
	{
		Play();
	}

	public override void Play()
	{
		FadeTime = 0f;
		base.Play();
	}

	public override void End()
	{
		FadeTime = 0f;
		base.End();
		Object.Destroy(base.gameObject.transform.parent.gameObject);
	}

	private void Update()
	{
		if (m_Runing)
		{
			FadeTime += Time.deltaTime * Speed;
			GetComponent<Renderer>().material.SetFloat("_FadeTime", FadeTime);
			if (FadeTime > 1f)
			{
				End();
			}
		}
	}
}
