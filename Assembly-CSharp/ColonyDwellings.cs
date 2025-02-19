using CSRecord;

public class ColonyDwellings : ColonyBase
{
	private int[] _Npcs;

	public ColonyDwellings(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSDwellingsData();
		_Npcs = new int[4];
		for (int i = 0; i < _Npcs.Length; i++)
		{
			_Npcs[i] = 0;
		}
	}

	public bool IsHaveEmpty()
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

	public bool AddNpcs(int npcID)
	{
		for (int i = 0; i < _Npcs.Length; i++)
		{
			if (_Npcs[i] == 0)
			{
				_Npcs[i] = npcID;
				return true;
			}
		}
		return false;
	}
}
