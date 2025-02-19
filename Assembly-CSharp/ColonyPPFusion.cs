using CSRecord;

public class ColonyPPFusion : ColonyPPCoal
{
	public ColonyPPFusion(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSPPFusionData();
		_MyData = (CSPPFusionData)_RecordData;
	}
}
