using System.Collections;
using UnityEngine;

public class MoveTaskM1Point : MonoBehaviour
{
	private const float angleMax = 180f;

	[SerializeField]
	private float minAngle;

	[SerializeField]
	private float maxAngle;

	[SerializeField]
	private float cd;

	[SerializeField]
	private float fadeSpeed;

	[SerializeField]
	private float maxChangePct = 0.1f;

	private float maxChange;

	private float target;

	private float current;

	private float temp;

	private RectTransform rt;

	private void Start()
	{
		maxChange = 180f * maxChangePct;
		current = 0.5f * (minAngle + maxAngle);
		rt = GetComponent<RectTransform>();
		rt.eulerAngles = new Vector3(180f, 0f, current);
		StartCoroutine(UpdateTarget());
	}

	private void FixedUpdate()
	{
		maxChange = 180f * maxChangePct;
		temp = Mathf.Lerp(current, target, fadeSpeed);
		if (Mathf.Abs(temp - current) / 180f < maxChangePct)
		{
			current = temp;
		}
		else
		{
			current += maxChange * Mathf.Sign(target - current);
		}
		rt.eulerAngles = new Vector3(180f, 0f, current);
	}

	private IEnumerator UpdateTarget()
	{
		while (true)
		{
			target = Random.Range(minAngle, maxAngle);
			yield return new WaitForSeconds(cd);
		}
	}
}
