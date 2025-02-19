using System.Collections;
using UnityEngine;

public class CSUI_AlphaTween : MonoBehaviour
{
	private static CSUI_AlphaTween m_Self;

	[SerializeField]
	private UILabel m_TipLbUI;

	public bool Reverse;

	public float FadeSpeed = 0.5f;

	private UIWidget[] m_Wighets;

	private bool m_Play;

	private bool m_Forwad;

	public static CSUI_AlphaTween Self => m_Self;

	public string Text
	{
		get
		{
			return m_TipLbUI.text;
		}
		set
		{
			m_TipLbUI.text = value;
		}
	}

	public void Play(bool play)
	{
		if (play)
		{
			m_Forwad = true;
			m_Play = true;
			return;
		}
		for (int i = 0; i < m_Wighets.Length; i++)
		{
			m_Wighets[i].alpha = 0f;
		}
		m_Play = false;
	}

	public void Play(float duration)
	{
		StopAllCoroutines();
		StartCoroutine(_play(duration));
	}

	private IEnumerator _play(float duration)
	{
		Play(play: true);
		yield return new WaitForSeconds(duration);
		Play(play: false);
	}

	private void Awake()
	{
		m_Wighets = base.gameObject.GetComponentsInChildren<UIWidget>();
		m_Self = this;
	}

	private void Start()
	{
		for (int i = 0; i < m_Wighets.Length; i++)
		{
			m_Wighets[i].alpha = 0f;
		}
	}

	private void Update()
	{
		if (!m_Play)
		{
			return;
		}
		if (m_Forwad)
		{
			for (int i = 0; i < m_Wighets.Length; i++)
			{
				m_Wighets[i].alpha += FadeSpeed * Time.deltaTime;
				if (m_Wighets[i].alpha >= 1f)
				{
					m_Forwad = false;
				}
			}
		}
		else
		{
			if (!Reverse)
			{
				return;
			}
			for (int j = 0; j < m_Wighets.Length; j++)
			{
				m_Wighets[j].alpha -= FadeSpeed * Time.deltaTime;
				if (m_Wighets[j].alpha <= 0f)
				{
					m_Forwad = true;
				}
			}
		}
	}
}
