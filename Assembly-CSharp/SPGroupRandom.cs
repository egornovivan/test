using System.Collections;
using UnityEngine;

public class SPGroupRandom : SPGroup
{
	public int id;

	public int minCount;

	public int maxCount;

	public float radius;

	public float rejectRadius;

	public bool ignoreTerrain;

	private AIResource res;

	private bool IsValid(Vector3 position)
	{
		AiObject[] componentsInChildren = base.transform.GetComponentsInChildren<AiObject>();
		AiObject[] array = componentsInChildren;
		foreach (AiObject aiObject in array)
		{
			if (AiUtil.SqrMagnitudeH(aiObject.position - position) < rejectRadius * rejectRadius)
			{
				return false;
			}
		}
		return true;
	}

	private Vector3 GetCorrectPosition()
	{
		if (!ignoreTerrain)
		{
			for (int i = 0; i < 5; i++)
			{
				Vector3 randomPosition = AiUtil.GetRandomPosition(base.transform.position, 0f, radius, 200f, AiUtil.groundedLayer, 5);
				if (IsValid(randomPosition))
				{
					return randomPosition;
				}
			}
		}
		else
		{
			for (int j = 0; j < 5; j++)
			{
				Vector3 vector = base.transform.position + Random.insideUnitSphere * radius;
				if (IsValid(vector))
				{
					return vector;
				}
			}
		}
		return Vector3.zero;
	}

	public override IEnumerator SpawnGroup()
	{
		if (res == null)
		{
			yield break;
		}
		int count = Random.Range(minCount, maxCount);
		for (int i = 0; i < count; i++)
		{
			Vector3 position = GetCorrectPosition();
			if (!(position == Vector3.zero))
			{
				Instantiate(id, position, Quaternion.identity);
				yield return new WaitForSeconds(0.1f);
			}
		}
		yield return new WaitForSeconds(0.5f);
	}

	private void Awake()
	{
		res = AIResource.Find(id);
	}
}
