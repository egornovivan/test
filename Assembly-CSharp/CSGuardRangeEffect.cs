using System.Collections;
using UnityEngine;

public class CSGuardRangeEffect : MonoBehaviour
{
	[SerializeField]
	private Projector m_Projector;

	[SerializeField]
	private LocateCubeEffectHanlder m_CubeEffect;

	public Transform CubeEffectFollower;

	public float Radius
	{
		get
		{
			return m_Projector.orthographicSize;
		}
		set
		{
			m_Projector.orthographicSize = value;
			m_Projector.farClipPlane = value * 2f;
			m_Projector.transform.localPosition = new Vector3(0f, value, 0f);
		}
	}

	public void DelayDestroy(float delay_time)
	{
		StartCoroutine(_delayDestroy(delay_time));
	}

	private IEnumerator _delayDestroy(float delay_time)
	{
		yield return new WaitForSeconds(delay_time);
		Object.Destroy(base.gameObject);
	}

	public void StopDestroy()
	{
		StopAllCoroutines();
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (CubeEffectFollower != null)
		{
			Vector3 position = CubeEffectFollower.position;
			m_CubeEffect.transform.position = new Vector3(position.x, position.y + m_CubeEffect.m_MaxHeightScale * 0.5f, position.z);
		}
	}
}
