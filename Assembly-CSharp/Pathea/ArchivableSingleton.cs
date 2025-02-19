using System.IO;

namespace Pathea;

public abstract class ArchivableSingleton<T> : MonoLikeSingleton<T>, ISerializable where T : class, new()
{
	void ISerializable.Serialize(PeRecordWriter w)
	{
		WriteData(w.binaryWriter);
	}

	protected override void OnInit()
	{
		base.OnInit();
		PeSingleton<ArchiveMgr>.Instance.Register(GetArchiveKey(), this, GetYird(), GetRecordName(), GetSaveFlagResetValue());
	}

	protected virtual string GetArchiveKey()
	{
		return typeof(T).ToString();
	}

	protected virtual bool GetYird()
	{
		return true;
	}

	protected virtual string GetRecordName()
	{
		return "world";
	}

	protected virtual bool GetSaveFlagResetValue()
	{
		return true;
	}

	public void SaveMe()
	{
		PeSingleton<ArchiveMgr>.Instance.SaveMe(GetArchiveKey());
	}

	public virtual void New()
	{
	}

	public virtual void Restore()
	{
		byte[] data = PeSingleton<ArchiveMgr>.Instance.GetData(GetArchiveKey());
		if (data == null || data.Length == 0)
		{
			New();
		}
		else
		{
			SetData(data);
		}
	}

	protected abstract void WriteData(BinaryWriter bw);

	protected abstract void SetData(byte[] data);
}
