public class ColonyPPFusion : ColonyPPCoal
{
	private float autoPercent = 0.2f;

	private int autoCount = 15;

	public override float AutoPercent => autoPercent;

	public override int AutoCount => autoCount;

	public override CSPPCoalInfo Info => CSPPCoalInfo.ppFusionInfo;

	public ColonyPPFusion(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSPPFusionData();
		_MyData = (CSPPFusionData)_RecordData;
		_SubData = (CSPPFusionData)_MyData;
		LoadData();
	}
}
