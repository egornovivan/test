using UnityEngine;

namespace Pathea;

public class PEActionParamVQ : PEActionParam
{
	public Vector3 vec;

	public Quaternion q;

	private static PEActionParamVQ gParam = new PEActionParamVQ();

	public static PEActionParamVQ param => gParam;

	private PEActionParamVQ()
	{
	}
}
