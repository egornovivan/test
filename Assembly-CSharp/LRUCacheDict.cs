using System.Collections.Generic;

public class LRUCacheDict<TKey, TValue>
{
	private Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> _dict = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();

	private LinkedList<KeyValuePair<TKey, TValue>> _list = new LinkedList<KeyValuePair<TKey, TValue>>();

	private TKey _lastRemovedKey;

	public TKey LastRemoved => _lastRemovedKey;

	public int MaxSize { get; set; }

	public LRUCacheDict(int maxsize = 1)
	{
		MaxSize = maxsize;
	}

	public void Add(TKey key, TValue value)
	{
		lock (_dict)
		{
			if (_dict.TryGetValue(key, out var value2))
			{
				_list.Remove(value2);
				_list.AddFirst(value2);
			}
			else
			{
				value2 = new LinkedListNode<KeyValuePair<TKey, TValue>>(new KeyValuePair<TKey, TValue>(key, value));
				_dict.Add(key, value2);
				_list.AddFirst(value2);
			}
			_lastRemovedKey = default(TKey);
			if (_dict.Count > MaxSize)
			{
				LinkedListNode<KeyValuePair<TKey, TValue>> last = _list.Last;
				if (last != null)
				{
					_lastRemovedKey = last.Value.Key;
					Remove(_lastRemovedKey);
				}
			}
		}
	}

	public bool Remove(TKey key)
	{
		lock (_dict)
		{
			if (_dict.TryGetValue(key, out var value))
			{
				_dict.Remove(key);
				_list.Remove(value);
				return true;
			}
			return false;
		}
	}

	public void Clear()
	{
		lock (_dict)
		{
			_dict.Clear();
			_list.Clear();
		}
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		lock (_dict)
		{
			LinkedListNode<KeyValuePair<TKey, TValue>> value2;
			bool result = _dict.TryGetValue(key, out value2);
			if (value2 != null)
			{
				value = value2.Value.Value;
				_list.Remove(value2);
				_list.AddFirst(value2);
			}
			else
			{
				value = default(TValue);
			}
			return result;
		}
	}
}
