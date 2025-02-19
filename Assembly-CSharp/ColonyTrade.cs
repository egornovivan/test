using CSRecord;

internal class ColonyTrade : ColonyBase
{
	private CSTradeData _MyData;

	public ColonyTrade(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSTradeData();
		_MyData = (CSTradeData)_RecordData;
	}
}
