using System;
using System.Collections;
using System.Collections.Generic;

namespace SkillSystem;

public class NumList : IEnumerable, IList<float>, ICollection<float>, IEnumerable<float>
{
	private float[] _data;

	private Action<NumList, int, float> _setter;

	public static readonly Action<NumList, int, float> DefSetter = delegate(NumList n, int i, float v)
	{
		n.Set(i, v);
	};

	public Action<NumList, int, float> Setter
	{
		get
		{
			return _setter;
		}
		set
		{
			_setter = value;
		}
	}

	public int Count => _data.Length;

	public bool IsReadOnly => _data.IsReadOnly;

	public float this[int i]
	{
		get
		{
			return Get(i);
		}
		set
		{
			_setter(this, i, value);
		}
	}

	public NumList(int n, Action<NumList, int, float> setter = null)
	{
		_data = new float[n];
		_setter = ((setter != null) ? setter : DefSetter);
	}

	IEnumerator<float> IEnumerable<float>.GetEnumerator()
	{
		return (IEnumerator<float>)_data.GetEnumerator();
	}

	public virtual float Get(int idx)
	{
		return _data[idx];
	}

	public virtual void Set(int idx, float v)
	{
		_data[idx] = v;
	}

	public void Add(float v)
	{
		throw new NotSupportedException("NotSupported_FixedSizeCollection");
	}

	public void Clear()
	{
		throw new NotSupportedException("NotSupported_ReadOnlyCollection");
	}

	public void Insert(int index, float v)
	{
		throw new NotSupportedException("NotSupported_FixedSizeCollection");
	}

	public bool Remove(float v)
	{
		throw new NotSupportedException("NotSupported_FixedSizeCollection");
	}

	public void RemoveAt(int index)
	{
		throw new NotSupportedException("NotSupported_FixedSizeCollection");
	}

	public void CopyTo(float[] a, int index)
	{
		_data.CopyTo(a, index);
	}

	public bool Contains(float v)
	{
		return Array.IndexOf(_data, v) != -1;
	}

	public int IndexOf(float v)
	{
		return Array.IndexOf(_data, v);
	}

	public IEnumerator GetEnumerator()
	{
		return _data.GetEnumerator();
	}
}
