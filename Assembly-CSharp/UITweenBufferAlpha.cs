using UnityEngine;

public class UITweenBufferAlpha : UITweener
{
	public float from = 1f;

	public float to = 1f;

	public bool refreshWidget = true;

	private UIWidget[] mWidgets;

	private UIEffectAlpha[] mEffectHandlers;

	private GameObject mGo;

	private void Awake()
	{
	}

	protected override void OnUpdate(float factor, bool isFinished)
	{
		if (refreshWidget)
		{
			mWidgets = base.gameObject.GetComponentsInChildren<UIWidget>(includeInactive: true);
			mEffectHandlers = base.gameObject.GetComponentsInChildren<UIEffectAlpha>(includeInactive: true);
			refreshWidget = false;
		}
		float num = Mathf.Lerp(from, to, factor);
		if (mWidgets != null)
		{
			UIWidget[] array = mWidgets;
			foreach (UIWidget uIWidget in array)
			{
				uIWidget.bufferAlpha = num;
				uIWidget.MarkAsChanged();
			}
		}
		if (mEffectHandlers != null)
		{
			UIEffectAlpha[] array2 = mEffectHandlers;
			foreach (UIEffectAlpha uIEffectAlpha in array2)
			{
				uIEffectAlpha.alpha = num;
			}
		}
	}

	public static TweenAlpha Begin(GameObject go, float duration, float alpha)
	{
		TweenAlpha tweenAlpha = UITweener.Begin<TweenAlpha>(go, duration);
		tweenAlpha.from = tweenAlpha.alpha;
		tweenAlpha.to = alpha;
		if (duration <= 0f)
		{
			tweenAlpha.Sample(1f, isFinished: true);
			tweenAlpha.enabled = false;
		}
		return tweenAlpha;
	}
}
