using System;
using System.Collections.Generic;
using System.IO;
using Pathea;
using UnityEngine;

public class UIBlockSaver : ArchivableSingleton<UIBlockSaver>
{
	private Dictionary<int, UIBuildMenuItemData> m_Datas = new Dictionary<int, UIBuildMenuItemData>();

	private bool m_First = true;

	private int m_Version = 2;

	public Dictionary<int, UIBuildMenuItemData> Datas => m_Datas;

	public bool First
	{
		get
		{
			return m_First;
		}
		set
		{
			m_First = value;
		}
	}

	public List<UIBuildMenuItemData> GetPageItemDatas(int page_index, int page_count)
	{
		List<UIBuildMenuItemData> list = new List<UIBuildMenuItemData>();
		foreach (KeyValuePair<int, UIBuildMenuItemData> data in m_Datas)
		{
			int num = data.Key / page_count;
			if (num == page_index)
			{
				list.Add(data.Value);
			}
		}
		return list;
	}

	public bool Contains(int index)
	{
		return m_Datas.ContainsKey(index);
	}

	public void SetData(int index, UIBuildWndItem item)
	{
		UIBuildMenuItemData uIBuildMenuItemData = null;
		if (m_Datas.ContainsKey(index))
		{
			uIBuildMenuItemData = m_Datas[index];
		}
		else
		{
			uIBuildMenuItemData = new UIBuildMenuItemData();
			m_Datas.Add(index, uIBuildMenuItemData);
		}
		uIBuildMenuItemData.m_Index = item.mIndex;
		uIBuildMenuItemData.m_TargetIndex = item.mTargetIndex;
		uIBuildMenuItemData.m_Type = (int)item.mTargetItemType;
		uIBuildMenuItemData.m_IconName = item.mContentSprite.spriteName;
		uIBuildMenuItemData.m_SubsetIndex = item.mSubsetIndex;
		uIBuildMenuItemData.m_ItemId = item.ItemId;
	}

	public void AddData(UIBuildMenuItemData data)
	{
		m_Datas[data.m_Index] = data;
	}

	public bool RemoveData(int index)
	{
		return m_Datas.Remove(index);
	}

	protected override bool GetYird()
	{
		return false;
	}

	protected override void SetData(byte[] data)
	{
		if (data == null)
		{
			return;
		}
		try
		{
			using MemoryStream input = new MemoryStream(data);
			BinaryReader binaryReader = new BinaryReader(input);
			switch (binaryReader.ReadInt32())
			{
			case 1:
			{
				m_First = false;
				int num2 = binaryReader.ReadInt32();
				for (int j = 0; j < num2; j++)
				{
					UIBuildMenuItemData uIBuildMenuItemData2 = new UIBuildMenuItemData();
					int key2 = binaryReader.ReadInt32();
					uIBuildMenuItemData2.m_Index = binaryReader.ReadInt32();
					uIBuildMenuItemData2.m_TargetIndex = binaryReader.ReadInt32();
					uIBuildMenuItemData2.m_Type = binaryReader.ReadInt32();
					uIBuildMenuItemData2.m_SubsetIndex = binaryReader.ReadInt32();
					uIBuildMenuItemData2.m_IconName = binaryReader.ReadString();
					uIBuildMenuItemData2.m_ItemId = binaryReader.ReadInt32();
					m_Datas.Add(key2, uIBuildMenuItemData2);
				}
				break;
			}
			case 2:
			{
				m_First = binaryReader.ReadBoolean();
				int num = binaryReader.ReadInt32();
				for (int i = 0; i < num; i++)
				{
					UIBuildMenuItemData uIBuildMenuItemData = new UIBuildMenuItemData();
					int key = binaryReader.ReadInt32();
					uIBuildMenuItemData.m_Index = binaryReader.ReadInt32();
					uIBuildMenuItemData.m_TargetIndex = binaryReader.ReadInt32();
					uIBuildMenuItemData.m_Type = binaryReader.ReadInt32();
					uIBuildMenuItemData.m_SubsetIndex = binaryReader.ReadInt32();
					uIBuildMenuItemData.m_IconName = binaryReader.ReadString();
					uIBuildMenuItemData.m_ItemId = binaryReader.ReadInt32();
					m_Datas.Add(key, uIBuildMenuItemData);
				}
				break;
			}
			}
			binaryReader.Close();
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
		}
	}

	protected override void WriteData(BinaryWriter w)
	{
		w.Write(m_Version);
		w.Write(m_First);
		w.Write(m_Datas.Count);
		foreach (KeyValuePair<int, UIBuildMenuItemData> data in m_Datas)
		{
			w.Write(data.Key);
			w.Write(data.Value.m_Index);
			w.Write(data.Value.m_TargetIndex);
			w.Write(data.Value.m_Type);
			w.Write(data.Value.m_SubsetIndex);
			w.Write(data.Value.m_IconName);
			w.Write(data.Value.m_ItemId);
		}
	}

	public override void New()
	{
		m_Datas = new Dictionary<int, UIBuildMenuItemData>();
	}
}
