using System.Collections.Generic;
using System.IO;

public class ColonyDwellings : ColonyBase
{
	public const int MAX_WORKER_COUNT = 4;

	private int[] _Npcs;

	public override int MaxWorkerCount => 4;

	public ColonyDwellings(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSDwellingsData();
		_Npcs = new int[4];
		LoadData();
		if (ColonyBase.IsNewPutOut)
		{
			TransferNpc();
		}
	}

	public override void MyUpdate()
	{
	}

	public override void CombomData(BinaryWriter writer)
	{
		BufferHelper.Serialize(writer, 0);
	}

	public override void ParseData(byte[] data, int ver)
	{
	}

	public override void InitMyData()
	{
	}

	public bool HaveEmpty()
	{
		for (int i = 0; i < _Npcs.Length; i++)
		{
			if (_Npcs[i] == 0)
			{
				return true;
			}
		}
		return false;
	}

	public override bool IsWorking()
	{
		if (ColonyMgr._Instance.HaveCore(_Network.TeamId, _Network.transform.position))
		{
			return true;
		}
		return false;
	}

	public bool CanAddNpc(int npcId)
	{
		for (int i = 0; i < _Npcs.Length; i++)
		{
			if (_Npcs[i] == 0 || _Npcs[i] == npcId)
			{
				return true;
			}
		}
		return false;
	}

	public bool AddNpcs(int npcID)
	{
		for (int i = 0; i < _Npcs.Length; i++)
		{
			if (_Npcs[i] == 0)
			{
				_Npcs[i] = npcID;
				ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(npcID);
				if (npcByID != null)
				{
					npcByID.m_DwellingsID = base.Id;
					npcByID.Save();
				}
				return true;
			}
		}
		return false;
	}

	public bool RemoveNpcs(int npcID)
	{
		for (int i = 0; i < _Npcs.Length; i++)
		{
			if (_Npcs[i] == npcID)
			{
				_Npcs[i] = 0;
				ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(npcID);
				if (npcByID != null)
				{
					npcByID.m_DwellingsID = 0;
					npcByID.Save();
				}
				return true;
			}
		}
		return false;
	}

	public override void DestroySelf()
	{
		base.DestroySelf();
		int[] npcs = _Npcs;
		foreach (int num in npcs)
		{
			if (num == 0)
			{
				continue;
			}
			ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(num);
			if (npcByID == null)
			{
				continue;
			}
			ColonyDwellings colonyDwellings = ColonyMgr.FindNewBed(_Network.TeamId, base.Id);
			if (colonyDwellings != null)
			{
				colonyDwellings.AddNpcs(npcByID._npcID);
				AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get<AiAdNpcNetwork>(npcByID._npcID);
				if (aiAdNpcNetwork != null)
				{
					aiAdNpcNetwork.RPCOthers(EPacketType.PT_CL_CLN_SetDwellingsID, npcByID.m_DwellingsID);
				}
				npcByID.Save();
				continue;
			}
			npcByID.m_DwellingsID = 0;
			npcByID.m_WorkRoomID = 0;
			AiAdNpcNetwork aiAdNpcNetwork2 = ObjNetInterface.Get<AiAdNpcNetwork>(npcByID._npcID);
			if (aiAdNpcNetwork2 != null)
			{
				ColonyNpcMgr.RemoveAt(base.TeamId, npcByID._npcID);
				aiAdNpcNetwork2.RPCOthers(EPacketType.PT_CL_CLN_RemoveNpc);
				aiAdNpcNetwork2.DismissByPlayer();
			}
		}
	}

	private void TransferNpc()
	{
		List<ColonyBase> colonyItemsByItemId = ColonyMgr._Instance.GetColonyItemsByItemId(_Network.TeamId, 1131);
		foreach (ColonyBase item in colonyItemsByItemId)
		{
			if (item.Id == base.Id || !(item is ColonyDwellings colonyDwellings) || colonyDwellings.IsWorking())
			{
				continue;
			}
			for (int i = 0; i < colonyDwellings._Npcs.Length; i++)
			{
				if (colonyDwellings._Npcs[i] != 0)
				{
					int npcID = colonyDwellings._Npcs[i];
					colonyDwellings.RemoveNpcs(npcID);
					AddNpcs(npcID);
					if (!HaveEmpty())
					{
						return;
					}
				}
			}
		}
	}

	public int[] GetAllNpcs()
	{
		return _Npcs;
	}
}
