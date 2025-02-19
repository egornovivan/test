using System.IO;

namespace Pathea;

public class RMRepositoryArchiveMgr : ArchivableSingleton<RMRepositoryArchiveMgr>
{
	private const string ArchiveKey = "ArchiveKeyRMRepository";

	protected override bool GetYird()
	{
		return false;
	}

	protected override string GetArchiveKey()
	{
		return "ArchiveKeyRMRepository";
	}

	protected override void SetData(byte[] data)
	{
		if (data != null)
		{
			RMRepository.Import(data);
		}
	}

	protected override void WriteData(BinaryWriter bw)
	{
		RMRepository.Export(bw);
	}
}
