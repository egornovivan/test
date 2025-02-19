using System.Collections.Generic;

public class GenericArrayPool<T>
{
	private Stack<T[]> _items;

	private object _sync = new object();

	private int _size;

	public GenericArrayPool(int arraySize, int preserved = 8)
	{
		_size = arraySize;
		_items = new Stack<T[]>(preserved);
		for (int i = 0; i < preserved; i++)
		{
			_items.Push(new T[_size]);
		}
	}

	public T[] Get()
	{
		lock (_sync)
		{
			if (_items.Count == 0)
			{
				return new T[_size];
			}
			return _items.Pop();
		}
	}

	public void Free(T[] item)
	{
		lock (_sync)
		{
			_items.Push(item);
		}
	}
}
