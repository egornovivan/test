using UnityEngine;

public class UIAlphaGroup : MonoBehaviour
{
	[SerializeField]
	private float mAlpha = 1f;

	[SerializeField]
	private float[] mAlphaGroup;

	[Range(1f, 30f)]
	public int m_Speed = 1;

	private float newAlpha = 1f;

	public float Alpha => mAlpha;

	public int State
	{
		set
		{
			newAlpha = ((value >= mAlphaGroup.Length) ? 1f : mAlphaGroup[value]);
		}
	}

	private void Update()
	{
		if (newAlpha != mAlpha)
		{
			mAlpha = Mathf.Lerp(mAlpha, newAlpha, Time.deltaTime * 2f * (float)m_Speed);
			if (Mathf.Abs(mAlpha - newAlpha) < 0.02f)
			{
				mAlpha = newAlpha;
			}
		}
	}
}
