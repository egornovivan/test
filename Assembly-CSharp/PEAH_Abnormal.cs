using Pathea;

public class PEAH_Abnormal : PEAbnormalHit
{
	public PEAbnormalType[] abnormals { get; set; }

	public AbnormalConditionCmpt abnormalCmpt { get; set; }

	public bool abnormalExist { get; set; }

	public override float HitRate()
	{
		for (int i = 0; i < abnormals.Length; i++)
		{
			if (abnormalCmpt.CheckAbnormalCondition(abnormals[i]))
			{
				return (!abnormalExist) ? 0f : 1f;
			}
		}
		return (!abnormalExist) ? 1f : 0f;
	}
}
