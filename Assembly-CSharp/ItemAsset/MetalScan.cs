using Pathea;

namespace ItemAsset;

public class MetalScan : Cmpt
{
	public int[] metalIds => itemObj.protoData.replicatorFormulaIds;

	public override string ProcessTooltip(string text)
	{
		PeEntity mainPlayer = PeSingleton<PeCreature>.Instance.mainPlayer;
		if (null != mainPlayer)
		{
			if (!MetalScanData.HasMetal(metalIds[0]))
			{
				return base.ProcessTooltip(text);
			}
			int length = text.LastIndexOf("\\n");
			string text2 = text.Substring(0, length);
			return text2 + PELocalization.GetString(4000001);
		}
		return base.ProcessTooltip(text);
	}
}
