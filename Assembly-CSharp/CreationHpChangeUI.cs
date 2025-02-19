using UnityEngine;

public class CreationHpChangeUI : MonoBehaviour
{
	private static CreationHpChangeUI s_Instance;

	public GameObject m_Res;

	public int m_Queue;

	public int m_DebugValue;

	public bool m_DebugTest;

	public static CreationHpChangeUI Instance => s_Instance;

	private void Awake()
	{
		s_Instance = this;
	}

	private void OnDestroy()
	{
		s_Instance = null;
	}

	private void Update()
	{
		if (Time.frameCount % 10 == 0 && m_Queue > 0)
		{
			m_Queue--;
		}
		if (m_DebugTest)
		{
			m_DebugTest = false;
			Popup(m_DebugValue);
		}
	}

	public void Popup(int val)
	{
		if (base.gameObject.activeInHierarchy && base.enabled)
		{
			GameObject gameObject = Object.Instantiate(m_Res);
			gameObject.transform.parent = base.transform;
			gameObject.layer = base.gameObject.layer;
			Vector3 from = gameObject.GetComponent<TweenPosition>().from;
			from.y += 80 * m_Queue;
			gameObject.GetComponent<TweenPosition>().from = from;
			gameObject.transform.localPosition = from;
			gameObject.GetComponent<UILabel>().text = val.ToString();
			gameObject.GetComponent<UILabel>().depth = 100;
			Vector3 from2 = Mathf.Log10(Mathf.Abs(val) + 500) * 60f * Vector3.one;
			gameObject.GetComponent<TweenScale>().from = from2;
			m_Queue++;
			if (m_Queue == 3)
			{
				m_Queue = 0;
			}
		}
	}
}
