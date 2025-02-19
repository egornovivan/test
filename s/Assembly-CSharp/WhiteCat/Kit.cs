using UnityEngine;

namespace WhiteCat;

public class Kit
{
	public const int sizeOfVector3 = 12;

	public const int sizeOfQuaternion = 16;

	public const int sizeOfUshort = 2;

	public static void WriteToBuffer(byte[] buffer, ref int offset, Vector3 value)
	{
		UnionValue unionValue = default(UnionValue);
		unionValue.floatValue = value.x;
		unionValue.WriteFloatTo(buffer, ref offset);
		unionValue.floatValue = value.y;
		unionValue.WriteFloatTo(buffer, ref offset);
		unionValue.floatValue = value.z;
		unionValue.WriteFloatTo(buffer, ref offset);
	}

	public static Vector3 ReadVector3FromBuffer(byte[] buffer, ref int offset)
	{
		Vector3 result = default(Vector3);
		UnionValue unionValue = default(UnionValue);
		unionValue.ReadFloatFrom(buffer, ref offset);
		result.x = unionValue.floatValue;
		unionValue.ReadFloatFrom(buffer, ref offset);
		result.y = unionValue.floatValue;
		unionValue.ReadFloatFrom(buffer, ref offset);
		result.z = unionValue.floatValue;
		return result;
	}

	public static void WriteToBuffer(byte[] buffer, ref int offset, Quaternion value)
	{
		UnionValue unionValue = default(UnionValue);
		unionValue.floatValue = value.x;
		unionValue.WriteFloatTo(buffer, ref offset);
		unionValue.floatValue = value.y;
		unionValue.WriteFloatTo(buffer, ref offset);
		unionValue.floatValue = value.z;
		unionValue.WriteFloatTo(buffer, ref offset);
		unionValue.floatValue = value.w;
		unionValue.WriteFloatTo(buffer, ref offset);
	}

	public static Quaternion ReadQuaternionFromBuffer(byte[] buffer, ref int offset)
	{
		Quaternion result = default(Quaternion);
		UnionValue unionValue = default(UnionValue);
		unionValue.ReadFloatFrom(buffer, ref offset);
		result.x = unionValue.floatValue;
		unionValue.ReadFloatFrom(buffer, ref offset);
		result.y = unionValue.floatValue;
		unionValue.ReadFloatFrom(buffer, ref offset);
		result.z = unionValue.floatValue;
		unionValue.ReadFloatFrom(buffer, ref offset);
		result.w = unionValue.floatValue;
		return result;
	}
}
