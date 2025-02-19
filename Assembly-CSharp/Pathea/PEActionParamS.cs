namespace Pathea;

public class PEActionParamS : PEActionParam
{
	public string str;

	private static PEActionParamS gParam = new PEActionParamS();

	public static PEActionParamS param => gParam;

	private PEActionParamS()
	{
	}
}
