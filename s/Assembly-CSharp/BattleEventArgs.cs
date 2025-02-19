using System;

internal class BattleEventArgs : EventArgs
{
	internal int _meat;

	internal float _point;

	internal BattleEventArgs(float point, int meat)
	{
		_point = point;
		_meat = meat;
	}
}
