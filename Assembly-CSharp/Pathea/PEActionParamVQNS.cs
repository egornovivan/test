using UnityEngine;

namespace Pathea;

public class PEActionParamVQNS : PEActionParam
{
	public Vector3 vec;

	public Quaternion q;

	public int n;

	public string str;

	private static PEActionParamVQNS gParam = new PEActionParamVQNS();

	public static PEActionParamVQNS param => gParam;

	private PEActionParamVQNS()
	{
	}
}
