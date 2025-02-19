using System.Collections;
using UnityEngine;

public class MoveTaskM1Slider : MonoBehaviour
{
	private const float sliderMax = 230f;

	[SerializeField]
	private float minPct;

	[SerializeField]
	private float maxPct;

	[SerializeField]
	private float cd;

	[SerializeField]
	private float fadeSpeed;

	[SerializeField]
	private float maxChangePct = 0.1f;

	private float min;

	private float max;

	private float maxChange;

	private float target;

	private float current;

	private float temp;

	private RectTransform rt;

	private void Start()
	{
		min = 230f * minPct;
		max = 230f * maxPct;
		maxChange = 230f * maxChangePct;
		current = 0.5f * (min + max);
		rt = GetComponent<RectTransform>();
		rt.offsetMax = new Vector2(current - 230f, 0f);
		StartCoroutine(UpdateTarget());
	}

	private void FixedUpdate()
	{
		temp = Mathf.Lerp(current, target, fadeSpeed);
		if (Mathf.Abs(temp - current) / 230f < maxChangePct)
		{
			current = temp;
		}
		else
		{
			current += maxChange * Mathf.Sign(target - current);
		}
		rt.offsetMax = new Vector2(current - 230f, 0f);
	}

	private IEnumerator UpdateTarget()
	{
		while (true)
		{
			target = Random.Range(min, max);
			yield return new WaitForSeconds(cd);
		}
	}
}
