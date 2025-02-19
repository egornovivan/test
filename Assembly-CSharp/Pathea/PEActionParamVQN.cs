using UnityEngine;

namespace Pathea;

public class PEActionParamVQN : PEActionParam
{
	public Vector3 vec;

	public Quaternion q;

	public int n;

	private static PEActionParamVQN gParam = new PEActionParamVQN();

	public static PEActionParamVQN param => gParam;

	private PEActionParamVQN()
	{
	}
}
