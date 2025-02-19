using System.IO;

namespace Pathea;

public class PeRecordWriter
{
	private BinaryWriter mWriter;

	private string mKey;

	public string key => mKey;

	public BinaryWriter binaryWriter => mWriter;

	public PeRecordWriter(string key, BinaryWriter writer)
	{
		mKey = key;
		mWriter = writer;
	}

	public void Write(int value)
	{
		mWriter.Write(value);
	}

	public void Write(byte[] buff)
	{
		if (buff != null)
		{
			mWriter.Write(buff);
		}
	}
}
