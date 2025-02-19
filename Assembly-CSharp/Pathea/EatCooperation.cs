using System;

namespace Pathea;

public class EatCooperation : Cooperation
{
	public EatCooperation(int memNum)
		: base(memNum)
	{
	}

	public override void DissolveCooper()
	{
		throw new NotImplementedException();
	}
}
