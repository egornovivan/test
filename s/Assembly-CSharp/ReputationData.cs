using System.Collections.Generic;
using System.IO;

public class ReputationData
{
	public bool active;

	public Dictionary<int, ReputationCamp> m_ReputationCamps = new Dictionary<int, ReputationCamp>();

	public void InitReputationCamp()
	{
	}

	public void Import(BinaryReader _in)
	{
		active = _in.ReadBoolean();
		int num = _in.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int key = _in.ReadInt32();
			if (!m_ReputationCamps.ContainsKey(key))
			{
				m_ReputationCamps[key] = new ReputationCamp();
			}
			m_ReputationCamps[key].reputationValue = _in.ReadInt32();
			m_ReputationCamps[key].exValue = _in.ReadInt32();
		}
	}

	public void Export(BinaryWriter _out)
	{
		_out.Write(active);
		_out.Write(m_ReputationCamps.Count);
		foreach (KeyValuePair<int, ReputationCamp> reputationCamp in m_ReputationCamps)
		{
			_out.Write(reputationCamp.Key);
			_out.Write(reputationCamp.Value.reputationValue);
			_out.Write(reputationCamp.Value.exValue);
		}
	}
}
