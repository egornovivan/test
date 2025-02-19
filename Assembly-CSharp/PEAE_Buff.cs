using System.Collections.Generic;
using SkillSystem;

public class PEAE_Buff : PEAbnormalEff
{
	public SkEntity entity { get; set; }

	public int[] buffList { get; set; }

	public bool addBuff { get; set; }

	public override void Do()
	{
		if (null == entity && buffList != null)
		{
			return;
		}
		for (int i = 0; i < buffList.Length; i++)
		{
			if (addBuff)
			{
				SkEntity.MountBuff(entity, buffList[i], new List<int>(), new List<float>());
			}
			else
			{
				entity.CancelBuffById(buffList[i]);
			}
		}
	}

	public override void End()
	{
		if (null == entity)
		{
			return;
		}
		for (int i = 0; i < buffList.Length; i++)
		{
			if (addBuff)
			{
				entity.CancelBuffById(buffList[i]);
			}
			else
			{
				SkEntity.MountBuff(entity, buffList[i], new List<int>(), new List<float>());
			}
		}
	}
}
