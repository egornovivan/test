using System.Collections.Generic;

namespace Pathea.Operate;

public class PEBed : Operation_Multiple
{
	public PESleep[] sleeps;

	public override List<Operation_Single> Singles => (sleeps != null && sleeps.Length != 0) ? new List<Operation_Single>(sleeps) : null;
}
