using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

namespace SkillSystem;

public class SkEffect
{
	internal int _id;

	internal int _seId;

	internal int _emitId;

	internal int _camEffId;

	internal int _procEffId;

	internal bool _repelEff;

	internal string[] _anms;

	internal int[] _effId;

	internal static Dictionary<int, SkEffect> s_SkEffectTbl;

	public static void LoadData()
	{
		if (s_SkEffectTbl != null)
		{
			return;
		}
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("skEffect");
		s_SkEffectTbl = new Dictionary<int, SkEffect>();
		while (sqliteDataReader.Read())
		{
			SkEffect skEffect = new SkEffect();
			skEffect._id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_id")));
			skEffect._anms = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_anms")).Split(',');
			skEffect._seId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_seId")));
			skEffect._emitId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_emitId")));
			skEffect._repelEff = Convert.ToBoolean(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_spEff")));
			skEffect._camEffId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_camEffId")));
			try
			{
				skEffect._procEffId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_procEff")));
			}
			catch
			{
			}
			string[] array = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_effId")).Split(',');
			skEffect._effId = new int[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				skEffect._effId[i] = Convert.ToInt32(array[i]);
			}
			s_SkEffectTbl.Add(skEffect._id, skEffect);
		}
	}

	private string PickAnim(SkEntity entity, SkRuntimeInfo info)
	{
		if (info is SkInst skInst)
		{
			switch (_anms[0].ToLower())
			{
			case "rnd":
				return _anms[SkInst.s_rand.Next(1, _anms.Length)];
			case "inc":
			{
				int num = skInst.GuideCnt - 1;
				num %= _anms.Length - 1;
				return _anms[num + 1];
			}
			case "dir":
				return _anms[1 + skInst.GetAtkDir()];
			}
		}
		return _anms[0];
	}

	public void Apply(SkEntity entity, SkRuntimeInfo info)
	{
		if (_seId > 0)
		{
			entity.ApplySe(_seId, info);
		}
		if (_repelEff)
		{
			entity.ApplyRepelEff(info);
		}
		if (_emitId > 0)
		{
			entity.ApplyEmission(_emitId, info);
		}
		if (_anms.Length > 0)
		{
			entity.ApplyAnim(PickAnim(entity, info), info);
		}
		int[] effId = _effId;
		foreach (int num in effId)
		{
			if (num > 0)
			{
				entity.ApplyEff(num, info);
			}
		}
		if (_camEffId > 0)
		{
			entity.ApplyCamEff(_camEffId, info);
		}
		if (_procEffId > 0)
		{
			SkEffExFunc.Apply(_procEffId, entity, info);
		}
	}
}
