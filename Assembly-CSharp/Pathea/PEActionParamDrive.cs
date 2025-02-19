using WhiteCat;

namespace Pathea;

public class PEActionParamDrive : PEActionParam
{
	public CarrierController controller;

	public int seatIndex;

	private static PEActionParamDrive gParam = new PEActionParamDrive();

	public static PEActionParamDrive param => gParam;

	private PEActionParamDrive()
	{
	}
}
