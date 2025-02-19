using System;
using Mono.Data.SqliteClient;
using PETools;
using UnityEngine;

namespace SkillSystem;

public class SkEffEmission
{
	internal string _path;

	internal string _bone;

	internal int _sound;

	internal int _effect;

	internal Vector3 _axis;

	internal static SkEffEmission Create(SqliteDataReader reader)
	{
		string @string = reader.GetString(reader.GetOrdinal("_descEmission"));
		string[] array = @string.Split(',');
		if (array.Length < 10)
		{
			return null;
		}
		SkEffEmission skEffEmission = new SkEffEmission();
		skEffEmission._path = array[0];
		skEffEmission._bone = array[1];
		skEffEmission._sound = Convert.ToInt32(array[2]);
		skEffEmission._effect = Convert.ToInt32(array[3]);
		skEffEmission._axis = PEUtil.ToVector3(array[4], '_');
		return skEffEmission;
	}
}
