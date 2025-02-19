namespace Pathea;

public class AttribInspectoscope : PESingleton<AttribInspectoscope>
{
	private AttribPair[] m_AttribPairs;

	public AttribInspectoscope()
	{
		InitPair();
	}

	public void CheckAttrib(PESkEntity entity, AttribType type, float value)
	{
		if (m_AttribPairs[(int)type] != null)
		{
			m_AttribPairs[(int)type].CheckAttrib(entity, type, value);
		}
	}

	private void InitPair()
	{
		m_AttribPairs = new AttribPair[97];
		m_AttribPairs[1] = (m_AttribPairs[0] = new AttribPair(AttribType.Hp, AttribType.HpMax));
		m_AttribPairs[3] = (m_AttribPairs[2] = new AttribPair(AttribType.Stamina, AttribType.StaminaMax));
		m_AttribPairs[8] = (m_AttribPairs[7] = new AttribPair(AttribType.Comfort, AttribType.ComfortMax));
		m_AttribPairs[12] = (m_AttribPairs[11] = new AttribPair(AttribType.Oxygen, AttribType.OxygenMax));
		m_AttribPairs[16] = (m_AttribPairs[15] = new AttribPair(AttribType.Hunger, AttribType.HungerMax));
		m_AttribPairs[19] = (m_AttribPairs[20] = new AttribPair(AttribType.Energy, AttribType.EnergyMax));
		m_AttribPairs[29] = (m_AttribPairs[28] = new AttribPair(AttribType.Rigid, AttribType.RigidMax));
		m_AttribPairs[31] = (m_AttribPairs[32] = new AttribPair(AttribType.Shield, AttribType.ShieldMax));
		m_AttribPairs[41] = (m_AttribPairs[40] = new AttribPair(AttribType.Hitfly, AttribType.HitflyMax));
	}
}
