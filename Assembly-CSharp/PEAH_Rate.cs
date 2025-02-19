public class PEAH_Rate : PEAbnormalHit
{
	public float rate { get; set; }

	public override float HitRate()
	{
		return rate;
	}
}
