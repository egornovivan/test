using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class BiLookup<TFirst, TSecond>
{
	private object obj4Lock = new object();

	private Dictionary<TFirst, TSecond> firstToSecond = new Dictionary<TFirst, TSecond>();

	private Dictionary<TSecond, TFirst> secondToFirst = new Dictionary<TSecond, TFirst>();

	private List<TFirst> indexByFirst = new List<TFirst>(1024);

	public TSecond this[TFirst first]
	{
		get
		{
			lock (obj4Lock)
			{
				return firstToSecond[first];
			}
		}
	}

	public TFirst this[TSecond second]
	{
		get
		{
			lock (obj4Lock)
			{
				return secondToFirst[second];
			}
		}
	}

	public int Count
	{
		get
		{
			lock (obj4Lock)
			{
				return indexByFirst.Count;
			}
		}
	}

	public void Lock()
	{
		Monitor.Enter(obj4Lock);
	}

	public void UnLock()
	{
		Monitor.Exit(obj4Lock);
	}

	public void Add(TFirst first, TSecond second)
	{
		lock (obj4Lock)
		{
			if (firstToSecond.TryGetValue(first, out var value))
			{
				firstToSecond.Remove(first);
				secondToFirst.Remove(value);
				indexByFirst.Remove(first);
			}
			if (secondToFirst.TryGetValue(second, out var value2))
			{
				firstToSecond.Remove(value2);
				secondToFirst.Remove(second);
				indexByFirst.Remove(value2);
				Debug.Log("BiLookup::Add--------");
			}
			firstToSecond.Add(first, second);
			secondToFirst.Add(second, first);
			indexByFirst.Add(first);
		}
	}

	public void TryAdd(TFirst first, TSecond second)
	{
		lock (obj4Lock)
		{
			if (firstToSecond.TryGetValue(first, out var value))
			{
				if (value.Equals(second))
				{
					return;
				}
				firstToSecond.Remove(first);
				secondToFirst.Remove(value);
				indexByFirst.Remove(first);
			}
			firstToSecond.Add(first, second);
			secondToFirst.Add(second, first);
			indexByFirst.Add(first);
		}
	}

	public void Insert(TFirst first, TSecond second, int index)
	{
		lock (obj4Lock)
		{
			if (firstToSecond.TryGetValue(first, out var value))
			{
				firstToSecond.Remove(first);
				secondToFirst.Remove(value);
				indexByFirst.Remove(first);
			}
			firstToSecond.Add(first, second);
			secondToFirst.Add(second, first);
			indexByFirst.Insert(index, first);
		}
	}

	public bool RemoveByKey(TFirst first)
	{
		lock (obj4Lock)
		{
			if (firstToSecond.TryGetValue(first, out var value))
			{
				firstToSecond.Remove(first);
				secondToFirst.Remove(value);
				indexByFirst.Remove(first);
				return true;
			}
			return false;
		}
	}

	public bool RemoveByValue(TSecond second)
	{
		lock (obj4Lock)
		{
			if (secondToFirst.TryGetValue(second, out var value))
			{
				firstToSecond.Remove(value);
				secondToFirst.Remove(second);
				indexByFirst.Remove(value);
				return true;
			}
			return false;
		}
	}

	public void RemoveAt(int index)
	{
		lock (obj4Lock)
		{
			TFirst key = indexByFirst[index];
			secondToFirst.Remove(firstToSecond[key]);
			firstToSecond.Remove(key);
			indexByFirst.RemoveAt(index);
		}
	}

	public void Clear()
	{
		lock (obj4Lock)
		{
			firstToSecond.Clear();
			secondToFirst.Clear();
			indexByFirst.Clear();
		}
	}

	public void Append(BiLookup<TFirst, TSecond> listToAppend)
	{
		lock (obj4Lock)
		{
			int count = listToAppend.indexByFirst.Count;
			for (int i = 0; i < count; i++)
			{
				TFirst val = listToAppend.indexByFirst[i];
				TSecond second = listToAppend.firstToSecond[val];
				Add(val, second);
			}
		}
	}

	public BiLookup<TFirst, TSecond> CutPaste()
	{
		lock (obj4Lock)
		{
			BiLookup<TFirst, TSecond> biLookup = new BiLookup<TFirst, TSecond>();
			int count = indexByFirst.Count;
			for (int i = 0; i < count; i++)
			{
				TFirst val = indexByFirst[i];
				TSecond second = firstToSecond[val];
				biLookup.Add(val, second);
			}
			Clear();
			return biLookup;
		}
	}

	public TSecond GetValueByKey_Unsafe(TFirst first)
	{
		return firstToSecond[first];
	}

	public TSecond GetValueByIdx_Unsafe(int index)
	{
		return firstToSecond[indexByFirst[index]];
	}

	public TFirst GetKeyByValue_Unsafe(TSecond second)
	{
		return secondToFirst[second];
	}

	public TFirst GetKeyByIdx_Unsafe(int index)
	{
		return indexByFirst[index];
	}

	public List<TFirst> ToKeyList()
	{
		lock (obj4Lock)
		{
			return firstToSecond.Keys.ToList();
		}
	}

	public List<TSecond> ToValueList()
	{
		lock (obj4Lock)
		{
			return firstToSecond.Values.ToList();
		}
	}

	public bool ContainsKey(TFirst first)
	{
		lock (obj4Lock)
		{
			return firstToSecond.ContainsKey(first);
		}
	}

	public bool ContainsValue(TSecond second)
	{
		lock (obj4Lock)
		{
			return secondToFirst.ContainsKey(second);
		}
	}

	public bool Contains(TFirst first, TSecond second)
	{
		lock (obj4Lock)
		{
			if (firstToSecond.TryGetValue(first, out var value) && value.Equals(second))
			{
				return true;
			}
			return false;
		}
	}
}
