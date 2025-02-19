using System.Collections.Generic;
using Mono.Data.SqliteClient;
using Pathea;
using PETools;
using SkillSystem;
using UnityEngine;

public class RepProcessor
{
	public enum ERepType
	{
		Player_Puja,
		Player_Paja
	}

	private struct RepVal
	{
		public int _id;

		public int[] _vals;
	}

	private static Dictionary<int, RepVal> s_dicRepVals = new Dictionary<int, RepVal>();

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("ReputationValue");
		int ordinal = sqliteDataReader.GetOrdinal("ID");
		int ordinal2 = sqliteDataReader.GetOrdinal("Fear");
		int ordinal3 = sqliteDataReader.GetOrdinal("Hatred");
		int ordinal4 = sqliteDataReader.GetOrdinal("Animosity");
		int ordinal5 = sqliteDataReader.GetOrdinal("Cold");
		int ordinal6 = sqliteDataReader.GetOrdinal("Neutral");
		int ordinal7 = sqliteDataReader.GetOrdinal("Cordial");
		int ordinal8 = sqliteDataReader.GetOrdinal("Amity");
		int ordinal9 = sqliteDataReader.GetOrdinal("Respectful");
		int ordinal10 = sqliteDataReader.GetOrdinal("Reverence");
		while (sqliteDataReader.Read())
		{
			RepVal value = default(RepVal);
			value._id = sqliteDataReader.GetInt32(ordinal);
			value._vals = new int[9];
			value._vals[0] = sqliteDataReader.GetInt32(ordinal2);
			value._vals[1] = sqliteDataReader.GetInt32(ordinal3);
			value._vals[2] = sqliteDataReader.GetInt32(ordinal4);
			value._vals[3] = sqliteDataReader.GetInt32(ordinal5);
			value._vals[4] = sqliteDataReader.GetInt32(ordinal6);
			value._vals[5] = sqliteDataReader.GetInt32(ordinal7);
			value._vals[6] = sqliteDataReader.GetInt32(ordinal8);
			value._vals[7] = sqliteDataReader.GetInt32(ordinal9);
			value._vals[8] = sqliteDataReader.GetInt32(ordinal10);
			s_dicRepVals.Add(value._id, value);
		}
	}

	public static void OnMonsterDeath(SkEntity cur, SkEntity caster)
	{
		caster = PEUtil.GetCaster(caster);
		SkAliveEntity skAliveEntity = cur as SkAliveEntity;
		SkAliveEntity skAliveEntity2 = caster as SkAliveEntity;
		if (!(skAliveEntity != null) || !(skAliveEntity2 != null))
		{
			return;
		}
		int num = Mathf.RoundToInt(skAliveEntity.GetAttribute(AttribType.DefaultPlayerID));
		if (!ReputationSystem.IsReputationTarget(num))
		{
			return;
		}
		int playerID = Mathf.RoundToInt(skAliveEntity2.GetAttribute(AttribType.DefaultPlayerID));
		int reputationLevel = (int)PeSingleton<ReputationSystem>.Instance.GetReputationLevel(playerID, num);
		if (PeGameMgr.IsAdventure && reputationLevel > 3)
		{
			int num2 = 3;
			int num3 = 0;
			for (int i = num2; i < reputationLevel; i++)
			{
				num3 -= ReputationSystem.GetLevelThreshold((ReputationSystem.ReputationLevel)i);
			}
			PeSingleton<ReputationSystem>.Instance.ChangeReputationValue(playerID, num, num3, changeOther: true);
		}
		else
		{
			MonsterProtoDb.Item item = MonsterProtoDb.Get(skAliveEntity.Entity.entityProto.protoId);
			if (item != null && s_dicRepVals.TryGetValue(item.repValId, out var value))
			{
				int addValue = (int)((float)value._vals[reputationLevel] * -1.2f);
				PeSingleton<ReputationSystem>.Instance.ChangeReputationValue(playerID, num, addValue, changeOther: true);
			}
		}
	}

	public static void OnDoodadDeath(SkEntity cur, SkEntity caster)
	{
		caster = PEUtil.GetCaster(caster);
		SkAliveEntity skAliveEntity = cur as SkAliveEntity;
		SkAliveEntity skAliveEntity2 = caster as SkAliveEntity;
		if (!(skAliveEntity != null) || !(skAliveEntity2 != null))
		{
			return;
		}
		int num = Mathf.RoundToInt(skAliveEntity.GetAttribute(AttribType.DefaultPlayerID));
		if (!ReputationSystem.IsReputationTarget(num))
		{
			return;
		}
		int playerID = Mathf.RoundToInt(skAliveEntity2.GetAttribute(AttribType.DefaultPlayerID));
		int reputationLevel = (int)PeSingleton<ReputationSystem>.Instance.GetReputationLevel(playerID, num);
		if (PeGameMgr.IsAdventure && reputationLevel > 3)
		{
			int num2 = 3;
			int num3 = 0;
			for (int i = num2; i < reputationLevel; i++)
			{
				num3 -= ReputationSystem.GetLevelThreshold((ReputationSystem.ReputationLevel)i);
			}
			PeSingleton<ReputationSystem>.Instance.ChangeReputationValue(playerID, num, num3, changeOther: true);
		}
		else
		{
			DoodadProtoDb.Item item = DoodadProtoDb.Get(skAliveEntity.Entity.entityProto.protoId);
			if (item != null && s_dicRepVals.TryGetValue(item.repValId, out var value))
			{
				int addValue = (int)((float)value._vals[reputationLevel] * -1.2f);
				PeSingleton<ReputationSystem>.Instance.ChangeReputationValue(playerID, num, addValue, changeOther: true);
			}
		}
	}
}
