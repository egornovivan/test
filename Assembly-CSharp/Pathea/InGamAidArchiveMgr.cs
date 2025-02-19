namespace Pathea;

public class InGamAidArchiveMgr : MonoLikeSingleton<InGamAidArchiveMgr>, ISerializable
{
	private const string InGameAidArchiveKey = "InGameAidArchiveKey";

	void ISerializable.Serialize(PeRecordWriter w)
	{
		if (w.key == "InGameAidArchiveKey")
		{
			w.binaryWriter.Write(InGameAidData.Serialize());
		}
	}

	protected override void OnInit()
	{
		base.OnInit();
		PeSingleton<ArchiveMgr>.Instance.Register("InGameAidArchiveKey", this);
	}

	public void Restore()
	{
		byte[] data = PeSingleton<ArchiveMgr>.Instance.GetData("InGameAidArchiveKey");
		if (data != null)
		{
			InGameAidData.Deserialize(data);
		}
	}

	public void New()
	{
		InGameAidData.ShowInGameAidCtrl = true;
		InGameAidData.Clear();
	}
}
