using System;
using UnityEngine;

namespace PeUIEffect;

[Serializable]
public class AcEffect
{
	public bool bActive;

	public AnimationCurve mAcShakeEffect;

	public float mScale = 1f;

	public float mPitch = 1f;

	public float EndTime => (mAcShakeEffect.keys.Length <= 0) ? 0f : (mAcShakeEffect.keys[mAcShakeEffect.length - 1].time * mScale);

	public float GetAcValue(float key)
	{
		return mAcShakeEffect.Evaluate(key / mScale) * mPitch;
	}
}
