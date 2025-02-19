using Pathea;

public class PEAE_Abnormal : PEAbnormalEff
{
	public PEAbnormalType[] abnormalType { get; set; }

	public bool addAbnormal { get; set; }

	public AbnormalConditionCmpt abnormalCmpt { get; set; }

	public override void Do()
	{
		for (int i = 0; i < abnormalType.Length; i++)
		{
			if (addAbnormal)
			{
				abnormalCmpt.StartAbnormalCondition(abnormalType[i]);
			}
			else
			{
				abnormalCmpt.EndAbnormalCondition(abnormalType[i]);
			}
		}
	}
}
