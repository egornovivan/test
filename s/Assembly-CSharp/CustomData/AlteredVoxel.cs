using uLink;

namespace CustomData;

public class AlteredVoxel
{
	public IntVector3 pos;

	public byte type;

	public byte volume;

	public static void WriteVoxel(BitStream stream, object obj, params object[] codecOptions)
	{
		AlteredVoxel alteredVoxel = obj as AlteredVoxel;
		stream.Write(alteredVoxel.pos);
		stream.Write(alteredVoxel.type);
		stream.Write(alteredVoxel.volume);
	}

	public static object ReadVoxel(BitStream stream, params object[] codecOptions)
	{
		AlteredVoxel alteredVoxel = new AlteredVoxel();
		alteredVoxel.pos = stream.Read<IntVector3>(new object[0]);
		alteredVoxel.type = stream.Read<byte>(new object[0]);
		alteredVoxel.volume = stream.Read<byte>(new object[0]);
		return alteredVoxel;
	}
}
