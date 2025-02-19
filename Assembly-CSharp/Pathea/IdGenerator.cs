using System.IO;
using UnityEngine;

namespace Pathea;

public class IdGenerator
{
	public const int Invalid = -1;

	private const int DefaultBegin = 0;

	private const int DefaultEnd = 100000;

	private int mCurId;

	private int mEnd;

	private int mBegin;

	public int Cur
	{
		get
		{
			return mCurId;
		}
		set
		{
			mCurId = value;
		}
	}

	public IdGenerator(int curId)
		: this(curId, 0, 100000)
	{
	}

	public IdGenerator(int curId, int min, int max)
	{
		mCurId = curId;
		mBegin = min;
		mEnd = max;
	}

	public int Fetch()
	{
		if (mCurId > mEnd || mCurId < mBegin)
		{
			Debug.LogError("id generater pool has run out, use it from start.");
			mCurId = mBegin;
		}
		return mCurId++;
	}

	public void Export(BinaryWriter bw)
	{
		bw.Write(Cur);
	}

	public void Import(byte[] buffer)
	{
		using MemoryStream input = new MemoryStream(buffer, writable: false);
		using BinaryReader binaryReader = new BinaryReader(input);
		Cur = binaryReader.ReadInt32();
	}

	public void Serialize(BinaryWriter bw)
	{
		bw.Write(Cur);
	}

	public void Deserialize(BinaryReader br)
	{
		Cur = br.ReadInt32();
	}
}
