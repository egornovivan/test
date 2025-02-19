using System.Collections.Generic;
using UnityEngine;

public class CreationDrivingTips : MonoBehaviour
{
	private static CreationDrivingTips s_Instance;

	public UILabel m_TipLabel;

	public UILabel m_WarningLabel;

	private List<string> m_Warnings;

	public string m_TestTip;

	public string m_TestWarning;

	public bool m_TestShowTip;

	public bool m_TestShowWarning;

	public bool m_TestHideWarning;

	private float m_TipAliveTime;

	public static CreationDrivingTips Instance => s_Instance;

	private void Awake()
	{
		s_Instance = this;
		m_TipLabel.text = string.Empty;
		m_TipLabel.color = new Color(1f, 1f, 1f, 0f);
		m_WarningLabel.text = string.Empty;
		m_WarningLabel.color = new Color(1f, 0f, 0f, 0f);
		m_TipAliveTime = 0f;
		m_Warnings = new List<string>();
	}

	private void OnEnable()
	{
		s_Instance = this;
		m_TipLabel.text = string.Empty;
		m_TipLabel.color = new Color(1f, 1f, 1f, 0f);
		m_WarningLabel.text = string.Empty;
		m_WarningLabel.color = new Color(1f, 0f, 0f, 0f);
		m_TipAliveTime = 0f;
		m_Warnings = new List<string>();
	}

	private void OnDestroy()
	{
		s_Instance = null;
	}

	private void Update()
	{
		if (m_TestShowTip)
		{
			m_TestShowTip = false;
			ShowTip(m_TestTip, 3f);
		}
		if (m_TestShowWarning)
		{
			m_TestShowWarning = false;
			ShowWarning(m_TestWarning);
		}
		if (m_TestHideWarning)
		{
			m_TestHideWarning = false;
			HideWarning();
		}
		m_TipAliveTime -= Time.deltaTime;
		if (m_TipAliveTime <= 0f)
		{
			HideTip();
		}
		if (m_Warnings.Count > 0)
		{
			ShowWarning(string.Empty);
			int num = Mathf.FloorToInt(Time.time / 1.6f);
			num %= m_Warnings.Count;
			m_WarningLabel.text = m_Warnings[num];
		}
		else
		{
			HideWarning();
		}
	}

	public void ShowTip(string text, float duration)
	{
		m_TipLabel.transform.localPosition = new Vector3(0f, 0f, 0f);
		m_TipLabel.pivot = UIWidget.Pivot.Bottom;
		m_TipLabel.text = text;
		m_TipLabel.GetComponent<TweenColor>().Play(forward: true);
		m_TipAliveTime = duration;
	}

	public void HideTip()
	{
		m_TipLabel.GetComponent<TweenColor>().Play(forward: false);
	}

	public void ClearWarning()
	{
		m_Warnings.Clear();
	}

	public void AddWarning(string text)
	{
		m_Warnings.Add(text);
	}

	public void ShowWarning(string text)
	{
		m_WarningLabel.text = text;
		m_WarningLabel.GetComponent<TweenColor>().Play(forward: true);
	}

	public void HideWarning()
	{
		m_WarningLabel.GetComponent<TweenColor>().Play(forward: false);
	}
}
