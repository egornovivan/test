using System;
using UnityEngine;

public class NpcSpeakSentenseItem : MonoBehaviour
{
	[SerializeField]
	private UILabel mLabel;

	[SerializeField]
	private UISlicedSprite mSpr;

	private float mDuration;

	private bool ableCalculate;

	private bool mStartCountDown;

	public event Action<NpcSpeakSentenseItem> OnDestroySelfEvent;

	private float GetDuration(int _length, float _interval)
	{
		float num = 0f;
		if (_length <= 50)
		{
			num = 5f;
		}
		else if (_length > 50 && _length <= 100)
		{
			num = 7f;
		}
		else if (_length > 100)
		{
			num = 9f;
		}
		if (_interval <= num && _interval > 0f)
		{
			num = _interval;
		}
		return num;
	}

	public void SayOneWord(string _content, float _interval)
	{
		mLabel.text = _content;
		mDuration = GetDuration(_content.Length, _interval);
		ableCalculate = true;
		mStartCountDown = true;
	}

	private void CalculateSentenseLenth()
	{
		if (ableCalculate)
		{
			float x = mLabel.relativeSize.x * mLabel.transform.localScale.x + 30f;
			Vector3 localScale = new Vector3(x, mSpr.transform.localScale.y, mSpr.transform.localScale.z);
			mSpr.transform.localScale = localScale;
			ableCalculate = false;
		}
	}

	private void OnCountDown()
	{
		if (!mStartCountDown || mDuration <= 0f)
		{
			return;
		}
		mDuration -= Time.deltaTime;
		if (mDuration <= 1f)
		{
			OnDisappear(mDuration);
		}
		if (mDuration <= 0f)
		{
			if (this.OnDestroySelfEvent != null)
			{
				this.OnDestroySelfEvent(this);
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void AheadDisappear()
	{
		if (this.OnDestroySelfEvent != null)
		{
			this.OnDestroySelfEvent(this);
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnDisappear(float _alpha)
	{
		mLabel.color = new Color(1f, 1f, 1f, _alpha);
		mSpr.color = new Color(1f, 1f, 1f, _alpha);
	}

	private void Update()
	{
		CalculateSentenseLenth();
		OnCountDown();
	}
}
