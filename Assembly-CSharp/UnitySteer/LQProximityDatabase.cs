using System;
using System.Collections;
using UnityEngine;

namespace UnitySteer;

public class LQProximityDatabase : AbstractProximityDatabase
{
	public class tokenType : AbstractTokenForProximityDatabase
	{
		private lqClientProxy proxy;

		private locationQueryDatabase lq;

		public tokenType(object parentObject, LQProximityDatabase lqsd)
		{
			proxy = new lqClientProxy(parentObject);
			lq = lqsd.lq;
		}

		~tokenType()
		{
			lq.lqRemoveFromBin(proxy);
		}

		public override void updateForNewPosition(Vector3 p)
		{
			lq.lqUpdateForNewLocation(proxy, p.x, p.y, p.z);
		}

		public override void findNeighbors(Vector3 center, float radius, ArrayList results)
		{
			ArrayList allObjectsInLocality = lq.getAllObjectsInLocality(center.x, center.y, center.z, radius);
			for (int i = 0; i < allObjectsInLocality.Count; i++)
			{
				lqClientProxy lqClientProxy2 = (lqClientProxy)allObjectsInLocality[i];
				results.Add((SteeringVehicle)lqClientProxy2.clientObject);
			}
		}
	}

	private locationQueryDatabase lq;

	public LQProximityDatabase(Vector3 center, Vector3 dimensions, Vector3 divisions)
	{
		Vector3 vector = dimensions * 0.5f;
		Vector3 vector2 = center - vector;
		lq = new locationQueryDatabase(vector2.x, vector2.y, vector2.z, dimensions.x, dimensions.y, dimensions.z, (int)Math.Round(divisions.x), (int)Math.Round(divisions.y), (int)Math.Round(divisions.z));
	}

	~LQProximityDatabase()
	{
	}

	public override AbstractTokenForProximityDatabase allocateToken(SteeringVehicle parentObject)
	{
		return new tokenType(parentObject, this);
	}

	public override int getPopulation()
	{
		return lq.getAllObjects().Count;
	}

	public override SteeringVehicle getNearestVehicle(Vector3 position, float radius)
	{
		lqClientProxy lqClientProxy2 = lq.lqFindNearestNeighborWithinRadius(position.x, position.y, position.z, radius, null);
		SteeringVehicle result = null;
		if (lqClientProxy2 != null)
		{
			result = (SteeringVehicle)lqClientProxy2.clientObject;
		}
		return result;
	}

	public override Vector3 getMostPopulatedBinCenter()
	{
		return lq.getMostPopulatedBinCenter();
	}
}
