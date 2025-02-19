using Pathea;

public class PEAE_BodyWeight : PEAbnormalEff
{
	public AvatarCmpt avatar { get; set; }

	public AbnormalData.ThresholdData[] datas { get; set; }

	public override void Do()
	{
		if (null != avatar)
		{
			for (int i = 0; i < datas.Length; i++)
			{
				avatar.apperaData.subBodyWeight[datas[i].type] = datas[i].threshold;
			}
			avatar.UpdateSmr();
		}
	}

	public override void End()
	{
		if (null != avatar)
		{
			for (int i = 0; i < datas.Length; i++)
			{
				avatar.apperaData.subBodyWeight[datas[i].type] = 0f;
			}
			avatar.UpdateSmr();
		}
	}
}
