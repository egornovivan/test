using UnityEngine;

namespace Pathea;

public class PEActionParamVQSNS : PEActionParam
{
	public Vector3 vec;

	public Quaternion q;

	public string strAnima;

	public int enitytID;

	public string boneStr;

	private static PEActionParamVQSNS gParam = new PEActionParamVQSNS();

	public static PEActionParamVQSNS param => gParam;

	private PEActionParamVQSNS()
	{
	}
}
