using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

namespace SkillAsset;

public class EffScope
{
	internal int m_centerType;

	internal float m_radius;

	internal float m_degStart;

	internal float m_degEnd;

	internal static List<EffScope> Create(SqliteDataReader reader)
	{
		string @string = reader.GetString(reader.GetOrdinal("_scope"));
		string[] array = @string.Split(',');
		if (array.Length < 4)
		{
			return null;
		}
		List<EffScope> list = new List<EffScope>();
		for (int i = 3; i < array.Length; i += 4)
		{
			EffScope effScope = new EffScope();
			effScope.m_centerType = Convert.ToInt32(array[i - 3]);
			effScope.m_radius = Convert.ToSingle(array[i - 2]);
			effScope.m_degStart = Convert.ToSingle(array[i - 1]);
			effScope.m_degEnd = Convert.ToSingle(array[i]);
			list.Add(effScope);
		}
		return list;
	}
}
