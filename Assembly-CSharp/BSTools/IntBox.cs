using UnityEngine;

namespace BSTools;

public struct IntBox
{
	public short xMin;

	public short yMin;

	public short zMin;

	public short xMax;

	public short yMax;

	public short zMax;

	public long Hash
	{
		get
		{
			return xMin | (zMin << 10) | (yMin << 20) | xMax | (zMax << 10) | (yMax << 20);
		}
		set
		{
			xMin = (short)(value & 0x3FF);
			zMin = (short)((value >> 10) & 0x3FF);
			yMin = (short)((value >> 20) & 0x3FF);
			xMax = (short)((value >> 32) & 0x3FF);
			zMax = (short)((value >> 42) & 0x3FF);
			yMax = (short)((value >> 52) & 0x3FF);
		}
	}

	public bool Contains(Vector3 point)
	{
		if (point.x >= (float)xMin && point.x <= (float)xMax && point.y >= (float)yMin && point.y <= (float)yMax && point.z >= (float)zMin && point.z <= (float)zMax)
		{
			return true;
		}
		return false;
	}
}
