using UnityEngine;

public class RandTarget : MonoBehaviour
{
	public GameObject m_CoppyTarget;

	public int m_Num = 20;

	public float m_Radius = 100f;

	public float m_Hight = 2f;

	private void Start()
	{
		if (null != m_CoppyTarget)
		{
			for (int i = 0; i < m_Num; i++)
			{
				GameObject gameObject = Object.Instantiate(m_CoppyTarget);
				Vector2 vector = Random.insideUnitCircle * m_Radius;
				gameObject.transform.parent = base.transform;
				gameObject.transform.position = new Vector3(vector.x, base.transform.position.y + Random.Range(-1f, 1f) * m_Hight, vector.y);
			}
		}
	}
}
