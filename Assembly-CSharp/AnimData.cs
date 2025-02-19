using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class AnimData
{
	public enum AnimId
	{
		Base_walk_1,
		Base_run_1,
		Base_turnL_1,
		Base_turnR_1,
		Base_startJump_0,
		Base_jump_1,
		Base_jumpEnd_0,
		Base_idle_1,
		Base_idle0_0,
		Base_idle1_0,
		Attack_aTKIdle_1,
		Attack_attack0_0,
		Attack_attack1_0,
		Attack_attack2_0,
		Attack_attack3_0,
		Attack_attack4_0,
		Attack_attack5_0,
		Attack_attack6_0,
		Attack_attack7_0,
		Attack_attack8_0,
		Attack_hit0_0,
		Attack_hit1_0,
		Attack_death0_0,
		Attack_death1_0,
		Rest_idle2_0,
		Rest_sleepdown0_0,
		Rest_sleep0_1,
		Rest_sleepup0_0,
		Rest_sleepdown1_0,
		Rest_sleep1_1,
		Rest_sleepup1_0,
		Rest_other0_0,
		Rest_other1_1,
		Rest_other2_0,
		Life_idle3_0,
		Life_idle4_0,
		Life_idle5_0,
		Life_alert_0,
		Life_call0_0,
		Life_call1_0,
		Life_threat0_0,
		Life_threat1_0,
		Life_layegg_0,
		BaseAlien_gunidle_1,
		BaseAlien_gunrun_1,
		BaseAlien_gunpull_0,
		BaseAlien_gunless_0,
		BaseAlien_swordidle_1,
		BaseAlien_swordrun_1,
		BaseAlien_swordpull_0,
		BaseAlien_swordless_0,
		AnimId_MAX
	}

	public int m_modelId;

	public string m_modelName;

	public AnimClip[] m_animClips;

	public static bool isLoaded;

	public static List<AnimData> s_tblAnimData;

	public static void LoadData()
	{
		s_tblAnimData = new List<AnimData>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("ai_anim_clip");
		int num = 51;
		if (sqliteDataReader.FieldCount != 3 * num + 2)
		{
			Debug.LogError("'AnimDataTbl' : Unrecognized table format");
		}
		while (sqliteDataReader.Read())
		{
			AnimData animData = new AnimData();
			animData.m_animClips = new AnimClip[num];
			animData.m_modelId = Convert.ToInt32(sqliteDataReader.GetString(0));
			animData.m_modelName = sqliteDataReader.GetString(1);
			for (int i = 0; i < num; i++)
			{
				animData.m_animClips[i] = new AnimClip();
				string name = sqliteDataReader.GetName(2 + i * 3);
				int length = name.IndexOf('_');
				string[] array = ((AnimId)i).ToString().Split('_');
				animData.m_animClips[i].m_isLoop = Convert.ToInt32(array[2]) != 0;
				animData.m_animClips[i].m_frm_Name = name.Substring(0, length).Trim();
				animData.m_animClips[i].m_frm_Start = Convert.ToInt32(sqliteDataReader.GetString(2 + i * 3));
				animData.m_animClips[i].m_frm_End = Convert.ToInt32(sqliteDataReader.GetString(3 + i * 3));
				animData.m_animClips[i].m_frm_Sound = Convert.ToInt32(sqliteDataReader.GetString(4 + i * 3));
			}
			s_tblAnimData.Add(animData);
		}
		isLoaded = true;
	}

	public static AnimData LoadAnimDataFromModelID(int modelID)
	{
		foreach (AnimData s_tblAnimDatum in s_tblAnimData)
		{
			if (s_tblAnimDatum.m_modelId == modelID)
			{
				return s_tblAnimDatum;
			}
		}
		return null;
	}

	public static AnimClip LoadAnimSoundFromModelID(int modelID, string animName)
	{
		foreach (AnimData s_tblAnimDatum in s_tblAnimData)
		{
			if (s_tblAnimDatum.m_modelId != modelID)
			{
				continue;
			}
			AnimClip[] animClips = s_tblAnimDatum.m_animClips;
			foreach (AnimClip animClip in animClips)
			{
				if (animClip.m_frm_Name.ToLower() == animName.ToLower())
				{
					return animClip;
				}
			}
		}
		return null;
	}

	public static AnimData LoadAnimDataFromModelName(string modelName)
	{
		foreach (AnimData s_tblAnimDatum in s_tblAnimData)
		{
			if (s_tblAnimDatum.m_modelName == modelName)
			{
				return s_tblAnimDatum;
			}
		}
		return null;
	}
}
