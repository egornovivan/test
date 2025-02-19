using System;
using Pathea;

public struct AttributeInfo
{
	private const string BuffGreenColFormat = "[B7EF54]+{0}[-]";

	public const string RedColFomat = "[FF1D05]{0}[-]";

	public const string GreenColFormat = "[B7EF54]{0}[-]";

	public const string YellowColFomat = "[FFE000]{0}[-]";

	private AttribType m_CurType;

	private AttribType m_MaxType;

	private PeEntity m_Player;

	public int CurValue => (!(m_Player == null)) ? Convert.ToInt32(m_Player.GetAttribute(m_CurType)) : 0;

	public int MaxValue => (!(m_Player == null)) ? Convert.ToInt32(m_Player.GetAttribute(m_MaxType)) : 0;

	public int BuffValue => (!(m_Player == null)) ? Convert.ToInt32(MaxValue - Convert.ToInt32(m_Player.GetAttribute(m_MaxType, bSum: false))) : 0;

	public AttributeInfo(AttribType curType, AttribType maxType)
	{
		m_CurType = curType;
		m_MaxType = maxType;
		m_Player = null;
	}

	public void SetEntity(PeEntity player)
	{
		m_Player = player;
	}

	public string GetCur_MaxStr()
	{
		string empty = string.Empty;
		int buffValue = BuffValue;
		if (buffValue > 0)
		{
			return string.Format("[FFE000]{0}[-]", CurValue + "/") + $"[B7EF54]{MaxValue}[-]";
		}
		if (buffValue < 0)
		{
			return string.Format("[FFE000]{0}[-]", CurValue + "/") + $"[FF1D05]{MaxValue}[-]";
		}
		return string.Format("[FFE000]{0}[-]", CurValue + "/" + MaxValue);
	}

	public string GetBuffStr()
	{
		string result = string.Empty;
		int buffValue = BuffValue;
		if (buffValue > 0)
		{
			result = $"[B7EF54]+{buffValue}[-]";
		}
		else if (buffValue < 0)
		{
			result = $"[FF1D05]{buffValue}[-]";
		}
		return result;
	}
}
