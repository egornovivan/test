using System.Collections.Generic;

namespace Pathea.Operate;

public class PEMachine : Operation_Multiple
{
	public PEWork[] works;

	public override List<Operation_Single> Singles => new List<Operation_Single>(works);
}
