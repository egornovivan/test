using System.Collections;

namespace UnitySteer;

public class BruteForceProximityDatabase : AbstractProximityDatabase
{
	public ArrayList group;

	public BruteForceProximityDatabase()
	{
		group = new ArrayList();
	}

	public override AbstractTokenForProximityDatabase allocateToken(SteeringVehicle parentObject)
	{
		return new tokenType(parentObject, this);
	}

	public override void RemoveToken(AbstractTokenForProximityDatabase token)
	{
		group.Remove(token);
	}

	public override int getPopulation()
	{
		return group.Count;
	}
}
