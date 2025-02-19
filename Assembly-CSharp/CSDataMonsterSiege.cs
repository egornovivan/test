using System.IO;

public class CSDataMonsterSiege
{
	public int lvl;

	public float lastHour;

	public float nextHour;

	public void Import(BinaryReader r)
	{
		lvl = r.ReadInt32();
		lastHour = r.ReadSingle();
		nextHour = r.ReadSingle();
	}

	public void Export(BinaryWriter w)
	{
		w.Write(lvl);
		w.Write(lastHour);
		w.Write(nextHour);
	}
}
