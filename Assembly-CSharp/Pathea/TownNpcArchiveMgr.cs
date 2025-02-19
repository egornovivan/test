using System.IO;

namespace Pathea;

public class TownNpcArchiveMgr : ArchivableSingleton<TownNpcArchiveMgr>
{
	private const string ArchiveKey = "TownNpcArchiveMgr";

	protected override string GetArchiveKey()
	{
		return "TownNpcArchiveMgr";
	}

	protected override void SetData(byte[] data)
	{
		if (data != null)
		{
			VATownNpcManager.Instance.Import(data);
		}
	}

	protected override void WriteData(BinaryWriter bw)
	{
		VATownNpcManager.Instance.Export(bw);
	}
}
