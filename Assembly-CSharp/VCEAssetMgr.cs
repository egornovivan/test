using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class VCEAssetMgr
{
	public struct IsoFileInfo
	{
		public bool m_IsFolder;

		public string m_Path;

		public string m_Name;
	}

	public static Dictionary<ulong, VCMaterial> s_Materials;

	public static Dictionary<ulong, VCMaterial> s_TempMaterials;

	public static Dictionary<ulong, VCDecalAsset> s_Decals;

	public static Dictionary<ulong, VCDecalAsset> s_TempDecals;

	public static void Init()
	{
		LoadMaterials();
		LoadDecals();
	}

	public static void Destroy()
	{
		if (s_Materials != null)
		{
			foreach (KeyValuePair<ulong, VCMaterial> s_Material in s_Materials)
			{
				s_Material.Value.Destroy();
			}
			s_Materials.Clear();
			s_Materials = null;
		}
		if (s_TempMaterials != null)
		{
			foreach (KeyValuePair<ulong, VCMaterial> s_TempMaterial in s_TempMaterials)
			{
				s_TempMaterial.Value.Destroy();
			}
			s_TempMaterials.Clear();
			s_TempMaterials = null;
		}
		if (s_Decals != null)
		{
			foreach (KeyValuePair<ulong, VCDecalAsset> s_Decal in s_Decals)
			{
				s_Decal.Value.Destroy();
			}
			s_Decals.Clear();
			s_Decals = null;
		}
		if (s_TempDecals == null)
		{
			return;
		}
		foreach (KeyValuePair<ulong, VCDecalAsset> s_TempDecal in s_TempDecals)
		{
			s_TempDecal.Value.Destroy();
		}
		s_TempDecals.Clear();
		s_TempDecals = null;
	}

	public static void LoadMaterials()
	{
		if (s_Materials == null)
		{
			s_Materials = new Dictionary<ulong, VCMaterial>();
		}
		if (s_TempMaterials == null)
		{
			s_TempMaterials = new Dictionary<ulong, VCMaterial>();
		}
		if (Directory.Exists(VCConfig.s_MaterialPath))
		{
			string[] files = Directory.GetFiles(VCConfig.s_MaterialPath, "*" + VCConfig.s_MaterialFileExt);
			if (files.Length > 0)
			{
				LoadMaterialFromList(files);
			}
			else
			{
				SendDefaultMaterials();
			}
		}
		else
		{
			Directory.CreateDirectory(VCConfig.s_MaterialPath);
			SendDefaultMaterials();
		}
	}

	private static void SendDefaultMaterials()
	{
		foreach (KeyValuePair<int, VCMatterInfo> s_Matter in VCConfig.s_Matters)
		{
			VCMatterInfo value = s_Matter.Value;
			VCMaterial vCMaterial = new VCMaterial();
			vCMaterial.m_Name = "Default".ToLocalizationString() + " " + value.Name;
			vCMaterial.m_MatterId = value.ItemIndex;
			vCMaterial.m_UseDefault = true;
			try
			{
				byte[] array = vCMaterial.Export();
				string text = CRC64.Compute(array).ToString("X").PadLeft(16, '0');
				FileStream fileStream = new FileStream(VCConfig.s_MaterialPath + text + VCConfig.s_MaterialFileExt, FileMode.Create, FileAccess.ReadWrite);
				fileStream.Write(array, 0, array.Length);
				fileStream.Close();
				vCMaterial.Import(array);
			}
			catch (Exception ex)
			{
				vCMaterial.Destroy();
				Debug.LogError("Save material [" + vCMaterial.m_Name + "] failed ! \r\n" + ex.ToString());
				continue;
			}
			if (s_Materials.ContainsKey(vCMaterial.m_Guid))
			{
				s_Materials[vCMaterial.m_Guid].Destroy();
				s_Materials[vCMaterial.m_Guid] = vCMaterial;
			}
			else
			{
				s_Materials.Add(vCMaterial.m_Guid, vCMaterial);
			}
		}
	}

	private static void LoadMaterialFromList(string[] files)
	{
		foreach (string text in files)
		{
			VCMaterial vCMaterial = new VCMaterial();
			try
			{
				FileStream fileStream = new FileStream(text, FileMode.Open, FileAccess.ReadWrite);
				byte[] array = new byte[(int)fileStream.Length];
				fileStream.Read(array, 0, (int)fileStream.Length);
				fileStream.Close();
				vCMaterial.Import(array);
				string text2 = new FileInfo(text).Name.ToUpper();
				string text3 = (vCMaterial.GUIDString + VCConfig.s_MaterialFileExt).ToUpper();
				if (text2 != text3)
				{
					throw new Exception("The name and GUID doesn't match!");
				}
			}
			catch (Exception ex)
			{
				vCMaterial.Destroy();
				Debug.LogError("Load material [" + text + "] failed ! \r\n" + ex.ToString());
				continue;
			}
			if (s_Materials.ContainsKey(vCMaterial.m_Guid))
			{
				s_Materials[vCMaterial.m_Guid].Destroy();
				s_Materials[vCMaterial.m_Guid] = vCMaterial;
			}
			else
			{
				s_Materials.Add(vCMaterial.m_Guid, vCMaterial);
			}
		}
	}

	public static void ClearTempMaterials()
	{
		if (s_TempMaterials == null)
		{
			return;
		}
		foreach (KeyValuePair<ulong, VCMaterial> s_TempMaterial in s_TempMaterials)
		{
			s_TempMaterial.Value.Destroy();
		}
		s_TempMaterials.Clear();
	}

	public static bool DeleteMaterialDataFile(ulong guid)
	{
		string path = VCConfig.s_MaterialPath + guid.ToString("X").PadLeft(16, '0') + VCConfig.s_MaterialFileExt;
		try
		{
			File.Delete(path);
		}
		catch (Exception)
		{
			return false;
		}
		return !File.Exists(path);
	}

	public static bool CreateMaterialDataFile(VCMaterial vcmat)
	{
		try
		{
			byte[] array = vcmat.Export();
			string text = CRC64.Compute(array).ToString("X").PadLeft(16, '0');
			FileStream fileStream = new FileStream(VCConfig.s_MaterialPath + text + VCConfig.s_MaterialFileExt, FileMode.Create, FileAccess.ReadWrite);
			fileStream.Write(array, 0, array.Length);
			fileStream.Close();
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static bool DeleteMaterial(ulong guid)
	{
		return s_Materials.Remove(guid) && DeleteMaterialDataFile(guid);
	}

	public static bool AddMaterialFromTemp(ulong guid)
	{
		if (s_TempMaterials.ContainsKey(guid))
		{
			VCMaterial vCMaterial = s_TempMaterials[guid];
			s_TempMaterials.Remove(guid);
			s_Materials.Add(guid, vCMaterial);
			return CreateMaterialDataFile(vCMaterial);
		}
		return false;
	}

	public static void LoadDecals()
	{
		if (s_Decals == null)
		{
			s_Decals = new Dictionary<ulong, VCDecalAsset>();
		}
		if (s_TempDecals == null)
		{
			s_TempDecals = new Dictionary<ulong, VCDecalAsset>();
		}
		if (Directory.Exists(VCConfig.s_DecalPath))
		{
			string[] files = Directory.GetFiles(VCConfig.s_DecalPath, "*" + VCConfig.s_DecalFileExt);
			if (files.Length > 0)
			{
				LoadDecalFromList(files);
			}
			else
			{
				SendDefaultDecals();
			}
		}
		else
		{
			Directory.CreateDirectory(VCConfig.s_DecalPath);
			SendDefaultDecals();
		}
	}

	private static void SendDefaultDecals()
	{
		for (int i = 0; i < 6; i++)
		{
			TextAsset textAsset = Resources.Load("Decals/Default" + i.ToString("00")) as TextAsset;
			if (!(textAsset == null))
			{
				VCDecalAsset vCDecalAsset = new VCDecalAsset();
				vCDecalAsset.Import(textAsset.bytes);
				try
				{
					byte[] array = vCDecalAsset.Export();
					string text = CRC64.Compute(array).ToString("X").PadLeft(16, '0');
					FileStream fileStream = new FileStream(VCConfig.s_DecalPath + text + VCConfig.s_DecalFileExt, FileMode.Create, FileAccess.ReadWrite);
					fileStream.Write(array, 0, array.Length);
					fileStream.Close();
				}
				catch (Exception ex)
				{
					vCDecalAsset.Destroy();
					Debug.LogError("Save decal [" + vCDecalAsset.GUIDString + "] failed ! \r\n" + ex.ToString());
					continue;
				}
				if (s_Decals.ContainsKey(vCDecalAsset.m_Guid))
				{
					s_Decals[vCDecalAsset.m_Guid].Destroy();
					s_Decals[vCDecalAsset.m_Guid] = vCDecalAsset;
				}
				else
				{
					s_Decals.Add(vCDecalAsset.m_Guid, vCDecalAsset);
				}
			}
		}
	}

	private static void LoadDecalFromList(string[] files)
	{
		foreach (string text in files)
		{
			VCDecalAsset vCDecalAsset = new VCDecalAsset();
			try
			{
				FileStream fileStream = new FileStream(text, FileMode.Open, FileAccess.ReadWrite);
				byte[] array = new byte[(int)fileStream.Length];
				fileStream.Read(array, 0, (int)fileStream.Length);
				fileStream.Close();
				vCDecalAsset.Import(array);
				string text2 = new FileInfo(text).Name.ToUpper();
				string text3 = (vCDecalAsset.GUIDString + VCConfig.s_DecalFileExt).ToUpper();
				if (text2 != text3)
				{
					throw new Exception("The name and GUID doesn't match!");
				}
			}
			catch (Exception ex)
			{
				vCDecalAsset.Destroy();
				Debug.LogError("Load decal [" + text + "] failed ! \r\n" + ex.ToString());
				continue;
			}
			if (s_Decals.ContainsKey(vCDecalAsset.m_Guid))
			{
				s_Decals[vCDecalAsset.m_Guid].Destroy();
				s_Decals[vCDecalAsset.m_Guid] = vCDecalAsset;
			}
			else
			{
				s_Decals.Add(vCDecalAsset.m_Guid, vCDecalAsset);
			}
		}
	}

	public static void ClearTempDecals()
	{
		if (s_TempDecals == null)
		{
			return;
		}
		foreach (KeyValuePair<ulong, VCDecalAsset> s_TempDecal in s_TempDecals)
		{
			s_TempDecal.Value.Destroy();
		}
		s_TempDecals.Clear();
	}

	public static bool DeleteDecalDataFile(ulong guid)
	{
		string path = VCConfig.s_DecalPath + guid.ToString("X").PadLeft(16, '0') + VCConfig.s_DecalFileExt;
		try
		{
			File.Delete(path);
		}
		catch (Exception)
		{
			return false;
		}
		return !File.Exists(path);
	}

	public static bool CreateDecalDataFile(VCDecalAsset vcdcl)
	{
		try
		{
			byte[] array = vcdcl.Export();
			string text = CRC64.Compute(array).ToString("X").PadLeft(16, '0');
			FileStream fileStream = new FileStream(VCConfig.s_DecalPath + text + VCConfig.s_DecalFileExt, FileMode.Create, FileAccess.ReadWrite);
			fileStream.Write(array, 0, array.Length);
			fileStream.Close();
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static bool DeleteDecal(ulong guid)
	{
		return s_Decals.Remove(guid) && DeleteDecalDataFile(guid);
	}

	public static bool AddDecalFromTemp(ulong guid)
	{
		if (s_TempDecals.ContainsKey(guid))
		{
			VCDecalAsset vCDecalAsset = s_TempDecals[guid];
			s_TempDecals.Remove(guid);
			s_Decals.Add(guid, vCDecalAsset);
			return CreateDecalDataFile(vCDecalAsset);
		}
		return false;
	}

	public static List<IsoFileInfo> SearchIso(string path, string keyword)
	{
		if (path == null)
		{
			return new List<IsoFileInfo>();
		}
		if (keyword == null)
		{
			keyword = string.Empty;
		}
		path = path.Trim();
		keyword = keyword.Trim();
		string searchPattern = "*" + VCConfig.s_IsoFileExt;
		string[] array = null;
		string[] array2 = null;
		try
		{
			if (keyword.Length < 1)
			{
				array = Directory.GetDirectories(path, "*");
				array2 = Directory.GetFiles(path, searchPattern);
			}
			else
			{
				array = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
				array2 = Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
			}
		}
		catch
		{
			return new List<IsoFileInfo>();
		}
		List<IsoFileInfo> list = new List<IsoFileInfo>();
		string[] array3 = array;
		foreach (string path2 in array3)
		{
			string name = new DirectoryInfo(path2).Name;
			if (keyword.Length < 1 || name.IndexOf(keyword) >= 0)
			{
				IsoFileInfo item = default(IsoFileInfo);
				item.m_IsFolder = true;
				item.m_Name = name;
				item.m_Path = path2;
				list.Add(item);
			}
		}
		string[] array4 = array2;
		foreach (string text in array4)
		{
			string name2 = new FileInfo(text).Name;
			name2 = name2.Substring(0, name2.Length - VCConfig.s_IsoFileExt.Length);
			if (keyword.Length < 1 || name2.IndexOf(keyword) >= 0)
			{
				IsoFileInfo item2 = default(IsoFileInfo);
				item2.m_IsFolder = false;
				item2.m_Name = name2;
				item2.m_Path = text;
				list.Add(item2);
			}
		}
		return list;
	}

	public static VCMaterial GetMaterial(ulong guid)
	{
		if (s_Materials.ContainsKey(guid))
		{
			return s_Materials[guid];
		}
		if (s_TempMaterials.ContainsKey(guid))
		{
			return s_TempMaterials[guid];
		}
		return null;
	}

	public static VCDecalAsset GetDecal(ulong guid)
	{
		if (s_Decals.ContainsKey(guid))
		{
			return s_Decals[guid];
		}
		if (s_TempDecals.ContainsKey(guid))
		{
			return s_TempDecals[guid];
		}
		return null;
	}
}
