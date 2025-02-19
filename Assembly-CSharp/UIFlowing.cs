using UnityEngine;

public class UIFlowing : MonoBehaviour
{
	private UITexture uitex;

	public float During = 0.5f;

	public float Counter = -1f;

	public Vector3 BeginPosition;

	public Vector3 EndPosition;

	public Vector3 BeginScale = Vector3.one;

	public Vector3 EndScale = Vector3.one;

	public Gradient ColorDuring;

	public AnimationCurve PosCurve;

	public AnimationCurve ScaleCurve;

	public float Pre;

	public float Post = 0.5f;

	private float t;

	private float c;

	private void OnEnable()
	{
		uitex = GetComponent<UITexture>();
		c = Counter;
	}

	private void OnDisable()
	{
		t = 0f;
	}

	private void Update()
	{
		float time = PosCurve.Evaluate((t - Pre) / During);
		base.transform.localPosition = Vector3.Lerp(BeginPosition, EndPosition, time);
		ScaleCurve.Evaluate((t - Pre) / During);
		base.transform.localScale = Vector3.Lerp(BeginScale, EndScale, time);
		if (uitex != null)
		{
			uitex.color = ColorDuring.Evaluate(time);
		}
		if (c != 0f)
		{
			t += Time.deltaTime;
		}
		if (t > During + Pre + Post)
		{
			c -= 1f;
			t = 0f;
		}
	}
}
