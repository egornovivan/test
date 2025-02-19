using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

namespace Pathea;

public class ActionRelationData
{
	public class DependData
	{
		public PEActionMask maskType;

		public bool maskValue;
	}

	private static ActionRelationData[] m_Relations;

	public PEActionType m_ActionType;

	public List<DependData> m_DependMask;

	public List<PEActionType> m_PauseAction;

	public List<PEActionType> m_EndAction;

	public List<PEActionType> m_EndImmediately;

	public static ActionRelationData GetData(PEActionType type)
	{
		if (m_Relations == null)
		{
			Debug.LogError("ActionRelationData not init");
			return null;
		}
		return m_Relations[(int)type];
	}

	public static void LoadActionRelation()
	{
		m_Relations = new ActionRelationData[63];
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("ActionRelation");
		int fieldCount = sqliteDataReader.FieldCount;
		while (sqliteDataReader.Read())
		{
			ActionRelationData actionRelationData = new ActionRelationData();
			actionRelationData.m_ActionType = (PEActionType)Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ActionType")));
			actionRelationData.m_DependMask = new List<DependData>();
			int num = 5;
			for (int i = num; i < fieldCount; i++)
			{
				int num2 = Convert.ToInt32(sqliteDataReader.GetString(i));
				if (num2 > 0)
				{
					DependData dependData = new DependData();
					dependData.maskType = (PEActionMask)(i - num);
					dependData.maskValue = num2 == 1;
					actionRelationData.m_DependMask.Add(dependData);
				}
			}
			actionRelationData.m_PauseAction = new List<PEActionType>();
			string[] array = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PauseAction")).Split(',');
			string[] array2 = array;
			foreach (string text in array2)
			{
				if ("0" != text)
				{
					actionRelationData.m_PauseAction.Add((PEActionType)Convert.ToInt32(text));
				}
			}
			actionRelationData.m_EndAction = new List<PEActionType>();
			string[] array3 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("EndAction")).Split(',');
			string[] array4 = array3;
			foreach (string text2 in array4)
			{
				if ("0" != text2)
				{
					actionRelationData.m_EndAction.Add((PEActionType)Convert.ToInt32(text2));
				}
			}
			actionRelationData.m_EndImmediately = new List<PEActionType>();
			string[] array5 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("EndImmediately")).Split(',');
			string[] array6 = array5;
			foreach (string text3 in array6)
			{
				if ("0" != text3)
				{
					actionRelationData.m_EndImmediately.Add((PEActionType)Convert.ToInt32(text3));
				}
			}
			if ((int)actionRelationData.m_ActionType < m_Relations.Length)
			{
				m_Relations[(int)actionRelationData.m_ActionType] = actionRelationData;
			}
		}
	}
}
