using System.IO;

namespace Pathea;

public class FarmArchiveMgr : ArchivableSingleton<FarmArchiveMgr>
{
	private const string ArchiveKey = "ArchiveKeyFarm";

	protected override string GetArchiveKey()
	{
		return "ArchiveKeyFarm";
	}

	protected override void SetData(byte[] data)
	{
		if (data != null)
		{
			FarmManager.Instance.Import(data);
		}
	}

	protected override void WriteData(BinaryWriter bw)
	{
		FarmManager.Instance.Export(bw);
	}
}
