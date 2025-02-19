using System;
using System.Collections;
using UnityEngine;

public class SPPlayerSummon : MonoBehaviour
{
	[Serializable]
	public class CaelumRexData
	{
		public int pathID;

		public float interval;

		public float minRadius;

		public float maxRadius;

		public float probability;
	}

	[Serializable]
	public class PlayerSleepData
	{
		public int minCount;

		public int maxCount;

		public float interval;

		public float minRadius;

		public float maxRadius;

		public float probability;
	}

	public CaelumRexData caelumrex;

	public PlayerSleepData sleep;

	private void Start()
	{
		StartCoroutine(Spawn(sleep));
		StartCoroutine(Spawn(caelumrex));
	}

	private IEnumerator Spawn(CaelumRexData data)
	{
		while (true)
		{
			if (IsCaelumRexReady(data))
			{
			}
			yield return new WaitForSeconds(data.interval);
		}
	}

	private bool IsCaelumRexReady(CaelumRexData data)
	{
		if (data.pathID == 0)
		{
			return false;
		}
		return true;
	}

	private Vector3 GetCaelumRexPosition(Vector3 center, float minRange, float maxRange)
	{
		Vector2 vector = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(minRange, maxRange);
		return center + new Vector3(vector.x, 0f, vector.y) + Vector3.up * UnityEngine.Random.Range(5, 10);
	}

	private void OnCaelumRexSpawned(GameObject obj)
	{
		if (!(obj == null))
		{
		}
	}

	private IEnumerator Spawn(PlayerSleepData data)
	{
		while (true)
		{
			if (IsPlayerSleeping(data))
			{
				Vector3 position = Vector3.zero;
				Quaternion rot = Quaternion.identity;
				int pathID = 0;
				int typeID = (int)AiUtil.GetPointType(position);
				if (Application.loadedLevelName.Equals("GameStory"))
				{
					pathID = AISpawnDataStory.GetRandomPathIDFromType(typeID, position);
				}
				else if (Application.loadedLevelName.Equals("GameAdventure"))
				{
					int mapID = AiUtil.GetMapID(position);
					int areaID = AiUtil.GetAreaID(position);
					pathID = AISpawnDataAdvSingle.GetPathID(mapID, areaID, typeID);
				}
				AIResource.Instantiate(pathID, position, rot, OnSleepSpawned);
			}
			yield return new WaitForSeconds(data.interval);
		}
	}

	private Vector3 GetSpawnPosition(Vector3 center, float minRange, float maxRange)
	{
		return AiUtil.GetRandomPosition(center, minRange, maxRange, 15f, AiUtil.groundedLayer, 10);
	}

	private bool IsPlayerSleeping(PlayerSleepData data)
	{
		if (data.minCount == 0 && data.maxCount == 0)
		{
			return false;
		}
		if (UnityEngine.Random.value > data.probability)
		{
			return false;
		}
		return true;
	}

	private void OnSleepSpawned(GameObject obj)
	{
	}
}
