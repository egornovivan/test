using System.Collections.Generic;
using System.IO;
using uLink;

public class RegisteredISO
{
	internal ulong HashCode;

	internal ulong UGCHandle;

	internal string IsoName;

	internal List<EVCComponent> Components = new List<EVCComponent>();

	public static void Serialize(BitStream stream, object obj, params object[] codecOptions)
	{
		RegisteredISO registeredISO = (RegisteredISO)obj;
		stream.Write(registeredISO.HashCode);
		stream.Write(registeredISO.IsoName);
		stream.Write(registeredISO.UGCHandle);
	}

	public static object Deserialize(BitStream stream, params object[] codecOptions)
	{
		RegisteredISO registeredISO = new RegisteredISO();
		registeredISO.HashCode = stream.Read<ulong>(new object[0]);
		registeredISO.IsoName = stream.Read<string>(new object[0]);
		registeredISO.UGCHandle = stream.Read<ulong>(new object[0]);
		return registeredISO;
	}

	internal byte[] GetComponentsData()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(memoryStream);
		BufferHelper.Serialize(writer, Components.Count);
		foreach (EVCComponent component in Components)
		{
			BufferHelper.Serialize(writer, (int)component);
		}
		return memoryStream.ToArray();
	}
}
