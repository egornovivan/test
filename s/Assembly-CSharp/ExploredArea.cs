public class ExploredArea
{
	public int Index;

	public int TeamId;

	public ExploredArea(int index, int teamId)
	{
		Index = index;
		TeamId = teamId;
	}

	public override string ToString()
	{
		return $"Index:{Index}, TeamId:{TeamId}";
	}
}
