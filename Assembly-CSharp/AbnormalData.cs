using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using Pathea;
using PETools;
using UnityEngine;

public class AbnormalData
{
	public class HitAttr
	{
		public AttribType attrType;

		public float minThreshold;

		public float maxThreshold;

		public float minRate;

		public float maxRate;

		public float GetRate(float val)
		{
			if (val < minThreshold || val > maxThreshold)
			{
				return 0f;
			}
			return Mathf.Lerp(minRate, maxRate, (val - minThreshold) / (maxThreshold - minThreshold));
		}

		public float GetRate(PeEntity entity)
		{
			if (null == entity)
			{
				return 0f;
			}
			return GetRate(entity.GetAttribute(attrType));
		}

		public static HitAttr GetHitAttr(SqliteDataReader reader, string fieldName)
		{
			return GetHitAttr(reader.GetString(reader.GetOrdinal(fieldName)));
		}

		private static HitAttr GetHitAttr(string attrStr)
		{
			if ("0" == attrStr)
			{
				return null;
			}
			HitAttr hitAttr = new HitAttr();
			string[] array = attrStr.Split(',');
			hitAttr.attrType = (AttribType)Convert.ToInt32(array[0]);
			hitAttr.minThreshold = Convert.ToSingle(array[1]);
			hitAttr.maxThreshold = Convert.ToSingle(array[2]);
			hitAttr.minRate = Convert.ToSingle(array[3]);
			hitAttr.maxRate = Convert.ToSingle(array[4]);
			return hitAttr;
		}

		public static HitAttr[] GetHitAttrArray(SqliteDataReader reader, string fieldName)
		{
			string @string = reader.GetString(reader.GetOrdinal(fieldName));
			if ("0" == @string)
			{
				return null;
			}
			string[] array = @string.Split(';');
			HitAttr[] array2 = new HitAttr[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = GetHitAttr(array[i]);
			}
			return array2;
		}
	}

	public class ThresholdData
	{
		public int type;

		public float threshold;

		private static ThresholdData GetThresholdData(string str)
		{
			float[] array = Db.ReadFloatArray(str);
			if (array == null)
			{
				return null;
			}
			ThresholdData thresholdData = new ThresholdData();
			thresholdData.type = Mathf.RoundToInt(array[0]);
			thresholdData.threshold = array[1];
			return thresholdData;
		}

		public static ThresholdData GetThresholdData(SqliteDataReader reader, string fieldName)
		{
			string @string = reader.GetString(reader.GetOrdinal(fieldName));
			if (string.IsNullOrEmpty(@string) || @string == "0")
			{
				return null;
			}
			return GetThresholdData(@string);
		}

		public static ThresholdData[] GetThresholdDatas(SqliteDataReader reader, string fieldName)
		{
			string @string = reader.GetString(reader.GetOrdinal(fieldName));
			if (string.IsNullOrEmpty(@string) || @string == "0")
			{
				return null;
			}
			string[] array = @string.Split(';');
			ThresholdData[] array2 = new ThresholdData[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = GetThresholdData(array[i]);
			}
			return array2;
		}
	}

	public class EffCamera
	{
		public int type;

		public float value;

		public static EffCamera GetEffCamera(SqliteDataReader reader, string fieldName)
		{
			string @string = reader.GetString(reader.GetOrdinal(fieldName));
			if ("0" == @string)
			{
				return null;
			}
			string[] array = @string.Split(',');
			EffCamera effCamera = new EffCamera();
			effCamera.type = Convert.ToInt32(array[0]);
			effCamera.value = Convert.ToSingle(array[1]);
			return effCamera;
		}
	}

	public PEAbnormalType type;

	public string name;

	public string iconName;

	public string description;

	public int target;

	public bool deathRemove;

	public bool updateByModel;

	public float trigger_TimeInterval;

	public int[] trigger_BuffAdd;

	public int[] trigger_ItemGet;

	public bool trigger_Damage;

	public bool trigger_InWater;

	public PEAbnormalType[] hit_MutexAbnormal;

	public PEAbnormalType[] hit_PreAbnormal;

	public int[] hit_BuffID;

	public HitAttr[] hit_Attr;

	public HitAttr hit_Damage;

	public float hit_TimeInterval;

	public float[] hit_AreaTime;

	public float hit_RainTime;

	public float hit_HitRate;

	public int[] eff_BuffAddList;

	public string eff_Anim;

	public EffCamera eff_Camera;

	public PEAbnormalType[] eff_AbnormalRemove;

	public int[] eff_Particles;

	public Color eff_SkinColor;

	public ThresholdData[] eff_BodyWeight;

	public bool rt_Immediate;

	public float rt_TimeInterval;

	public int[] rt_BuffRemove;

	public bool rt_EffectEnd;

	public bool rt_OutsideWater;

	public int[] rh_BuffList;

	public HitAttr[] rh_Attr;

	public int[] re_BuffRemove;

	public int[] re_BuffAdd;

	public PEAbnormalType[] re_AbnormalAdd;

	public string re_Anim;

	public EffCamera re_Camera;

	public int[] re_Particles;

	private static Dictionary<PEAbnormalType, AbnormalData> g_DataDic;

