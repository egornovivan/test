using System.Collections.Generic;
using Mono.Data.SqliteClient;
using PETools;

namespace Pathea;

public class NpcVoiceDb
{
	public class Item
	{
		[DbReader.DbField("ID", false)]
		public int _Id;

		[DbReader.DbField("ScenarioID", false)]
		public int _SecnarioID;

		public int[] VoiceType;

		[DbReader.DbField("V1", false)]
		public int _V1
		{
			set
			{
				VoiceType = new int[50];
				VoiceType[0] = value;
			}
		}

		[DbReader.DbField("V2", false)]
		public int _V2
		{
			set
			{
				VoiceType[1] = value;
			}
		}

		[DbReader.DbField("V3", false)]
		public int _V3
		{
			set
			{
				VoiceType[2] = value;
			}
		}

		[DbReader.DbField("V4", false)]
		public int _V4
		{
			set
			{
				VoiceType[3] = value;
			}
		}

		[DbReader.DbField("V5", false)]
		public int _V5
		{
			set
			{
				VoiceType[4] = value;
			}
		}

		[DbReader.DbField("V6", false)]
		public int _V6
		{
			set
			{
				VoiceType[5] = value;
			}
		}

		[DbReader.DbField("V7", false)]
		public int _V7
		{
			set
			{
				VoiceType[6] = value;
			}
		}

		[DbReader.DbField("V8", false)]
		public int _V8
		{
			set
			{
				VoiceType[7] = value;
			}
		}

		[DbReader.DbField("V9", false)]
		public int _V9
		{
			set
			{
				VoiceType[8] = value;
			}
		}

		[DbReader.DbField("V10", false)]
		public int _V10
		{
			set
			{
				VoiceType[9] = value;
			}
		}

		[DbReader.DbField("V11", false)]
		public int _V11
		{
			set
			{
				VoiceType[10] = value;
			}
		}

		[DbReader.DbField("V12", false)]
		public int _V12
		{
			set
			{
				VoiceType[11] = value;
			}
		}

		[DbReader.DbField("V13", false)]
		public int _V13
		{
			set
			{
				VoiceType[12] = value;
			}
		}

		[DbReader.DbField("V14", false)]
		public int _V14
		{
			set
			{
				VoiceType[13] = value;
			}
		}

		[DbReader.DbField("V15", false)]
		public int _V15
		{
			set
			{
				VoiceType[14] = value;
			}
		}

		[DbReader.DbField("V16", false)]
		public int _V16
		{
			set
			{
				VoiceType[15] = value;
			}
		}

		[DbReader.DbField("V17", false)]
		public int _V17
		{
			set
			{
				VoiceType[16] = value;
			}
		}

		[DbReader.DbField("V18", false)]
		public int _V18
		{
			set
			{
				VoiceType[17] = value;
			}
		}

		[DbReader.DbField("V19", false)]
		public int _V19
		{
			set
			{
				VoiceType[18] = value;
			}
		}

		[DbReader.DbField("V20", false)]
		public int _V20
		{
			set
			{
				VoiceType[19] = value;
			}
		}

		[DbReader.DbField("V21", false)]
		public int _V21
		{
			set
			{
				VoiceType[20] = value;
			}
		}

		[DbReader.DbField("V22", false)]
		public int _V22
		{
			set
			{
				VoiceType[21] = value;
			}
		}

		[DbReader.DbField("V23", false)]
		public int _V23
		{
			set
			{
				VoiceType[22] = value;
			}
		}

		[DbReader.DbField("V24", false)]
		public int _V24
		{
			set
			{
				VoiceType[23] = value;
			}
		}

		[DbReader.DbField("V25", false)]
		public int _V25
		{
			set
			{
				VoiceType[24] = value;
			}
		}

		[DbReader.DbField("V26", false)]
		public int _V26
		{
			set
			{
				VoiceType[25] = value;
			}
		}

		[DbReader.DbField("V27", false)]
		public int _V27
		{
			set
			{
				VoiceType[26] = value;
			}
		}

		[DbReader.DbField("V28", false)]
		public int _V28
		{
			set
			{
				VoiceType[27] = value;
			}
		}

		[DbReader.DbField("V29", false)]
		public int _V29
		{
			set
			{
				VoiceType[28] = value;
			}
		}

		[DbReader.DbField("V30", false)]
		public int _V30
		{
			set
			{
				VoiceType[29] = value;
			}
		}

		[DbReader.DbField("V31", false)]
		public int _V31
		{
			set
			{
				VoiceType[30] = value;
			}
		}

		[DbReader.DbField("V32", false)]
		public int _V32
		{
			set
			{
				VoiceType[31] = value;
			}
		}

		[DbReader.DbField("V33", false)]
		public int _V33
		{
			set
			{
				VoiceType[32] = value;
			}
		}

		[DbReader.DbField("V34", false)]
		public int _V34
		{
			set
			{
				VoiceType[33] = value;
			}
		}

		[DbReader.DbField("V35", false)]
		public int _V35
		{
			set
			{
				VoiceType[34] = value;
			}
		}

		[DbReader.DbField("V36", false)]
		public int _V36
		{
			set
			{
				VoiceType[35] = value;
			}
		}

		[DbReader.DbField("V37", false)]
		public int _V37
		{
			set
			{
				VoiceType[36] = value;
			}
		}

		[DbReader.DbField("V38", false)]
		public int _V38
		{
			set
			{
				VoiceType[37] = value;
			}
		}

		[DbReader.DbField("V39", false)]
		public int _V39
		{
			set
			{
				VoiceType[38] = value;
			}
		}
	}

	private static Dictionary<int, Item> mData;

	public static void LoadData()
	{
		mData = new Dictionary<int, Item>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("NPCVoice");
		while (sqliteDataReader.Read())
		{
			Item item = DbReader.ReadItem<Item>(sqliteDataReader);
			mData.Add(item._SecnarioID, item);
		}
	}

	public static void Release()
	{
		mData = null;
	}

	public static int GetVoiceId(int secnarioID, int voiceType)
	{
		return (mData[secnarioID] == null || voiceType <= 0) ? (-1) : mData[secnarioID].VoiceType[voiceType - 1];
	}
}
