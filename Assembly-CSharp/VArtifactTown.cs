using System.Collections.Generic;
using System.Linq;
using TownData;
using UnityEngine;
using VANativeCampXML;
using VArtifactTownXML;

public class VArtifactTown
{
	public int templateId;

	public int townId;

	public int allyId;

	public int genAllyId;

	public bool isMainTown;

	public int townNameId;

	public IntVector2 PosGen;

	public IntVector2 PosStart;

	public int areaId;

	public int level;

	public IntVector2 PosEnd;

	public IntVector2 PosCenter;

	public List<IntVector2> occupiedTile;

	public List<VArtifactUnit> VAUnits = new List<VArtifactUnit>();

	public int buildingNo;

	public bool isRandomed;

	public VArtifactType type;

	public NativeType nativeType = NativeType.Paja;

	public int ms_id;

	public double lastHour;

	public double nextHour;

	public float lastCheckTime;

	public bool isEmpty;

	public NativeTower nativeTower;

	public int height = 1;

	public int radius;

	public int AllyColorId => VATownGenerator.Instance.GetAllyColor(allyId);

	public int AllyId
	{
		get
		{
			return allyId;
		}
		set
		{
			allyId = value;
			genAllyId = value;
		}
	}

	public bool IsPlayerTown => allyId == 0;

	public IntVector2 PosEntrance => VAUnits[0].PosEntrance;

	public IntVector2 PosEntranceLeft => VAUnits[0].PosEntrance + new IntVector2(-radius, 0);

	public IntVector2 PosEntranceRight => VAUnits[0].PosEntrance + new IntVector2(radius, 0);

	public VArtifactType Type
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
		}
	}

	public List<VATownNpcInfo> npcList => VAUnits[0].npcPosInfo.Values.ToList();

	public int Height => height;

	public Vector3 TransPos => new Vector3(PosCenter.x, height + 2, PosCenter.y);

	public int SmallRadius => Mathf.Max(Mathf.Abs(PosEnd.x - PosStart.x), Mathf.Abs(PosEnd.y - PosStart.y)) / 2 - 8;

	public int MiddleRadius => Mathf.Max(Mathf.Abs(PosEnd.x - PosStart.x), Mathf.Abs(PosEnd.y - PosStart.y)) / 2;

	public bool IsExplored { get; set; }

	public VArtifactTown()
	{
	}

	public VArtifactTown(VATown vat, IntVector2 posGen)
	{
		type = VArtifactType.NpcTown;
		templateId = vat.tid;
		level = vat.level;
		PosGen = posGen;
	}

	public VArtifactTown(NativeCamp nc, IntVector2 posGen)
	{
		type = VArtifactType.NativeCamp;
		templateId = nc.cid;
		level = nc.level;
		nativeTower = nc.nativeTower;
		PosGen = posGen;
		nativeType = (NativeType)nc.nativeType;
	}

	public void SetMsId(int ms_id)
	{
		this.ms_id = ms_id;
		VArtifactTownManager.Instance.SetSaveData(townId, ms_id);
	}

	public void RandomSiege(float minHour, float maxHour)
	{
		lastHour = GameTime.Timer.Hour;
		nextHour = Random.Range(minHour, maxHour);
		VArtifactTownManager.Instance.SetSaveData(townId, lastHour, nextHour);
	}

	public bool InYArea(float y)
	{
		return y >= VAUnits[0].worldPos.y && y < (float)(height + 50);
	}

	public static VArtifactTown GetStandTown(Vector3 pos)
	{
		float num = float.MaxValue;
		VArtifactTown vArtifactTown = null;
		foreach (VArtifactTown value in VArtifactTownManager.Instance.townPosInfo.Values)
		{
			if (value.type == VArtifactType.NpcTown)
			{
				float magnitude = (pos - value.TransPos).magnitude;
				if (magnitude < num)
				{
					num = magnitude;
					vArtifactTown = value;
				}
			}
		}
		if (vArtifactTown != null && num < (float)vArtifactTown.MiddleRadius)
		{
			return vArtifactTown;
		}
		return null;
	}
}
