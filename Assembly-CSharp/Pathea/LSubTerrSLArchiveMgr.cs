using System.IO;

namespace Pathea;

public class LSubTerrSLArchiveMgr : ArchivableSingleton<LSubTerrSLArchiveMgr>
{
	private const string ArchiveKey = "LSubTerrSLArchiveMgr";

	protected override string GetArchiveKey()
	{
		return "LSubTerrSLArchiveMgr";
	}

	protected override void SetData(byte[] data)
	{
		if (data != null)
		{
			LSubTerrSL.Import(data);
		}
	}

	protected override void WriteData(BinaryWriter bw)
	{
		LSubTerrSL.Export(bw);
	}
}
