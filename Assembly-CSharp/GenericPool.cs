using System.Collections.Generic;

public class GenericPool<T> where T : new()
{
	private Stack<T> _items;

	private object _sync = new object();

	public GenericPool(int preserved = 8)
	{
		_items = new Stack<T>(preserved);
		for (int i = 0; i < preserved; i++)
		{
			_items.Push(new T());
		}
	}

	public T Get()
	{
		lock (_sync)
		{
			if (_items.Count == 0)
			{
				return new T();
			}
			return _items.Pop();
		}
	}

	public void Free(T item)
	{
		lock (_sync)
		{
			_items.Push(item);
		}
	}
}
