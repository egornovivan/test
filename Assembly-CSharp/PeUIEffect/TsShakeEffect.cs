using System;
using UnityEngine;

namespace PeUIEffect;

[Serializable]
public class TsShakeEffect : UIEffect
{
	public AcEffect effectPos_x;

	public AcEffect effectPos_y;

	public float Pitch = 1f;

	private Transform mTargetTs;

	private float time;

	private Vector3 pos = Vector3.zero;

	public Transform TargetTs
	{
		get
		{
			return mTargetTs;
		}
		set
		{
			mTargetTs = value;
		}
	}

	private float EndTime => (!(effectPos_x.EndTime > effectPos_y.EndTime)) ? effectPos_y.EndTime : effectPos_x.EndTime;

	public void Play(float _pitch)
	{
		Pitch *= _pitch;
		Play();
	}

	public override void Play()
	{
		if (!m_Runing)
		{
			if (mTargetTs == null)
			{
				mTargetTs = base.gameObject.transform;
			}
			time = 0f;
			pos = mTargetTs.localPosition;
			base.Play();
		}
	}

	public override void End()
	{
		mTargetTs.localPosition = pos;
		base.End();
	}

	private void Update()
	{
		if (m_Runing && !(mTargetTs == null))
		{
			time += Time.deltaTime;
			Vector3 zero = Vector3.zero;
			if (effectPos_x.bActive)
			{
				zero.x += (int)(effectPos_x.GetAcValue(time) * Pitch);
			}
			if (effectPos_y.bActive)
			{
				zero.y += (int)(effectPos_y.GetAcValue(time) * Pitch);
			}
			mTargetTs.localPosition = pos + zero;
			if (time > EndTime)
			{
				End();
			}
		}
	}
}
