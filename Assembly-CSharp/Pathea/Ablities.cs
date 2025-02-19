using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pathea;

public class Ablities : List<int>
{
	private const int VERSION_0000 = 0;

	private const int CURRENT_VERSION = 0;

	private bool _bDirty;

	public Ablities(int capacity)
		: base(capacity)
	{
	}

	public Ablities()
	{
	}

	public Ablities(IEnumerable<int> collection)
		: base(collection)
	{
	}

	public new bool Remove(int item)
	{
		_bDirty = true;
		return base.Remove(item);
	}

	public new void AddRange(IEnumerable<int> collection)
	{
		base.AddRange(collection);
		_bDirty = true;
	}

	public new void RemoveAt(int idx)
	{
		base.RemoveAt(idx);
		_bDirty = true;
	}

	public new void RemoveAll(Predicate<int> match)
	{
		base.RemoveAll(match);
		_bDirty = true;
	}

	public new void Add(int idx)
	{
		base.Add(idx);
		_bDirty = true;
	}

	public bool GetDrity()
	{
		return _bDirty;
	}

	public void SetDirty(bool bDirty)
	{
		_bDirty = bDirty;
	}

	public void ClearDrity()
	{
		_bDirty = false;
	}

	public byte[] Export()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
		{
			binaryWriter.Write(0);
			binaryWriter.Write(Count);
			for (int i = 0; i < Count; i++)
			{
				binaryWriter.Write(this[i]);
			}
		}
		return memoryStream.ToArray();
	}

	public void Import(byte[] buffer)
	{
		using MemoryStream input = new MemoryStream(buffer, writable: false);
		using BinaryReader binaryReader = new BinaryReader(input);
		int num = binaryReader.ReadInt32();
		if (num > 0)
		{
			Debug.LogError("error version:" + num);
		}
		int num2 = binaryReader.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			int idx = binaryReader.ReadInt32();
			Add(idx);
		}
	}
}
