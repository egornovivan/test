using System.Collections.Generic;
using SkillSystem;

public class SkEntityMgr
{
	private static Dictionary<int, SkEntity> _skEntitySet = new Dictionary<int, SkEntity>();

	public static void Add(int id, SkEntity entity)
	{
		_skEntitySet[id] = entity;
	}

	public static void Remove(int id)
	{
		_skEntitySet.Remove(id);
	}

	public static SkEntity GetSkEntity(int id)
	{
		if (_skEntitySet.ContainsKey(id))
		{
			return _skEntitySet[id];
		}
		return null;
	}
}
