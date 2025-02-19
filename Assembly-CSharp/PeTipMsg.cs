using Pathea;
using UnityEngine;

public class PeTipMsg
{
	public enum EMsgType
	{
		Misc,
		Stroy,
		Colony
	}

	public enum EMsgLevel
	{
		Norm,
		Warning,
		Error,
		HighLightRed
	}

	public enum EStyle
	{
		Text,
		Icon,
		Texture
	}

	private static Color s_NormalColor = Color.white;

	private static Color s_WarningColor = Color.yellow;

	private static Color s_ErrorColor = new Color(0.99f, 0.3f, 0.3f, 1f);

	private static Color s_HighLightRed = new Color(1f, 0.35f, 0.35f, 1f);

	private string m_IconName;

	private string m_Content;

	private Texture m_IconTex;

	private EMsgType m_MsgType;

	private Color m_Color;

	private int m_MusicID;

	private EStyle m_Style;

	public EMsgType MsgType => m_MsgType;

	public PeTipMsg(string content, string iconName, EMsgLevel level, EMsgType msgType = EMsgType.Misc, int musicID = -1)
	{
		m_IconName = iconName;
		m_Content = content;
		m_MsgType = msgType;
		m_MusicID = musicID;
		m_Style = EStyle.Icon;
		switch (level)
		{
		case EMsgLevel.Norm:
			m_Color = s_NormalColor;
			break;
		case EMsgLevel.Warning:
			m_Color = s_WarningColor;
			break;
		case EMsgLevel.Error:
			m_Color = s_ErrorColor;
			break;
		case EMsgLevel.HighLightRed:
			m_Color = s_HighLightRed;
			break;
		}
		PeSingleton<PeTipsMsgMan>.Instance.AddTipMsg(this);
	}

	public PeTipMsg(string content, EMsgLevel level, EMsgType msgType = EMsgType.Misc, int musicID = -1)
	{
		m_IconName = string.Empty;
		m_Content = content;
		m_MsgType = msgType;
		m_MusicID = musicID;
		m_Style = EStyle.Text;
		switch (level)
		{
		case EMsgLevel.Norm:
			m_Color = s_NormalColor;
			break;
		case EMsgLevel.Warning:
			m_Color = s_WarningColor;
			break;
		case EMsgLevel.Error:
			m_Color = s_ErrorColor;
			break;
		case EMsgLevel.HighLightRed:
			m_Color = s_HighLightRed;
			break;
		}
		PeSingleton<PeTipsMsgMan>.Instance.AddTipMsg(this);
	}

	public PeTipMsg(string content, Texture iconTex, EMsgLevel level, EMsgType msgType = EMsgType.Misc, int musicID = -1)
	{
		m_IconTex = iconTex;
		m_Content = content;
		m_MsgType = msgType;
		m_MusicID = musicID;
		m_Style = EStyle.Texture;
		switch (level)
		{
		case EMsgLevel.Norm:
			m_Color = s_NormalColor;
			break;
		case EMsgLevel.Warning:
			m_Color = s_WarningColor;
			break;
		case EMsgLevel.Error:
			m_Color = s_ErrorColor;
			break;
		case EMsgLevel.HighLightRed:
			m_Color = s_HighLightRed;
			break;
		}
		PeSingleton<PeTipsMsgMan>.Instance.AddTipMsg(this);
	}

	public string GetIconName()
	{
		return m_IconName;
	}

	public Texture GetIconTex()
	{
		return m_IconTex;
	}

	public string GetContent()
	{
		return m_Content;
	}

	public Color GetColor()
	{
		return m_Color;
	}

	public int GetMusicID()
	{
		return m_MusicID;
	}

	public EStyle GetEStyle()
	{
		return m_Style;
	}

	public static void Register(string content, string iconName, EMsgLevel level, EMsgType msgType = EMsgType.Misc, int musicID = -1)
	{
		new PeTipMsg(content, iconName, level, msgType, musicID);
	}

	public static void Register(string content, EMsgLevel level, EMsgType msgType = EMsgType.Misc, int musicID = -1)
	{
		new PeTipMsg(content, level, msgType, musicID);
	}

	public static void Register(string content, Texture iconTex, EMsgLevel level, EMsgType msgType = EMsgType.Misc, int musicID = -1)
	{
		new PeTipMsg(content, iconTex, level, msgType, musicID);
	}
}
