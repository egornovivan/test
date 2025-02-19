using Pathea;

namespace ItemAsset;

public class ReplicatorFormula : Cmpt
{
	public int[] formulaId => itemObj.protoData.replicatorFormulaIds;

	public override string ProcessTooltip(string text)
	{
		PeEntity mainPlayer = PeSingleton<PeCreature>.Instance.mainPlayer;
		if (null != mainPlayer)
		{
			ReplicatorCmpt cmpt = mainPlayer.GetCmpt<ReplicatorCmpt>();
			if (cmpt.replicator.GetKnownFormula(formulaId[0]) == null)
			{
				return base.ProcessTooltip(text);
			}
			return text + PELocalization.GetString(4000001);
		}
		return base.ProcessTooltip(text);
	}

	private static string GetKnownScriptTooltip()
	{
		return PELocalization.GetString(4000001);
	}
}
