using UnityEngine;

public class DestroyTimer : MonoBehaviour
{
	public float m_LifeTime;

	public bool m_DisableDestroy;

	private void Update()
	{
		m_LifeTime -= Time.deltaTime;
		if (m_LifeTime < 0f)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDisable()
	{
		if (m_DisableDestroy)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
