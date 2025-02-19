using System.IO;
using Pathea;

public class EntityCreatedArchiveMgr : ArchivableSingleton<EntityCreatedArchiveMgr>
{
	private const string ArchiveKey = "ArchiveKeyEntityCreated";

	public static bool m_Finished;

	public override void New()
	{
		base.New();
		m_Finished = true;
	}

	protected override bool GetYird()
	{
		return false;
	}

	protected override string GetArchiveKey()
	{
		return "ArchiveKeyEntityCreated";
	}

	protected override void SetData(byte[] data)
	{
		if (data != null)
		{
			EntityCreateMgr.Instance.ReadEntityCreated(data);
			m_Finished = true;
		}
	}

	protected override void WriteData(BinaryWriter bw)
	{
		if (!(PeSingleton<PeCreature>.Instance.mainPlayer == null))
		{
			EntityCreateMgr.Instance.SaveEntityCreated(bw);
		}
	}
}
