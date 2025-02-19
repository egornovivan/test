using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BSIsoData
{
	public const int ISO_VERSION = 16842753;

	public BSIsoHeadData m_HeadInfo;

	public Dictionary<int, BSVoxel> m_Voxels;

	public static int IPosToKey(IntVector3 pos)
	{
		return pos.x | (pos.z << 10) | (pos.y << 20);
	}

	public static int IPosToKey(int x, int y, int z)
	{
		return x | (z << 10) | (y << 20);
	}

	public static IntVector3 KeyToIPos(int key)
	{
		return new IntVector3(key & 0x3FF, key >> 20, (key >> 10) & 0x3FF);
	}

	public void Init(EBSVoxelType voxel_type)
	{
		Destroy();
		m_Voxels = new Dictionary<int, BSVoxel>();
		m_HeadInfo = default(BSIsoHeadData);
		m_HeadInfo.Version = 16842753;
		m_HeadInfo.Mode = voxel_type;
		m_HeadInfo.Author = string.Empty;
		m_HeadInfo.Name = string.Empty;
		m_HeadInfo.Desc = string.Empty;
		m_HeadInfo.Remarks = "<REMARKS />";
		m_HeadInfo.xSize = 256;
		m_HeadInfo.ySize = 256;
		m_HeadInfo.zSize = 256;
		m_HeadInfo.IconTex = new byte[0];
		m_HeadInfo.costs = new Dictionary<byte, uint>();
		m_HeadInfo.EnsureIconTexValid();
	}

	public void Destroy()
	{
		if (m_Voxels != null)
		{
			m_Voxels.Clear();
		}
	}

	public BSVoxel GetVoxel(int pos)
	{
		m_Voxels.TryGetValue(pos, out var value);
		return value;
	}

	public void SetVoxel(int pos, BSVoxel voxel)
	{
		if (m_HeadInfo.Mode == EBSVoxelType.Block)
		{
			if (voxel.type == 0)
			{
				m_Voxels.Remove(pos);
			}
			else
			{
				m_Voxels[pos] = voxel;
			}
		}
		else if (m_HeadInfo.Mode == EBSVoxelType.Voxel)
		{
			if (voxel.volmue == 0)
			{
				m_Voxels.Remove(pos);
			}
			else
			{
				m_Voxels[pos] = voxel;
			}
		}
		else
		{
			Debug.LogError("Unknow voxel type");
		}
	}

	public void CaclCosts()
	{
		m_HeadInfo.costs.Clear();
		foreach (KeyValuePair<int, BSVoxel> voxel in m_Voxels)
		{
			if (m_HeadInfo.costs.ContainsKey(voxel.Value.materialType))
			{
				Dictionary<byte, uint> costs;
				Dictionary<byte, uint> dictionary = (costs = m_HeadInfo.costs);
				byte materialType;
				byte key = (materialType = voxel.Value.materialType);
				uint num = costs[materialType];
				dictionary[key] = num + 1;
			}
			else
			{
				m_HeadInfo.costs.Add(voxel.Value.materialType, 1u);
			}
		}
	}

	public byte[] Export()
	{
		using MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write("BSISO");
		m_HeadInfo.EnsureIconTexValid();
		binaryWriter.Write(16842753);
		binaryWriter.Write((int)m_HeadInfo.Mode);
		binaryWriter.Write(m_HeadInfo.Author);
		binaryWriter.Write(m_HeadInfo.Name);
		binaryWriter.Write(m_HeadInfo.Desc);
		binaryWriter.Write(m_HeadInfo.Remarks);
		binaryWriter.Write(m_HeadInfo.xSize);
		binaryWriter.Write(m_HeadInfo.ySize);
		binaryWriter.Write(m_HeadInfo.zSize);
		binaryWriter.Write(m_HeadInfo.IconTex.Length);
		binaryWriter.Write(m_HeadInfo.IconTex, 0, m_HeadInfo.IconTex.Length);
		binaryWriter.Write(m_HeadInfo.costs.Count);
		foreach (KeyValuePair<byte, uint> cost in m_HeadInfo.costs)
		{
			binaryWriter.Write(cost.Key);
			binaryWriter.Write(cost.Value);
		}
		binaryWriter.Write(m_Voxels.Count);
		foreach (KeyValuePair<int, BSVoxel> voxel in m_Voxels)
		{
			binaryWriter.Write(voxel.Key);
			binaryWriter.Write(voxel.Value.value0);
			binaryWriter.Write(voxel.Value.value1);
		}
		binaryWriter.Close();
		return memoryStream.ToArray();
	}

	public bool Import(byte[] buffer)
	{
		if (buffer == null)
		{
			return false;
		}
		Init(EBSVoxelType.Block);
		try
		{
			using MemoryStream input = new MemoryStream(buffer);
			BinaryReader binaryReader = new BinaryReader(input);
			string text = binaryReader.ReadString();
			int num3;
			if (text != "BSISO")
			{
				binaryReader.BaseStream.Seek(0L, SeekOrigin.Begin);
				int num = binaryReader.ReadInt32();
				int num2 = binaryReader.ReadInt32();
				num3 = num;
				if (num3 == 1)
				{
					Dictionary<IntVector3, B45Block> dictionary = new Dictionary<IntVector3, B45Block>();
					for (int i = 0; i < num2; i++)
					{
						IntVector3 key = new IntVector3(binaryReader.ReadInt32(), binaryReader.ReadInt32(), binaryReader.ReadInt32());
						dictionary[key] = new B45Block(binaryReader.ReadByte(), binaryReader.ReadByte());
					}
					int count = binaryReader.ReadInt32();
					byte[] iconTex = binaryReader.ReadBytes(count);
					m_HeadInfo.IconTex = iconTex;
					IntVector3 intVector = new IntVector3(10000, 10000, 10000);
					IntVector3 intVector2 = new IntVector3(-10000, -10000, -10000);
					foreach (IntVector3 key4 in dictionary.Keys)
					{
						if (intVector.x > key4.x)
						{
							intVector.x = key4.x;
						}
						else if (intVector2.x < key4.x)
						{
							intVector2.x = key4.x;
						}
						if (intVector.y > key4.y)
						{
							intVector.y = key4.y;
						}
						else if (intVector2.y < key4.y)
						{
							intVector2.y = key4.y;
						}
						if (intVector.z > key4.z)
						{
							intVector.z = key4.z;
						}
						else if (intVector2.z < key4.z)
						{
							intVector2.z = key4.z;
						}
					}
					IntVector3 intVector3 = new IntVector3(intVector2.x - intVector.x + 1, intVector2.y - intVector.y + 1, intVector2.z - intVector.z + 1);
					m_HeadInfo.xSize = intVector3.x;
					m_HeadInfo.ySize = intVector3.y;
					m_HeadInfo.zSize = intVector3.z;
					foreach (KeyValuePair<IntVector3, B45Block> item in dictionary)
					{
						IntVector3 pos = new IntVector3(item.Key.x + Mathf.CeilToInt((float)intVector3.x / 2f), item.Key.y, item.Key.z + Mathf.CeilToInt((float)intVector3.z / 2f));
						int key2 = IPosToKey(pos);
						m_Voxels[key2] = new BSVoxel(item.Value);
					}
					CaclCosts();
				}
				binaryReader.Close();
				return true;
			}
			m_HeadInfo.Version = binaryReader.ReadInt32();
			num3 = m_HeadInfo.Version;
			if (num3 != 16842753)
			{
				binaryReader.Close();
				return false;
			}
			m_HeadInfo.Mode = (EBSVoxelType)binaryReader.ReadInt32();
			m_HeadInfo.Author = binaryReader.ReadString();
			m_HeadInfo.Name = binaryReader.ReadString();
			m_HeadInfo.Desc = binaryReader.ReadString();
			m_HeadInfo.Remarks = binaryReader.ReadString();
			m_HeadInfo.xSize = binaryReader.ReadInt32();
			m_HeadInfo.ySize = binaryReader.ReadInt32();
			m_HeadInfo.zSize = binaryReader.ReadInt32();
			int count2 = binaryReader.ReadInt32();
			m_HeadInfo.IconTex = binaryReader.ReadBytes(count2);
			count2 = binaryReader.ReadInt32();
			for (int j = 0; j < count2; j++)
			{
				m_HeadInfo.costs.Add(binaryReader.ReadByte(), binaryReader.ReadUInt32());
			}
			count2 = binaryReader.ReadInt32();
			for (int k = 0; k < count2; k++)
			{
				BSVoxel value = default(BSVoxel);
				int key3 = binaryReader.ReadInt32();
				value.value0 = binaryReader.ReadByte();
				value.value1 = binaryReader.ReadByte();
				m_Voxels[key3] = value;
			}
			binaryReader.Close();
		}
		catch (Exception ex)
		{
			Debug.LogError("Importing ISO Failed :" + ex.ToString());
			return false;
		}
		return true;
	}

	private static int ExtractHeader(Stream stream, out BSIsoHeadData iso_header)
	{
		iso_header = default(BSIsoHeadData);
		int num = 0;
		iso_header.Init();
		try
		{
			BinaryReader binaryReader = new BinaryReader(stream);
			string text = binaryReader.ReadString();
			if (text != "BSISO")
			{
				binaryReader.BaseStream.Seek(0L, SeekOrigin.Begin);
				int num2 = binaryReader.ReadInt32();
				int num3 = binaryReader.ReadInt32();
				int num4 = num2;
				if (num4 == 1)
				{
					Dictionary<IntVector3, B45Block> dictionary = new Dictionary<IntVector3, B45Block>();
					for (int i = 0; i < num3; i++)
					{
						IntVector3 key = new IntVector3(binaryReader.ReadInt32(), binaryReader.ReadInt32(), binaryReader.ReadInt32());
						dictionary[key] = new B45Block(binaryReader.ReadByte(), binaryReader.ReadByte());
					}
					int count = binaryReader.ReadInt32();
					byte[] iconTex = binaryReader.ReadBytes(count);
					iso_header.IconTex = iconTex;
					IntVector3 intVector = new IntVector3(10000, 10000, 10000);
					IntVector3 intVector2 = new IntVector3(-10000, -10000, -10000);
					foreach (IntVector3 key3 in dictionary.Keys)
					{
						if (intVector.x > key3.x)
						{
							intVector.x = key3.x;
						}
						else if (intVector2.x < key3.x)
						{
							intVector2.x = key3.x;
						}
						if (intVector.y > key3.y)
						{
							intVector.y = key3.y;
						}
						else if (intVector2.y < key3.y)
						{
							intVector2.y = key3.y;
						}
						if (intVector.z > key3.z)
						{
							intVector.z = key3.z;
						}
						else if (intVector2.z < key3.z)
						{
							intVector2.z = key3.z;
						}
					}
					IntVector3 intVector3 = new IntVector3(intVector2.x - intVector.x + 1, intVector2.y - intVector.y + 1, intVector2.z - intVector.z + 1);
					iso_header.xSize = intVector3.x;
					iso_header.ySize = intVector3.y;
					iso_header.zSize = intVector3.z;
					foreach (KeyValuePair<IntVector3, B45Block> item in dictionary)
					{
						IntVector3 intVector4 = new IntVector3(item.Key.x + intVector3.x / 2, item.Key.y, item.Key.z + intVector3.z / 2);
						if (iso_header.costs.ContainsKey(item.Value.materialType))
						{
							Dictionary<byte, uint> costs;
							Dictionary<byte, uint> dictionary2 = (costs = iso_header.costs);
							byte materialType;
							byte key2 = (materialType = item.Value.materialType);
							uint num5 = costs[materialType];
							dictionary2[key2] = num5 + 1;
						}
						else
						{
							iso_header.costs.Add(item.Value.materialType, 1u);
						}
					}
				}
				binaryReader.Close();
				return (int)stream.Length;
			}
			iso_header.Version = binaryReader.ReadInt32();
			iso_header.Mode = (EBSVoxelType)binaryReader.ReadInt32();
			iso_header.Author = binaryReader.ReadString();
			iso_header.Name = binaryReader.ReadString();
			iso_header.Desc = binaryReader.ReadString();
			iso_header.Remarks = binaryReader.ReadString();
			iso_header.xSize = binaryReader.ReadInt32();
			iso_header.ySize = binaryReader.ReadInt32();
			iso_header.zSize = binaryReader.ReadInt32();
			int count2 = binaryReader.ReadInt32();
			iso_header.IconTex = binaryReader.ReadBytes(count2);
			count2 = binaryReader.ReadInt32();
			for (int j = 0; j < count2; j++)
			{
				iso_header.costs.Add(binaryReader.ReadByte(), binaryReader.ReadUInt32());
			}
			num = (int)stream.Length;
			stream.Close();
		}
		catch (Exception)
		{
			return 0;
		}
		iso_header.EnsureIconTexValid();
		return num;
	}

	public static int ExtractHeader(string filename, out BSIsoHeadData iso_header)
	{
		iso_header = default(BSIsoHeadData);
		if (!File.Exists(filename))
		{
			return 0;
		}
		try
		{
			iso_header.Name = filename;
			using FileStream stream = File.Open(filename, FileMode.Open);
			return ExtractHeader(stream, out iso_header);
		}
		catch (Exception)
		{
			return 0;
		}
	}

	public static int ExtractHeader(byte[] buffer, out BSIsoHeadData iso_header)
	{
		iso_header = default(BSIsoHeadData);
		if (buffer == null)
		{
			return 0;
		}
		using MemoryStream stream = new MemoryStream(buffer);
		return ExtractHeader(stream, out iso_header);
	}
}
