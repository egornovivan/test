using System.IO;

namespace Pathea;

public class MountsArchiveMgr : ArchivableSingleton<MountsArchiveMgr>
{
	private const string ArchiveKey = "MountsArchiveMgr";

	public override void New()
	{
		base.New();
		RelationshipDataMgr.Clear();
	}

	protected override string GetArchiveKey()
	{
		return "MountsArchiveMgr";
	}

	protected override void SetData(byte[] data)
	{
		if (data != null)
		{
			RelationshipDataMgr.Import(data);
		}
	}

	protected override void WriteData(BinaryWriter bw)
	{
		RelationshipDataMgr.Export(bw);
	}
}
