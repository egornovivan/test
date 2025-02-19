using System.Collections.Generic;

public class DungeonIsos
{
	public enum IsoState
	{
		IsoState_None,
		IsoState_Exporting,
		IsoState_Exported
	}

	public class DungeonIsoData
	{
		public ulong _fileId;

		public ulong _publishId;

		public IsoState _export;

		public ulong _hashCode;
	}

	public int _amount;

	public int _dungeonId;

	public List<DungeonIsoData> _isos = new List<DungeonIsoData>();

	public static List<DungeonIsos> DungeonIsosMgr = new List<DungeonIsos>();

	public DungeonIsos(int amount, int dungeonId)
	{
		_amount = amount;
		_dungeonId = dungeonId;
	}

	public static bool AddItem(int amount, int dungenoId)
	{
		foreach (DungeonIsos item2 in DungeonIsosMgr)
		{
			if (item2._dungeonId == dungenoId)
			{
				return false;
			}
		}
		DungeonIsos item = new DungeonIsos(amount, dungenoId);
		DungeonIsosMgr.Add(item);
		return true;
	}

	public static bool AddIsos(int dungeonId, ulong[] fileIds, ulong[] publishIds)
	{
		foreach (DungeonIsos item in DungeonIsosMgr)
		{
			if (item._dungeonId == dungeonId && item._isos.Count == 0)
			{
				for (int i = 0; i < fileIds.Length; i++)
				{
					DungeonIsoData dungeonIsoData = new DungeonIsoData();
					dungeonIsoData._fileId = fileIds[i];
					dungeonIsoData._publishId = publishIds[i];
					item._isos.Add(dungeonIsoData);
				}
				return true;
			}
		}
		return false;
	}

	public static bool CheckExported(int dungeonId, int index)
	{
		foreach (DungeonIsos item in DungeonIsosMgr)
		{
			if (item._dungeonId == dungeonId)
			{
				if (item._isos[index]._export == IsoState.IsoState_None)
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}

	public static void SetIsoState(int dungeonId, int index, IsoState state)
	{
		foreach (DungeonIsos item in DungeonIsosMgr)
		{
			if (item._dungeonId == dungeonId)
			{
				item._isos[index]._export = state;
			}
		}
	}

	public static void SetHashCode(int dungeonId, int index, ulong hashcode)
	{
		foreach (DungeonIsos item in DungeonIsosMgr)
		{
			if (item._dungeonId == dungeonId)
			{
				item._isos[index]._hashCode = hashcode;
			}
		}
	}

	public static bool IsDungeonIso(ulong hashcode, out int dungeonId)
	{
		foreach (DungeonIsos item in DungeonIsosMgr)
		{
			foreach (DungeonIsoData iso in item._isos)
			{
				if (iso._hashCode == hashcode && iso._export == IsoState.IsoState_Exporting)
				{
					dungeonId = item._dungeonId;
					iso._export = IsoState.IsoState_Exported;
					return true;
				}
			}
		}
		dungeonId = -1;
		return false;
	}

	public static ulong GetFileId(int dungeonId, int index)
	{
		foreach (DungeonIsos item in DungeonIsosMgr)
		{
			if (item._dungeonId == dungeonId)
			{
				return item._isos[index]._fileId;
			}
		}
		return 0uL;
	}
}
