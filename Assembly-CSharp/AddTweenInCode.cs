using UnityEngine;
using WhiteCat;

public class AddTweenInCode : BaseBehaviour
{
	private TweenInterpolator interpolator;

	private Vector2 from;

	private Vector2 to;

	private void Awake()
	{
		interpolator = TweenInterpolator.Create(onUpdate: delegate(float factor)
		{
			base.rectTransform.anchoredPosition = from + (to - from) * factor;
		}, gameObject: base.gameObject, isPlaying: false, delay: 0f, duration: 0.8f, speed: 1f, method: TweenMethod.EaseInBackOut);
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			from = base.rectTransform.anchoredPosition;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(base.rectTransform.parent as RectTransform, Input.mousePosition, null, out to);
			interpolator.Replay();
		}
	}
}
