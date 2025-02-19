namespace Pathfinding;

public interface IUpdatableGraph
{
	int UpdateArea(GraphUpdateObject o);

	void UpdateAreaInit(GraphUpdateObject o);

	GraphUpdateThreading CanUpdateAsync(GraphUpdateObject o);
}
