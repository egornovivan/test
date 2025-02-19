using UnityEngine;

public class UIFadeInOut : MonoBehaviour
{
	public float During = 0.5f;

	public AnimationCurve FadeInX;

	public AnimationCurve FadeInY;

	public AnimationCurve FadeOutX;

	public AnimationCurve FadeOutY;

	private float t;

	public float direction;

	public void FadeIn()
	{
		direction = 1f;
	}

	public void FadeOut()
	{
		direction = -1f;
	}

	private void Update()
	{
		t += Time.deltaTime / During * direction;
		t = Mathf.Clamp01(t);
		if (t <= 0f)
		{
			base.transform.localScale = new Vector3(0f, 0f, 1f);
		}
		else if (t >= 1f)
		{
			base.transform.localScale = new Vector3(1f, 1f, 1f);
		}
		else if (direction > 0f)
		{
			base.transform.localScale = new Vector3(FadeInX.Evaluate(t), FadeInY.Evaluate(t), 1f);
		}
		else
		{
			base.transform.localScale = new Vector3(FadeOutX.Evaluate(t), FadeOutY.Evaluate(t), 1f);
		}
	}
}
