using System.IO;

namespace Pathea;

public class VABuildingArchiveMgr : ArchivableSingleton<VABuildingArchiveMgr>
{
	private const string ArchiveKey = "VABuildingArchiveMgr";

	protected override string GetArchiveKey()
	{
		return "VABuildingArchiveMgr";
	}

	protected override void SetData(byte[] data)
	{
		if (data != null)
		{
			VABuildingManager.Instance.Import(data);
		}
	}

	protected override void WriteData(BinaryWriter bw)
	{
		VABuildingManager.Instance.Export(bw);
	}
}
