using System;

public class LoadFileEventArgs : EventArgs
{
	private ulong _hashCode;

	internal ulong HashCode => _hashCode;

	internal LoadFileEventArgs(ulong hashCode)
	{
		_hashCode = hashCode;
	}
}
