using System.Collections.Generic;
using System.IO;
using UnityEngine;

public struct BSIsoHeadData
{
	public int Version;

	public EBSVoxelType Mode;

	public string Author;

	public string Name;

	public string Desc;

	public string Remarks;

	public int xSize;

	public int ySize;

	public int zSize;

	public byte[] IconTex;

	public Dictionary<byte, uint> costs;

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

	public void Init()
	{
		Version = 0;
		Author = string.Empty;
		Name = string.Empty;
		Remarks = string.Empty;
		xSize = 0;
		ySize = 0;
		zSize = 0;
		IconTex = new byte[0];
		costs = new Dictionary<byte, uint>();
	}

	public void EnsureIconTexValid()
	{
		Texture2D texture2D = new Texture2D(2, 2);
		if (IconTex == null || IconTex.Length < 32 || !texture2D.LoadImage(IconTex))
		{
			texture2D = Object.Instantiate(Resources.Load("Textures/default_iso_icon") as Texture2D);
			IconTex = texture2D.EncodeToPNG();
		}
		Object.Destroy(texture2D);
	}
}
