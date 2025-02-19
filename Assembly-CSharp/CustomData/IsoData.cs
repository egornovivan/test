using uLink;

namespace CustomData;

public class IsoData
{
	public ulong isohashcode;

	public ulong uploader;

	public static void WriteMsg(BitStream stream, object obj, params object[] codecOptions)
	{
		IsoData isoData = (IsoData)obj;
		stream.Write(isoData.isohashcode);
		stream.Write(isoData.uploader);
	}

	public static object ReadMsg(BitStream stream, params object[] codecOptions)
	{
		IsoData isoData = new IsoData();
		isoData.isohashcode = stream.Read<ulong>(new object[0]);
		isoData.uploader = stream.Read<ulong>(new object[0]);
		return isoData;
	}
}
