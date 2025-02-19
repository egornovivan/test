using System.Collections.Generic;
using Mono.Data.SqliteClient;
using PETools;

namespace Pathea;

public class NpcTypeDb
{
	public class Item
	{
		[DbReader.DbField("TypeID", false)]
		public int _id;

		[DbReader.DbField("TypeName", false)]
		public string _name;

		private int[] mControlInfo = new int[25];

		[DbReader.DbField("Leisure", false)]
		private int _Leisure
		{
			set
			{
				mControlInfo[1] = value;
			}
		}

		[DbReader.DbField("Interaction", false)]
		private int _Interaction
		{
			set
			{
				mControlInfo[2] = value;
			}
		}

		[DbReader.DbField("Stroll", false)]
		private int _Stroll
		{
			set
			{
				mControlInfo[3] = value;
			}
		}

		[DbReader.DbField("Patrol", false)]
		private int _Patrol
		{
			set
			{
				mControlInfo[4] = value;
			}
		}

		[DbReader.DbField("Guard", false)]
		private int _Guard
		{
			set
			{
				mControlInfo[5] = value;
			}
		}

		[DbReader.DbField("Dining", false)]
		private int _Dining
		{
			set
			{
				mControlInfo[6] = value;
			}
		}

		[DbReader.DbField("Sleep", false)]
		private int _Sleep
		{
			set
			{
				mControlInfo[7] = value;
			}
		}

		[DbReader.DbField("MoveTo", false)]
		private int _MoveTo
		{
			set
			{
				mControlInfo[8] = value;
			}
		}

		[DbReader.DbField("Work", false)]
		private int _Work
		{
			set
			{
				mControlInfo[10] = value;
			}
		}

		[DbReader.DbField("Cure", false)]
		private int _Cure
		{
			set
			{
				mControlInfo[9] = value;
			}
		}

		[DbReader.DbField("ChangeRole", false)]
		private int _ChangeRole
		{
			set
			{
				mControlInfo[11] = value;
			}
		}

		[DbReader.DbField("AddHatred", false)]
		private int _AddHatred
		{
			set
			{
				mControlInfo[12] = value;
			}
		}

		[DbReader.DbField("ReceiveHatred", false)]
		private int _ReceiveHatred
		{
			set
			{
				mControlInfo[13] = value;
			}
		}

		[DbReader.DbField("InjuredHatred", false)]
		private int _InjuredHatred
		{
			set
			{
				mControlInfo[14] = value;
			}
		}

		[DbReader.DbField("SelfDefense", false)]
		private int _SelfDefense
		{
			set
			{
				mControlInfo[15] = value;
			}
		}

		[DbReader.DbField("Pursuit", false)]
		private int _Pursuit
		{
			set
			{
				mControlInfo[16] = value;
			}
		}

		[DbReader.DbField("Assist", false)]
		private int _Assist
		{
			set
			{
				mControlInfo[17] = value;
			}
		}

		[DbReader.DbField("Recourse", false)]
		private int _Recourse
		{
			set
			{
				mControlInfo[18] = value;
			}
		}

		[DbReader.DbField("Attack", false)]
		private int Attack
		{
			set
			{
				mControlInfo[19] = value;
			}
		}

		[DbReader.DbField("Dodge", false)]
		private int _Dodge
		{
			set
			{
				mControlInfo[20] = value;
			}
		}

		[DbReader.DbField("Block", false)]
		private int _Block
		{
			set
			{
				mControlInfo[21] = value;
			}
		}

		[DbReader.DbField("CanTalk", false)]
		private int _CanTalk
		{
			set
			{
				mControlInfo[22] = value;
			}
		}

		[DbReader.DbField("CanHanded", false)]
		private int _CanHanded
		{
			set
			{
				mControlInfo[23] = value;
			}
		}

		public bool CanRun(ENpcControlType type)
		{
			return mControlInfo[(int)type] != 0;
		}
	}

	private static List<Item> sList;

	public static void Load()
	{
		sList = new List<Item>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("NPCType");
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
		int count = sList.Count;
		for (int i = 0; i < count; i++)
		{
			if (sList[i]._id == id)
			{
				return sList[i];
			}
		}
		return null;
	}

	public static bool CanRun(int typeId, ENpcControlType type)
	{
		return Get(typeId)?.CanRun(type) ?? false;
	}
}
