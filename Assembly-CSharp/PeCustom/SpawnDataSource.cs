using System.Collections.Generic;
using System.IO;
using Pathea;

namespace PeCustom;

public class SpawnDataSource : PeCustomScene.SceneElement, Pathea.ISerializable
{
	public const string ArchiveKey = "CustomSceneSpawnPoints";

	private const int VERSION = 4;

	private Dictionary<int, MonsterSpawnPoint> mMsts;

	private Dictionary<int, MonsterSpawnArea> mMstAreas;

	private Dictionary<int, NPCSpawnPoint> mNpcs;

	private Dictionary<int, DoodadSpawnPoint> mDoodads;

	private Dictionary<int, ItemSpwanPoint> mItems;

	private Dictionary<int, EffectSpwanPoint> mEffects;

	private int mMaxSpawnPointId = 10000;

	public Dictionary<int, MonsterSpawnPoint> monsters => mMsts;

	public Dictionary<int, MonsterSpawnArea> areas => mMstAreas;

	public Dictionary<int, NPCSpawnPoint> npcs => mNpcs;

	public Dictionary<int, DoodadSpawnPoint> doodads => mDoodads;

	public Dictionary<int, EffectSpwanPoint> effects => mEffects;

	public Dictionary<int, ItemSpwanPoint> items => mItems;

	public SpawnDataSource()
	{
		mMsts = new Dictionary<int, MonsterSpawnPoint>(50);
		mMstAreas = new Dictionary<int, MonsterSpawnArea>(50);
		mNpcs = new Dictionary<int, NPCSpawnPoint>(50);
		mDoodads = new Dictionary<int, DoodadSpawnPoint>(20);
		mEffects = new Dictionary<int, EffectSpwanPoint>(20);
		mItems = new Dictionary<int, ItemSpwanPoint>(50);
		PeSingleton<ArchiveMgr>.Instance.Register("CustomSceneSpawnPoints", this);
	}

	void Pathea.ISerializable.Serialize(PeRecordWriter w)
	{
		w.Write(GetData());
	}

	public int GenerateId()
	{
		return ++mMaxSpawnPointId;
	}

	public bool ContainMonster(int id)
	{
		return mMsts.ContainsKey(id);
	}

	public bool ContainArea(int id)
	{
		return mMstAreas.ContainsKey(id);
	}

	public bool ContainNpc(int id)
	{
		return mNpcs.ContainsKey(id);
	}

	public bool ContainDoodad(int id)
	{
		return mDoodads.ContainsKey(id);
	}

	public bool ContainItem(int id)
	{
		return mItems.ContainsKey(id);
	}

	public bool ContainEffect(int id)
	{
		return mEffects.ContainsKey(id);
	}

	public SpawnPoint GetSpawnPoint(int id)
	{
		if (ContainMonster(id))
		{
			return GetMonster(id);
		}
		if (ContainArea(id))
		{
			return GetMonsterArea(id);
		}
		if (ContainNpc(id))
		{
			return GetNpc(id);
		}
		if (ContainDoodad(id))
		{
			return GetDoodad(id);
		}
		if (ContainItem(id))
		{
			return GetItem(id);
		}
		if (mEffects.ContainsKey(id))
		{
			return GetEffect(id);
		}
		return null;
	}

	public MonsterSpawnPoint GetMonster(int id)
	{
		if (!mMsts.ContainsKey(id))
		{
			return null;
		}
		return mMsts[id];
	}

	public MonsterSpawnArea GetMonsterArea(int id)
	{
		if (!mMstAreas.ContainsKey(id))
		{
			return null;
		}
		return mMstAreas[id];
	}

	public NPCSpawnPoint GetNpc(int id)
	{
		if (!mNpcs.ContainsKey(id))
		{
			return null;
		}
		return mNpcs[id];
	}

	public DoodadSpawnPoint GetDoodad(int id)
	{
		if (!mDoodads.ContainsKey(id))
		{
			return null;
		}
		return mDoodads[id];
	}

	public ItemSpwanPoint GetItem(int id)
	{
		return mItems[id];
	}

	public EffectSpwanPoint GetEffect(int id)
	{
		return mEffects[id];
	}

	public bool AddMonster(MonsterSpawnPoint msp)
	{
		if (!mMsts.ContainsKey(msp.ID))
		{
			mMsts.Add(msp.ID, msp);
			return true;
		}
		return false;
	}

	public bool AddNpc(NPCSpawnPoint nsp)
	{
		if (!mNpcs.ContainsKey(nsp.ID))
		{
			mNpcs.Add(nsp.ID, nsp);
			return true;
		}
		return false;
	}

