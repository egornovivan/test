using System;
using UnityEngine;

namespace DunGen;

[AddComponentMenu("DunGen/Random Props/Random Prefab")]
public class RandomPrefab : RandomProp
{
	public GameObjectChanceTable Props = new GameObjectChanceTable();

	public override void Process(System.Random randomStream, Tile tile)
	{
		if (Props.Weights.Count > 0)
		{
			GameObjectChance random = Props.GetRandom(randomStream, tile.Placement.IsOnMainPath, tile.Placement.NormalizedDepth, null, allowImmediateRepeats: true, removeFromTable: true);
			GameObject value = random.Value;
			GameObject gameObject = UnityEngine.Object.Instantiate(value);
			gameObject.transform.parent = base.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			RandomPrefab[] componentsInChildren = gameObject.GetComponentsInChildren<RandomPrefab>();
			foreach (RandomPrefab randomPrefab in componentsInChildren)
			{
				randomPrefab.Process(randomStream, tile);
			}
		}
	}
}
