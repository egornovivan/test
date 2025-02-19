using UnityEngine;

namespace Pathea;

public class PEActionParamVQSN : PEActionParam
{
	public Vector3 vec;

	public Quaternion q;

	public string str;

	public int n;

	private static PEActionParamVQSN gParam = new PEActionParamVQSN();

	public static PEActionParamVQSN param => gParam;

	private PEActionParamVQSN()
	{
	}
}
