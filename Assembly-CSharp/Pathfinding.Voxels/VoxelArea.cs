using System;
using UnityEngine;

namespace Pathfinding.Voxels;

public class VoxelArea
{
	public const uint MaxHeight = 65536u;

	public const int MaxHeightInt = 65536;

	public const uint InvalidSpanValue = uint.MaxValue;

	public const float AvgSpanLayerCountEstimate = 8f;

	public readonly int width;

	public readonly int depth;

	public CompactVoxelSpan[] compactSpans;

	public CompactVoxelCell[] compactCells;

	public int compactSpanCount;

	public ushort[] tmpUShortArr;

	public int[] areaTypes;

	public ushort[] dist;

	public ushort maxDistance;

	public int maxRegions;

	public int[] DirectionX;

	public int[] DirectionZ;

	public Vector3[] VectorDirection;

	private int linkedSpanCount;

	public LinkedVoxelSpan[] linkedSpans;

	private int[] removedStack = new int[128];

	private int removedStackCount;

	public VoxelArea(int width, int depth)
	{
		this.width = width;
		this.depth = depth;
		int num = width * depth;
		compactCells = new CompactVoxelCell[num];
		linkedSpans = new LinkedVoxelSpan[((int)((float)num * 8f) + 15) & -16];
		ResetLinkedVoxelSpans();
		DirectionX = new int[4] { -1, 0, 1, 0 };
		DirectionZ = new int[4]
		{
			0,
			width,
			0,
			-width
		};
		VectorDirection = new Vector3[4]
		{
			Vector3.left,
			Vector3.forward,
			Vector3.right,
			Vector3.back
		};
	}

	public void Reset()
	{
		ResetLinkedVoxelSpans();
		for (int i = 0; i < compactCells.Length; i++)
		{
			compactCells[i].count = 0u;
			compactCells[i].index = 0u;
		}
	}

	private void ResetLinkedVoxelSpans()
	{
		int num = linkedSpans.Length;
		linkedSpanCount = width * depth;
		LinkedVoxelSpan linkedVoxelSpan = new LinkedVoxelSpan(uint.MaxValue, uint.MaxValue, -1, -1);
		int num2;
		for (num2 = 0; num2 < num; num2++)
		{
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
		}
		removedStackCount = 0;
	}

	public int GetSpanCountAll()
	{
		int num = 0;
		int num2 = width * depth;
		for (int i = 0; i < num2; i++)
		{
			int num3 = i;
			while (num3 != -1 && linkedSpans[num3].bottom != uint.MaxValue)
			{
				num++;
				num3 = linkedSpans[num3].next;
			}
		}
		return num;
	}

	public int GetSpanCount()
	{
		int num = 0;
		int num2 = width * depth;
		for (int i = 0; i < num2; i++)
		{
			int num3 = i;
			while (num3 != -1 && linkedSpans[num3].bottom != uint.MaxValue)
			{
				if (linkedSpans[num3].area != 0)
				{
					num++;
				}
				num3 = linkedSpans[num3].next;
			}
		}
		return num;
	}

	public void AddLinkedSpan(int index, uint bottom, uint top, int area, int voxelWalkableClimb)
	{
		if (linkedSpans[index].bottom == uint.MaxValue)
		{
			ref LinkedVoxelSpan reference = ref linkedSpans[index];
			reference = new LinkedVoxelSpan(bottom, top, area);
			return;
		}
		int num = -1;
		int num2 = index;
		while (index != -1 && linkedSpans[index].bottom <= top)
		{
			if (linkedSpans[index].top < bottom)
			{
				num = index;
				index = linkedSpans[index].next;
				continue;
			}
			if (linkedSpans[index].bottom < bottom)
			{
				bottom = linkedSpans[index].bottom;
			}
			if (linkedSpans[index].top > top)
			{
				top = linkedSpans[index].top;
			}
			if (AstarMath.Abs((int)(top - linkedSpans[index].top)) <= voxelWalkableClimb)
			{
				area = AstarMath.Max(area, linkedSpans[index].area);
			}
			int next = linkedSpans[index].next;
			if (num != -1)
			{
				linkedSpans[num].next = next;
				if (removedStackCount == removedStack.Length)
				{
					int[] dst = new int[removedStackCount * 4];
					Buffer.BlockCopy(removedStack, 0, dst, 0, removedStackCount * 4);
					removedStack = dst;
				}
				removedStack[removedStackCount] = index;
				removedStackCount++;
				index = next;
				continue;
			}
			if (next != -1)
			{
				ref LinkedVoxelSpan reference2 = ref linkedSpans[num2];
				reference2 = linkedSpans[next];
				if (removedStackCount == removedStack.Length)
				{
					int[] dst2 = new int[removedStackCount * 4];
					Buffer.BlockCopy(removedStack, 0, dst2, 0, removedStackCount * 4);
					removedStack = dst2;
				}
				removedStack[removedStackCount] = next;
				removedStackCount++;
				index = linkedSpans[num2].next;
				continue;
			}
			ref LinkedVoxelSpan reference3 = ref linkedSpans[num2];
			reference3 = new LinkedVoxelSpan(bottom, top, area);
			return;
		}
		if (linkedSpanCount >= linkedSpans.Length)
		{
			LinkedVoxelSpan[] array = linkedSpans;
			int num3 = linkedSpanCount;
			int num4 = removedStackCount;
			linkedSpans = new LinkedVoxelSpan[linkedSpans.Length * 2];
			ResetLinkedVoxelSpans();
			linkedSpanCount = num3;
			removedStackCount = num4;
			for (int i = 0; i < linkedSpanCount; i++)
			{
				ref LinkedVoxelSpan reference4 = ref linkedSpans[i];
				reference4 = array[i];
			}
			Debug.Log("Layer estimate too low, doubling size of buffer.\nThis message is harmless.");
		}
		int num5;
		if (removedStackCount > 0)
		{
			removedStackCount--;
			num5 = removedStack[removedStackCount];
		}
		else
		{
			num5 = linkedSpanCount;
			linkedSpanCount++;
		}
		if (num != -1)
		{
			ref LinkedVoxelSpan reference5 = ref linkedSpans[num5];
			reference5 = new LinkedVoxelSpan(bottom, top, area, linkedSpans[num].next);
			linkedSpans[num].next = num5;
		}
		else
		{
			ref LinkedVoxelSpan reference6 = ref linkedSpans[num5];
			reference6 = linkedSpans[num2];
			ref LinkedVoxelSpan reference7 = ref linkedSpans[num2];
			reference7 = new LinkedVoxelSpan(bottom, top, area, num5);
		}
	}
}
