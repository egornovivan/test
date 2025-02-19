using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using PETools;
using UnityEngine;

namespace Pathea.Projectile;

public class ProjectileData
{
	public int _id;

	public string _path;

	public string _bone;

	public int _sound;

	public int _effect;

	public Vector3 _axis;

	private static Dictionary<int, ProjectileData> m_data = new Dictionary<int, ProjectileData>();

	public static ProjectileData GetProjectileData(int pID)
	{
		return (!m_data.ContainsKey(pID)) ? null : m_data[pID];
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("projectile");
		while (sqliteDataReader.Read())
		{
			ProjectileData projectileData = new ProjectileData();
			projectileData._id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			projectileData._path = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("path"));
			projectileData._bone = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("bone"));
			projectileData._sound = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("sound")));
			projectileData._effect = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("effect")));
			projectileData._axis = PEUtil.ToVector3(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("axis")), ',');
			m_data.Add(projectileData._id, projectileData);
		}
	}
}
