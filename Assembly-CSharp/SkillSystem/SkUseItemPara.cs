using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SkillSystem;

public class SkUseItemPara : IEnumerable, IList<float>, ICollection<float>, IEnumerable<float>, ISkPara, ISkAttribsModPara, ISkParaNet
{
	private float[] _data;

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

	public int TypeId => 0;

	public SkUseItemPara()
	{
	}

	public SkUseItemPara(List<int> idxList, List<float> valList)
	{
		_data = new float[idxList.Max() + 1];
		int count = idxList.Count;
		for (int i = 0; i < count; i++)
		{
			int num = idxList[i];
			_data[num] = valList[i];
		}
	}

	IEnumerator<float> IEnumerable<float>.GetEnumerator()
	{
		return (IEnumerator<float>)_data.GetEnumerator();
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

	public float[] ToFloatArray()
	{
		int num = _data.Length;
		float[] array = new float[num + 1];
		array[0] = (float)TypeId + 0.1f;
		Array.Copy(_data, 0, array, 1, num);
		return array;
	}

	public void FromFloatArray(float[] data)
	{
		int num = data.Length - 1;
		_data = new float[num];
		Array.Copy(data, 1, _data, 0, num);
	}
}
