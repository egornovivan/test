using UnityEngine;

namespace Pathea;

public class PEActionParamFVFS : PEActionParam
{
	public float f1;

	public Vector3 vec;

	public float f2;

	public string str;

	private static PEActionParamFVFS gParam = new PEActionParamFVFS();

	public static PEActionParamFVFS param => gParam;

	private PEActionParamFVFS()
	{
	}
}
