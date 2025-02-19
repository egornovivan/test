using System.Collections;
using UnityEngine;

public class SPHatch : SPGroup
{
	public int id;

	public int minCount;

	public int maxCount;

	public float radius;

	public float delayTime;

	private AIResource res;

	public override IEnumerator SpawnGroup()
	{
		if (res == null)
		{
			yield break;
		}
		yield return new WaitForSeconds(delayTime);
		int count = Random.Range(minCount, maxCount);
		for (int i = 0; i < count; i++)
		{
			Vector3 position = base.transform.position + Random.insideUnitSphere * radius;
			if (AiUtil.CheckPositionOnGround(ref position, 10f, AiUtil.groundedLayer))
			{
				AIResource.Instantiate(id, position, Quaternion.identity, OnSpawned);
				yield return new WaitForSeconds(0.1f);
			}
		}
		yield return new WaitForSeconds(0.5f);
	}

	private void OnSpawned(GameObject go)
	{
		if (!(go == null))
		{
		}
	}

	private void Awake()
	{
		res = AIResource.Find(id);
	}
}
