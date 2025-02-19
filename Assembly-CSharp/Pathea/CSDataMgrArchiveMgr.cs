using System.IO;

namespace Pathea;

public class CSDataMgrArchiveMgr : ArchivableSingleton<CSDataMgrArchiveMgr>
{
	private const string ArchiveKey = "ArchiveKeyCSDataMgr";

	protected override string GetArchiveKey()
	{
		return "ArchiveKeyCSDataMgr";
	}

	protected override void SetData(byte[] data)
	{
		if (data != null)
		{
			CSDataMgr.Import(data);
		}
	}

	protected override void WriteData(BinaryWriter bw)
	{
		CSDataMgr.Export(bw);
	}
}
