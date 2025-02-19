using System.IO;

namespace Pathea;

public class GrassDataSLArchiveMgr : ArchivableSingleton<GrassDataSLArchiveMgr>
{
	private const string ArchiveKey = "ArchiveKeyGrassDataSL";

	protected override string GetArchiveKey()
	{
		return "ArchiveKeyGrassDataSL";
	}

	protected override void SetData(byte[] data)
	{
		GrassDataSL.Init();
		if (data != null)
		{
			GrassDataSL.Import(data);
		}
	}

	protected override void WriteData(BinaryWriter bw)
	{
		GrassDataSL.Export(bw);
	}

	public override void New()
	{
		base.New();
		GrassDataSL.Init();
	}
}
