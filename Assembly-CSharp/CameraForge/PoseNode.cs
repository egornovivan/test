using System;
using System.IO;
using UnityEngine;

namespace CameraForge;

public abstract class PoseNode
{
	public Controller controller;

	public Vector2 editorPos;

	public byte[] data = new byte[0];

	public abstract PoseSlot[] poseslots { get; }

	public abstract Slot[] slots { get; }

	public abstract Pose Calculate();

	internal virtual void Write(BinaryWriter w)
	{
		if (controller == null)
		{
			Debug.LogError("no controller!");
			return;
		}
		w.Write(editorPos.x);
		w.Write(editorPos.y);
		Slot[] array = slots;
		w.Write(array.Length);
		foreach (Slot slot in array)
		{
			w.Write(slot.name);
			if (slot.input != null)
			{
				int value = -1;
				for (int j = 0; j < controller.nodes.Count; j++)
				{
					if (slot.input == controller.nodes[j])
					{
						value = j;
						break;
					}
				}
				w.Write(value);
			}
			else
			{
				w.Write(-1);
				slot.value.Write(w);
			}
		}
		PoseSlot[] array2 = poseslots;
		w.Write(array2.Length);
		foreach (PoseSlot poseSlot in array2)
		{
			w.Write(poseSlot.name);
			if (poseSlot.input != null)
			{
				int value2 = -1;
				for (int l = 0; l < controller.posenodes.Count; l++)
				{
					if (poseSlot.input == controller.posenodes[l])
					{
						value2 = l;
						break;
					}
				}
				w.Write(value2);
			}
			else
			{
				w.Write(-1);
			}
		}
		data = new byte[0];
		if (this is Modifier)
		{
			ModifierAsset modifierAsset = ScriptableObject.CreateInstance<ModifierAsset>();
			modifierAsset.modifier = this as Modifier;
			modifierAsset.Save();
			data = new byte[modifierAsset.data.Length];
			Array.Copy(modifierAsset.data, data, modifierAsset.data.Length);
			UnityEngine.Object.DestroyImmediate(modifierAsset);
		}
		w.Write(data.Length);
		w.Write(data, 0, data.Length);
	}

	internal virtual void Read(BinaryReader r)
	{
		if (controller == null)
		{
			Debug.LogError("no modifier!");
			return;
		}
		editorPos.x = r.ReadSingle();
		editorPos.y = r.ReadSingle();
		Slot[] array = slots;
		int num = r.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string text = r.ReadString();
			Slot slot = null;
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j].name == text)
				{
					slot = array[j];
					break;
				}
			}
			if (slot != null)
			{
				int num2 = r.ReadInt32();
				if (num2 != -1)
				{
					slot.input = null;
					if (num2 >= 0 && num2 < controller.nodes.Count)
					{
						slot.input = controller.nodes[num2];
					}
				}
				else
				{
					slot.input = null;
					slot.value.Read(r);
				}
			}
			else
			{
				int num3 = r.ReadInt32();
				if (num3 == -1)
				{
					Var @null = Var.Null;
					@null.Read(r);
				}
			}
		}
		PoseSlot[] array2 = poseslots;
		int num4 = r.ReadInt32();
		for (int k = 0; k < num4; k++)
		{
			string text2 = r.ReadString();
			PoseSlot poseSlot = null;
			for (int l = 0; l < array2.Length; l++)
			{
				if (array2[l].name == text2)
				{
					poseSlot = array2[l];
					break;
				}
			}
			int num5 = r.ReadInt32();
			if (poseSlot == null)
			{
				continue;
			}
			if (num5 != -1)
			{
				poseSlot.input = null;
				if (num5 >= 0 && num5 < controller.posenodes.Count)
				{
					poseSlot.input = controller.posenodes[num5];
				}
				if (poseSlot.input == this)
				{
					poseSlot.input = null;
				}
			}
			else
			{
				poseSlot.input = null;
				poseSlot.value = Pose.Default;
			}
		}
		int num6 = r.ReadInt32();
		data = r.ReadBytes(num6);
		if (this is Modifier)
		{
			ModifierAsset modifierAsset = ScriptableObject.CreateInstance<ModifierAsset>();
			modifierAsset.modifier = this as Modifier;
			modifierAsset.data = new byte[num6];
			Array.Copy(data, modifierAsset.data, num6);
			modifierAsset.Load(bCreate: false);
			UnityEngine.Object.DestroyImmediate(modifierAsset);
		}
	}
}
