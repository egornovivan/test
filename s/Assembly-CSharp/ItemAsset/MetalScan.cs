namespace ItemAsset;

public class MetalScan : Cmpt
{
	public int[] metalIds => itemObj.protoData.replicatorFormulaIds;
}
