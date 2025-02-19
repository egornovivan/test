using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DunGen;

[Serializable]
public class GameObjectChanceTable
{
	public List<GameObjectChance> Weights = new List<GameObjectChance>();

	public GameObjectChanceTable Clone()
	{
		GameObjectChanceTable gameObjectChanceTable = new GameObjectChanceTable();
		foreach (GameObjectChance weight in Weights)
		{
			gameObjectChanceTable.Weights.Add(new GameObjectChance(weight.Value, weight.MainPathWeight, weight.BranchPathWeight, weight.TileSet)
			{
				UseDepthScale = weight.UseDepthScale,
				DepthWeightScale = weight.DepthWeightScale
			});
		}
		return gameObjectChanceTable;
	}

	public bool ContainsGameObject(GameObject obj)
	{
		foreach (GameObjectChance weight in Weights)
		{
			if (weight.Value == obj)
			{
				return true;
			}
		}
		return false;
	}

	public GameObjectChance GetRandom(System.Random random, bool isOnMainPath, float normalizedDepth, GameObject previouslyChosen, bool allowImmediateRepeats, bool removeFromTable = false)
	{
		float num = 0f;
		foreach (GameObjectChance weight2 in Weights)
		{
			if (allowImmediateRepeats || previouslyChosen == null || weight2.Value != previouslyChosen)
			{
				num += weight2.GetWeight(isOnMainPath, normalizedDepth);
			}
		}
		float num2 = (float)(random.NextDouble() * (double)num);
		foreach (GameObjectChance weight3 in Weights)
		{
			if (weight3.Value == previouslyChosen && Weights.Count > 1 && !allowImmediateRepeats)
			{
				continue;
			}
			float weight = weight3.GetWeight(isOnMainPath, normalizedDepth);
			if (num2 < weight)
			{
				if (removeFromTable)
				{
					Weights.Remove(weight3);
				}
				return weight3;
			}
			num2 -= weight;
		}
		return null;
	}

	public static GameObject GetCombinedRandom(System.Random random, bool isOnMainPath, float normalizedDepth, params GameObjectChanceTable[] tables)
	{
		float num = tables.SelectMany((GameObjectChanceTable x) => x.Weights.Select((GameObjectChance y) => y.GetWeight(isOnMainPath, normalizedDepth))).Sum();
		float num2 = (float)(random.NextDouble() * (double)num);
		foreach (GameObjectChance item in tables.SelectMany((GameObjectChanceTable x) => x.Weights))
		{
			float weight = item.GetWeight(isOnMainPath, normalizedDepth);
			if (num2 < weight)
			{
				return item.Value;
			}
			num2 -= weight;
		}
		return null;
	}

	public static GameObjectChanceTable Combine(params GameObjectChanceTable[] tables)
	{
		GameObjectChanceTable gameObjectChanceTable = new GameObjectChanceTable();
		foreach (GameObjectChanceTable gameObjectChanceTable2 in tables)
		{
			foreach (GameObjectChance weight in gameObjectChanceTable2.Weights)
			{
				gameObjectChanceTable.Weights.Add(new GameObjectChance(weight.Value, weight.MainPathWeight, weight.BranchPathWeight, weight.TileSet)
				{
					UseDepthScale = weight.UseDepthScale,
					DepthWeightScale = weight.DepthWeightScale
				});
			}
		}
		return gameObjectChanceTable;
	}
}
