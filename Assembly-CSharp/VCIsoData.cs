using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VCIsoData
{
	public const int ISO_VERSION = 33751041;

	public const int MAT_ROW_CNT = 4;

	public const int MAT_COL_CNT = 4;

	public const int MAT_ARR_CNT = 16;

	public const int DECAL_ARR_CNT = 4;

	public VCIsoHeadData m_HeadInfo;

	public VCMaterial[] m_Materials;

	public VCDecalAsset[] m_DecalAssets;

	public Dictionary<int, VCVoxel> m_Voxels;

	public List<VCComponentData> m_Components;

	public Dictionary<int, Color32> m_Colors;

	public static Color32 BLANK_COLOR = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0);

	private VCIsoOption m_Options;

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

	public static void GetIPosFromKey(int key, IntVector3 output)
	{
		output.x = key & 0x3FF;
		output.y = key >> 20;
		output.z = (key >> 10) & 0x3FF;
	}

	public static IntVector3 IPosToColorPos(Vector3 iso_pos)
	{
		return new IntVector3((int)((iso_pos.x + 0.75f) * 2f), (int)((iso_pos.y + 0.75f) * 2f), (int)((iso_pos.z + 0.75f) * 2f));
	}

	public static int ColorPosToColorKey(IntVector3 color_pos)
	{
		return color_pos.x | (color_pos.z << 10) | (color_pos.y << 20);
	}

	public static int IPosToColorKey(Vector3 iso_pos)
	{
		return (int)((iso_pos.x + 0.75f) * 2f) | ((int)((iso_pos.z + 0.75f) * 2f) << 10) | ((int)((iso_pos.y + 0.75f) * 2f) << 20);
	}

	public void Reset(VCIsoOption options)
	{
		Destroy();
		m_HeadInfo = default(VCIsoHeadData);
		m_HeadInfo.Author = string.Empty;
		m_HeadInfo.Name = string.Empty;
		m_HeadInfo.Desc = string.Empty;
		m_HeadInfo.Remarks = "<REMARKS />";
		m_HeadInfo.IconTex = new byte[0];
		m_Materials = new VCMaterial[16];
		m_DecalAssets = new VCDecalAsset[4];
		m_Voxels = new Dictionary<int, VCVoxel>();
		m_Components = new List<VCComponentData>();
		m_Colors = new Dictionary<int, Color32>();
		m_Options = options;
	}

	public void Init(int version, VCESceneSetting setting, VCIsoOption options)
	{
		Destroy();
		m_HeadInfo = default(VCIsoHeadData);
		m_HeadInfo.Version = version;
		m_HeadInfo.Category = setting.m_Category;
		m_HeadInfo.Author = string.Empty;
		m_HeadInfo.Name = string.Empty;
		m_HeadInfo.Desc = string.Empty;
		m_HeadInfo.Remarks = "<REMARKS />";
		m_HeadInfo.xSize = setting.m_EditorSize.x;
		m_HeadInfo.ySize = setting.m_EditorSize.y;
		m_HeadInfo.zSize = setting.m_EditorSize.z;
		m_HeadInfo.IconTex = new byte[0];
		m_HeadInfo.EnsureIconTexValid();
		m_Materials = new VCMaterial[16];
		m_DecalAssets = new VCDecalAsset[4];
		m_Voxels = new Dictionary<int, VCVoxel>();
		m_Components = new List<VCComponentData>();
		m_Colors = new Dictionary<int, Color32>();
		m_Options = options;
	}

	public void Destroy()
	{
		m_HeadInfo = default(VCIsoHeadData);
		DestroyMaterials();
		m_Materials = null;
		DestroyDecalAssets();
		m_DecalAssets = null;
		if (m_Voxels != null)
		{
			m_Voxels.Clear();
			m_Voxels = null;
		}
		if (m_Components != null)
		{
			if (m_Options.ForEditor)
			{
				foreach (VCComponentData component in m_Components)
				{
					component.DestroyEntity();
				}
			}
			m_Components.Clear();
			m_Components = null;
		}
		if (m_Colors != null)
		{
			m_Colors.Clear();
			m_Colors = null;
		}
	}

	public void DestroyMaterials()
	{
		if (m_Materials == null)
		{
			return;
		}
		if (!m_Options.ForEditor)
		{
			VCMaterial[] materials = m_Materials;
			for (int i = 0; i < materials.Length; i++)
			{
				materials[i]?.Destroy();
			}
		}
		for (int j = 0; j < 16; j++)
		{
			m_Materials[j] = null;
		}
	}

	public void DestroyDecalAssets()
	{
		if (m_DecalAssets == null)
		{
			return;
		}
		if (!m_Options.ForEditor)
		{
			VCDecalAsset[] decalAssets = m_DecalAssets;
			for (int i = 0; i < decalAssets.Length; i++)
			{
				decalAssets[i]?.Destroy();
			}
		}
		for (int j = 0; j < 4; j++)
		{
			m_DecalAssets[j] = null;
		}
	}

	public void Clear()
	{
		DestroyMaterials();
		DestroyDecalAssets();
		m_Voxels.Clear();
		if (m_Options.ForEditor)
		{
			foreach (VCComponentData component in m_Components)
			{
				component.DestroyEntity();
			}
		}
		m_Components.Clear();
		m_Colors.Clear();
	}

	public int QueryVoxelType(VCMaterial vcmat)
	{
		if (vcmat == null)
		{
			return -1;
		}
		for (int i = 0; i < 16; i++)
		{
			if (m_Materials[i] != null && vcmat.m_Guid == m_Materials[i].m_Guid)
			{
				return i;
			}
		}
		for (int j = 0; j < 16; j++)
		{
			if (m_Materials[j] == null)
			{
				return j;
			}
		}
		int[] array = new int[16];
		for (int k = 0; k < 16; k++)
		{
			array[k] = 0;
		}
		foreach (KeyValuePair<int, VCVoxel> voxel in m_Voxels)
		{
			array[voxel.Value.Type]++;
		}
		for (int l = 0; l < 16; l++)
		{
			if (array[l] == 0)
			{
				return l;
			}
		}
		return -1;
	}

	public int QueryMaterialIndex(VCMaterial vcmat)
	{
		if (vcmat == null)
		{
			return -1;
		}
		for (int i = 0; i < 16; i++)
		{
			if (m_Materials[i] != null && vcmat.m_Guid == m_Materials[i].m_Guid)
			{
				return i;
			}
		}
		return -1;
	}

	public VCMaterial QueryMaterial(ulong guid)
	{
		for (int i = 0; i < 16; i++)
		{
			if (m_Materials[i] != null && guid == m_Materials[i].m_Guid)
			{
				return m_Materials[i];
			}
		}
		return null;
	}

	public int MaterialUsedCount()
	{
		int num = 0;
		for (int i = 0; i < 16; i++)
		{
			if (m_Materials[i] != null)
			{
				num++;
			}
		}
		return num;
	}

	public ulong MaterialGUID(int index)
	{
		if (index < 0 || index >= 16)
		{
			return 0uL;
		}
		if (m_Materials[index] == null)
		{
			return 0uL;
		}
		return m_Materials[index].m_Guid;
	}

	public int QueryNewDecalIndex(VCDecalAsset vcdcl)
	{
		if (vcdcl == null)
		{
			return -1;
		}
		for (int i = 0; i < 4; i++)
		{
			if (m_DecalAssets[i] != null && vcdcl.m_Guid == m_DecalAssets[i].m_Guid)
			{
				return i;
			}
		}
		for (int j = 0; j < 4; j++)
		{
			if (m_DecalAssets[j] == null)
			{
				return j;
			}
		}
		int[] array = new int[4];
		for (int k = 0; k < 4; k++)
		{
			array[k] = 0;
		}
		foreach (VCComponentData component in m_Components)
		{
			if (component.m_Type == EVCComponent.cpDecal && component is VCDecalData { m_AssetIndex: >=0, m_AssetIndex: <4 } vCDecalData)
			{
				array[vCDecalData.m_AssetIndex]++;
			}
		}
		for (int l = 0; l < 4; l++)
		{
			if (array[l] == 0)
			{
				return l;
			}
		}
		return -1;
	}

	public int QueryExistDecalIndex(VCDecalAsset vcdcl)
	{
		if (vcdcl == null)
		{
			return -1;
		}
		for (int i = 0; i < 4; i++)
		{
			if (m_DecalAssets[i] != null && vcdcl.m_Guid == m_DecalAssets[i].m_Guid)
			{
				return i;
			}
		}
		return -1;
	}

	public VCDecalAsset QueryExistDecal(ulong guid)
	{
		if (guid == 0L)
		{
			return null;
		}
		for (int i = 0; i < 4; i++)
		{
			if (m_DecalAssets[i] != null && guid == m_DecalAssets[i].m_Guid)
			{
				return m_DecalAssets[i];
			}
		}
		return null;
	}

	public int DecalUsedCount()
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			if (m_DecalAssets[i] != null)
			{
				num++;
			}
		}
		return num;
	}

	public ulong DecalGUID(int index)
	{
		if (index < 0 || index >= 4)
		{
			return 0uL;
		}
		if (m_DecalAssets[index] == null)
		{
			return 0uL;
		}
		return m_DecalAssets[index].m_Guid;
	}

	public bool IsColorPosIn(int x, int y, int z)
	{
		if (x < 1)
		{
			return false;
		}
		if (y < 1)
		{
			return false;
		}
		if (z < 1)
		{
			return false;
		}
		if (x > m_HeadInfo.xSize * 2 + 1)
		{
			return false;
		}
		if (y > m_HeadInfo.ySize * 2 + 1)
		{
			return false;
		}
		if (z > m_HeadInfo.zSize * 2 + 1)
		{
			return false;
		}
		return true;
	}

	public bool IsColorPosIn(IntVector3 pos)
	{
		if (pos.x < 1)
		{
			return false;
		}
		if (pos.y < 1)
		{
			return false;
		}
		if (pos.z < 1)
		{
			return false;
		}
		if (pos.x > m_HeadInfo.xSize * 2 + 1)
		{
			return false;
		}
		if (pos.y > m_HeadInfo.ySize * 2 + 1)
		{
			return false;
		}
		if (pos.z > m_HeadInfo.zSize * 2 + 1)
		{
			return false;
		}
		return true;
	}

	public bool IsPointIn(int x, int y, int z)
	{
		if (x < 0)
		{
			return false;
		}
		if (y < 0)
		{
			return false;
		}
		if (z < 0)
		{
			return false;
		}
		if (x >= m_HeadInfo.xSize)
		{
			return false;
		}
		if (y >= m_HeadInfo.ySize)
		{
			return false;
		}
		if (z >= m_HeadInfo.zSize)
		{
			return false;
		}
		return true;
	}

	public bool IsPointIn(IntVector3 pos)
	{
		if (pos == null)
		{
			return false;
		}
		if (pos.x < 0)
		{
			return false;
		}
		if (pos.y < 0)
		{
			return false;
		}
		if (pos.z < 0)
		{
			return false;
		}
		if (pos.x >= m_HeadInfo.xSize)
		{
			return false;
		}
		if (pos.y >= m_HeadInfo.ySize)
		{
			return false;
		}
		if (pos.z >= m_HeadInfo.zSize)
		{
			return false;
		}
		if (m_HeadInfo.Category == EVCCategory.cgDbSword && pos.x > Mathf.CeilToInt(0.5f * (float)m_HeadInfo.xSize - 6f) && pos.x < Mathf.CeilToInt(0.5f * (float)m_HeadInfo.xSize + 5f))
		{
			return false;
		}
		return true;
	}

	public bool IsPointIn(Vector3 pos)
	{
		if (pos.x < 0f)
		{
			return false;
		}
		if (pos.y < 0f)
		{
			return false;
		}
		if (pos.z < 0f)
		{
			return false;
		}
		if (pos.x >= (float)m_HeadInfo.xSize)
		{
			return false;
		}
		if (pos.y >= (float)m_HeadInfo.ySize)
		{
			return false;
		}
		if (pos.z >= (float)m_HeadInfo.zSize)
		{
			return false;
		}
		if (m_HeadInfo.Category == EVCCategory.cgDbSword && pos.x > (float)Mathf.CeilToInt(0.5f * (float)m_HeadInfo.xSize - 6f) && pos.x < (float)Mathf.CeilToInt(0.5f * (float)m_HeadInfo.xSize + 5f))
		{
			return false;
		}
		return true;
	}

	public bool IsComponentIn(Vector3 pos)
	{
		VCESceneSetting vCESceneSetting = m_HeadInfo.FindSceneSetting();
		if (vCESceneSetting == null)
		{
			return false;
		}
		float voxelSize = vCESceneSetting.m_VoxelSize;
		if (pos.x < 0f)
		{
			return false;
		}
		if (pos.y < 0f)
		{
			return false;
		}
		if (pos.z < 0f)
		{
			return false;
		}
		if (pos.x > (float)m_HeadInfo.xSize * voxelSize)
		{
			return false;
		}
		if (pos.y > (float)m_HeadInfo.ySize * voxelSize)
		{
			return false;
		}
		if (pos.z > (float)m_HeadInfo.zSize * voxelSize)
		{
			return false;
		}
		return true;
	}

	public Vector3 ClampPointWorldCoord(Vector3 pos)
	{
		VCESceneSetting vCESceneSetting = m_HeadInfo.FindSceneSetting();
		if (vCESceneSetting == null)
		{
			return Vector3.zero;
		}
		float voxelSize = vCESceneSetting.m_VoxelSize;
		float num = (float)m_HeadInfo.xSize * voxelSize;
		float num2 = (float)m_HeadInfo.ySize * voxelSize;
		float num3 = (float)m_HeadInfo.zSize * voxelSize;
		if (pos.x < 0f)
		{
			pos.x = 0f;
		}
		else if (pos.x > num)
		{
			pos.x = num;
		}
		if (pos.y < 0f)
		{
			pos.y = 0f;
		}
		else if (pos.y > num2)
		{
			pos.y = num2;
		}
		if (pos.z < 0f)
		{
			pos.z = 0f;
		}
		else if (pos.z > num3)
		{
			pos.z = num3;
		}
		return pos;
	}

	public Vector3 ClampPointF(Vector3 pos)
	{
		if (pos.x < 0f)
		{
			pos.x = 0f;
		}
		else if (pos.x >= (float)m_HeadInfo.xSize)
		{
			pos.x = (float)m_HeadInfo.xSize - 0.001f;
		}
		if (pos.y < 0f)
		{
			pos.y = 0f;
		}
		else if (pos.y >= (float)m_HeadInfo.ySize)
		{
			pos.y = (float)m_HeadInfo.ySize - 0.001f;
		}
		if (pos.z < 0f)
		{
			pos.z = 0f;
		}
		else if (pos.z >= (float)m_HeadInfo.zSize)
		{
			pos.z = (float)m_HeadInfo.zSize - 0.001f;
		}
		return pos;
	}

	public IntVector3 ClampPointI(IntVector3 pos)
	{
		if (pos.x < 0)
		{
			pos.x = 0;
		}
		else if (pos.x >= m_HeadInfo.xSize)
		{
			pos.x = m_HeadInfo.xSize - 1;
		}
		if (pos.y < 0)
		{
			pos.y = 0;
		}
		else if (pos.y >= m_HeadInfo.ySize)
		{
			pos.y = m_HeadInfo.ySize - 1;
		}
		if (pos.z < 0)
		{
			pos.z = 0;
		}
		else if (pos.z >= m_HeadInfo.zSize)
		{
			pos.z = m_HeadInfo.zSize - 1;
		}
		return pos;
	}

	public IntVector3 ClampPointI(IntVector3 pos, bool right)
	{
		if (m_HeadInfo.Category == EVCCategory.cgDbSword)
		{
			int num = 5;
			if (right && pos.x < 0)
			{
				pos.x = 0;
			}
			else if (!right && pos.x < Mathf.CeilToInt(0.5f * (float)m_HeadInfo.xSize + (float)num))
			{
				pos.x = Mathf.CeilToInt(0.5f * (float)m_HeadInfo.xSize + (float)num);
			}
			else if (right && pos.x >= Mathf.CeilToInt(0.5f * (float)m_HeadInfo.xSize - (float)num - 1f))
			{
				pos.x = Mathf.CeilToInt(0.5f * (float)m_HeadInfo.xSize - (float)num - 1f);
			}
			else if (!right && pos.x > m_HeadInfo.xSize)
			{
				pos.x = m_HeadInfo.xSize - 1;
			}
			else if (pos.y < 0)
			{
				pos.y = 0;
			}
			else if (pos.y >= m_HeadInfo.ySize)
			{
				pos.y = m_HeadInfo.ySize - 1;
			}
			if (pos.z < 0)
			{
				pos.z = 0;
			}
			else if (pos.z >= m_HeadInfo.zSize)
			{
				pos.z = m_HeadInfo.zSize - 1;
			}
			return pos;
		}
		if (pos.x < 0)
		{
			pos.x = 0;
		}
		else if (pos.x >= m_HeadInfo.xSize)
		{
			pos.x = m_HeadInfo.xSize - 1;
		}
		if (pos.y < 0)
		{
			pos.y = 0;
		}
		else if (pos.y >= m_HeadInfo.ySize)
		{
			pos.y = m_HeadInfo.ySize - 1;
		}
		if (pos.z < 0)
		{
			pos.z = 0;
		}
		else if (pos.z >= m_HeadInfo.zSize)
		{
			pos.z = m_HeadInfo.zSize - 1;
		}
		return pos;
	}

	public bool CanSee(Vector3 iso_pos)
	{
		int num = Mathf.FloorToInt(iso_pos.x);
		int num2 = Mathf.FloorToInt(iso_pos.y);
		int num3 = Mathf.FloorToInt(iso_pos.z);
		if (num <= 0 || num2 <= 0 || num3 <= 0 || num >= m_HeadInfo.xSize - 1 || num2 >= m_HeadInfo.ySize - 1 || num3 >= m_HeadInfo.zSize - 1)
		{
			return true;
		}
		if (GetVoxel(IPosToKey(num + 1, num2, num3)).Volume < 128)
		{
			return true;
		}
		if (GetVoxel(IPosToKey(num - 1, num2, num3)).Volume < 128)
		{
			return true;
		}
		if (GetVoxel(IPosToKey(num, num2 + 1, num3)).Volume < 128)
		{
			return true;
		}
		if (GetVoxel(IPosToKey(num, num2 - 1, num3)).Volume < 128)
		{
			return true;
		}
		if (GetVoxel(IPosToKey(num, num2, num3 + 1)).Volume < 128)
		{
			return true;
		}
		if (GetVoxel(IPosToKey(num, num2, num3 - 1)).Volume < 128)
		{
			return true;
		}
		return false;
	}

	public int GetComponentIndex(VCComponentData cdata)
	{
		if (cdata != null)
		{
			for (int i = 0; i < m_Components.Count; i++)
			{
				if (m_Components[i] == cdata)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public List<VCComponentData> FindComponentsAtPos(Vector3 wpos, int id = 0)
	{
		List<VCComponentData> list = new List<VCComponentData>();
		VCESceneSetting vCESceneSetting = m_HeadInfo.FindSceneSetting();
		if (vCESceneSetting == null)
		{
			return list;
		}
		foreach (VCComponentData component in m_Components)
		{
			if (Vector3.Distance(component.m_Position, wpos) < vCESceneSetting.m_VoxelSize * 0.01f && (id == 0 || component.m_ComponentId == id))
			{
				list.Add(component);
			}
		}
		return list;
	}

	public VCVoxel GetVoxel(int pos)
	{
		m_Voxels.TryGetValue(pos, out var value);
		return value;
	}

	public void SetVoxel(int pos, VCVoxel voxel)
	{
		if (voxel.Volume == 0)
		{
			if (m_Voxels.ContainsKey(pos))
			{
				m_Voxels.Remove(pos);
			}
		}
		else
		{
			m_Voxels[pos] = voxel;
		}
	}

	public Color32 GetColor(int pos)
	{
		if (m_Colors.TryGetValue(pos, out var value))
		{
			return value;
		}
		return BLANK_COLOR;
	}

	public void SetColor(int pos, Color32 color)
	{
		if (color.r == BLANK_COLOR.r && color.g == BLANK_COLOR.g && color.b == BLANK_COLOR.b && color.a == BLANK_COLOR.a)
		{
			if (m_Colors.ContainsKey(pos))
			{
				m_Colors.Remove(pos);
			}
		}
		else if (m_Colors.ContainsKey(pos))
		{
			m_Colors[pos] = color;
		}
		else
		{
			m_Colors.Add(pos, color);
		}
	}

	public bool IsGarbageVoxel(int pos)
	{
		byte volume = GetVoxel(pos).Volume;
		if (volume < 128)
		{
			int pos2 = pos + 1;
			int pos3 = pos - 1;
			int pos4 = pos + 1024;
			int pos5 = pos - 1024;
			int pos6 = pos + 1048576;
			int pos7 = pos - 1048576;
			if (GetVoxel(pos2).Volume < 128 && GetVoxel(pos3).Volume < 128 && GetVoxel(pos4).Volume < 128 && GetVoxel(pos5).Volume < 128 && GetVoxel(pos6).Volume < 128 && GetVoxel(pos7).Volume < 128)
			{
				return true;
			}
		}
		return false;
	}

	public void NormalizeAllVoxels()
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		foreach (KeyValuePair<int, VCVoxel> voxel in m_Voxels)
		{
			int pos = voxel.Key + 1;
			int pos2 = voxel.Key - 1;
			int pos3 = voxel.Key + 1024;
			int pos4 = voxel.Key - 1024;
			int pos5 = voxel.Key + 1048576;
			int pos6 = voxel.Key - 1048576;
			if (voxel.Value.Volume >= 128)
			{
				if (GetVoxel(pos).Volume >= 128 && GetVoxel(pos2).Volume >= 128 && GetVoxel(pos3).Volume >= 128 && GetVoxel(pos4).Volume >= 128 && GetVoxel(pos5).Volume >= 128 && GetVoxel(pos6).Volume >= 128)
				{
					list2.Add(voxel.Key);
				}
			}
			else if (GetVoxel(pos).Volume < 128 && GetVoxel(pos2).Volume < 128 && GetVoxel(pos3).Volume < 128 && GetVoxel(pos4).Volume < 128 && GetVoxel(pos5).Volume < 128 && GetVoxel(pos6).Volume < 128)
			{
				list.Add(voxel.Key);
			}
		}
		foreach (int item in list)
		{
			m_Voxels.Remove(item);
		}
		foreach (int item2 in list2)
		{
			VCVoxel value = m_Voxels[item2];
			value.Volume = byte.MaxValue;
			m_Voxels[item2] = value;
		}
		list.Clear();
		list2.Clear();
	}

	public byte[] Export()
	{
		using MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write("VCISO");
		m_HeadInfo.EnsureIconTexValid();
		binaryWriter.Write(33751041);
		binaryWriter.Write((int)m_HeadInfo.Category);
		string text = string.Empty;
		for (int i = 0; i < m_HeadInfo.Author.Length; i++)
		{
			text += (char)(m_HeadInfo.Author[i] ^ 0xAC);
		}
		binaryWriter.Write(text);
		binaryWriter.Write(m_HeadInfo.Name);
		binaryWriter.Write(m_HeadInfo.Desc);
		string text2 = string.Empty;
		for (int j = 0; j < m_HeadInfo.Remarks.Length; j++)
		{
			text2 += (char)(m_HeadInfo.Remarks[j] ^ 0xAC);
		}
		binaryWriter.Write(text2);
		binaryWriter.Write(m_HeadInfo.xSize);
		binaryWriter.Write(m_HeadInfo.ySize);
		binaryWriter.Write(m_HeadInfo.zSize);
		Texture2D texture2D = new Texture2D(4, 4);
		texture2D.LoadImage(m_HeadInfo.IconTex);
		m_HeadInfo.IconTex = texture2D.EncodeToJPG(25);
		UnityEngine.Object.Destroy(texture2D);
		binaryWriter.Write(m_HeadInfo.IconTex.Length);
		binaryWriter.Write(m_HeadInfo.IconTex, 0, m_HeadInfo.IconTex.Length);
		binaryWriter.Write(16);
		binaryWriter.Write(4);
		binaryWriter.Write(m_Components.Count);
		binaryWriter.Write(m_Voxels.Count);
		binaryWriter.Write(m_Colors.Count);
		for (int k = 0; k < 16; k++)
		{
			if (m_Materials[k] != null)
			{
				byte[] array = m_Materials[k].Export();
				binaryWriter.Write(m_Materials[k].m_Guid);
				binaryWriter.Write(array.Length);
				binaryWriter.Write(array, 0, array.Length);
			}
			else
			{
				ulong value = 0uL;
				binaryWriter.Write(value);
			}
		}
		for (int l = 0; l < 4; l++)
		{
			if (m_DecalAssets[l] != null)
			{
				byte[] array2 = m_DecalAssets[l].Export();
				binaryWriter.Write(m_DecalAssets[l].m_Guid);
				binaryWriter.Write(array2.Length);
				binaryWriter.Write(array2, 0, array2.Length);
			}
			else
			{
				ulong value2 = 0uL;
				binaryWriter.Write(value2);
			}
		}
		using (MemoryStream memoryStream2 = new MemoryStream())
		{
			BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream2);
			foreach (VCComponentData component in m_Components)
			{
				if (component != null)
				{
					byte[] array3 = component.Export();
					binaryWriter2.Write(array3.Length);
					binaryWriter2.Write(array3, 0, array3.Length);
					binaryWriter2.Write((int)component.m_Type);
				}
				else
				{
					int value3 = 0;
					binaryWriter.Write(value3);
				}
			}
			binaryWriter2.Write(m_HeadInfo.HeadSignature);
			foreach (KeyValuePair<int, VCVoxel> voxel in m_Voxels)
			{
				binaryWriter2.Write(voxel.Key);
				binaryWriter2.Write(voxel.Value);
			}
			foreach (KeyValuePair<int, Color32> color in m_Colors)
			{
				binaryWriter2.Write(color.Key);
				binaryWriter2.Write(color.Value.r);
				binaryWriter2.Write(color.Value.g);
				binaryWriter2.Write(color.Value.b);
				binaryWriter2.Write(color.Value.a);
			}
			using (MemoryStream memoryStream3 = new MemoryStream())
			{
				memoryStream2.Seek(0L, SeekOrigin.Begin);
				IonicZlib.Compress(memoryStream2, memoryStream3);
				binaryWriter.Write((int)memoryStream3.Length);
				binaryWriter.Write(memoryStream3.GetBuffer(), 0, (int)memoryStream3.Length);
			}
			binaryWriter2.Close();
		}
		binaryWriter.Close();
		return memoryStream.ToArray();
	}

	public bool Import(byte[] buffer, VCIsoOption options)
	{
		if (buffer == null)
		{
			return false;
		}
		Reset(options);
		int num = 48;
		if (buffer.Length < num)
		{
			return false;
		}
		try
		{
			using MemoryStream input = new MemoryStream(buffer);
			BinaryReader binaryReader = new BinaryReader(input);
			string text = binaryReader.ReadString();
			if (text != "VCISO")
			{
				binaryReader.Close();
				return false;
			}
			int num2 = 0;
			m_HeadInfo.Version = binaryReader.ReadInt32();
			m_HeadInfo.Category = (EVCCategory)binaryReader.ReadInt32();
			string text2 = string.Empty;
			if (m_HeadInfo.Version >= 33685504)
			{
				text2 = binaryReader.ReadString();
			}
			m_HeadInfo.Name = binaryReader.ReadString();
			m_HeadInfo.Desc = binaryReader.ReadString();
			string text3 = string.Empty;
			if (m_HeadInfo.Version >= 33685504)
			{
				text3 = binaryReader.ReadString();
			}
			m_HeadInfo.xSize = binaryReader.ReadInt32();
			m_HeadInfo.ySize = binaryReader.ReadInt32();
			m_HeadInfo.zSize = binaryReader.ReadInt32();
			num2 = binaryReader.ReadInt32();
			m_HeadInfo.IconTex = binaryReader.ReadBytes(num2);
			m_HeadInfo.EnsureIconTexValid();
			m_HeadInfo.Author = string.Empty;
			m_HeadInfo.Remarks = string.Empty;
			if (m_HeadInfo.Version >= 33685504)
			{
				for (int i = 0; i < text2.Length; i++)
				{
					m_HeadInfo.Author += (char)(text2[i] ^ 0xAC);
				}
				for (int j = 0; j < text3.Length; j++)
				{
					m_HeadInfo.Remarks += (char)(text3[j] ^ 0xAC);
				}
			}
			int version = m_HeadInfo.Version;
			if (version == 33685504 || version == 33685505 || version == 33554432 || version == 33619968 || version == 33751041)
			{
				int num3 = binaryReader.ReadInt32();
				int num4 = 0;
				if (m_HeadInfo.Version >= 33619968)
				{
					num4 = binaryReader.ReadInt32();
				}
				int num5 = binaryReader.ReadInt32();
				int num6 = binaryReader.ReadInt32();
				int num7 = binaryReader.ReadInt32();
				for (int k = 0; k < num3; k++)
				{
					ulong num8 = binaryReader.ReadUInt64();
					if (num8 == 0L)
					{
						continue;
					}
					num2 = binaryReader.ReadInt32();
					byte[] buffer2 = binaryReader.ReadBytes(num2);
					if (m_Options.ForEditor)
					{
						if (VCEAssetMgr.s_Materials.ContainsKey(num8))
						{
							m_Materials[k] = VCEAssetMgr.s_Materials[num8];
							continue;
						}
						VCMaterial vCMaterial = new VCMaterial();
						vCMaterial.Import(buffer2);
						VCEAssetMgr.s_TempMaterials[num8] = vCMaterial;
						m_Materials[k] = vCMaterial;
					}
					else
					{
						m_Materials[k] = new VCMaterial();
						m_Materials[k].Import(buffer2);
					}
				}
				for (int l = 0; l < num4; l++)
				{
					ulong num9 = binaryReader.ReadUInt64();
					if (num9 == 0L)
					{
						continue;
					}
					num2 = binaryReader.ReadInt32();
					byte[] buffer3 = binaryReader.ReadBytes(num2);
					if (m_Options.ForEditor)
					{
						if (VCEAssetMgr.s_Decals.ContainsKey(num9))
						{
							m_DecalAssets[l] = VCEAssetMgr.s_Decals[num9];
							continue;
						}
						VCDecalAsset vCDecalAsset = new VCDecalAsset();
						vCDecalAsset.Import(buffer3);
						VCEAssetMgr.s_TempDecals[num9] = vCDecalAsset;
						m_DecalAssets[l] = vCDecalAsset;
					}
					else
					{
						m_DecalAssets[l] = new VCDecalAsset();
						m_DecalAssets[l].Import(buffer3);
					}
				}
				using (MemoryStream memoryStream = new MemoryStream())
				{
					num2 = binaryReader.ReadInt32();
					memoryStream.Write(binaryReader.ReadBytes(num2), 0, num2);
					using MemoryStream memoryStream2 = new MemoryStream();
					memoryStream.Seek(0L, SeekOrigin.Begin);
					IonicZlib.Decompress(memoryStream, memoryStream2);
					memoryStream2.Seek(0L, SeekOrigin.Begin);
					BinaryReader binaryReader2 = new BinaryReader(memoryStream2);
					for (int m = 0; m < num5; m++)
					{
						num2 = binaryReader2.ReadInt32();
						if (num2 > 0)
						{
							byte[] buffer4 = binaryReader2.ReadBytes(num2);
							EVCComponent type = (EVCComponent)binaryReader2.ReadInt32();
							VCComponentData vCComponentData = VCComponentData.Create(type, buffer4);
							if (vCComponentData != null)
							{
								vCComponentData.m_CurrIso = this;
								m_Components.Add(vCComponentData);
							}
						}
					}
					if (m_HeadInfo.Version >= 33685505)
					{
						ulong num10 = binaryReader2.ReadUInt64();
						if (m_HeadInfo.HeadSignature != num10)
						{
							Debug.LogError("Check sig failed");
							return false;
						}
					}
					for (int n = 0; n < num6; n++)
					{
						int key = binaryReader2.ReadInt32();
						ushort num11 = binaryReader2.ReadUInt16();
						m_Voxels[key] = num11;
					}
					Color32 value = default(Color32);
					for (int num12 = 0; num12 < num7; num12++)
					{
						int key2 = binaryReader2.ReadInt32();
						value.r = binaryReader2.ReadByte();
						value.g = binaryReader2.ReadByte();
						value.b = binaryReader2.ReadByte();
						value.a = binaryReader2.ReadByte();
						m_Colors.Add(key2, value);
					}
					binaryReader2.Close();
				}
				binaryReader.Close();
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			Debug.LogError("Importing ISO Failed :" + ex.ToString());
			return false;
		}
	}

	private static int ExtractHeader(Stream stream, out VCIsoHeadData iso_header)
	{
		iso_header = default(VCIsoHeadData);
		int num = 0;
		try
		{
			BinaryReader binaryReader = new BinaryReader(stream);
			string text = binaryReader.ReadString();
			if (text != "VCISO")
			{
				stream.Close();
				return 0;
			}
			int num2 = 0;
			iso_header.Version = binaryReader.ReadInt32();
			iso_header.Category = (EVCCategory)binaryReader.ReadInt32();
			string text2 = string.Empty;
			if (iso_header.Version >= 33685504)
			{
				text2 = binaryReader.ReadString();
			}
			iso_header.Name = binaryReader.ReadString();
			iso_header.Desc = binaryReader.ReadString();
			string text3 = string.Empty;
			if (iso_header.Version >= 33685504)
			{
				text3 = binaryReader.ReadString();
			}
			iso_header.xSize = binaryReader.ReadInt32();
			iso_header.ySize = binaryReader.ReadInt32();
			iso_header.zSize = binaryReader.ReadInt32();
			num2 = binaryReader.ReadInt32();
			iso_header.IconTex = binaryReader.ReadBytes(num2);
			num = (int)stream.Length;
			stream.Close();
			iso_header.Author = string.Empty;
			iso_header.Remarks = string.Empty;
			if (iso_header.Version >= 33685504)
			{
				for (int i = 0; i < text2.Length; i++)
				{
					iso_header.Author += (char)(text2[i] ^ 0xAC);
				}
				for (int j = 0; j < text3.Length; j++)
				{
					iso_header.Remarks += (char)(text3[j] ^ 0xAC);
				}
			}
			if (iso_header.Name.Length > 256)
			{
				return 0;
			}
			if (iso_header.Desc.Length > 2048)
			{
				return 0;
			}
			if (iso_header.Author.Length > 256)
			{
				return 0;
			}
			if (iso_header.Remarks.Length > 8192)
			{
				return 0;
			}
			if (iso_header.xSize < 0)
			{
				return 0;
			}
			if (iso_header.xSize >= 512)
			{
				return 0;
			}
			if (iso_header.ySize < 0)
			{
				return 0;
			}
			if (iso_header.ySize >= 512)
			{
				return 0;
			}
			if (iso_header.zSize < 0)
			{
				return 0;
			}
			if (iso_header.zSize >= 512)
			{
				return 0;
			}
		}
		catch (Exception)
		{
			return 0;
		}
		iso_header.EnsureIconTexValid();
		return num;
	}

	public static int ExtractHeader(string filename, out VCIsoHeadData iso_header)
	{
		iso_header = default(VCIsoHeadData);
		if (!File.Exists(filename))
		{
			return 0;
		}
		try
		{
			using FileStream stream = File.Open(filename, FileMode.Open);
			return ExtractHeader(stream, out iso_header);
		}
		catch (Exception)
		{
			return 0;
		}
	}

	public static int ExtractHeader(byte[] buffer, out VCIsoHeadData iso_header)
	{
		iso_header = default(VCIsoHeadData);
		if (buffer == null)
		{
			return 0;
		}
		using MemoryStream stream = new MemoryStream(buffer);
		return ExtractHeader(stream, out iso_header);
	}
}
