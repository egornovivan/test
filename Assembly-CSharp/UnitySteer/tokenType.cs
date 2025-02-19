using System.Collections;
using UnityEngine;

namespace UnitySteer;

public class tokenType : AbstractTokenForProximityDatabase
{
	private BruteForceProximityDatabase bfpd;

	private SteeringVehicle tParentObject;

	private Vector3 position;

	public tokenType(SteeringVehicle parentObject, BruteForceProximityDatabase pd)
	{
		bfpd = pd;
		tParentObject = parentObject;
		bfpd.group.Add(this);
	}

	~tokenType()
	{
		bfpd.group.Remove(this);
	}

	public override void updateForNewPosition(Vector3 newPosition)
	{
		position = newPosition;
	}

	public override void findNeighbors(Vector3 center, float radius, ArrayList results)
	{
		float num = radius * radius;
		for (int i = 0; i < bfpd.group.Count; i++)
		{
			tokenType tokenType2 = (tokenType)bfpd.group[i];
			float sqrMagnitude = (center - tokenType2.position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				results.Add(tokenType2.tParentObject);
			}
		}
	}
}
