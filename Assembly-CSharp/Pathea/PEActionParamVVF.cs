using UnityEngine;

namespace Pathea;

public class PEActionParamVVF : PEActionParam
{
	public Vector3 vec1;

	public Vector3 vec2;

	public float f;

	private static PEActionParamVVF gParam = new PEActionParamVVF();

	public static PEActionParamVVF param => gParam;

	private PEActionParamVVF()
	{
	}
}
