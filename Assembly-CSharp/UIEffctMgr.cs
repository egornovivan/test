using System;
using System.Collections.Generic;
using PeUIEffect;
using UnityEngine;

public class UIEffctMgr : MonoBehaviour
{
	[Serializable]
	public class ShakeEffect
	{
		public float mPitch = 1f;

		private List<TsShakeEffect> mList = new List<TsShakeEffect>();

		public void Init(TsShakeEffect[] ts)
		{
			mList.AddRange(ts);
		}

		public void Play(float pitch)
		{
			pitch = Mathf.Clamp01(pitch);
			foreach (TsShakeEffect m in mList)
			{
				m.Play(mPitch * pitch);
			}
		}
	}

	private static UIEffctMgr mInstance;

	public ShakeEffect mShakeEffect;

	public static UIEffctMgr Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
		mShakeEffect.Init(GetComponentsInChildren<TsShakeEffect>(includeInactive: true));
	}

	public void TestPlay()
	{
		mShakeEffect.Play(1f);
	}
}
