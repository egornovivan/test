using System.IO;

namespace Pathea;

public class SPPlayerBaseArchiveMgr : ArchivableSingleton<SPPlayerBaseArchiveMgr>
{
	private const string ArchiveKey = "ArchiveKeySPPlayerBase";

	protected override string GetArchiveKey()
	{
		return "ArchiveKeySPPlayerBase";
	}

	protected override void SetData(byte[] data)
	{
		if (data != null)
		{
			SPPlayerBase.Single.Import(data);
		}
	}

	protected override void WriteData(BinaryWriter bw)
	{
		SPPlayerBase.Single.Export(bw);
	}
}
