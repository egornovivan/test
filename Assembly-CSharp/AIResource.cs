using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class AIResource
{
	public int id;

	public int aiId;

	public float height;

	public float minScale;

	public float maxScale;

	public string name;

	public string path;

	private static List<AIResource> m_data = new List<AIResource>();

	public static AIResource Find(int argId)
	{
		return m_data.Find((AIResource ret) => ret.id == argId);
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("path");
		while (sqliteDataReader.Read())
		{
			AIResource aIResource = new AIResource();
			aIResource.id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			aIResource.aiId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("aiid")));
			aIResource.name = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("name"));
			aIResource.path = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("path"));
			aIResource.height = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("height")));
			aIResource.minScale = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("minscale")));
			aIResource.maxScale = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("maxscale")));
			if (Find(aIResource.id) != null)
			{
				Debug.LogError("Can't have the same id!");
			}
			else
			{
				m_data.Add(aIResource);
			}
		}
	}

	public static bool IsGroup(int pathid)
	{
		return Find(pathid)?.path.StartsWith("Group") ?? false;
	}

	public static Vector3 FixedHeightOfAIResource(int id, Vector3 position)
	{
		AIResource aIResource = Find(id);
		if (aIResource == null)
		{
			return Vector3.zero;
		}
		Vector3 position2 = position;
		if (FixedHeight(aIResource, ref position2))
		{
			return position2;
		}
		return Vector3.zero;
	}

	private static bool FixedHeight(AIResource res, ref Vector3 position)
	{
		if (res == null)
		{
			return false;
		}
		if (res.height > -1E-45f && res.height < float.Epsilon)
		{
			return true;
		}
		if (res.height > float.Epsilon)
		{
			position += Vector3.up * res.height;
			return true;
		}
		if (AiUtil.CheckPositionUnderWater(position, out var waterHeight))
		{
			float num = waterHeight;
			if (AiUtil.CheckPositionInCave(position, out var point))
			{
				num = point.y;
			}
			if (position.y < num + res.height)
			{
				position += Vector3.up * UnityEngine.Random.Range(0f, num + res.height - position.y);
				return true;
			}
		}
		Debug.LogWarning("Can't find right spawn point!");
		return false;
	}

	public static AssetReq Instantiate(int id, Vector3 position, Quaternion rot, AssetReq.ReqFinishDelegate onSpawned = null)
	{
		if (position == Vector3.zero)
		{
			return null;
		}
		AIResource aIResource = Find(id);
		if (aIResource != null)
		{
			AssetReq assetReq = AssetsLoader.Instance.AddReq(aIResource.path, position, rot, Vector3.one);
			assetReq.ReqFinishHandler += onSpawned;
			assetReq.ReqFinishHandler += aIResource.OnSpawnedObject;
			return assetReq;
		}
		return null;
	}

	public void OnSpawnedObject(GameObject obj)
	{
		if (!(obj == null) && GameConfig.IsMultiMode)
		{
		}
	}
}
