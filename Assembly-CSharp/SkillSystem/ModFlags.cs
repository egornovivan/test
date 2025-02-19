using System;
using System.Collections;
using System.Collections.Generic;

namespace SkillSystem;

public class ModFlags : IEnumerable, IList<bool>, ICollection<bool>, IEnumerable<bool>
{
	private bool[] _data;

	private SkEntity[] _casters;

	public int Count => _data.Length;

	public bool IsReadOnly => _data.IsReadOnly;

	public bool this[int i]
	{
		get
		{
			return Get(i);
		}
		set
		{
			Set(i, value);
		}
	}

	public ModFlags(int n)
	{
		_data = new bool[n];
		_casters = new SkEntity[n];
	}

	IEnumerator<bool> IEnumerable<bool>.GetEnumerator()
	{
		return (IEnumerator<bool>)_data.GetEnumerator();
	}

	public bool Get(int idx)
	{
		return _data[idx];
	}

	public void Set(int idx, bool v)
	{
		_data[idx] = v;
		if (v && SkRuntimeInfo.Current != null)
		{
			_casters[idx] = SkRuntimeInfo.Current.Caster;
		}
		else
		{
			_casters[idx] = null;
		}
	}

	public void Add(bool v)
	{
		throw new NotSupportedException("NotSupported_FixedSizeCollection");
	}

	public void Clear()
	{
		Array.Clear(_data, 0, _data.Length);
		Array.Clear(_casters, 0, _casters.Length);
	}

	public void Insert(int index, bool v)
	{
		throw new NotSupportedException("NotSupported_FixedSizeCollection");
	}

	public bool Remove(bool v)
	{
		throw new NotSupportedException("NotSupported_FixedSizeCollection");
	}

	public void RemoveAt(int index)
	{
		throw new NotSupportedException("NotSupported_FixedSizeCollection");
	}

	public void CopyTo(bool[] a, int index)
	{
		_data.CopyTo(a, index);
	}

	public bool Contains(bool v)
	{
		return Array.IndexOf(_data, v) != -1;
	}

	public int IndexOf(bool v)
	{
		return Array.IndexOf(_data, v);
	}

	public IEnumerator GetEnumerator()
	{
		return _data.GetEnumerator();
	}

	public SkEntity GetCaster(int idx)
	{
		if (SkRuntimeInfo.Current != null)
		{
			return SkRuntimeInfo.Current.Caster;
		}
		return _casters[idx];
	}
}
