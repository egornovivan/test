using CSRecord;

public class ColonyBase
{
	internal ColonyNetwork _Network;

	public CSObjectData _RecordData;

	public void SetNetwork(ColonyNetwork network)
	{
		if (!(network == null))
		{
			_Network = network;
			ColonyMgr.AddColonyItem(this);
		}
	}

	public void RecycleItems()
	{
		_Network.RPCServer(EPacketType.PT_CL_Recycle);
	}

	public void Repair()
	{
		_Network.RPCServer(EPacketType.PT_CL_Repair);
	}
}
