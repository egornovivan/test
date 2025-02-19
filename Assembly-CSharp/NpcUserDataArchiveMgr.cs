using System.IO;
using Pathea;

public class NpcUserDataArchiveMgr : ArchivableSingleton<NpcUserDataArchiveMgr>
{
	private const string ArchiveKey = "ArchiveKeyNpcUserData";

	protected override bool GetYird()
	{
		return false;
	}

	protected override string GetArchiveKey()
	{
		return "ArchiveKeyNpcUserData";
	}

	protected override void SetData(byte[] data)
	{
		if (data != null)
		{
			EntityCreateMgr.Instance.Import(data);
		}
	}

	protected override void WriteData(BinaryWriter bw)
	{
		if (!(PeSingleton<PeCreature>.Instance.mainPlayer == null))
		{
			EntityCreateMgr.Instance.Export(bw);
		}
	}
}
