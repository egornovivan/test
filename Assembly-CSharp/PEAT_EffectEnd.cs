public class PEAT_EffectEnd : PEAbnormalTrigger
{
	public PEAbnormal_N abnormal { get; set; }

	public override bool Hit()
	{
		return abnormal.effectEnd;
	}
}
