using System.Collections.Generic;

namespace CameraForge;

public class Controller : Modifier
{
	public CameraController executor;

	public FinalPose final;

	public List<PoseNode> posenodes;

	public Controller()
	{
		controller = this;
		final = new FinalPose();
		final.controller = this;
		nodes = new List<Node>();
		posenodes = new List<PoseNode>();
	}

	public Controller(string name)
		: base(name)
	{
		controller = this;
		final = new FinalPose();
		final.controller = this;
		nodes = new List<Node>();
		posenodes = new List<PoseNode>();
	}

	public void AddPoseNode(PoseNode node)
	{
		node.controller = this;
		posenodes.Add(node);
	}

	public void DeletePoseNode(PoseNode node)
	{
		for (int num = posenodes.Count - 1; num >= 0; num--)
		{
			PoseSlot[] array = posenodes[num].poseslots;
			foreach (PoseSlot poseSlot in array)
			{
				if (poseSlot.input == node)
				{
					if (node is Modifier)
					{
						poseSlot.input = (node as Modifier).Prev.input;
					}
					else
					{
						poseSlot.input = null;
					}
				}
			}
			if (posenodes[num] == node)
			{
				posenodes[num] = null;
				posenodes.RemoveAt(num);
			}
		}
		if (final.Final.input == node)
		{
			if (node is Modifier)
			{
				final.Final.input = (node as Modifier).Prev.input;
			}
			else
			{
				final.Final.input = null;
			}
		}
	}

	public override void AddNode(Node node)
	{
		node.modifier = this;
		nodes.Add(node);
	}

	public override void DeleteNode(Node node)
	{
		for (int num = posenodes.Count - 1; num >= 0; num--)
		{
			Slot[] array = posenodes[num].slots;
			foreach (Slot slot in array)
			{
				if (slot.input == node)
				{
					slot.input = null;
				}
			}
		}
		for (int num2 = nodes.Count - 1; num2 >= 0; num2--)
		{
			Slot[] array2 = nodes[num2].slots;
			foreach (Slot slot2 in array2)
			{
				if (slot2.input == node)
				{
					slot2.input = null;
				}
			}
			if (nodes[num2] == node)
			{
				nodes[num2] = null;
				nodes.RemoveAt(num2);
			}
		}
	}

	public override void Tick(float deltaTime)
	{
		time += deltaTime;
		foreach (PoseNode posenode in posenodes)
		{
			if (posenode is Modifier)
			{
				(posenode as Modifier).Tick(deltaTime);
			}
		}
	}

	public override Pose Calculate()
	{
		return final.Calculate();
	}
}
