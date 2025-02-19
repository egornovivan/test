using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

namespace AiAsset;

public class AiDataBlock
{
	public struct DropData
	{
		public int id;

		public float pro;
	}

	public class ItemDrop
	{
		public int count;

		public List<DropData> DropList = new List<DropData>();

		public void Clear()
		{
			count = 0;
			DropList.Clear();
		}
	}

	public Dietary dietary;

	public Nature nature;

	public bool cavern;

	public bool darkness;

	public bool social;

	public bool isboss;

	public string name = string.Empty;

	public string uiName = string.Empty;

	public string xmlPath = string.Empty;

	public int dataId;

	public int model;

	public int camp;

	public int harm;

	public int life = 100;

	public int music;

	public int attackType = 8;

	public int defenseType = 7;

	public float walkSpeed = 5f;

	public float runSpeed = 10f;

	public float turnSpeed = 5f;

	public float jumpHeight = 2f;

	public int damage = 100;

	public int buildDamage = 100;

	public int defence = 100;

	public float wanderRange = 10f;

	public float moveRange = 10f;

	public float alertRange = 10f;

	public float chaseRange = 10f;

	public float hearRange = 8f;

	public float horizonRange = 8f;

	public float attackRangeMin = 2f;

	public float attackRangeMax = 60f;

	public float attackAngle = 30f;

	public float pitchAngle = 60f;

	public float horizonAngle = 60f;

	public float escapeRange = 10f;

	public float minScale = 1f;

	public float maxScale = 1f;

	public int strongWeight;

	public int normalWeight;

	public int sickWeight;

	public int pregnancyWeight;

	public float pregnancyTime;

	public float eggDrop;

	public float restTimeMin;

	public float restTimeMax;

	public float fatigueMulDay;

	public float fatigueMulNight;

	public float wakeRate;

	public int deathSkill;

	public int drinkSkill;

	public int sleepSkill;

	public float callRate;

	public float calledRate;

	public float escapePercent;

	public float damageSimulate;

	public float maxHpSimulate;

	public int colonyLevDamage;

	public int colonyLevEscape;

	public float colonyEscapeValue;

	public float colonyEscapeRate;

	public int[] deathEffect;

	public int[] equipmentIDs;

	public int[] lifeFormIDs;

	public SkillData[] attackSkill;

	public ItemDrop m_ItemDrop = new ItemDrop();

	private int mCamp;

	private int mHarm;

	private static Dictionary<int, AiDataBlock> m_data = new Dictionary<int, AiDataBlock>();

	public int Camp => mCamp;

	public int Harm => mHarm;

	public static AiDataBlock GetAIDataBase(int pID)
	{
		return (!m_data.ContainsKey(pID)) ? null : m_data[pID];
	}

	public static void Reset()
	{
		foreach (KeyValuePair<int, AiDataBlock> datum in m_data)
		{
			datum.Value.ResetCamp();
			datum.Value.ResetHarm();
		}
	}

	public static AiDataBlock GetAiDataBaseByAiName(string name)
	{
		foreach (AiDataBlock value in m_data.Values)
		{
			if (value != null && value.xmlPath.Equals(name))
			{
				return value;
			}
		}
		return null;
	}

	public static void SetCamp(int dataID, int camp)
	{
		AiDataBlock aIDataBase = GetAIDataBase(dataID);
		if (aIDataBase != null && aIDataBase.mCamp != camp)
		{
			aIDataBase.mCamp = camp;
		}
	}

	public static void SetHarm(int dataID, int harm)
	{
		AiDataBlock aIDataBase = GetAIDataBase(dataID);
		if (aIDataBase != null && aIDataBase.mHarm != harm)
		{
			aIDataBase.mHarm = harm;
		}
	}

	public static string GetAIDataName(int id)
	{
		if (m_data.ContainsKey(id))
		{
			return m_data[id].name;
		}
		return string.Empty;
	}

	public static string GetIconName(int id)
	{
		if (m_data.ContainsKey(id))
		{
			return m_data[id].uiName;
		}
		return string.Empty;
	}

