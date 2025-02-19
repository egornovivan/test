using UnityEngine;

public class CSUI_StatusBar : MonoBehaviour
{
	private static CSUI_StatusBar s_Instance;

	private string m_text = string.Empty;

	private float m_textRemainTime;

	private float m_typeEffectTime;

	private bool m_showTypeEffect;

	private Color m_typeEffectColor = new Color(0f, 0.2f, 1f, 0f);

	public static CSUI_StatusBar Instance => s_Instance;

	private void Awake()
	{
		s_Instance = this;
	}

	private void OnDestroy()
	{
		s_Instance = null;
	}

	private void Start()
	{
	}

	private void Update()
	{
		m_textRemainTime -= Time.deltaTime;
		m_typeEffectTime += Time.deltaTime;
		if (m_textRemainTime < 0f)
		{
			m_textRemainTime = 0f;
			m_text = string.Empty;
		}
		if (m_showTypeEffect)
		{
			float num = 0.05f;
			int num2 = (int)(m_typeEffectTime / num);
			string text = string.Empty;
			for (int i = 0; i < m_text.Length && i <= num2; i++)
			{
				if (i < num2 - 4)
				{
					text += m_text[i];
				}
				else if (i < num2)
				{
					float t = (float)(num2 - i) * 0.2f;
					string empty = string.Empty;
					Color32 color = Color.Lerp(m_typeEffectColor, Color.white, t);
					empty += color.r.ToString("X").PadLeft(2, '0');
					empty += color.g.ToString("X").PadLeft(2, '0');
					empty += color.b.ToString("X").PadLeft(2, '0');
					string text2 = text;
					text = text2 + "[" + empty + "]" + m_text[i] + "[-]";
				}
				else
				{
					string empty2 = string.Empty;
					Color32 color2 = m_typeEffectColor;
					empty2 += color2.r.ToString("X").PadLeft(2, '0');
					empty2 += color2.g.ToString("X").PadLeft(2, '0');
					empty2 += color2.b.ToString("X").PadLeft(2, '0');
					string text2 = text;
					text = text2 + "[" + empty2 + "]" + m_text[i] + "[-]";
				}
			}
			GetComponent<UILabel>().text = text;
		}
		else
		{
			GetComponent<UILabel>().text = m_text;
		}
	}

	public static void ShowText(string text, Color effectColor, float remain = 0f)
	{
		if (!(s_Instance == null))
		{
			ShowText(text, remain, typeeffect: true);
			s_Instance.m_typeEffectColor = effectColor;
		}
	}

	public static void ShowText(string text, float remain = 0f, bool typeeffect = false)
	{
		if (!(s_Instance == null))
		{
			s_Instance.m_text = text;
			if (remain == 0f)
			{
				s_Instance.m_textRemainTime = 100000000f;
			}
			else
			{
				s_Instance.m_textRemainTime = remain;
			}
			s_Instance.m_typeEffectTime = 0f;
			s_Instance.m_showTypeEffect = typeeffect;
		}
	}

	public static void ShowTextLowPriority(string text, Color effectColor, float remain = 0f)
	{
		if (!(s_Instance == null) && s_Instance.m_text.Length == 0)
		{
			ShowText(text, effectColor, remain);
		}
	}

	public static void ShowTextLowPriority(string text, float remain = 0f, bool typeeffect = false)
	{
		if (!(s_Instance == null) && s_Instance.m_text.Length == 0)
		{
			ShowText(text, remain, typeeffect);
		}
	}

	public static void ClearText()
	{
		if (!(s_Instance == null))
		{
			s_Instance.m_text = string.Empty;
			s_Instance.m_textRemainTime = 0f;
			s_Instance.m_typeEffectTime = 0f;
			s_Instance.m_showTypeEffect = false;
		}
	}
}
