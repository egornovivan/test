using UnityEngine;

namespace PeUIEffect;

public class UISightingOnShootEffect : UIEffect
{
	[SerializeField]
	private AcEffect effect;

	[SerializeField]
	private float mValue;

	private float time;

	public float Value => mValue;

	public override void Play()
	{
		base.Play();
		time = 0f;
	}

	public override void End()
	{
		base.End();
		mValue = 0f;
	}

	private void Update()
	{
		if (m_Runing)
		{
			time += Time.deltaTime;
			mValue = effect.GetAcValue(time);
			if (time > effect.EndTime)
			{
				End();
			}
		}
		else
		{
			time = 0f;
		}
	}
}
