using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using Pathea;
using PETools;
using UnityEngine;

public class Camp
{
	public const int UndefinedId = 99999;

	public const string UndefinedName = "Undefined";

	private int _id;

	private int _nameID;

	private Vector3 _pos;

	private float _radius;

	private List<SleepPostion> _LayDatas;

	public EatDesc mEatInfo = new EatDesc();

	public Vector3 mTalkCenterPos = default(Vector3);

	public List<TimeSlot> mTalkTime = new List<TimeSlot>();

	public float mSleepTime;

	public float mWakeupTime;

	public List<int> m_PreLimit = new List<int>();

	public List<int> m_TalkList = new List<int>();

	public string[] mPaths;

	public int[] MedicalType;

	public int[] RepairType;

	public int[] ComputerType;

	public int[] TentType;

	private bool mCampIsActive;

	private bool mCampInAlert;

	private List<int> _NpcentityIds = new List<int>();

	private static List<Camp> _camps = new List<Camp>();

	public List<SleepPostion> LayDatas => _LayDatas;

	public List<int> NpcentityIds => _NpcentityIds;

	public int Id
	{
		get
		{
			return _id;
		}
		set
		{
			_id = value;
		}
	}

	public float Radius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = value;
		}
	}

	public Vector3 Pos
	{
		get
		{
			return _pos;
		}
		set
		{
			_pos = value;
		}
	}

	public bool CampIsActive
	{
		get
		{
			return mCampIsActive;
		}
		set
		{
			mCampIsActive = value;
		}
	}

	public string Name => PELocalization.GetString(_nameID);

	public List<Camp> Camps => _camps;

	public Camp()
	{
	}

	public Camp(int id, int nameID, Vector3 pos, float radius, List<int> assetIds)
	{
		_id = id;
		_nameID = nameID;
		_pos = pos;
		_radius = radius;
	}

	public string GetPath(int index)
	{
		if (mPaths == null || index >= mPaths.Length)
		{
			return null;
		}
		return mPaths[index];
	}

	public Vector3 GetObjectPostion(int asseId)
	{
		if (asseId == 0)
		{
			return Vector3.zero;
		}
		if (StoryDoodadMap.s_dicDoodadData != null)
		{
			return StoryDoodadMap.s_dicDoodadData[asseId]._pos;
		}
		return Vector3.zero;
	}

	public void AddNpcIntoCamp(int enityId)
	{
		if (_id != 1 && !_NpcentityIds.Contains(enityId) && (PeSingleton<PeCreature>.Instance == null || !(PeSingleton<PeCreature>.Instance.mainPlayer != null) || PeSingleton<PeCreature>.Instance.mainPlayer.Id != enityId))
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(enityId);
			if (!(peEntity == null) && peEntity.entityProto != null && peEntity.entityProto.proto != 0 && peEntity.entityProto.proto != EEntityProto.Monster && peEntity.entityProto.proto != EEntityProto.Tower)
			{
				_NpcentityIds.Add(enityId);
				UpdateNpcAlert(mCampInAlert);
			}
		}
	}

	public Vector3 CalculatePostion(int SelfId, Vector3 SelfPos, float Radius)
	{
		foreach (int npcentityId in _NpcentityIds)
		{
			if (npcentityId != SelfId)
			{
				PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(npcentityId);
				if (peEntity != null && !peEntity.Equals(this) && (SelfPos - peEntity.position).sqrMagnitude >= 1f && (SelfPos - peEntity.position).sqrMagnitude < Radius * Radius)
				{
					return peEntity.position;
				}
			}
		}
		return Vector3.zero;
	}

	public bool CalculatePostion(PeEntity Self, float Radius)
	{
		PeEntity Target = null;
		foreach (int npcentityId in _NpcentityIds)
		{
			if (npcentityId == Self.Id || !GetRoundNpc(Self.Id, npcentityId, Radius, out Target))
			{
				continue;
			}
			Self.NpcCmpt.ChatTarget = Target;
			return true;
		}
		Self.NpcCmpt.ChatTarget = null;
		return false;
	}

	public bool CalculatePostion(PeEntity Self, float Radius, out PeEntity _target)
	{
		foreach (int npcentityId in _NpcentityIds)
		{
			if (npcentityId == Self.Id || !GetRoundNpc(Self.Id, npcentityId, Radius, out _target))
			{
				continue;
			}
			return true;
		}
		_target = null;
		return false;
	}

	private bool GetRoundNpc(int slefId, int targetId, float radius, out PeEntity Target)
	{
		Target = null;
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(slefId);
		if (null == peEntity)
		{
			return false;
		}
		PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get(targetId);
		if (null == peEntity2)
		{
			return false;
		}
		float num = PEUtil.SqrMagnitudeH(peEntity.peTrans.position, peEntity2.peTrans.position);
		if (num < radius * radius)
		{
			float num2 = Vector3.Angle(peEntity.peTrans.trans.forward, peEntity2.peTrans.trans.forward);
			if (num2 > -90f && num2 < 90f)
			{
				Target = peEntity2;
				return true;
			}
		}
		return false;
	}

	public bool CantainTarget(float _radiu, PeEntity self, PeEntity target)
	{
		if ((self.position - target.position).sqrMagnitude >= 1f && (self.position - target.position).sqrMagnitude < _radiu * _radiu)
		{
			return true;
		}
		return false;
	}

	public void RemoveFromCamp(int enityId)
	{
		if (_NpcentityIds != null && _NpcentityIds.Contains(enityId))
		{
			_NpcentityIds.Remove(enityId);
		}
	}

	public SleepPostion HasSleep(int enityid)
	{
		for (int i = 0; i < LayDatas.Count; i++)
		{
			if (LayDatas[i]._Id == enityid)
			{
				return LayDatas[i];
			}
		}
		return null;
	}

	public int[] GetPosByType(EPosType type)
	{
		return type switch
		{
			EPosType.Medical => MedicalType, 
			EPosType.Repair => RepairType, 
			EPosType.Computer => ComputerType, 
			EPosType.Tent => TentType, 
			_ => null, 
		};
	}

	public void SetCampNpcAlert(bool value)
	{
		mCampInAlert = value;
		UpdateNpcAlert(value);
	}

	public void UpdateNpcAlert(bool value)
	{
		if (_NpcentityIds == null)
		{
			return;
		}
		for (int i = 0; i < _NpcentityIds.Count; i++)
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(_NpcentityIds[i]);
			if (peEntity != null)
			{
				peEntity.SetNpcAlert(value);
			}
		}
	}

	public static Camp GetCamp(int id)
	{
		int count = _camps.Count;
		for (int i = 0; i < count; i++)
		{
			if (_camps[i].Id == id)
			{
				return _camps[i];
			}
		}
		return null;
	}

	public static Camp GetCamp(Vector3 pos)
	{
		int count = _camps.Count;
		for (int i = 0; i < count; i++)
		{
			if (_camps[i].CampIsActive && (_camps[i].Pos - pos).sqrMagnitude < _camps[i].Radius * _camps[i].Radius)
			{
				return _camps[i];
			}
		}
		return null;
	}

	public static bool SetCampActive(int campid, bool isActive)
	{
		Camp camp = GetCamp(campid);
		if (camp == null)
		{
			return false;
		}
		camp.CampIsActive = isActive;
		return true;
	}

	public static void LoadData()
	{
		_camps.Clear();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("sceneCampList");
		while (sqliteDataReader.Read())
		{
			int id = Convert.ToInt32(sqliteDataReader.GetString(0));
			int nameID = Convert.ToInt32(sqliteDataReader.GetString(1));
			string[] array = sqliteDataReader.GetString(2).Split(',');
			Vector3 pos = new Vector3(Convert.ToSingle(array[0]), Convert.ToSingle(array[1]), Convert.ToSingle(array[2]));
			float radius = Convert.ToSingle(sqliteDataReader.GetString(3));
			List<int> assetIds = StrToCampAssetDesc(sqliteDataReader.GetString(4));
			Camp camp = new Camp(id, nameID, pos, radius, assetIds);
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Sleep"));
			string[] array2 = @string.Split('_');
			if (array2.Length == 2)
			{
				camp.mSleepTime = Convert.ToSingle(array2[0]) + UnityEngine.Random.Range(0.1f, 0.2f);
				camp.mWakeupTime = Convert.ToSingle(array2[1]) + UnityEngine.Random.Range(0.1f, 0.2f);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Eat"));
			if (@string != "0")
			{
				array2 = @string.Split(':');
				camp.mEatInfo.assesID = Convert.ToInt32(array2[0]);
				string[] array3 = array2[1].Split(',');
				for (int i = 0; i < array3.Length; i++)
				{
					if (!(array3[i] == "0"))
					{
						string[] array4 = array3[i].Split('_');
						if (array4.Length == 2)
						{
							camp.mEatInfo.Eattimes.Add(new TimeSlot(Convert.ToSingle(array4[0]), Convert.ToSingle(array4[1])));
						}
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("SleepPos"));
			camp._LayDatas = StrToSleepPostion(@string);
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PatrolPos"));
			if (@string != "0")
			{
				camp.mPaths = @string.Split(';');
			}
			camp.MedicalType = PEUtil.ToArrayInt32(sqliteDataReader.GetString(9), ',');
			camp.RepairType = PEUtil.ToArrayInt32(sqliteDataReader.GetString(10), ',');
			camp.ComputerType = PEUtil.ToArrayInt32(sqliteDataReader.GetString(11), ',');
			camp.TentType = PEUtil.ToArrayInt32(sqliteDataReader.GetString(12), ',');
			camp.CampIsActive = Convert.ToBoolean(PEUtil.ToArrayByte(sqliteDataReader.GetString(13), ',')[0]);
			_camps.Add(camp);
			CampPathDb.LoadData(camp.mPaths);
		}
	}

	private static List<SleepPostion> StrToSleepPostion(string _str)
	{
		string[] array = _str.Split(';');
		List<SleepPostion> list = new List<SleepPostion>();
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(':');
			if (array2.Length == 2)
			{
				string[] array3 = array2[0].Split(',');
				string[] array4 = array2[1].Split('_');
				if (array4.Length == 2)
				{
					string[] array5 = array4[0].Split(',');
					SleepPostion sleepPostion = new SleepPostion();
					sleepPostion._Doorpos = new Vector3(Convert.ToSingle(array3[0]), Convert.ToSingle(array3[1]), Convert.ToSingle(array3[2]));
					sleepPostion._Pos = new Vector3(Convert.ToSingle(array5[0]), Convert.ToSingle(array5[1]), Convert.ToSingle(array5[2]));
					sleepPostion._Rate = Convert.ToSingle(array4[1]);
					sleepPostion.Occpyied = false;
					sleepPostion._Id = 0;
					list.Add(sleepPostion);
				}
			}
		}
		return list;
	}

	private static Vector4 StrToCampPosScope(string strPosScopeDesc)
	{
		Vector4 result = default(Vector4);
		string[] array = strPosScopeDesc.Split(',');
		if (array.Length < 4)
		{
			Debug.LogError("Unexpected CampPosScopeDesc string." + strPosScopeDesc);
			return result;
		}
		result.x = Convert.ToSingle(array[0]);
		result.y = Convert.ToSingle(array[1]);
		result.z = Convert.ToSingle(array[2]);
		result.w = Convert.ToSingle(array[3]);
		return result;
	}

	private static List<int> StrToCampAssetDesc(string strAssetsDescs)
	{
		List<int> list = new List<int>();
		string[] array = strAssetsDescs.Split(',');
		string[] array2 = array;
		foreach (string value in array2)
		{
			list.Add(Convert.ToInt32(value));
		}
		return list;
	}
}
