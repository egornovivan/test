public class PEAT_AbnormalHit : PEAbnormalTrigger
{
	private bool hitAbnormal;

	public int[] hitAbnormals { get; set; }

	public override bool Hit()
	{
		if (hitAbnormal)
		{
			hitAbnormal = false;
			return true;
		}
		return base.Hit();
	}

	public void OnHitAbnormal(PEAbnormalType type)
	{
		for (int i = 0; i < hitAbnormals.Length; i++)
		{
			if (hitAbnormals[i] == (int)type)
			{
				hitAbnormal = true;
				break;
			}
		}
	}

	public override void Update()
	{
		hitAbnormal = false;
	}
}
