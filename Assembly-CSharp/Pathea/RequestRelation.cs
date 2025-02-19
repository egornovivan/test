using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

namespace Pathea;

public class RequestRelation
{
	public int id;

	public string name;

	public int[] relations;

	private static List<RequestRelation> m_RelationList = new List<RequestRelation>();

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AI_ActionRelation");
		int num = sqliteDataReader.FieldCount - 2;
		while (sqliteDataReader.Read())
		{
			RequestRelation relation = new RequestRelation();
			relation.relations = new int[num];
			relation.id = sqliteDataReader.GetInt32(0);
			relation.name = sqliteDataReader.GetString(1);
			for (int i = 0; i < relation.relations.Length; i++)
			{
				relation.relations[i] = sqliteDataReader.GetInt32(i + 2);
			}
			RequestRelation requestRelation = m_RelationList.Find((RequestRelation ret) => ret.id == relation.id || ret.name == relation.name);
			if (requestRelation != null)
			{
				Debug.LogError("The same relation ID = " + relation.id + " is exist!");
			}
			m_RelationList.Add(relation);
		}
	}

	public static int GetRelationValue(int src, int dst)
	{
		RequestRelation requestRelation = m_RelationList.Find((RequestRelation ret) => ret.id == src);
		if (requestRelation == null)
		{
			Debug.LogError("Can't find relation data: " + src);
			return 0;
		}
		if (dst < 0 || dst >= requestRelation.relations.Length)
		{
			Debug.LogError("Can't find relation : " + dst);
			return 0;
		}
		return requestRelation.relations[dst];
	}

	public static int GetRelationValue(string srcName, string dstName)
	{
		RequestRelation requestRelation = m_RelationList.Find((RequestRelation ret) => ret.name == srcName);
		if (requestRelation == null)
		{
			Debug.LogError("Can't find relation data: " + srcName);
			return 0;
		}
		int num = Name2ID(dstName);
		if (num < 0 || num >= requestRelation.relations.Length)
		{
			Debug.LogError("Can't find relation : " + num);
			return 0;
		}
		return requestRelation.relations[num];
	}

	public static int Name2ID(string name)
	{
		return m_RelationList.Find((RequestRelation ret) => ret.name == name)?.id ?? (-1);
	}

	public static string ID2Name(int id)
	{
		RequestRelation requestRelation = m_RelationList.Find((RequestRelation ret) => ret.id == id);
		return (requestRelation == null) ? string.Empty : requestRelation.name;
	}

	public static bool Contains(string name)
	{
		return m_RelationList.Find((RequestRelation ret) => ret.name == name) != null;
	}

	public static bool Contains(int id)
	{
		return m_RelationList.Find((RequestRelation ret) => ret.id == id) != null;
	}
}
