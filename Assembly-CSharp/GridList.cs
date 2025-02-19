using System.Collections.Generic;

public class GridList
{
	public List<GridInfo> OreList;

	public List<GridInfo> HerbList;

	public List<GridInfo> OtherList;

	public GridList()
	{
		OreList = new List<GridInfo>();
		HerbList = new List<GridInfo>();
		OtherList = new List<GridInfo>();
	}

	public void ClearList()
	{
		OreList.Clear();
		HerbList.Clear();
		OtherList.Clear();
	}
}
