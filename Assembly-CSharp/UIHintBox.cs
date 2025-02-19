using System;
using AnimationOrTween;
using UnityEngine;

public class UIHintBox : MonoBehaviour
{
	private const int MaxPointCount = 6;

	[SerializeField]
	private UILabel msgLb;

	[SerializeField]
	private TweenScale tweenScale;

	public string Msg = "Check";

	public float multiplicator = 0.02f;

	public float errorVal = 0.01f;

	public bool isProcessing = true;

	private float mCurTime;

	private int mPointCount;

	public event Action onOpen;

	public event Action onClose;

	public void Open()
	{
		mCurTime = 0f;
		mPointCount = 0;
		base.gameObject.SetActive(value: true);
		if (this.onOpen != null)
		{
			this.onOpen();
		}
		tweenScale.Play(forward: true);
	}

	public void Close()
	{
		tweenScale.Play(forward: false);
	}

	private void Update()
	{
		if (isProcessing)
		{
			mCurTime = Mathf.Lerp(mCurTime, 1f, multiplicator);
			mPointCount = (int)Mathf.Clamp(6f * (mCurTime / 1f), 0f, 6f);
			if (Mathf.Abs(mCurTime - 1f) < errorVal)
			{
				mCurTime = 0f;
				mPointCount = 0;
			}
			string text = string.Empty;
			for (int i = 0; i < mPointCount; i++)
			{
				text += ".";
			}
			msgLb.text = Msg + text;
		}
		else
		{
			mCurTime = 0f;
			msgLb.text = Msg;
		}
		CalcuMsgLabelPos();
	}

	private void OnTweenFinished(UITweener tween)
	{
		if (tween.direction == Direction.Reverse)
		{
			base.gameObject.SetActive(value: false);
			if (this.onClose != null)
			{
				this.onClose();
			}
		}
	}

	private void CalcuMsgLabelPos()
	{
		Vector3 vector = msgLb.font.CalculatePrintedSize(Msg, encoding: false, UIFont.SymbolStyle.None);
		vector.x *= msgLb.transform.localScale.x;
		Vector3 localPosition = msgLb.transform.localPosition;
		localPosition.x = -Mathf.FloorToInt(vector.x * 0.5f) - 5;
		msgLb.transform.localPosition = localPosition;
	}
}
