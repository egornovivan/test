using System.Collections.Generic;

namespace Pathea.Operate;

public class PETrainner : Operation_Multiple
{
	public PEPractice[] Instructors;

	public override List<Operation_Single> Singles => new List<Operation_Single>(Instructors);
}
