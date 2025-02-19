using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

public class BattleUnitData
{
	public static List<BattleUnitData> s_tblBattleUnitData;

	public int mID;

	public int mType;

	public string mName;

	public string mPerfabPath;

	public float mMaxHp;

	public float mMaxEn;

	public float mMaxAmmo;

	public float mAtk;

	public int mAtkType;

	public float mAtkInterval;

	public EffectRange mAtkRange;

	public float mDef;

	public int mDefType;

	public float mRps;

	public float mHealPs;

	public int mHealType;

	public EffectRange mHealRange;

	public float mEnCostPs;

	public float mAmmoCostPs;

	public bool mPlayerForce;

	public float mMoveInterval;

	public float mSpreadFactor;

	public static BattleUnitData GetBattleUnitData(int id)
	{
		return s_tblBattleUnitData.Find((BattleUnitData itr) => itr.mID == id);
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("simulation");
		s_tblBattleUnitData = new List<BattleUnitData>();
		while (sqliteDataReader.Read())
		{
			BattleUnitData battleUnitData = new BattleUnitData();
			battleUnitData.mID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			battleUnitData.mName = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Name"));
			battleUnitData.mPerfabPath = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PerfabPath"));
			battleUnitData.mType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Type")));
			battleUnitData.mMaxHp = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MaxHP")));
			battleUnitData.mAtk = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Atk")));
			battleUnitData.mAtkType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Atk_T")));
			battleUnitData.mDef = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Def")));
			battleUnitData.mDefType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Def_T")));
			battleUnitData.mAtkInterval = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Atk_P")));
			battleUnitData.mAtkRange = (EffectRange)Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("AtkRange")));
			battleUnitData.mRps = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("RPS")));
			battleUnitData.mHealPs = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("HealPS")));
			battleUnitData.mHealType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Heal_T")));
			battleUnitData.mHealRange = (EffectRange)Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("HealRange")));
			battleUnitData.mMaxEn = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("EN")));
			battleUnitData.mMaxAmmo = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Ammo")));
			battleUnitData.mEnCostPs = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("EnCostPS")));
			battleUnitData.mAmmoCostPs = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("AmmoCost")));
			battleUnitData.mPlayerForce = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PlaterForce"))) != 0;
			battleUnitData.mMoveInterval = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MoveInt")));
			battleUnitData.mSpreadFactor = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("SpreadFac")));
			s_tblBattleUnitData.Add(battleUnitData);
		}
	}
}
