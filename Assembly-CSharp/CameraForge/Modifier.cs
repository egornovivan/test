using System.Collections.Generic;
using UnityEngine;

namespace CameraForge;

public class Modifier : PoseNode
{
	private OutputNode _output;

	public List<Node> nodes;

	public Slot Name;

	public PoseSlot Prev;

	public float time;

	public Slot Life;

	public Slot Col;

	public Vector2 graphCenter;

	public override Slot[] slots => new Slot[3] { Name, Life, Col };

	public override PoseSlot[] poseslots => new PoseSlot[1] { Prev };

	public OutputNode output
	{
		get
		{
			return _output;
		}
		set
		{
			_output = value;
			if (_output != null)
			{
				_output.modifier = this;
			}
		}
	}

	public Modifier()
	{
		output = new YPROutput();
		nodes = new List<Node>();
		Name = new Slot("Name");
		Name.value = "Camera Mode";
		Prev = new PoseSlot("Prev");
		time = 0f;
		Life = new Slot("Life");
		Life.value = 1E+20;
		Col = new Slot("Color");
		Col.value = new Color(1f, 0.8f, 1f, 1f);
		graphCenter = new Vector2(-150f, 100f);
	}

	public Modifier(string name)
	{
		output = new YPROutput();
		nodes = new List<Node>();
		Name = new Slot("Name");
		Name.value = name;
		Prev = new PoseSlot("Prev");
		time = 0f;
		Life = new Slot("Life");
		Life.value = 1E+20;
		Col = new Slot("Color");
		Col.value = new Color(1f, 0.8f, 1f, 1f);
		graphCenter = new Vector2(-150f, 100f);
	}

	public override Pose Calculate()
	{
		Name.Calculate();
		Life.Calculate();
		Col.Calculate();
		Prev.Calculate();
		return output.Output();
	}

	public virtual void AddNode(Node node)
	{
		node.modifier = this;
		nodes.Add(node);
	}

	public virtual void DeleteNode(Node node)
	{
		node.modifier = null;
		for (int num = nodes.Count - 1; num >= 0; num--)
		{
			Slot[] array = nodes[num].slots;
			foreach (Slot slot in array)
			{
				if (slot.input == node)
				{
					slot.input = null;
				}
			}
			if (nodes[num] == node)
			{
				nodes[num] = null;
				nodes.RemoveAt(num);
			}
		}
		Slot[] array2 = output.slots;
		foreach (Slot slot2 in array2)
		{
			if (slot2.input == node)
			{
				slot2.input = null;
			}
		}
	}

	public virtual void Tick(float deltaTime)
	{
		time += deltaTime;
		if (time > Life.value.value_f && controller != null)
		{
			controller.DeletePoseNode(this);
		}
	}
}
