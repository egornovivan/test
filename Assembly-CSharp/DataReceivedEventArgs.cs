using System;

public class DataReceivedEventArgs : EventArgs
{
	private byte[] _data;

	private int _length;

	public byte[] Data => _data;

	public int Length => _length;

	public DataReceivedEventArgs(byte[] data, int length)
	{
		_data = data;
		_length = length;
	}
}
