using System.IO;

namespace Pathea;

public class VArtifactTownArchiveMgr : ArchivableSingleton<VArtifactTownArchiveMgr>
{
	private const string ArchiveKey = "VArtifactTownArchiveMgr";

	protected override string GetArchiveKey()
	{
		return "VArtifactTownArchiveMgr";
	}

	protected override void SetData(byte[] data)
	{
		if (data != null)
		{
			VArtifactTownManager.Instance.Import(data);
		}
	}

	protected override void WriteData(BinaryWriter bw)
	{
		VArtifactTownManager.Instance.Export(bw);
	}

	protected override bool GetYird()
	{
		return false;
	}
}
