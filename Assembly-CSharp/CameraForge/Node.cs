using System.IO;
using UnityEngine;

namespace CameraForge;

public abstract class Node
{
	public Modifier modifier;

	public Vector2 editorPos;

	public abstract Slot[] slots { get; }

	public abstract Var Calculate();

	public bool IsDependencyOf(Node node)
	{
		if (node == null)
		{
			return false;
		}
		if (this == node)
		{
			return true;
		}
		Slot[] array = node.slots;
		foreach (Slot slot in array)
		{
			if (IsDependencyOf(slot.input))
			{
				return true;
			}
		}
		return false;
	}

	internal virtual void Write(BinaryWriter w)
	{
		if (modifier == null)
		{
			Debug.LogError("no modifier!");
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
				for (int j = 0; j < modifier.nodes.Count; j++)
				{
					if (slot.input == modifier.nodes[j])
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
	}

	internal virtual void Read(BinaryReader r)
	{
		if (modifier == null)
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
					if (num2 >= 0 && num2 < modifier.nodes.Count)
					{
						slot.input = modifier.nodes[num2];
					}
					if (slot.input == this)
					{
						slot.input = null;
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
	}
}
