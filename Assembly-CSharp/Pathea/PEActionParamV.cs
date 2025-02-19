using UnityEngine;

namespace Pathea;

public class PEActionParamV : PEActionParam
{
	public Vector3 vec;

	private static PEActionParamV gParam = new PEActionParamV();

	public static PEActionParamV param => gParam;

	private PEActionParamV()
	{
	}
}
