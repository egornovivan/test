using System.IO;

namespace Pathea;

public class CreationDataArchiveMgr : ArchivableSingleton<CreationDataArchiveMgr>
{
	private const string ArchiveKey = "ArchiveKeyCreation";

	protected override string GetArchiveKey()
	{
		return "ArchiveKeyCreation";
	}

	private void Init()
	{
		CreationMgr.Init();
	}

	protected override bool GetYird()
	{
		return false;
	}

	public override void New()
	{
		Init();
		base.New();
	}

	public override void Restore()
	{
		Init();
		base.Restore();
	}

	protected override void SetData(byte[] data)
	{
		if (data != null)
		{
			CreationMgr.Import(data);
		}
	}

	protected override void WriteData(BinaryWriter bw)
	{
		CreationMgr.Export(bw);
	}
}
