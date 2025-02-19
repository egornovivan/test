using UnityEngine;

namespace Pathea;

public class PEActionParamVBB : PEActionParam
{
	public Vector3 vec;

	public bool b1;

	public bool b2;

	private static PEActionParamVBB gParam = new PEActionParamVBB();

	public static PEActionParamVBB param => gParam;

	private PEActionParamVBB()
	{
	}
}
