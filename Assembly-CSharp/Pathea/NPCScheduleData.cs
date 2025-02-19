using System.Collections.Generic;
using Mono.Data.SqliteClient;
using PETools;

namespace Pathea;

public class NPCScheduleData
{
	public class Item
	{
		[DbReader.DbField("ID", false)]
		public int id;

		[DbReader.DbField("team", false)]
		public int team;

		public List<CheckSlot> Slots = new List<CheckSlot>();

		[DbReader.DbField("time", false)]
		public string time
		{
			set
			{
				LoadSlots(value);
			}
		}

		public EScheduleType ScheduleType => (EScheduleType)id;

		private void LoadSlots(string str)
		{
			if (!(str != string.Empty))
			{
				return;
			}
			string[] array = PEUtil.ToArrayString(str, ',');
			string[] array2 = array;
			foreach (string str2 in array2)
			{
				float[] array3 = PEUtil.ToArraySingle(str2, '_');
				if (array3.Length == 2)
				{
					CheckSlot item = new CheckSlot(array3[0], array3[1]);
					Slots.Add(item);
				}
			}
		}
	}

	private static List<Item> sList;

	public static void Load()
	{
		sList = new List<Item>(4);
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("NPCSchedule");
		while (sqliteDataReader.Read())
		{
			Item item = DbReader.ReadItem<Item>(sqliteDataReader);
			sList.Add(item);
		}
	}

	public static void Release()
	{
		sList = null;
	}

	public static Item Get(int id)
	{
		return sList.Find((Item item) => (item.id == id) ? true : false);
	}
}
