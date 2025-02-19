public class PEAH_Damage : PEAbnormalHit
{
	private float m_Damage;

	public AbnormalData.HitAttr attr { get; set; }

	public override float HitRate()
	{
		return attr.GetRate(m_Damage);
	}

	public void OnGetDamage(float damage)
	{
		m_Damage = damage;
	}
}
