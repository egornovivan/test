using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CameraForge;

public class ControllerAsset : ScriptableObject
{
	public const int CURRENT_VERSION = 10000;

	public Controller controller;

	[SerializeField]
	private int version;

	[SerializeField]
	private byte[] data;

	public void Save()
	{
		version = 10000;
		using MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write("ControllerAsset");
		binaryWriter.Write(10000);
		binaryWriter.Write(controller.nodes.Count);
		binaryWriter.Write(controller.posenodes.Count);
		for (int i = 0; i < controller.nodes.Count; i++)
		{
			Node node = controller.nodes[i];
			binaryWriter.Write(node.GetType().ToString());
		}
		for (int j = 0; j < controller.nodes.Count; j++)
		{
			Node node2 = controller.nodes[j];
			node2.Write(binaryWriter);
		}
		for (int k = 0; k < controller.posenodes.Count; k++)
		{
			PoseNode poseNode = controller.posenodes[k];
			binaryWriter.Write(poseNode.GetType().ToString());
		}
		for (int l = 0; l < controller.posenodes.Count; l++)
		{
			PoseNode poseNode2 = controller.posenodes[l];
			poseNode2.Write(binaryWriter);
		}
		controller.final.Write(binaryWriter);
		binaryWriter.Write(controller.graphCenter.x);
		binaryWriter.Write(controller.graphCenter.y);
		data = memoryStream.ToArray();
		binaryWriter.Close();
	}

	public void Load()
	{
		controller = new Controller(base.name);
		using MemoryStream input = new MemoryStream(data);
		BinaryReader binaryReader = new BinaryReader(input);
		string text = binaryReader.ReadString();
		if (text == "ControllerAsset")
		{
			version = binaryReader.ReadInt32();
			if (version == 10000)
			{
				Assembly assembly = Assembly.GetAssembly(typeof(Node));
				int num = binaryReader.ReadInt32();
				int num2 = binaryReader.ReadInt32();
				if (num < 0 || num > 4096 || num2 < 0 || num2 > 4096)
				{
					throw new Exception("read node count error");
				}
				for (int i = 0; i < num; i++)
				{
					string typeName = binaryReader.ReadString();
					Node node = assembly.CreateInstance(typeName) as Node;
					controller.AddNode(node);
				}
				for (int j = 0; j < num; j++)
				{
					controller.nodes[j].Read(binaryReader);
				}
				for (int k = 0; k < num2; k++)
				{
					string typeName2 = binaryReader.ReadString();
					PoseNode node2 = assembly.CreateInstance(typeName2) as PoseNode;
					controller.AddPoseNode(node2);
				}
				for (int l = 0; l < num2; l++)
				{
					controller.posenodes[l].Read(binaryReader);
				}
				controller.final = new FinalPose();
				controller.final.controller = controller;
				controller.final.Read(binaryReader);
				controller.graphCenter.x = binaryReader.ReadSingle();
				controller.graphCenter.y = binaryReader.ReadSingle();
			}
			binaryReader.Close();
			return;
		}
		throw new Exception("Invalid controller asset!");
	}
}
