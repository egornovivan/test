using System.Collections.Generic;
using System.IO;
using CSRecord;
using UnityEngine;

public class CSRecordMgr
{
	public const int VERSION = 259;

	private static CSRecordMgr s_Instance;

	public Dictionary<int, CSObjectData> m_ObjectDatas;

	public Dictionary<int, CSPersonnelData> m_PersonnelDatas;

	public static CSRecordMgr Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = new CSRecordMgr();
			}
			return s_Instance;
		}
	}

	public CSRecordMgr()
	{
		m_ObjectDatas = new Dictionary<int, CSObjectData>();
		m_PersonnelDatas = new Dictionary<int, CSPersonnelData>();
	}

	public bool AssignData(int id, int type, ref CSDefaultData refData)
	{
		if (type == 50)
		{
			if (m_PersonnelDatas.ContainsKey(id))
			{
				Debug.Log("The Personnel Data ID [" + id + "] is exist.");
				refData = m_PersonnelDatas[id];
				return false;
			}
			refData = new CSPersonnelData();
			refData.ID = id;
			m_PersonnelDatas.Add(id, refData as CSPersonnelData);
			return true;
		}
		if (m_ObjectDatas.ContainsKey(id))
		{
			Debug.Log("The Object data ID [" + id + "] is exist.");
			refData = m_ObjectDatas[id];
			return false;
		}
		switch (type)
		{
		case 1:
			refData = new CSAssemblyData();
			break;
		case 2:
			refData = new CSStorageData();
			break;
		case 3:
			refData = new CSEngineerData();
			break;
		case 4:
			refData = new CSEnhanceData();
			break;
		case 5:
			refData = new CSRepairData();
			break;
		case 6:
			refData = new CSRecycleData();
			break;
		case 33:
			refData = new CSPPCoalData();
			break;
		case 21:
			refData = new CSDwellingsData();
			break;
		case 8:
			refData = new CSFactoryData();
			break;
		case 9:
			refData = new CSProcessingData();
			break;
		default:
			refData = new CSObjectData();
			break;
		}
		refData.ID = id;
		m_ObjectDatas.Add(id, refData as CSObjectData);
		return true;
	}

	public void RemoveData(int id, int type)
	{
		if (type == 50)
		{
			RemovePersonnelData(id);
		}
		else
		{
			RemoveObjectData(id);
		}
	}

	public void RemoveObjectData(int id)
	{
		if (!m_ObjectDatas.ContainsKey(id))
		{
			Debug.LogWarning("You want to remove a object data, but it not exist!");
		}
		else
		{
			m_ObjectDatas.Remove(id);
		}
	}

	public void RemovePersonnelData(int id)
	{
		if (!m_PersonnelDatas.ContainsKey(id))
		{
			Debug.LogWarning("You want to remove a Personnel data, but it not exist!");
		}
		else
		{
			m_PersonnelDatas.Remove(id);
		}
	}

	public Dictionary<int, CSObjectData>.ValueCollection GetObjectRecords()
	{
		return m_ObjectDatas.Values;
	}

	public Dictionary<int, CSPersonnelData>.ValueCollection GetPersonnelRecords()
	{
		return m_PersonnelDatas.Values;
	}

	public void ClearData()
	{
		m_ObjectDatas.Clear();
		m_PersonnelDatas.Clear();
	}

	public void Import(byte[] buffer)
	{
		if (buffer == null || buffer.Length < 8)
		{
			return;
		}
		MemoryStream input = new MemoryStream(buffer);
		BinaryReader binaryReader = new BinaryReader(input);
		int num = binaryReader.ReadInt32();
		if (num != 259)
		{
			Debug.LogWarning("The version of ColonyrecordMgr is newer than the record.");
		}
		switch (num)
		{
		case 257:
		{
			int num5 = binaryReader.ReadInt32();
			for (int m = 0; m < num5; m++)
			{
				CSObjectData cSObjectData2 = null;
				switch (binaryReader.ReadInt32())
				{
				case 1:
				{
					cSObjectData2 = new CSAssemblyData();
					CSAssemblyData cSAssemblyData2 = cSObjectData2 as CSAssemblyData;
					_readCSObjectData(binaryReader, cSAssemblyData2, num);
					cSAssemblyData2.m_Level = binaryReader.ReadInt32();
					cSAssemblyData2.m_UpgradeTime = binaryReader.ReadSingle();
					cSAssemblyData2.m_CurUpgradeTime = binaryReader.ReadSingle();
					break;
				}
				case 33:
				{
					cSObjectData2 = new CSPPCoalData();
					CSPPCoalData cSPPCoalData2 = cSObjectData2 as CSPPCoalData;
					_readCSObjectData(binaryReader, cSPPCoalData2, num);
					cSPPCoalData2.m_CurWorkedTime = binaryReader.ReadSingle();
					cSPPCoalData2.m_WorkedTime = binaryReader.ReadSingle();
					break;
				}
				case 2:
				{
					cSObjectData2 = new CSStorageData();
					CSStorageData cSStorageData2 = cSObjectData2 as CSStorageData;
					_readCSObjectData(binaryReader, cSStorageData2, num);
					int num6 = binaryReader.ReadInt32();
					for (int n = 0; n < num6; n++)
					{
						cSStorageData2.m_Items.Add(binaryReader.ReadInt32(), binaryReader.ReadInt32());
					}
					break;
				}
				case 3:
				{
					cSObjectData2 = new CSEngineerData();
					CSEngineerData cSEngineerData2 = cSObjectData2 as CSEngineerData;
					_readCSObjectData(binaryReader, cSEngineerData2, num);
					cSEngineerData2.m_EnhanceItemID = binaryReader.ReadInt32();
					cSEngineerData2.m_CurEnhanceTime = binaryReader.ReadSingle();
					cSEngineerData2.m_EnhanceTime = binaryReader.ReadSingle();
					cSEngineerData2.m_PatchItemID = binaryReader.ReadInt32();
					cSEngineerData2.m_CurPatchTime = binaryReader.ReadSingle();
					cSEngineerData2.m_PatchTime = binaryReader.ReadSingle();
					cSEngineerData2.m_RecycleItemID = binaryReader.ReadInt32();
					cSEngineerData2.m_CurRecycleTime = binaryReader.ReadSingle();
					cSEngineerData2.m_RecycleTime = binaryReader.ReadSingle();
					break;
				}
				case 4:
				{
					cSObjectData2 = new CSEnhanceData();
					CSEnhanceData cSEnhanceData2 = cSObjectData2 as CSEnhanceData;
					_readCSObjectData(binaryReader, cSEnhanceData2, num);
					cSEnhanceData2.m_ObjID = binaryReader.ReadInt32();
					cSEnhanceData2.m_CurTime = binaryReader.ReadSingle();
					cSEnhanceData2.m_Time = binaryReader.ReadSingle();
					break;
				}
				case 5:
				{
					cSObjectData2 = new CSRepairData();
					CSRepairData cSRepairData2 = cSObjectData2 as CSRepairData;
					_readCSObjectData(binaryReader, cSRepairData2, num);
					cSRepairData2.m_ObjID = binaryReader.ReadInt32();
					cSRepairData2.m_CurTime = binaryReader.ReadSingle();
					cSRepairData2.m_Time = binaryReader.ReadSingle();
					break;
				}
				case 6:
				{
					cSObjectData2 = new CSRecycleData();
					CSRecycleData cSRecycleData2 = cSObjectData2 as CSRecycleData;
					_readCSObjectData(binaryReader, cSRecycleData2, num);
					cSRecycleData2.m_ObjID = binaryReader.ReadInt32();
					cSRecycleData2.m_CurTime = binaryReader.ReadSingle();
					cSRecycleData2.m_Time = binaryReader.ReadSingle();
					break;
				}
				case 21:
				{
					cSObjectData2 = new CSDwellingsData();
					CSDwellingsData csod2 = cSObjectData2 as CSDwellingsData;
					_readCSObjectData(binaryReader, csod2, num);
					break;
				}
				default:
					cSObjectData2 = new CSObjectData();
					break;
				}
				m_ObjectDatas.Add(cSObjectData2.ID, cSObjectData2);
			}
			num5 = binaryReader.ReadInt32();
			for (int num7 = 0; num7 < num5; num7++)
			{
				CSPersonnelData cSPersonnelData2 = new CSPersonnelData();
				cSPersonnelData2.ID = binaryReader.ReadInt32();
				cSPersonnelData2.dType = binaryReader.ReadInt32();
				cSPersonnelData2.m_State = binaryReader.ReadInt32();
				cSPersonnelData2.m_DwellingsID = binaryReader.ReadInt32();
				cSPersonnelData2.m_WorkRoomID = binaryReader.ReadInt32();
				m_PersonnelDatas.Add(cSPersonnelData2.ID, cSPersonnelData2);
			}
			break;
		}
		case 258:
		case 259:
		{
			int num2 = binaryReader.ReadInt32();
			for (int i = 0; i < num2; i++)
			{
				CSObjectData cSObjectData = null;
				switch (binaryReader.ReadInt32())
				{
				case 1:
				{
					cSObjectData = new CSAssemblyData();
					CSAssemblyData cSAssemblyData = cSObjectData as CSAssemblyData;
					_readCSObjectData(binaryReader, cSAssemblyData, num);
					cSAssemblyData.m_Level = binaryReader.ReadInt32();
					cSAssemblyData.m_UpgradeTime = binaryReader.ReadSingle();
					cSAssemblyData.m_CurUpgradeTime = binaryReader.ReadSingle();
					break;
				}
				case 33:
				{
					cSObjectData = new CSPPCoalData();
					CSPPCoalData cSPPCoalData = cSObjectData as CSPPCoalData;
					_readCSObjectData(binaryReader, cSPPCoalData, num);
					int num4 = binaryReader.ReadInt32();
					for (int k = 0; k < num4; k++)
					{
						cSPPCoalData.m_ChargingItems.Add(binaryReader.ReadInt32(), binaryReader.ReadInt32());
					}
					cSPPCoalData.m_CurWorkedTime = binaryReader.ReadSingle();
					cSPPCoalData.m_WorkedTime = binaryReader.ReadSingle();
					break;
				}
				case 2:
				{
					cSObjectData = new CSStorageData();
					CSStorageData cSStorageData = cSObjectData as CSStorageData;
					_readCSObjectData(binaryReader, cSStorageData, num);
					int num3 = binaryReader.ReadInt32();
					for (int j = 0; j < num3; j++)
					{
						cSStorageData.m_Items.Add(binaryReader.ReadInt32(), binaryReader.ReadInt32());
					}
					break;
				}
				case 3:
				{
					cSObjectData = new CSEngineerData();
					CSEngineerData cSEngineerData = cSObjectData as CSEngineerData;
					_readCSObjectData(binaryReader, cSEngineerData, num);
					cSEngineerData.m_EnhanceItemID = binaryReader.ReadInt32();
					cSEngineerData.m_CurEnhanceTime = binaryReader.ReadSingle();
					cSEngineerData.m_EnhanceTime = binaryReader.ReadSingle();
					cSEngineerData.m_PatchItemID = binaryReader.ReadInt32();
					cSEngineerData.m_CurPatchTime = binaryReader.ReadSingle();
					cSEngineerData.m_PatchTime = binaryReader.ReadSingle();
					cSEngineerData.m_RecycleItemID = binaryReader.ReadInt32();
					cSEngineerData.m_CurRecycleTime = binaryReader.ReadSingle();
					cSEngineerData.m_RecycleTime = binaryReader.ReadSingle();
					break;
				}
				case 4:
				{
					cSObjectData = new CSEnhanceData();
					CSEnhanceData cSEnhanceData = cSObjectData as CSEnhanceData;
					_readCSObjectData(binaryReader, cSEnhanceData, num);
					cSEnhanceData.m_ObjID = binaryReader.ReadInt32();
					cSEnhanceData.m_CurTime = binaryReader.ReadSingle();
					cSEnhanceData.m_Time = binaryReader.ReadSingle();
					break;
				}
				case 5:
				{
					cSObjectData = new CSRepairData();
					CSRepairData cSRepairData = cSObjectData as CSRepairData;
					_readCSObjectData(binaryReader, cSRepairData, num);
					cSRepairData.m_ObjID = binaryReader.ReadInt32();
					cSRepairData.m_CurTime = binaryReader.ReadSingle();
					cSRepairData.m_Time = binaryReader.ReadSingle();
					break;
				}
				case 6:
				{
					cSObjectData = new CSRecycleData();
					CSRecycleData cSRecycleData = cSObjectData as CSRecycleData;
					_readCSObjectData(binaryReader, cSRecycleData, num);
					cSRecycleData.m_ObjID = binaryReader.ReadInt32();
					cSRecycleData.m_CurTime = binaryReader.ReadSingle();
					cSRecycleData.m_Time = binaryReader.ReadSingle();
					break;
				}
				case 21:
				{
					cSObjectData = new CSDwellingsData();
					CSDwellingsData csod = cSObjectData as CSDwellingsData;
					_readCSObjectData(binaryReader, csod, num);
					break;
				}
				default:
					cSObjectData = new CSObjectData();
					break;
				}
				m_ObjectDatas.Add(cSObjectData.ID, cSObjectData);
			}
			num2 = binaryReader.ReadInt32();
			for (int l = 0; l < num2; l++)
			{
				CSPersonnelData cSPersonnelData = new CSPersonnelData();
				cSPersonnelData.ID = binaryReader.ReadInt32();
				cSPersonnelData.dType = binaryReader.ReadInt32();
				cSPersonnelData.m_State = binaryReader.ReadInt32();
				cSPersonnelData.m_DwellingsID = binaryReader.ReadInt32();
				cSPersonnelData.m_WorkRoomID = binaryReader.ReadInt32();
				m_PersonnelDatas.Add(cSPersonnelData.ID, cSPersonnelData);
			}
			break;
		}
		}
	}

	public byte[] Export()
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(259);
		binaryWriter.Write(m_ObjectDatas.Count);
		foreach (KeyValuePair<int, CSObjectData> objectData in m_ObjectDatas)
		{
			binaryWriter.Write(objectData.Value.dType);
			switch (objectData.Value.dType)
			{
			case 1:
			{
				CSAssemblyData cSAssemblyData = objectData.Value as CSAssemblyData;
				_writeCSObjectData(binaryWriter, cSAssemblyData);
				binaryWriter.Write(cSAssemblyData.m_Level);
				binaryWriter.Write(cSAssemblyData.m_UpgradeTime);
				binaryWriter.Write(cSAssemblyData.m_CurUpgradeTime);
				break;
			}
			case 33:
			{
				CSPPCoalData cSPPCoalData = objectData.Value as CSPPCoalData;
				_writeCSObjectData(binaryWriter, cSPPCoalData);
				binaryWriter.Write(cSPPCoalData.m_ChargingItems.Count);
				foreach (KeyValuePair<int, int> chargingItem in cSPPCoalData.m_ChargingItems)
				{
					binaryWriter.Write(chargingItem.Key);
					binaryWriter.Write(chargingItem.Value);
				}
				binaryWriter.Write(cSPPCoalData.m_CurWorkedTime);
				binaryWriter.Write(cSPPCoalData.m_WorkedTime);
				break;
			}
			case 2:
			{
				CSStorageData cSStorageData = objectData.Value as CSStorageData;
				_writeCSObjectData(binaryWriter, cSStorageData);
				binaryWriter.Write(cSStorageData.m_Items.Count);
				foreach (KeyValuePair<int, int> item in cSStorageData.m_Items)
				{
					binaryWriter.Write(item.Key);
					binaryWriter.Write(item.Value);
				}
				break;
			}
			case 3:
			{
				CSEngineerData cSEngineerData = objectData.Value as CSEngineerData;
				_writeCSObjectData(binaryWriter, cSEngineerData);
				binaryWriter.Write(cSEngineerData.m_EnhanceItemID);
				binaryWriter.Write(cSEngineerData.m_CurEnhanceTime);
				binaryWriter.Write(cSEngineerData.m_EnhanceTime);
				binaryWriter.Write(cSEngineerData.m_PatchItemID);
				binaryWriter.Write(cSEngineerData.m_CurPatchTime);
				binaryWriter.Write(cSEngineerData.m_PatchTime);
				binaryWriter.Write(cSEngineerData.m_RecycleItemID);
				binaryWriter.Write(cSEngineerData.m_CurRecycleTime);
				binaryWriter.Write(cSEngineerData.m_RecycleTime);
				break;
			}
			case 4:
			{
				CSEnhanceData cSEnhanceData = objectData.Value as CSEnhanceData;
				_writeCSObjectData(binaryWriter, cSEnhanceData);
				binaryWriter.Write(cSEnhanceData.m_ObjID);
				binaryWriter.Write(cSEnhanceData.m_CurTime);
				binaryWriter.Write(cSEnhanceData.m_Time);
				break;
			}
			case 5:
			{
				CSRepairData cSRepairData = objectData.Value as CSRepairData;
				_writeCSObjectData(binaryWriter, cSRepairData);
				binaryWriter.Write(cSRepairData.m_ObjID);
				binaryWriter.Write(cSRepairData.m_CurTime);
				binaryWriter.Write(cSRepairData.m_Time);
				break;
			}
			case 6:
			{
				CSRecycleData cSRecycleData = objectData.Value as CSRecycleData;
				_writeCSObjectData(binaryWriter, cSRecycleData);
				binaryWriter.Write(cSRecycleData.m_ObjID);
				binaryWriter.Write(cSRecycleData.m_CurTime);
				binaryWriter.Write(cSRecycleData.m_Time);
				break;
			}
			case 21:
			{
				CSDwellingsData csod = objectData.Value as CSDwellingsData;
				_writeCSObjectData(binaryWriter, csod);
				break;
			}
			}
		}
		binaryWriter.Write(m_PersonnelDatas.Count);
		foreach (KeyValuePair<int, CSPersonnelData> personnelData in m_PersonnelDatas)
		{
			binaryWriter.Write(personnelData.Value.ID);
			binaryWriter.Write(personnelData.Value.dType);
			binaryWriter.Write(personnelData.Value.m_State);
			binaryWriter.Write(personnelData.Value.m_DwellingsID);
			binaryWriter.Write(personnelData.Value.m_WorkRoomID);
		}
		binaryWriter.Close();
		return memoryStream.ToArray();
	}

	private void _readCSObjectData(BinaryReader r, CSObjectData csod, int version)
	{
		if (version == 259)
		{
			csod.ID = r.ReadInt32();
			csod.m_Name = r.ReadString();
			csod.m_Position = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
			csod.m_Durability = r.ReadSingle();
			csod.m_CurRepairTime = r.ReadSingle();
			csod.m_RepairTime = r.ReadSingle();
			csod.m_RepairValue = r.ReadSingle();
			csod.m_CurDeleteTime = r.ReadSingle();
			csod.m_DeleteTime = r.ReadSingle();
			csod.m_Bounds = new Bounds(new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()), new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()));
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				csod.m_DeleteGetsItem.Add(r.ReadInt32(), r.ReadInt32());
			}
		}
		else
		{
			csod.ID = r.ReadInt32();
			csod.m_Name = r.ReadString();
			csod.m_Position = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
			csod.m_Durability = r.ReadSingle();
			csod.m_CurRepairTime = r.ReadSingle();
			csod.m_RepairTime = r.ReadSingle();
			csod.m_RepairValue = r.ReadSingle();
			csod.m_CurDeleteTime = r.ReadSingle();
			csod.m_DeleteTime = r.ReadSingle();
			int num2 = r.ReadInt32();
			for (int j = 0; j < num2; j++)
			{
				csod.m_DeleteGetsItem.Add(r.ReadInt32(), r.ReadInt32());
			}
		}
	}

	private void _writeCSObjectData(BinaryWriter w, CSObjectData csod)
	{
		w.Write(csod.ID);
		w.Write(csod.m_Name);
		w.Write(csod.m_Position.x);
		w.Write(csod.m_Position.y);
		w.Write(csod.m_Position.z);
		w.Write(csod.m_Durability);
		w.Write(csod.m_CurRepairTime);
		w.Write(csod.m_RepairTime);
		w.Write(csod.m_RepairValue);
		w.Write(csod.m_CurDeleteTime);
		w.Write(csod.m_DeleteTime);
		w.Write(csod.m_Bounds.center.x);
		w.Write(csod.m_Bounds.center.y);
		w.Write(csod.m_Bounds.center.z);
		w.Write(csod.m_Bounds.size.x);
		w.Write(csod.m_Bounds.size.y);
		w.Write(csod.m_Bounds.size.z);
		w.Write(csod.m_DeleteGetsItem.Count);
		foreach (KeyValuePair<int, int> item in csod.m_DeleteGetsItem)
		{
			w.Write(item.Key);
			w.Write(item.Value);
		}
	}
}
