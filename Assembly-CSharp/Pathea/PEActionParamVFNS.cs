using UnityEngine;

namespace Pathea;

public class PEActionParamVFNS : PEActionParam
{
	public Vector3 vec;

	public float f;

	public int n;

	public string str;

	private static PEActionParamVFNS gParam = new PEActionParamVFNS();

	public static PEActionParamVFNS param => gParam;

	private PEActionParamVFNS()
	{
	}
}
