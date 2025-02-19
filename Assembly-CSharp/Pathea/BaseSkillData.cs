using System.IO;
using PETools;

namespace Pathea;

public class BaseSkillData
{
	private const int VERSION_0000 = 0;

	private const int VERSION_0001 = 1;

	private const int CURRENT_VERSION = 1;

	public int m_SkillL;

	public int m_SKillIDSpace;

	public int m_Skillpounce;

	public string m_PounceAnim;

	public BaseSkillData(int skill0 = 0, int space = 0, int prounce = 0)
	{
		m_SkillL = skill0;
		m_SKillIDSpace = space;
		m_Skillpounce = prounce;
		m_PounceAnim = MountsSkillDb.GetpounceAnim(prounce);
	}

	public bool canUse()
	{
		return canAttack() || canSpace() || canProunce();
	}

	public bool canAttack()
	{
		return m_SkillL != 0;
	}

	public bool canSpace()
	{
		return m_SKillIDSpace != 0;
	}

	public bool canProunce()
	{
		return m_Skillpounce != 0 && !string.IsNullOrEmpty(m_PounceAnim);
	}

	public BaseSkillData CopyTo()
	{
		BaseSkillData baseSkillData = new BaseSkillData();
		baseSkillData.m_SKillIDSpace = m_SKillIDSpace;
		baseSkillData.m_SkillL = m_SkillL;
		baseSkillData.m_Skillpounce = m_Skillpounce;
		baseSkillData.m_PounceAnim = m_PounceAnim;
		return baseSkillData;
	}

	public void Reset(int skill0 = 0, int space = 0, int prounce = 0)
	{
		m_SkillL = skill0;
		m_SKillIDSpace = space;
		m_Skillpounce = prounce;
		m_PounceAnim = MountsSkillDb.GetpounceAnim(prounce);
	}

	public void Export(BinaryWriter bw)
	{
		bw.Write(1);
		bw.Write(m_SkillL);
		bw.Write(m_SKillIDSpace);
		bw.Write(m_Skillpounce);
		Serialize.WriteNullableString(bw, m_PounceAnim);
	}

	public void Import(BinaryReader r)
	{
		int num = r.ReadInt32();
		m_SkillL = r.ReadInt32();
		m_SKillIDSpace = r.ReadInt32();
		if (num >= 1)
		{
			m_Skillpounce = r.ReadInt32();
			m_PounceAnim = Serialize.ReadNullableString(r);
		}
	}
}
