using System.Collections.Generic;

namespace Pathea.Operate;

public class PESeat : Operation_Multiple
{
	public PESit[] sits;

	public override List<Operation_Single> Singles => (sits != null && sits.Length != 0) ? new List<Operation_Single>(sits) : null;
}
