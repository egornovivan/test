using System.IO;

namespace Pathea;

public class TownEditorArchiveMgr : ArchivableSingleton<TownEditorArchiveMgr>
{
	private const string ArchiveKey = "ArchiveKeyTownEditor";

	protected override string GetArchiveKey()
	{
		return "ArchiveKeyTownEditor";
	}

	protected override void SetData(byte[] data)
	{
		if (data == null)
		{
		}
	}

	protected override void WriteData(BinaryWriter bw)
	{
		TownEditor.Instance.Export(bw);
	}
}
