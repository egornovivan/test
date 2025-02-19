using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using Pathea;
using PETools;
using UnityEngine;

public class MountsSkillDb
{
	private static int[] _mounstersIndex;

	private static int[][][] _MskillDatas;

	private static Dictionary<int, string> _prounceData;

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("MountsSkill");
		_mounstersIndex = new int[100];
		_MskillDatas = new int[50][][];
		_prounceData = new Dictionary<int, string>();
		try
		{
			while (sqliteDataReader.Read())
			{
				int num = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
				int num2 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MonsterID")));
				_mounstersIndex[num2] = num;
				_MskillDatas[num] = new int[4][];
				_MskillDatas[num][0] = PEUtil.ToArrayInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("SkillLeft")), ',');
				_MskillDatas[num][1] = PEUtil.ToArrayInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("SkillSpace")), ',');
				string[] array = PEUtil.ToArrayString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("SkillJump")), ',');
				List<int> list = new List<int>();
				if (array != null)
				{
					for (int i = 0; i < array.Length; i++)
					{
						string[] array2 = PEUtil.ToArrayString(array[i], ':');
						if (array2.Length == 2)
						{
							list.Add(Convert.ToInt32(array2[0]));
							_prounceData.Add(Convert.ToInt32(array2[0]), array2[1]);
						}
					}
				}
				_MskillDatas[num][2] = list.ToArray();
				_MskillDatas[num][3] = PEUtil.ToArrayInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("RideAnim")), ',');
			}
		}
		catch
		{
			Debug.LogError("mounstsSKill !!!!");
		}
	}

	public static void Relese()
	{
		_mounstersIndex = null;
		_MskillDatas = null;
		_prounceData = null;
	}

	public static int[] GetSkills(int monstersProID, MountsSkillKey key)
	{
		return (_MskillDatas == null || _mounstersIndex[monstersProID] == 0 || _MskillDatas[_mounstersIndex[monstersProID]] == null) ? null : _MskillDatas[_mounstersIndex[monstersProID]][(int)key];
	}

	public static int GetRandomSkill(int monstersProID, MountsSkillKey key)
	{
		int[] skills = GetSkills(monstersProID, key);
		return (skills != null && skills.Length > 0) ? skills[UnityEngine.Random.Range(0, skills.Length)] : 0;
	}

	public static string GetpounceAnim(int pounceSkillId)
	{
		return (_prounceData == null || !_prounceData.ContainsKey(pounceSkillId)) ? string.Empty : _prounceData[pounceSkillId];
	}
}
