using System.IO;

namespace Pathea;

public class ArchiveIndex
{
	private string mFileName;

	private long mBeginPos;

	private long mLength;

	private bool mYird;

	public string fileName => mFileName;

	public bool yird => mYird;

	public long beginPos => mBeginPos;

	public long length => mLength;

	public ArchiveIndex()
	{
	}

	public ArchiveIndex(string fileName, bool yird, long beginPos, long endPos)
	{
		mFileName = fileName;
		mYird = yird;
		mBeginPos = beginPos;
		mLength = endPos - beginPos;
	}

	public void Read(BinaryReader r)
	{
		mFileName = r.ReadString();
		mYird = r.ReadBoolean();
		mBeginPos = r.ReadInt64();
		mLength = r.ReadInt64();
	}

	public void Write(BinaryWriter w)
	{
		w.Write(mFileName);
		w.Write(mYird);
		w.Write(mBeginPos);
		w.Write(mLength);
	}
}
