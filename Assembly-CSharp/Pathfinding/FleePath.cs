using System;
using UnityEngine;

namespace Pathfinding;

public class FleePath : RandomPath
{
	[Obsolete("Please use the Construct method instead")]
	public FleePath(Vector3 start, Vector3 avoid, int length, OnPathDelegate callbackDelegate = null)
		: base(start, length, callbackDelegate)
	{
		throw new Exception("Please use the Construct method instead");
	}

	public FleePath()
	{
	}

	public static FleePath Construct(Vector3 start, Vector3 avoid, int searchLength, OnPathDelegate callback = null)
	{
		FleePath fleePath = PathPool<FleePath>.GetPath();
		fleePath.Setup(start, avoid, searchLength, callback);
		return fleePath;
	}

	protected void Setup(Vector3 start, Vector3 avoid, int searchLength, OnPathDelegate callback)
	{
		Setup(start, searchLength, callback);
		aim = avoid - start;
		aim *= 10f;
		aim = start - aim;
	}

	protected override void Recycle()
	{
		PathPool<FleePath>.Recycle(this);
	}
}
