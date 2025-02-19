namespace Pathea;

public class PEActionParamB : PEActionParam
{
	public bool b;

	private static PEActionParamB gParam = new PEActionParamB();

	public static PEActionParamB param => gParam;

	private PEActionParamB()
	{
	}
}
