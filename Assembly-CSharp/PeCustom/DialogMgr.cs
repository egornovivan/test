using System;
using System.Collections.Generic;
using System.IO;

namespace PeCustom;

public class DialogMgr
{
	private const int VERSION = 1;

	public Action<int, int> onNpoQuestsChanged;

	public Action onChoiceChanged;

	private Dictionary<int, SortedList<int, string>> m_Quests;

	private SortedList<int, string> m_Choices;

	private bool m_BeginChooseGroup;

	public DialogMgr()
	{
		m_Quests = new Dictionary<int, SortedList<int, string>>(32);
		m_Choices = new SortedList<int, string>();
	}

	public IList<string> GetQuests(int world_index, int npo_id)
	{
		int key = WorldIndexAndNpoIdToKey(world_index, npo_id);
		if (m_Quests.ContainsKey(key))
		{
			return m_Quests[npo_id].Values;
		}
		return null;
	}

	public int GetQuestId(int world_index, int npo_id, int index)
	{
		if (index < 0)
		{
			return -1;
		}
		int key = WorldIndexAndNpoIdToKey(world_index, npo_id);
		if (m_Quests.ContainsKey(key))
		{
			IList<int> keys = m_Quests[key].Keys;
			if (index < keys.Count)
			{
				return m_Quests[key].Keys[index];
			}
			return -1;
		}
		return -1;
	}

	public void SetQuest(int world_index, int npo_id, int quest_id, string text)
	{
		int key = WorldIndexAndNpoIdToKey(world_index, npo_id);
		if (!m_Quests.ContainsKey(key))
		{
			SortedList<int, string> sortedList = new SortedList<int, string>(5);
			sortedList[quest_id] = text;
			m_Quests.Add(key, sortedList);
		}
		else
		{
			m_Quests[key][quest_id] = text;
		}
		if (onNpoQuestsChanged != null)
		{
			onNpoQuestsChanged(world_index, npo_id);
		}
	}

	public void RemoveQuest(int world_index, int npo_id, int quest_id)
	{
		int key = WorldIndexAndNpoIdToKey(world_index, npo_id);
		if (m_Quests.ContainsKey(key))
		{
			m_Quests[key].Remove(quest_id);
			if (onNpoQuestsChanged != null)
			{
				onNpoQuestsChanged(world_index, npo_id);
			}
		}
	}

	public void Clear(int world_index, int npo_id)
	{
		int key = WorldIndexAndNpoIdToKey(world_index, npo_id);
		if (m_Quests.ContainsKey(key))
		{
			m_Quests[key].Clear();
			m_Quests.Remove(key);
			if (onNpoQuestsChanged != null)
			{
				onNpoQuestsChanged(world_index, npo_id);
			}
		}
	}

	public void ClearAll()
	{
		foreach (KeyValuePair<int, SortedList<int, string>> quest in m_Quests)
		{
			quest.Value.Clear();
			int world_index = 0;
			int npo_id = 0;
			KeyToWorldIndexAndNpoId(quest.Key, out world_index, out npo_id);
			if (onNpoQuestsChanged != null)
			{
				onNpoQuestsChanged(world_index, npo_id);
			}
		}
		m_Quests.Clear();
	}

	public IList<string> GetChoices()
	{
		return m_Choices.Values;
	}

	public int GetChoiceId(int index)
	{
		if (index < 0 && index > m_Choices.Count)
		{
			return -1;
		}
		return m_Choices.Keys[index];
	}

	public bool BeginChooseGroup()
	{
		if (!m_BeginChooseGroup)
		{
			m_Choices.Clear();
			if (onChoiceChanged != null)
			{
				onChoiceChanged();
			}
			m_BeginChooseGroup = true;
			return true;
		}
		return false;
	}

	public bool AddChoose(int choose_id, string text)
	{
		if (m_BeginChooseGroup)
		{
			m_Choices[choose_id] = text;
			if (onChoiceChanged != null)
			{
				onChoiceChanged();
			}
			return true;
		}
		return false;
	}

	public bool EndChooseGroup()
	{
		if (m_BeginChooseGroup)
		{
			m_BeginChooseGroup = false;
			return true;
		}
		return false;
	}

	public void Import(BinaryReader r)
	{
		r.ReadInt32();
		int num = r.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int key = r.ReadInt32();
			SortedList<int, string> sortedList = new SortedList<int, string>(5);
			int num2 = r.ReadInt32();
			for (int j = 0; j < num2; j++)
			{
				sortedList.Add(r.ReadInt32(), r.ReadString());
			}
			m_Quests.Add(key, sortedList);
		}
		m_BeginChooseGroup = r.ReadBoolean();
		num = r.ReadInt32();
		for (int k = 0; k < num; k++)
		{
			m_Choices.Add(r.ReadInt32(), r.ReadString());
		}
	}

	public byte[] Export()
	{
		byte[] array = null;
		using MemoryStream memoryStream = new MemoryStream();
		BinaryWriter w = new BinaryWriter(memoryStream);
		Export(w);
		array = memoryStream.ToArray();
		memoryStream.Close();
		return array;
	}

	public void Export(BinaryWriter w)
	{
		w.Write(1);
		w.Write(m_Quests.Count);
		foreach (KeyValuePair<int, SortedList<int, string>> quest in m_Quests)
		{
			w.Write(quest.Key);
			w.Write(quest.Value.Count);
			for (int i = 0; i < quest.Value.Count; i++)
			{
				w.Write(quest.Value.Keys[i]);
				w.Write(quest.Value.Values[i]);
			}
		}
		w.Write(m_BeginChooseGroup);
		w.Write(m_Choices.Count);
		for (int j = 0; j < m_Choices.Count; j++)
		{
			w.Write(m_Choices.Keys[j]);
			w.Write(m_Choices.Values[j]);
		}
	}

	public static int WorldIndexAndNpoIdToKey(int world_index, int npo_id)
	{
		return (world_index << 16) + npo_id;
	}

	public static void KeyToWorldIndexAndNpoId(int key, out int world_index, out int npo_id)
	{
		world_index = key >> 16;
		npo_id = key - (world_index << 16);
	}
}
