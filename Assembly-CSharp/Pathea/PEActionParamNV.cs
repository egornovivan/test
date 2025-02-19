using UnityEngine;

namespace Pathea;

public class PEActionParamNV : PEActionParam
{
	public int n;

	public Vector3 vec;

	private static PEActionParamNV gParam = new PEActionParamNV();

	public static PEActionParamNV param => gParam;

	private PEActionParamNV()
	{
	}
}
