namespace Pathea.PeEntityExtPlayerPackage;

public static class PeEntityExtPlayerPackage
{
	public static int GetPkgItemCount(this PeEntity entity, int prototypeId)
	{
		PlayerPackageCmpt cmpt = entity.GetCmpt<PlayerPackageCmpt>();
		if (null == cmpt)
		{
			return 0;
		}
		return cmpt.package.GetCount(prototypeId);
	}

	public static bool AddToPkg(this PeEntity entity, int prototypeId, int count)
	{
		PlayerPackageCmpt cmpt = entity.GetCmpt<PlayerPackageCmpt>();
		if (null == cmpt)
		{
			return false;
		}
		return cmpt.Add(prototypeId, count);
	}

	public static bool RemoveFromPkg(this PeEntity entity, int prototypeId, int count)
	{
		PlayerPackageCmpt cmpt = entity.GetCmpt<PlayerPackageCmpt>();
		if (null == cmpt)
		{
			return false;
		}
		return cmpt.Destory(prototypeId, count);
	}

	public static int GetCreationItemCount(this PeEntity entity, ECreation type)
	{
		PlayerPackageCmpt cmpt = entity.GetCmpt<PlayerPackageCmpt>();
		if (null == cmpt)
		{
			return 0;
		}
		return cmpt.package.GetCreationCount(type);
	}
}
