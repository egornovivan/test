using System;
using UnityEngine;

internal class ChunkEventArgs : EventArgs
{
	internal Vector3 _pos;

	internal ChunkEventArgs(Vector3 pos)
	{
		_pos = pos;
	}
}
