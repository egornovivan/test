using UnityEngine;

namespace Pathea;

public class PEActionParamVQS : PEActionParam
{
	public Vector3 vec;

	public Quaternion q;

	public string str;

	private static PEActionParamVQS gParam = new PEActionParamVQS();

	public static PEActionParamVQS param => gParam;

	private PEActionParamVQS()
	{
	}
}
