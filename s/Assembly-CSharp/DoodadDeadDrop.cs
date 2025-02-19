using System;
using uLink;

public class DoodadDeadDrop : IDoodad
{
	public int _dropCount;

	public override void Create(MapObjNetwork net, NetworkMessageInfo info)
	{
		base.Create(net, info);
		string value = info.networkView.initialData.Read<string>(new object[0]);
		_dropCount = Convert.ToInt32(value);
		if (_net._owner != null && _dropCount != 0)
		{
			DeadDropItem(_dropCount);
		}
		_net.DestroyByTimeOut();
	}

	public void DeadDropItem(int dropCount)
	{
		if (!(_net._owner == null))
		{
			_net._owner.DeadDropItem(_net, dropCount);
		}
	}

	public override void Insert(BitStream stream, NetworkMessageInfo info)
	{
	}
}
