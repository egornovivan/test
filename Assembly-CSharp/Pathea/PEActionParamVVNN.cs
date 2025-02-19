using UnityEngine;

namespace Pathea;

public class PEActionParamVVNN : PEActionParam
{
	public Vector3 vec1;

	public Vector3 vec2;

	public int n1;

	public int n2;

	private static PEActionParamVVNN gParam = new PEActionParamVVNN();

	public static PEActionParamVVNN param => gParam;

	private PEActionParamVVNN()
	{
	}
}