	public static ItemDrop GetItemDrop(int id)
	{
		return GetAIDataBase(id)?.m_ItemDrop;
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("ai_data");
		while (sqliteDataReader.Read())
		{
			AiDataBlock aiDataBlock = new AiDataBlock();
			aiDataBlock.dataId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("monster_ID")));
			aiDataBlock.uiName = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ui_name"));
			aiDataBlock.name = PELocalization.GetString(Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("eng_name"))));
			aiDataBlock.xmlPath = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("tree_path"));
			aiDataBlock.model = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("model_ID")));
			aiDataBlock.nature = (Nature)Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("environment")));
			aiDataBlock.dietary = (Dietary)Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("feeding_habits")));
			aiDataBlock.social = Convert.ToBoolean(Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("social"))));
			aiDataBlock.cavern = Convert.ToBoolean(Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("cavern"))));
			aiDataBlock.darkness = Convert.ToBoolean(Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("darkness"))));
			aiDataBlock.isboss = Convert.ToBoolean(Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("isboss"))));
			aiDataBlock.camp = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("camp")));
			aiDataBlock.harm = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("harm")));
			aiDataBlock.damage = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("attack")));
			aiDataBlock.buildDamage = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("damage")));
			aiDataBlock.defence = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("defense")));
			aiDataBlock.life = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("hp")));
			aiDataBlock.music = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("music")));
			aiDataBlock.attackType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("attack_type")));
			aiDataBlock.defenseType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("defense_type")));
			aiDataBlock.walkSpeed = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("walking_speed")));
			aiDataBlock.runSpeed = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("running_speed")));
			aiDataBlock.jumpHeight = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("jump_height")));
			aiDataBlock.alertRange = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("alert_rad")));
			aiDataBlock.wanderRange = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("patrol_rad")));
			aiDataBlock.moveRange = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("active_range")));
			aiDataBlock.horizonAngle = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("horizon_angle")));
			aiDataBlock.horizonRange = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("horizon_rad")));
			aiDataBlock.hearRange = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("hear_rad")));
			aiDataBlock.attackRangeMin = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("damage_rad_min")));
			aiDataBlock.attackRangeMax = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("damage_rad_max")));
			aiDataBlock.attackAngle = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("damage_angle")));
			aiDataBlock.turnSpeed = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("turn_speed")));
			aiDataBlock.escapeRange = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("escape_distance")));
			aiDataBlock.damageSimulate = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("dps")));
			aiDataBlock.maxHpSimulate = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("hp_relative")));
			aiDataBlock.pregnancyTime = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("pregnancy_time")));
			aiDataBlock.eggDrop = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("egg_drop")));
			aiDataBlock.restTimeMin = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_restTimeMin")));
			aiDataBlock.restTimeMax = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_restTimeMax")));
			aiDataBlock.fatigueMulDay = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_fatigueMulDaytime")));
			aiDataBlock.fatigueMulNight = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_fatigueMulNight")));
			aiDataBlock.wakeRate = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_sleepWaked")));
			aiDataBlock.deathSkill = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_deathSkill")));
			aiDataBlock.colonyLevDamage = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_DSLvInjureEnable")));
			aiDataBlock.colonyLevEscape = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_DSLvEscape")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_DSEscapeInfo"));
			string[] array = AiUtil.Split(@string, ',');
			if (array.Length == 2)
			{
				aiDataBlock.colonyEscapeValue = Convert.ToSingle(array[0]);
				aiDataBlock.colonyEscapeRate = Convert.ToSingle(array[1]);
			}
			string string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_deathEffect"));
			if (string2 != "0")
			{
				string[] array2 = string2.Split(',');
				aiDataBlock.deathEffect = new int[array2.Length];
				for (int i = 0; i < aiDataBlock.deathEffect.Length; i++)
				{
					aiDataBlock.deathEffect[i] = Convert.ToInt32(array2[i]);
				}
			}
			string string3 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_equip"));
			if (!string.IsNullOrEmpty(string3))
			{
				string[] array3 = string3.Split(',');
				aiDataBlock.equipmentIDs = new int[array3.Length];
				for (int j = 0; j < aiDataBlock.equipmentIDs.Length; j++)
				{
					aiDataBlock.equipmentIDs[j] = Convert.ToInt32(array3[j]);
				}
			}
			aiDataBlock.drinkSkill = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_drinkSkill")));
			aiDataBlock.sleepSkill = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_sleepSkill")));
			string string4 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_attackSkill"));
			if (string4 != "0")
			{
				string[] array4 = string4.Split(',');
				aiDataBlock.attackSkill = new SkillData[array4.Length];
				for (int k = 0; k < aiDataBlock.attackSkill.Length; k++)
				{
					string[] array5 = array4[k].Split(':');
					aiDataBlock.attackSkill[k].id = Convert.ToInt32(array5[0]);
					aiDataBlock.attackSkill[k].probability = Convert.ToSingle(array5[1]);
				}
			}
			aiDataBlock.callRate = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_callProbability")));
			aiDataBlock.calledRate = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_calledProbability")));
			aiDataBlock.escapePercent = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_escapeHpPercent")));
			string string5 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("item_drop"));
			string[] array6 = string5.Split(';');
			if (array6.Length == 2)
			{
				int num = Convert.ToInt32(array6[0]);
				if (num > 0)
				{
					aiDataBlock.m_ItemDrop.count = num;
					string[] array7 = array6[1].Split(',');
					if (array7.Length > 0)
					{
						for (int l = 0; l < array7.Length; l++)
						{
							string[] array8 = array7[l].Split('_');
							if (array8.Length == 2)
							{
								DropData item = default(DropData);
								item.id = Convert.ToInt32(array8[0]);
								item.pro = Convert.ToSingle(array8[1]);
								aiDataBlock.m_ItemDrop.DropList.Add(item);
							}
						}
					}
				}
			}
			string5 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("meat_drop"));
			array6 = string5.Split(';');
			if (array6.Length == 2)
			{
			}
			string5 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("item_carry"));
			array6 = string5.Split(';');
			for (int m = 0; m < array6.Length; m++)
			{
				string[] array9 = array6[m].Split(',');
				if (array9.Length == 2)
				{
				}
			}
			string string6 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_lfrList"));
			string[] array10 = AiUtil.Split(string6, ',');
			aiDataBlock.lifeFormIDs = new int[array10.Length];
			for (int n = 0; n < aiDataBlock.lifeFormIDs.Length; n++)
			{
				aiDataBlock.lifeFormIDs[n] = Convert.ToInt32(array10[n]);
			}
			aiDataBlock.ResetCamp();
			aiDataBlock.ResetHarm();
			m_data.Add(aiDataBlock.dataId, aiDataBlock);
		}
	}

	public void ResetCamp()
	{
		mCamp = camp;
	}

	public void ResetHarm()
	{
		mHarm = harm;
	}
}
