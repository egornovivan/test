using UnityEngine;

public class CSUI_PopupHint : MonoBehaviour
{
	public Vector3 m_Velocity;

	public Vector3 m_Pos;

	public Vector3 m_LocalScale;

	public float m_DuringTime = 5f;

	public float m_MaxScale = 1.5f;

	public float m_ScaleRate = 0.1f;

	private float m_StartTime;

	private float m_Scale = 1f;

	private bool m_ScaleDir = true;

	private float m_ScaleStayDuring = 1f;

	private float m_CurStarTime;

	[SerializeField]
	private UILabel m_TextLabGreen;

	[SerializeField]
	private UILabel m_TextLabRed;

	public bool bGreen;

	public string Text
	{
		get
		{
			return m_TextLabGreen.text;
		}
		set
		{
			m_TextLabGreen.text = value;
			m_TextLabRed.text = value;
		}
	}

	public void Tween()
	{
		base.transform.position = m_Pos;
		base.transform.localScale = m_LocalScale;
		m_Scale = 1f;
		m_StartTime = 0f;
		m_ScaleDir = true;
		m_CurStarTime = 0f;
		base.gameObject.SetActive(value: true);
	}

	private void Awake()
	{
		base.gameObject.SetActive(value: false);
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (bGreen)
		{
			m_TextLabGreen.enabled = true;
			m_TextLabRed.enabled = false;
		}
		else
		{
			m_TextLabGreen.enabled = false;
			m_TextLabRed.enabled = true;
		}
		m_StartTime += Time.deltaTime;
		if (m_StartTime < m_DuringTime)
		{
			float num = Mathf.Pow(1f - m_StartTime / m_DuringTime, 8f);
			float y = 2.5f * num;
			m_Velocity = new Vector3(0f, y, 0f);
			base.transform.localPosition += m_Velocity;
		}
		if (m_ScaleDir)
		{
			m_Scale = Mathf.Lerp(m_Scale, m_MaxScale, m_ScaleRate);
			Vector3 vector = m_LocalScale * m_Scale;
			base.transform.localScale = new Vector3(vector.x, vector.y, 1f);
			if (Mathf.Abs(m_MaxScale - m_Scale) < 0.01f)
			{
				m_ScaleDir = false;
			}
			return;
		}
		m_CurStarTime += Time.deltaTime;
		if (m_CurStarTime > m_ScaleStayDuring)
		{
			m_Scale -= (m_Scale - 0.1f) * m_ScaleRate;
			Vector3 vector2 = m_LocalScale * m_Scale;
			base.transform.localScale = new Vector3(vector2.x, vector2.y, 1f);
			if (vector2.x <= 0.1f)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
