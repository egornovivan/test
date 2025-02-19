namespace SkillSystem;

public static class SkEffExFunc
{
	internal static void Apply(int procEffId, SkEntity entity, SkRuntimeInfo info)
	{
		switch (procEffId)
		{
		case 1:
			DoEff1(entity, info);
			break;
		case 2:
			DoEff2(entity, info);
			break;
		}
	}

	private static void DoEff1(SkEntity entity, SkRuntimeInfo info)
	{
		if (info is SkBuffInst skBuffInst)
		{
			entity.OnBuffAdd(skBuffInst._buff._id);
		}
	}

	private static void DoEff2(SkEntity entity, SkRuntimeInfo info)
	{
		if (info is SkBuffInst skBuffInst)
		{
			entity.OnBuffRemove(skBuffInst._buff._id);
		}
	}
}
