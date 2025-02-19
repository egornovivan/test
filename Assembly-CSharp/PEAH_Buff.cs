using SkillSystem;

public class PEAH_Buff : PEAbnormalHit
{
	public SkEntity entity { get; set; }

	public int[] buffList { get; set; }

	public bool buffExist { get; set; }

	public override float HitRate()
	{
		for (int i = 0; i < buffList.Length; i++)
		{
			if (buffExist != (null != entity.GetSkBuffInst(buffList[i])))
			{
				return 0f;
			}
		}
		return 1f;
	}
}
