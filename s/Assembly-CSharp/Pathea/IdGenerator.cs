using System.IO;

namespace Pathea;

public class IdGenerator
{
	public static int _npcId = 10000;

	private static int _monsterId = 100000;

	private static int _itemId = 1000000;

	private static int _doodadId = 2000000;

	public static int NewNpcId => ++_npcId;

	public static int CurNpcId => _npcId;

	public static int NewMonsterId => ++_monsterId;

	public static int CurMonsterId => _monsterId;

	public static int NewItemId => ++_itemId;

	public static int CurItemId => _itemId;

	public static int NewDoodadId => ++_doodadId;

	public static int CurDoodadId => _doodadId;

	public static void InitCurId(int npcId, int monsterId, int itemId, int doodadId)
	{
		_npcId = npcId;
		_monsterId = monsterId;
		_itemId = itemId;
		_doodadId = doodadId;
	}

	public static void InitCurItemId(int itemId)
	{
		_itemId = itemId;
	}

	public static byte[] Export()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(CurNpcId);
		binaryWriter.Write(CurMonsterId);
		binaryWriter.Write(CurItemId);
		binaryWriter.Write(CurDoodadId);
		return memoryStream.ToArray();
	}

	public static void Import(byte[] buffer)
	{
		using MemoryStream input = new MemoryStream(buffer, writable: false);
		using BinaryReader binaryReader = new BinaryReader(input);
		int npcId = binaryReader.ReadInt32();
		int monsterId = binaryReader.ReadInt32();
		int itemId = binaryReader.ReadInt32();
		int doodadId = binaryReader.ReadInt32();
		InitCurId(npcId, monsterId, itemId, doodadId);
	}
}
