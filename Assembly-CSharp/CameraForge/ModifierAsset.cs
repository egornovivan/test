using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CameraForge;

public class ModifierAsset : ScriptableObject
{
	public const int CURRENT_VERSION = 10000;

	public Modifier modifier;

	[SerializeField]
	public int version;

	[SerializeField]
	public byte[] data;

	public void Save()
	{
		version = 10000;
		using MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write("ModifierAsset");
		binaryWriter.Write(10000);
		binaryWriter.Write(modifier.nodes.Count);
		for (int i = 0; i < modifier.nodes.Count; i++)
		{
			Node node = modifier.nodes[i];
			binaryWriter.Write(node.GetType().ToString());
		}
		for (int j = 0; j < modifier.nodes.Count; j++)
		{
			Node node2 = modifier.nodes[j];
			node2.Write(binaryWriter);
		}
		if (modifier.output != null)
		{
			binaryWriter.Write(modifier.output.GetType().ToString());
			modifier.output.Write(binaryWriter);
		}
		else
		{
			binaryWriter.Write(string.Empty);
		}
		binaryWriter.Write(modifier.graphCenter.x);
		binaryWriter.Write(modifier.graphCenter.y);
		data = memoryStream.ToArray();
		binaryWriter.Close();
	}

	public void Load(bool bCreate = true)
	{
		if (modifier == null || bCreate)
		{
			modifier = new Modifier(base.name);
		}
		using MemoryStream input = new MemoryStream(data);
		BinaryReader binaryReader = new BinaryReader(input);
		string text = binaryReader.ReadString();
		if (text == "ModifierAsset")
		{
			version = binaryReader.ReadInt32();
			if (version == 10000)
			{
				Assembly assembly = Assembly.GetAssembly(typeof(Node));
				int num = binaryReader.ReadInt32();
				if (num < 0 || num > 4096)
				{
					throw new Exception("read node count error");
				}
				for (int i = 0; i < num; i++)
				{
					string typeName = binaryReader.ReadString();
					Node node = assembly.CreateInstance(typeName) as Node;
					modifier.AddNode(node);
				}
				for (int j = 0; j < num; j++)
				{
					modifier.nodes[j].Read(binaryReader);
				}
				string text2 = binaryReader.ReadString();
				if (string.IsNullOrEmpty(text2))
				{
					modifier.output = null;
				}
				else
				{
					modifier.output = assembly.CreateInstance(text2) as OutputNode;
					modifier.output.modifier = modifier;
					modifier.output.Read(binaryReader);
				}
				modifier.graphCenter.x = binaryReader.ReadSingle();
				modifier.graphCenter.y = binaryReader.ReadSingle();
			}
			binaryReader.Close();
			return;
		}
		throw new Exception("Invalid modifier asset!");
	}
}
