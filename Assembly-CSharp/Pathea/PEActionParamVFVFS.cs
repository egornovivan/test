using UnityEngine;

namespace Pathea;

public class PEActionParamVFVFS : PEActionParam
{
	public Vector3 vec1;

	public float f1;

	public Vector3 vec2;

	public float f2;

	public string str;

	private static PEActionParamVFVFS gParam = new PEActionParamVFVFS();

	public static PEActionParamVFVFS param => gParam;

	private PEActionParamVFVFS()
	{
	}
}
