namespace Pathea;

public class WorldInfoMgr : MonoLikeSingleton<WorldInfoMgr>, ISerializable
{
	private const string ArchiveKeyNameGenerator = "ArchiveKeyNameGenerator";

	private const string ArchiveKeyIdGenerator = "ArchiveKeyIdGenerator";

	public const int IdBeginForAssign = 5000;

	public const int IdEndForAssign = 15000;

	public const int IdBeginForAuto = 20000;

	public const int IdEndForAuto = 2146483647;

	public const int IdBeginNonRecord = 2146483647;

	public const int IdEndNonRecord = int.MaxValue;

	private IdGenerator mRecordEntityIdGen;

	private IdGenerator mNonRecordEntityIdGen;

	private NameGenerater mNameGenerater;

	void ISerializable.Serialize(PeRecordWriter w)
	{
		if (w.key == "ArchiveKeyIdGenerator")
		{
			mRecordEntityIdGen.Export(w.binaryWriter);
		}
		else if (w.key == "ArchiveKeyNameGenerator")
		{
			w.Write(mNameGenerater.Export());
		}
	}

	protected override void OnInit()
	{
		base.OnInit();
		mRecordEntityIdGen = new IdGenerator(20000, 20000, 2146483647);
		mNonRecordEntityIdGen = new IdGenerator(2146483647, 2146483647, int.MaxValue);
		mNameGenerater = new NameGenerater();
		PeSingleton<ArchiveMgr>.Instance.Register("ArchiveKeyNameGenerator", this);
		PeSingleton<ArchiveMgr>.Instance.Register("ArchiveKeyIdGenerator", this);
	}

	public void New()
	{
	}

	public void Restore()
	{
		byte[] data = PeSingleton<ArchiveMgr>.Instance.GetData("ArchiveKeyIdGenerator");
		if (data != null)
		{
			mRecordEntityIdGen.Import(data);
		}
		data = PeSingleton<ArchiveMgr>.Instance.GetData("ArchiveKeyNameGenerator");
		if (data != null)
		{
			mNameGenerater.Import(data);
		}
	}

	public bool IsAssignId(int id)
	{
		if (id >= 5000 && id <= 15000)
		{
			return true;
		}
		return false;
	}

	public bool IsNonRecordAutoId(int id)
	{
		if (id >= 2146483647 && id < int.MaxValue)
		{
			return true;
		}
		return false;
	}

	public bool IsRecordAutoId(int id)
	{
		if (id >= 20000 && id < 2146483647)
		{
			return true;
		}
		return false;
	}

	public int FetchNonRecordAutoId()
	{
		return mNonRecordEntityIdGen.Fetch();
	}

	public int FetchRecordAutoId()
	{
		return mRecordEntityIdGen.Fetch();
	}

	public CharacterName FetchName(PeSex sex, int race)
	{
		return mNameGenerater.Fetch(sex, race);
	}
}