	public static void LoadData()
	{
		g_DataDic = new Dictionary<PEAbnormalType, AbnormalData>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AbnormalType");
		while (sqliteDataReader.Read())
		{
			AbnormalData abnormalData = new AbnormalData();
			abnormalData.type = (PEAbnormalType)Db.GetInt(sqliteDataReader, "AbnormalId");
			abnormalData.name = PELocalization.GetString(Db.GetInt(sqliteDataReader, "TranslationNameId"));
			abnormalData.iconName = Db.GetString(sqliteDataReader, "Icon");
			abnormalData.description = PELocalization.GetString(Db.GetInt(sqliteDataReader, "TranslationDescribeId"));
			abnormalData.target = Db.GetInt(sqliteDataReader, "AbnormalTarget");
			abnormalData.deathRemove = Db.GetBool(sqliteDataReader, "IsDeathRemove");
			abnormalData.updateByModel = Db.GetBool(sqliteDataReader, "UpdateByModel");
			abnormalData.trigger_TimeInterval = Db.GetFloat(sqliteDataReader, "Trigger_Time");
			abnormalData.trigger_BuffAdd = Db.GetIntArray(sqliteDataReader, "Trigger_BuffAdd");
			abnormalData.trigger_ItemGet = Db.GetIntArray(sqliteDataReader, "Trigger_ItemGet");
			abnormalData.trigger_Damage = Db.GetBool(sqliteDataReader, "Trigger_Damage");
			abnormalData.trigger_InWater = Db.GetBool(sqliteDataReader, "Trigger_IntoWater");
			abnormalData.hit_MutexAbnormal = GetAbnormalType(sqliteDataReader, "Hit_MutexAbnormal");
			abnormalData.hit_PreAbnormal = GetAbnormalType(sqliteDataReader, "Hit_PreAbnormal");
			abnormalData.hit_BuffID = Db.GetIntArray(sqliteDataReader, "Hit_BuffList");
			abnormalData.hit_Attr = HitAttr.GetHitAttrArray(sqliteDataReader, "Hit_Attr");
			abnormalData.hit_Damage = HitAttr.GetHitAttr(sqliteDataReader, "Hit_Damage");
			abnormalData.hit_TimeInterval = Db.GetFloat(sqliteDataReader, "Hit_Time");
			abnormalData.hit_AreaTime = Db.GetFloatArray(sqliteDataReader, "Hit_AreaTime");
			abnormalData.hit_RainTime = Db.GetFloat(sqliteDataReader, "Hit_RainTime");
			abnormalData.hit_HitRate = Db.GetFloat(sqliteDataReader, "Hit_Rate");
			abnormalData.eff_BuffAddList = Db.GetIntArray(sqliteDataReader, "Eff_BuffAdd");
			abnormalData.eff_Anim = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Eff_Anim"));
			abnormalData.eff_Camera = EffCamera.GetEffCamera(sqliteDataReader, "Eff_Camera");
			abnormalData.eff_AbnormalRemove = GetAbnormalType(sqliteDataReader, "Eff_RemoveAbnormal");
			abnormalData.eff_Particles = Db.GetIntArray(sqliteDataReader, "Eff_Particle");
			abnormalData.eff_SkinColor = Db.GetColor(sqliteDataReader, "Eff_SkinColor");
			abnormalData.eff_BodyWeight = ThresholdData.GetThresholdDatas(sqliteDataReader, "Eff_BodyWeight");
			abnormalData.rt_Immediate = Db.GetBool(sqliteDataReader, "RT_Imm");
			abnormalData.rt_TimeInterval = Db.GetFloat(sqliteDataReader, "RT_Time");
			abnormalData.rt_BuffRemove = Db.GetIntArray(sqliteDataReader, "RT_BuffRemove");
			abnormalData.rt_EffectEnd = Db.GetBool(sqliteDataReader, "RT_EffEnd");
			abnormalData.rt_OutsideWater = Db.GetBool(sqliteDataReader, "RT_OutWater");
			abnormalData.rh_BuffList = Db.GetIntArray(sqliteDataReader, "RH_BuffRemove");
			abnormalData.rh_Attr = HitAttr.GetHitAttrArray(sqliteDataReader, "RH_Attr");
			abnormalData.re_BuffRemove = Db.GetIntArray(sqliteDataReader, "RE_BuffRemove");
			abnormalData.re_BuffAdd = Db.GetIntArray(sqliteDataReader, "RE_BuffAdd");
			abnormalData.re_AbnormalAdd = GetAbnormalType(sqliteDataReader, "RE_AbnormalAdd");
			abnormalData.re_Anim = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("RE_Anim"));
			abnormalData.re_Camera = EffCamera.GetEffCamera(sqliteDataReader, "RE_Camera");
			abnormalData.re_Particles = Db.GetIntArray(sqliteDataReader, "RE_Particle");
			g_DataDic.Add(abnormalData.type, abnormalData);
		}
	}

	private static PEAbnormalType[] GetAbnormalType(SqliteDataReader reader, string fieldName)
	{
		int[] intArray = Db.GetIntArray(reader, fieldName);
		if (intArray == null || intArray.Length == 0)
		{
			return null;
		}
		PEAbnormalType[] array = new PEAbnormalType[intArray.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (PEAbnormalType)intArray[i];
		}
		return array;
	}

	public static AbnormalData GetData(PEAbnormalType type)
	{
		if (g_DataDic.ContainsKey(type))
		{
			return g_DataDic[type];
		}
		Debug.LogError("Can't find abnormaltype:" + type);
		return null;
	}
}