	public bool AddDoodad(DoodadSpawnPoint dsp)
	{
		if (!mDoodads.ContainsKey(dsp.ID))
		{
			mDoodads.Add(dsp.ID, dsp);
			return true;
		}
		return false;
	}

	public bool AddItem(ItemSpwanPoint isp)
	{
		if (!mItems.ContainsKey(isp.ID))
		{
			mItems.Add(isp.ID, isp);
			return true;
		}
		return false;
	}

	public bool RemoveMonster(int id)
	{
		return mMsts.Remove(id);
	}

	public bool RemoveNpc(int id)
	{
		return mNpcs.Remove(id);
	}

	public bool RemoveDoodad(int id)
	{
		return mDoodads.Remove(id);
	}

	public bool RemoveItem(int id)
	{
		return mItems.Remove(id);
	}

	public void ClearMonster()
	{
		mMsts.Clear();
		mMstAreas.Clear();
	}

	public void ClearNpc()
	{
		mNpcs.Clear();
	}

	public void ClearDoodad()
	{
		mDoodads.Clear();
	}

	public void ClearItems()
	{
		mItems.Clear();
	}

	public void ClearEffects()
	{
		mEffects.Clear();
	}

	private void SetMonsters(IEnumerable<WEMonster> items)
	{
		if (items == null)
		{
			return;
		}
		ClearMonster();
		foreach (WEMonster item in items)
		{
			if (item.AreaSpwan)
			{
				MonsterSpawnArea monsterSpawnArea = new MonsterSpawnArea(item);
				monsterSpawnArea.CalcSpawns();
				mMstAreas.Add(monsterSpawnArea.ID, monsterSpawnArea);
			}
			else
			{
				MonsterSpawnPoint monsterSpawnPoint = new MonsterSpawnPoint(item);
				mMsts.Add(monsterSpawnPoint.ID, monsterSpawnPoint);
			}
		}
	}

	private void SetNPCs(IEnumerable<WENPC> items)
	{
		if (items == null)
		{
			return;
		}
		ClearNpc();
		foreach (WENPC item in items)
		{
			NPCSpawnPoint nPCSpawnPoint = new NPCSpawnPoint(item);
			mNpcs.Add(nPCSpawnPoint.ID, nPCSpawnPoint);
		}
	}

	private void SetDoodads(IEnumerable<WEDoodad> items)
	{
		if (items == null)
		{
			return;
		}
		ClearDoodad();
		foreach (WEDoodad item in items)
		{
			DoodadSpawnPoint doodadSpawnPoint = new DoodadSpawnPoint(item);
			mDoodads.Add(doodadSpawnPoint.ID, doodadSpawnPoint);
		}
	}

	private void SetItems(IEnumerable<WEItem> items)
	{
		if (items == null)
		{
			return;
		}
		ClearItems();
		foreach (WEItem item in items)
		{
			ItemSpwanPoint itemSpwanPoint = new ItemSpwanPoint(item);
			mItems.Add(itemSpwanPoint.ID, itemSpwanPoint);
		}
	}

	private void SetEffects(IEnumerable<WEEffect> items)
	{
		if (items == null)
		{
			return;
		}
		foreach (WEEffect item in items)
		{
			EffectSpwanPoint effectSpwanPoint = new EffectSpwanPoint(item);
			mEffects.Add(effectSpwanPoint.ID, effectSpwanPoint);
		}
	}

	public void Restore(YirdData yird)
	{
		byte[] data = PeSingleton<ArchiveMgr>.Instance.GetData("CustomSceneSpawnPoints");
		if (data == null || data.Length == 0)
		{
			New(yird);
		}
		else
		{
			SetData(data);
		}
		SetEffects(yird.GetEffects());
	}

	public void New(YirdData yird)
	{
		SetMonsters(yird.GetMonsters());
		SetNPCs(yird.GetNpcs());
		SetDoodads(yird.GetDoodads());
		SetItems(yird.GetItems());
		SetEffects(yird.GetEffects());
	}

	private byte[] GetData()
	{
		byte[] array = null;
		using MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(4);
		binaryWriter.Write(mMaxSpawnPointId);
		binaryWriter.Write(mMsts.Count);
		foreach (KeyValuePair<int, MonsterSpawnPoint> mMst in mMsts)
		{
			mMst.Value.Serialize(binaryWriter);
		}
		binaryWriter.Write(mMstAreas.Count);
		foreach (KeyValuePair<int, MonsterSpawnArea> mMstArea in mMstAreas)
		{
			mMstArea.Value.Serialize(binaryWriter);
		}
		binaryWriter.Write(mNpcs.Count);
		foreach (KeyValuePair<int, NPCSpawnPoint> mNpc in mNpcs)
		{
			mNpc.Value.Serialize(binaryWriter);
		}
		binaryWriter.Write(mDoodads.Count);
		foreach (KeyValuePair<int, DoodadSpawnPoint> mDoodad in mDoodads)
		{
			mDoodad.Value.Serialize(binaryWriter);
		}
		binaryWriter.Write(mItems.Count);
		foreach (KeyValuePair<int, ItemSpwanPoint> mItem in mItems)
		{
			mItem.Value.Serialize(binaryWriter);
		}
		array = memoryStream.ToArray();
		binaryWriter.Close();
		return array;
	}

