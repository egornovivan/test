using System;
using Pathea;
using UnityEngine;

namespace SkillSystem;

public class SkNumericAttribs
{
	private int _n;

	private float[] _rawData;

	private float[] _sumData;

	private bool[] _dirties;

	public event Action<int> _OnAlterNumAttribs;

	public SkNumericAttribs(int n)
	{
		_n = n;
		_rawData = new float[_n];
		_sumData = new float[_n];
		_dirties = new bool[_n + 1];
	}

	public float[] ToFloatParaArray()
	{
		return _rawData;
	}

	public void Reset(int idx)
	{
		_sumData[idx] = _rawData[idx];
	}

	public void SetAttribute(AttribType type, float value, bool isBase = false)
	{
		if (_rawData.Length <= (int)type)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("index is out type = " + type);
			}
		}
		else if (isBase)
		{
			_rawData[(int)type] = value;
		}
		else
		{
			_sumData[(int)type] = value;
		}
	}

	public float GetAttribute(AttribType type, bool isBase = false)
	{
		if (_rawData.Length <= (int)type)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("index is out type = " + type);
			}
			return 0f;
		}
		if (isBase)
		{
			return _rawData[(int)type];
		}
		return _sumData[(int)type];
	}

	public void SetAllAttribute(AttribType type, float value)
	{
		_rawData[(int)type] = value;
		_sumData[(int)type] = value;
	}

	public void SetDirty(AttribType type, bool bValue)
	{
		if (_dirties[(int)type] && !bValue && this._OnAlterNumAttribs != null)
		{
			this._OnAlterNumAttribs((int)type);
		}
		_dirties[(int)type] = bValue;
	}

	public bool GetDirty(AttribType type)
	{
		return _dirties[(int)type];
	}
}
