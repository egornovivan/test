using UnityEngine;

public class UIFlareFlash : MonoBehaviour
{
	private UITexture uitex;

	public float During = 0.5f;

	public Vector3 BeginScale;

	public Vector3 EndScale;

	public Gradient ColorDuring;

	public AnimationCurve Curve;

	public float Randomness;

	public float RandomT = 20f;

	private float t;

	private void OnEnable()
	{
		uitex = GetComponent<UITexture>();
	}

	private void OnDisable()
	{
		t = 0f;
	}

	private void Update()
	{
		if (uitex != null)
		{
			float num = Curve.Evaluate(t / During);
			num *= (Mathf.PerlinNoise(Time.time * RandomT, Time.time * RandomT) * 2f - 1f) * Randomness + 1f;
			base.transform.localScale = Vector3.Lerp(BeginScale, EndScale, num);
			uitex.color = ColorDuring.Evaluate(num);
		}
		t += Time.deltaTime;
	}
}
