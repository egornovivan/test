using UnityEngine;

namespace Pathea;

public class PEActionParamNVB : PEActionParam
{
	public int n;

	public Vector3 vec;

	public bool b;

	private static PEActionParamNVB gParam = new PEActionParamNVB();

	public static PEActionParamNVB param => gParam;

	private PEActionParamNVB()
	{
	}
}
