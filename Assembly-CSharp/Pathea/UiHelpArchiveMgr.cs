namespace Pathea;

public class UiHelpArchiveMgr : MonoLikeSingleton<UiHelpArchiveMgr>, ISerializable
{
	private const string TutorialDataArchiveKey = "TutorialData";

	private const string MetalScanDataArchiveKey = "MetalScanData";

	private const string MessageDataArchveKey = "MessageData";

	private const string MonsterHandbookDataArchveKey = "MonsterHandbookData";

	void ISerializable.Serialize(PeRecordWriter w)
	{
		if (w.key == "TutorialData")
		{
			w.binaryWriter.Write(TutorialData.Serialize());
		}
		else if (w.key == "MetalScanData")
		{
			w.binaryWriter.Write(MetalScanData.Serialize());
		}
		else if (w.key == "MessageData")
		{
			w.binaryWriter.Write(MessageData.Serialize());
		}
		else if (w.key == "MonsterHandbookData")
		{
			w.binaryWriter.Write(MonsterHandbookData.Serialize());
		}
	}

	protected override void OnInit()
	{
		base.OnInit();
		PeSingleton<ArchiveMgr>.Instance.Register("TutorialData", this);
		PeSingleton<ArchiveMgr>.Instance.Register("MetalScanData", this);
		PeSingleton<ArchiveMgr>.Instance.Register("MessageData", this);
		PeSingleton<ArchiveMgr>.Instance.Register("MonsterHandbookData", this);
	}

	public void Restore()
	{
		byte[] data = PeSingleton<ArchiveMgr>.Instance.GetData("TutorialData");
		if (data != null)
		{
			TutorialData.Deserialize(data);
		}
		data = PeSingleton<ArchiveMgr>.Instance.GetData("MetalScanData");
		if (data != null)
		{
			MetalScanData.Deserialize(data);
		}
		data = PeSingleton<ArchiveMgr>.Instance.GetData("MessageData");
		if (data != null)
		{
			MessageData.Deserialize(data);
		}
		data = PeSingleton<ArchiveMgr>.Instance.GetData("MonsterHandbookData");
		if (data != null)
		{
			MonsterHandbookData.Deserialize(data);
		}
	}

	public void New()
	{
		MetalScanData.Clear();
		MessageData.Clear();
		TutorialData.Clear();
		MonsterHandbookData.Clear();
	}
}
