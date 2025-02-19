using System.Collections.Generic;
using UnityEngine;

public class ScanningEffectHandler : MonoBehaviour
{
	public float m_EmitRate = 10f;

	public GameObject m_Resource;

	public Vector3 m_From;

	public Vector3 m_To;

	private List<ScanningCircleHandler> m_Circles;

	public float m_LifeTime = 2f;

	private void Start()
	{
		m_Circles = new List<ScanningCircleHandler>();
	}

	private void Update()
	{
		EmitLogic();
		MotionLogic();
	}

	private void EmitLogic()
	{
		m_LifeTime -= Time.deltaTime;
		if (m_LifeTime > 0f)
		{
			float num = m_EmitRate * Time.deltaTime;
			int num2 = Mathf.FloorToInt(num);
			for (int i = 0; i < num2; i++)
			{
				Emit();
			}
			if (Random.value < num - (float)num2)
			{
				Emit();
			}
		}
		if (!(m_LifeTime < -10f) || m_Circles.Count <= 0)
		{
			return;
		}
		foreach (ScanningCircleHandler circle in m_Circles)
		{
			Object.Destroy(circle.gameObject);
		}
		m_Circles.Clear();
	}

	private void MotionLogic()
	{
		float num = Vector3.Distance(m_From, m_To);
		foreach (ScanningCircleHandler circle in m_Circles)
		{
			circle.transform.localPosition = Vector3.Lerp(circle.transform.localPosition, m_To, 0.02f);
			float a = Vector3.Distance(circle.transform.localPosition, m_From);
			float b = Vector3.Distance(circle.transform.localPosition, m_To);
			circle.m_CircleBrightness = Mathf.Pow(Mathf.Min(a, b) / num * 2f, 1.5f) * 0.2f;
		}
	}

	private void Emit()
	{
		if (m_Resource != null)
		{
			GameObject gameObject = Object.Instantiate(m_Resource);
			gameObject.transform.parent = base.transform;
			gameObject.transform.localPosition = m_From;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;
			ScanningCircleHandler component = gameObject.GetComponent<ScanningCircleHandler>();
			component.m_Brightness = Random.value * 0.8f + 0.2f;
			component.m_CircleBrightness = 0f;
			component.TakeEffect();
			m_Circles.Add(component);
		}
	}
}
