using System;
using Mono.Data.SqliteClient;
using UnityEngine;

namespace SkillAsset;

public class GroundScope
{
	internal int mType;

	internal float mRadius;

	internal Vector3 mCenter;

	internal static GroundScope Create(SqliteDataReader reader)
	{
		string @string = reader.GetString(reader.GetOrdinal("_rScope"));
		string[] array = @string.Split(',');
		GroundScope groundScope = new GroundScope();
		groundScope.mType = Convert.ToInt32(array[0]);
		groundScope.mRadius = Convert.ToSingle(array[1]);
		groundScope.mCenter = new Vector3(Convert.ToSingle(array[2]), Convert.ToSingle(array[3]), Convert.ToSingle(array[4]));
		if (groundScope.mType == 0)
		{
			return null;
		}
		return groundScope;
	}
}
