using UnityEngine;

namespace Pathea;

public class PEActionParamVVN : PEActionParam
{
	public Vector3 vec1;

	public Vector3 vec2;

	public int n;

	private static PEActionParamVVN gParam = new PEActionParamVVN();

	public static PEActionParamVVN param => gParam;

	private PEActionParamVVN()
	{
	}
}
