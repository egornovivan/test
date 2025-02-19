using System;
using System.Collections.Generic;
using System.IO;
using Pathea;

public class NPCTalkHistroy : ArchivableSingleton<NPCTalkHistroy>
{
	public struct Histroy
	{
		public string npcName;

		public string countent;
	}

	private const int CURRENT_VERSION = 1;

	private const int SaveCount = 20;

	private List<Histroy> m_Histroy = new List<Histroy>();

	public Action<Histroy> onAddHistroy;

	public Action onRemoveHistroy;

	public List<Histroy> histroies => m_Histroy;

	public void AddHistroy(string npcName, string countent)
	{
		for (int i = 0; i < m_Histroy.Count; i++)
		{
			if (countent == m_Histroy[i].countent)
			{
				return;
			}
		}
		Histroy histroy = default(Histroy);
		histroy.npcName = npcName;
		histroy.countent = countent;
		m_Histroy.Add(histroy);
		if (onAddHistroy != null)
		{
			onAddHistroy(histroy);
		}
		if (m_Histroy.Count > 20)
		{
			m_Histroy.RemoveAt(0);
			if (onRemoveHistroy != null)
			{
				onRemoveHistroy();
			}
		}
	}

	public void Clear()
	{
		m_Histroy.Clear();
	}

	protected override bool GetYird()
	{
		return false;
	}

	protected override void WriteData(BinaryWriter bw)
	{
		bw.Write(1);
		bw.Write(m_Histroy.Count);
		for (int i = 0; i < m_Histroy.Count; i++)
		{
			bw.Write(m_Histroy[i].npcName);
			bw.Write(m_Histroy[i].countent);
		}
	}

	protected override void SetData(byte[] data)
	{
		m_Histroy.Clear();
		MemoryStream memoryStream = new MemoryStream(data);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		binaryReader.ReadInt32();
		int num = binaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			Histroy item = default(Histroy);
			item.npcName = binaryReader.ReadString();
			item.countent = binaryReader.ReadString();
			m_Histroy.Add(item);
		}
		binaryReader.Close();
		memoryStream.Close();
	}
}
