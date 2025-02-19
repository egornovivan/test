namespace Pathea;

public class PEActionParamN : PEActionParam
{
	public int n;

	private static PEActionParamN gParam = new PEActionParamN();

	public static PEActionParamN param => gParam;

	private PEActionParamN()
	{
	}
}
