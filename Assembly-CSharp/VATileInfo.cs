using System.Collections.Generic;

public class VATileInfo
{
	public List<VArtifactUnit> unitList;

	public VArtifactTown town;

	public VATileInfo(List<VArtifactUnit> unitList, VArtifactTown town)
	{
		this.unitList = unitList;
		this.town = town;
	}

	public void AddUnit(VArtifactUnit unit)
	{
		if (unitList == null)
		{
			unitList = new List<VArtifactUnit>();
		}
		unitList.Add(unit);
	}
}
