using System.Collections.Generic;
using UnityEngine;

public class LightCtrl : MonoBehaviour
{
	public Light mLight;

	public float mLifetime;

	public List<float> mIntensity;

	public List<float> mRange;

	public List<Color> mColor;

	private float mCurrentTime;

	private void Update()
	{
		mCurrentTime += Time.deltaTime;
		if (mCurrentTime < mLifetime)
		{
			if (mIntensity.Count == 0)
			{
				mLight.intensity = 1f;
			}
			else if (mIntensity.Count == 1)
			{
				mLight.intensity = mIntensity[0];
			}
			else
			{
				int num = mIntensity.Count - 1;
				float num2 = mLifetime / (float)num;
				float t = mCurrentTime % num2 / num2;
				int num3 = (int)(mCurrentTime / num2);
				mLight.intensity = Mathf.Lerp(mIntensity[num3], mIntensity[num3 + 1], t);
			}
			if (mRange.Count == 0)
			{
				mLight.range = 1f;
			}
			else if (mRange.Count == 1)
			{
				mLight.range = mRange[0];
			}
			else
			{
				int num4 = mRange.Count - 1;
				float num5 = mLifetime / (float)num4;
				float t2 = mCurrentTime % num5 / num5;
				int num6 = (int)(mCurrentTime / num5);
				mLight.range = Mathf.Lerp(mRange[num6], mRange[num6 + 1], t2);
			}
			if (mColor.Count != 0)
			{
				if (mColor.Count == 1)
				{
					mLight.color = mColor[0];
					return;
				}
				int num7 = mColor.Count - 1;
				float num8 = mLifetime / (float)num7;
				float t3 = mCurrentTime % num8 / num8;
				int num9 = (int)(mCurrentTime / num8);
				mLight.color = Color.Lerp(mColor[num9], mColor[num9 + 1], t3);
			}
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
