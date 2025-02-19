using System.IO;

namespace Pathea;

public class AdRMRepositoryArchiveMgr : ArchivableSingleton<AdRMRepositoryArchiveMgr>
{
	private const string ArchiveKey = "ArchiveKeyAdRMRepository";

	protected override string GetArchiveKey()
	{
		return "ArchiveKeyAdRMRepository";
	}

	protected override void SetData(byte[] data)
	{
		if (data != null)
		{
			AdRMRepository.Import(data);
		}
	}

	protected override void WriteData(BinaryWriter bw)
	{
		AdRMRepository.Export(bw);
	}
}
