using System.Collections.Generic;

namespace Pathea.Operate;

public class PEPatients : Operation_Multiple
{
	public PELay[] Lays;

	public override List<Operation_Single> Singles => new List<Operation_Single>(Lays);
}
