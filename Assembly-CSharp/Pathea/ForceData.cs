using System.IO;

namespace Pathea;

public class ForceData
{
	private const int VERSION_0000 = 0;

	private const int CURRENT_VERSION = 0;

	public int _campID;

	public int _damageID;

	public int _defaultPlyerID;

	public ForceData(int campid = 0, int damageid = 0, int defaulplayerid = 0)
	{
		_campID = campid;
		_damageID = damageid;
		_defaultPlyerID = defaulplayerid;
	}

	public ForceData Copy()
	{
		return new ForceData(_campID, _damageID, _defaultPlyerID);
	}

	public void Export(BinaryWriter bw)
	{
		bw.Write(0);
		bw.Write(_campID);
		bw.Write(_damageID);
		bw.Write(_defaultPlyerID);
	}

	public void Import(BinaryReader r)
	{
		int num = r.ReadInt32();
		_campID = r.ReadInt32();
		_damageID = r.ReadInt32();
		_defaultPlyerID = r.ReadInt32();
	}
}
