using System.Collections.Generic;

namespace Pathea.Operate;

public class PETable : Operation_Multiple
{
	public PEEat[] eats;

	public override List<Operation_Single> Singles => new List<Operation_Single>(eats);
}
