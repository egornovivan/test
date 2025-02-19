using System.IO;

namespace Pathea;

public class RSubTerrSLArchiveMgr : ArchivableSingleton<RSubTerrSLArchiveMgr>
{
	private const string ArchiveKey = "RSubTerrSLArchiveMgr";

	protected override string GetArchiveKey()
	{
		return "RSubTerrSLArchiveMgr";
	}

	protected override void SetData(byte[] data)
	{
		if (data != null)
		{
			RSubTerrSL.Import(data);
		}
	}

	protected override void WriteData(BinaryWriter bw)
	{
		RSubTerrSL.Export(bw);
	}
}
