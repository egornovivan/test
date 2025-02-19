using UnityEngine;

public class UIHoloActive : MonoBehaviour
{
	private UIHolographicHandler mHandler;

	private float Intensity;

	private float Speed;

	private float Twinkle;

	[SerializeField]
	private float deIntensity = 0.1f;

	[SerializeField]
	private float deSpeed = 0.05f;

	[SerializeField]
	private float deTwinkle = 0.1f;

	public bool mActive
	{
		set
		{
			if (value)
			{
				if (mHandler != null)
				{
					mHandler.Intensity = Intensity;
					mHandler.Speed = Speed;
					mHandler.Twinkle = Twinkle;
				}
			}
			else if (mHandler != null)
			{
				mHandler.Intensity = deIntensity;
				mHandler.Speed = deSpeed;
				mHandler.Twinkle = deTwinkle;
			}
		}
	}

	private void Start()
	{
		mHandler = GetComponent<UIHolographicHandler>();
		if (mHandler != null)
		{
			Intensity = mHandler.Intensity;
			Speed = mHandler.Speed;
			Twinkle = mHandler.Twinkle;
		}
	}
}
