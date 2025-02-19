using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public struct VCIsoHeadData
{
	public int Version;

	public EVCCategory Category;

	public string Author;

	public string Name;

	public string Desc;

	public string Remarks;

	public int xSize;

	public int ySize;

	public int zSize;

	public byte[] IconTex;

	public byte[] SteamPreview
	{
		get
		{
			using MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			memoryStream.Write(IconTex, 0, IconTex.Length);
			int value = (int)memoryStream.Length;
			binaryWriter.Write(Remarks);
			binaryWriter.Write(value);
			binaryWriter.Close();
			return memoryStream.ToArray();
		}
		set
		{
			if (value == null)
			{
				IconTex = null;
				Remarks = string.Empty;
				EnsureIconTexValid();
				return;
			}
			using MemoryStream memoryStream = new MemoryStream(value);
			BinaryReader binaryReader = new BinaryReader(memoryStream);
			memoryStream.Seek(-4L, SeekOrigin.End);
			int num = binaryReader.ReadInt32();
			memoryStream.Seek(0L, SeekOrigin.Begin);
			IconTex = new byte[num];
			memoryStream.Read(IconTex, 0, num);
			memoryStream.Seek(num, SeekOrigin.Begin);
			Remarks = binaryReader.ReadString();
			binaryReader.Close();
		}
	}

	public string SteamDesc => "Author: " + ((Author.Trim().Length <= 1) ? "-" : Author.Trim()) + "\r\n" + Desc;

	public ulong HeadSignature
	{
		get
		{
			string[] obj = new string[10] { "#s;al.t", null, null, null, null, null, null, null, null, null };
			int category = (int)Category;
			obj[1] = category.ToString();
			obj[2] = Author;
			obj[3] = Name;
			obj[4] = Desc;
			obj[5] = Remarks;
			obj[6] = xSize.ToString();
			obj[7] = ySize.ToString();
			obj[8] = zSize.ToString();
			obj[9] = "e.nd;s,a+lt";
			string s = string.Concat(obj);
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			return CRC64.Compute(bytes);
		}
	}

	public void Init()
	{
		Version = 0;
		Category = EVCCategory.cgAbstract;
		Author = string.Empty;
		Name = string.Empty;
		Remarks = string.Empty;
		xSize = 0;
		ySize = 0;
		zSize = 0;
		IconTex = new byte[0];
	}

	public VCESceneSetting FindSceneSetting()
	{
		if (VCConfig.s_EditorScenes == null)
		{
			return null;
		}
		foreach (VCESceneSetting s_EditorScene in VCConfig.s_EditorScenes)
		{
			if (s_EditorScene.m_Category == Category && s_EditorScene.m_EditorSize.x == xSize && s_EditorScene.m_EditorSize.y == ySize && s_EditorScene.m_EditorSize.z == zSize)
			{
				return s_EditorScene;
			}
		}
		return null;
	}

	public string[] ScenePaths()
	{
		VCESceneSetting vCESceneSetting = FindSceneSetting();
		List<string> list = new List<string>();
		while (vCESceneSetting != null)
		{
			list.Add(vCESceneSetting.m_Name);
			int parentId = vCESceneSetting.m_ParentId;
			vCESceneSetting = null;
			foreach (VCESceneSetting s_EditorScene in VCConfig.s_EditorScenes)
			{
				if (s_EditorScene.m_Id == parentId)
				{
					vCESceneSetting = s_EditorScene;
					break;
				}
			}
		}
		if (list.Count == 0)
		{
			return new string[0];
		}
		string[] array = new string[list.Count];
		int num = array.Length;
		foreach (string item in list)
		{
			array[--num] = item;
		}
		array[0] = "Creation";
		return array;
	}

	public void EnsureIconTexValid()
	{
		Texture2D texture2D = new Texture2D(2, 2);
		if (IconTex == null || IconTex.Length < 32 || !texture2D.LoadImage(IconTex))
		{
			texture2D = Object.Instantiate(Resources.Load("Textures/default_iso_icon") as Texture2D);
			IconTex = texture2D.EncodeToJPG(25);
		}
		Object.Destroy(texture2D);
	}

	public byte[] GetExtendedIconTex(int width, int height)
	{
		Texture2D texture2D = new Texture2D(2, 2);
		texture2D.LoadImage(IconTex);
		width = Mathf.Clamp(width, texture2D.width, 512);
		height = Mathf.Clamp(height, texture2D.height, 512);
		Texture2D texture2D2 = new Texture2D(width, height, TextureFormat.ARGB32, mipmap: false);
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				texture2D2.SetPixel(i, j, Color.white);
			}
		}
		int num = Mathf.FloorToInt((float)(width - texture2D.width) * 0.5f);
		int num2 = Mathf.FloorToInt((float)(height - texture2D.height) * 0.5f);
		for (int k = 0; k < texture2D.width; k++)
		{
			for (int l = 0; l < texture2D.height; l++)
			{
				texture2D2.SetPixel(k + num, l + num2, texture2D.GetPixel(k, l));
			}
		}
		texture2D2.Apply();
		byte[] result = texture2D2.EncodeToPNG();
		Object.Destroy(texture2D);
		Object.Destroy(texture2D2);
		return result;
	}
}
