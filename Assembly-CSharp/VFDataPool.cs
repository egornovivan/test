using System.Collections.Generic;

public class VFDataPool
{
	private Stack<byte[]> _items;

	private object _sync = new object();

	private int _size;

	public VFDataPool(int arraySize, int preserved = 8)
	{
		_size = arraySize;
		_items = new Stack<byte[]>(preserved);
		for (int i = 0; i < preserved; i++)
		{
			_items.Push(new byte[_size]);
		}
	}

	public byte[] Get()
	{
		lock (_sync)
		{
			if (_items.Count == 0)
			{
				return new byte[_size];
			}
			return _items.Pop();
		}
	}

	public void Free(byte[] item)
	{
		lock (_sync)
		{
			_items.Push(item);
		}
	}
}
