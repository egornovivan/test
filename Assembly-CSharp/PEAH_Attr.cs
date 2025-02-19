using Pathea;

public class PEAH_Attr : PEAbnormalHit
{
	public PeEntity entity { get; set; }

	public AbnormalData.HitAttr[] attrs { get; set; }

	public override float HitRate()
	{
		float num = 1f;
		for (int i = 0; i < attrs.Length; i++)
		{
			num *= attrs[i].GetRate(entity);
		}
		return num;
	}
}
