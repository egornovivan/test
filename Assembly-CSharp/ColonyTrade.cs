using CSRecord;

internal class ColonyTrade : ColonyBase
{
	public ColonyTrade(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSTradeData();
	}
}