	private void SetData(byte[] data)
	{
		using MemoryStream input = new MemoryStream(data);
		BinaryReader binaryReader = new BinaryReader(input);
		int num = binaryReader.ReadInt32();
		switch (num)
		{
		case 1:
		case 2:
		case 3:
		{
			int num3 = binaryReader.ReadInt32();
			for (int n = 0; n < num3; n++)
			{
				MonsterSpawnPoint monsterSpawnPoint2 = new MonsterSpawnPoint();
				monsterSpawnPoint2.Deserialize(num, binaryReader);
				mMsts.Add(monsterSpawnPoint2.ID, monsterSpawnPoint2);
			}
			num3 = binaryReader.ReadInt32();
			for (int num4 = 0; num4 < num3; num4++)
			{
				MonsterSpawnArea monsterSpawnArea2 = new MonsterSpawnArea();
				monsterSpawnArea2.Deserialize(num, binaryReader);
				mMstAreas.Add(monsterSpawnArea2.ID, monsterSpawnArea2);
			}
			num3 = binaryReader.ReadInt32();
			for (int num5 = 0; num5 < num3; num5++)
			{
				NPCSpawnPoint nPCSpawnPoint2 = new NPCSpawnPoint();
				nPCSpawnPoint2.Deserialize(num, binaryReader);
				mNpcs.Add(nPCSpawnPoint2.ID, nPCSpawnPoint2);
			}
			num3 = binaryReader.ReadInt32();
			for (int num6 = 0; num6 < num3; num6++)
			{
				DoodadSpawnPoint doodadSpawnPoint2 = new DoodadSpawnPoint();
				doodadSpawnPoint2.Deserialize(num, binaryReader);
				mDoodads.Add(doodadSpawnPoint2.ID, doodadSpawnPoint2);
			}
			num3 = binaryReader.ReadInt32();
			for (int num7 = 0; num7 < num3; num7++)
			{
				ItemSpwanPoint itemSpwanPoint2 = new ItemSpwanPoint();
				itemSpwanPoint2.Deserialize(num, binaryReader);
				mItems.Add(itemSpwanPoint2.ID, itemSpwanPoint2);
			}
			break;
		}
		case 4:
		{
			mMaxSpawnPointId = binaryReader.ReadInt32();
			int num2 = binaryReader.ReadInt32();
			for (int i = 0; i < num2; i++)
			{
				MonsterSpawnPoint monsterSpawnPoint = new MonsterSpawnPoint();
				monsterSpawnPoint.Deserialize(num, binaryReader);
				mMsts.Add(monsterSpawnPoint.ID, monsterSpawnPoint);
			}
			num2 = binaryReader.ReadInt32();
			for (int j = 0; j < num2; j++)
			{
				MonsterSpawnArea monsterSpawnArea = new MonsterSpawnArea();
				monsterSpawnArea.Deserialize(num, binaryReader);
				mMstAreas.Add(monsterSpawnArea.ID, monsterSpawnArea);
			}
			num2 = binaryReader.ReadInt32();
			for (int k = 0; k < num2; k++)
			{
				NPCSpawnPoint nPCSpawnPoint = new NPCSpawnPoint();
				nPCSpawnPoint.Deserialize(num, binaryReader);
				mNpcs.Add(nPCSpawnPoint.ID, nPCSpawnPoint);
			}
			num2 = binaryReader.ReadInt32();
			for (int l = 0; l < num2; l++)
			{
				DoodadSpawnPoint doodadSpawnPoint = new DoodadSpawnPoint();
				doodadSpawnPoint.Deserialize(num, binaryReader);
				mDoodads.Add(doodadSpawnPoint.ID, doodadSpawnPoint);
			}
			num2 = binaryReader.ReadInt32();
			for (int m = 0; m < num2; m++)
			{
				ItemSpwanPoint itemSpwanPoint = new ItemSpwanPoint();
				itemSpwanPoint.Deserialize(num, binaryReader);
				mItems.Add(itemSpwanPoint.ID, itemSpwanPoint);
			}
			break;
		}
		}
	}
}
