using System;
using System.Collections;
using System.Collections.Generic;

namespace SkillSystem;

public class SkCarrierCollisionPara : IEnumerable, IList<float>, ICollection<float>, IEnumerable<float>, ISkPara, ISkAttribsModPara, ISkParaNet
{
	private float[] _data;

	public int TypeId => 2;

	public int Count => _data.Length;

	public bool IsReadOnly => _data.IsReadOnly;

	public float this[int i]
	{
		get
		{
			return _data[i];
		}
		set
		{
			_data[i] = value;
		}
	}

	public SkCarrierCollisionPara(float momentum)
	{
		_data = new float[2] { 2.1f, momentum };
	}

	public SkCarrierCollisionPara()
	{
	}

	IEnumerator<float> IEnumerable<float>.GetEnumerator()
	{
		return (IEnumerator<float>)_data.GetEnumerator();
	}

	public float[] ToFloatArray()
	{
		return _data;
	}

	public void FromFloatArray(float[] data)
	{
		_data = data;
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
