using uLink;

namespace CustomData;

public class RegisteredISO
{
	internal ulong _hashCode;

	internal ulong UGCHandle;

	internal string _isoName;

	public static void Serialize(BitStream stream, object obj, params object[] codecOptions)
	{
		RegisteredISO registeredISO = (RegisteredISO)obj;
		stream.Write(registeredISO._hashCode);
		stream.Write(registeredISO._isoName);
		stream.Write(registeredISO.UGCHandle);
	}

	public static object Deserialize(BitStream stream, params object[] codecOptions)
	{
		RegisteredISO registeredISO = new RegisteredISO();
		registeredISO._hashCode = stream.Read<ulong>(new object[0]);
		registeredISO._isoName = stream.Read<string>(new object[0]);
		registeredISO.UGCHandle = stream.Read<ulong>(new object[0]);
		return registeredISO;
	}
}
