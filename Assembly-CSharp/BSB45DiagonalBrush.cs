using System.Collections.Generic;
using UnityEngine;

public class BSB45DiagonalBrush : BSFreeSizeBrush
{
	private enum ECalcuHeight
	{
		XDir,
		ZDir
	}

	public bool upVSeal = true;

	public bool upBSeal;

	public int m_Rot;

	[SerializeField]
	private BSB45Computer m_Computer;

	[SerializeField]
	private Material meshMat;

	private Vector3 _prevSize = new Vector3(0f, 0f, 0f);

	public override Bounds brushBound
	{
		get
		{
			Bounds result = default(Bounds);
			result.min = base.Min;
			result.max = base.Max;
			return result;
		}
	}

	protected override bool ExtraAdjust()
	{
		if (dataSource != BuildingMan.Blocks)
		{
			if (gizmoCube.gameObject.activeSelf)
			{
				gizmoCube.gameObject.SetActive(value: false);
			}
			return false;
		}
		if (pattern.type == EBSVoxelType.Block && Input.GetKeyDown(KeyCode.T))
		{
			m_Rot = ((++m_Rot <= 3) ? m_Rot : 0);
			_prevSize = Vector3.zero;
		}
		return true;
	}

	public override bool CanDrawGL()
	{
		if (dataSource == BuildingMan.Blocks)
		{
			return true;
		}
		return false;
	}

	protected override void AdjustHeightExtraDo(ECoordPlane drag_plane)
	{
		if (m_Rot == 0 || m_Rot == 2)
		{
			Vector3 vector = m_Begin * dataSource.ScaleInverted;
			Vector3 vector2 = m_End * dataSource.ScaleInverted;
			switch (drag_plane)
			{
			case ECoordPlane.XZ:
			{
				Vector3 vector4 = CalcuSizeY(vector, vector2, ECalcuHeight.XDir);
				vector2 = vector + vector4;
				m_End = new Vector3(vector2.x * dataSource.Scale, vector2.y * dataSource.Scale, vector2.z * dataSource.Scale);
				break;
			}
			case ECoordPlane.ZY:
			{
				Vector3 vector3 = CalcuSizeX(vector, vector2, ECalcuHeight.XDir);
				vector2 = vector + vector3;
				m_End = new Vector3(vector2.x * dataSource.Scale, vector2.y * dataSource.Scale, vector2.z * dataSource.Scale);
				break;
			}
			}
		}
		else if (m_Rot == 1 || m_Rot == 3)
		{
			Vector3 vector5 = m_Begin * dataSource.ScaleInverted;
			Vector3 vector6 = m_End * dataSource.ScaleInverted;
			switch (drag_plane)
			{
			case ECoordPlane.XZ:
			{
				Vector3 vector8 = CalcuSizeY(vector5, vector6, ECalcuHeight.ZDir);
				vector6 = vector5 + vector8;
				m_End = new Vector3(vector6.x * dataSource.Scale, vector6.y * dataSource.Scale, vector6.z * dataSource.Scale);
				break;
			}
			case ECoordPlane.XY:
			{
				Vector3 vector7 = CalcuSizeX(vector5, vector6, ECalcuHeight.ZDir);
				vector6 = vector5 + vector7;
				m_End = new Vector3(vector6.x * dataSource.Scale, vector6.y * dataSource.Scale, vector6.z * dataSource.Scale);
				break;
			}
			}
		}
	}

	protected override void DragPlaneExtraDo(ECoordPlane drag_plane)
	{
		Vector3 vector = m_Begin * dataSource.ScaleInverted;
		Vector3 vector2 = m_End * dataSource.ScaleInverted;
		if (m_Rot == 0 || m_Rot == 2)
		{
			if (drag_plane == ECoordPlane.XY)
			{
				Vector3 vector3 = CalcuSizeXY(vector, vector2);
				vector2 = vector + vector3;
				m_End = new Vector3(vector2.x * dataSource.Scale, vector2.y * dataSource.Scale, vector2.z * dataSource.Scale);
			}
		}
		else if ((m_Rot == 1 || m_Rot == 3) && drag_plane == ECoordPlane.ZY)
		{
			Vector3 vector4 = CalcuSizeYZ(vector, vector2);
			vector2 = vector + vector4;
			m_End = new Vector3(vector2.x * dataSource.Scale, vector2.y * dataSource.Scale, vector2.z * dataSource.Scale);
		}
	}

	private static bool EqualsZero(float v)
	{
		return v > -0.0001f && v < 0.0001f;
	}

	protected override void AfterDo()
	{
		if (m_Phase != 0)
		{
			Vector3 size = base.Size;
			if (!object.Equals(size, _prevSize))
			{
				m_Computer.ClearDataDS();
				DestroyPreviewMesh();
				_prevSize = size;
				Debug.LogWarning(" Prev Size :" + _prevSize.ToString() + " Size :" + size.ToString());
				if (m_Rot == 0)
				{
					IntVector3 intVector = CorrectSizeXY(size);
					int x_ = 0;
					int x = intVector.x;
					int num = 0;
					int z = intVector.z;
					int y = intVector.y;
					if (x < 1 || y < 1 || z <= 0)
					{
						return;
					}
					for (int i = num; i < z; i++)
					{
						IntVector3 up = new IntVector3(x, y, i);
						IntVector3 dn = new IntVector3(x_, 0, i);
						ComputerPreview(up, dn, upVSeal, upBSeal, materialType);
						m_Computer.RebuildMesh();
					}
				}
				else if (m_Rot == 1)
				{
					IntVector3 intVector2 = CorrectSizeZY(size);
					int num2 = 0;
					int x2 = intVector2.x;
					int z_ = 0;
					int z2 = intVector2.z;
					int y2 = intVector2.y;
					if (z2 < 1 || y2 < 1 || x2 <= 0)
					{
						return;
					}
					for (int j = num2; j < x2; j++)
					{
						IntVector3 up2 = new IntVector3(j, y2, z2);
						IntVector3 dn2 = new IntVector3(j, 0, z_);
						ComputerPreview(up2, dn2, upVSeal, upBSeal, materialType);
						m_Computer.RebuildMesh();
					}
				}
				else if (m_Rot == 2)
				{
					IntVector3 intVector3 = CorrectSizeXY(size);
					int x_2 = 0;
					int x3 = intVector3.x;
					int num3 = 0;
					int z3 = intVector3.z;
					int y3 = intVector3.y;
					if (x3 < 1 || y3 < 1 || z3 <= 0)
					{
						return;
					}
					for (int k = num3; k < z3; k++)
					{
						IntVector3 up3 = new IntVector3(x_2, y3, k);
						IntVector3 dn3 = new IntVector3(x3, 0, k);
						ComputerPreview(up3, dn3, upVSeal, upBSeal, materialType);
						m_Computer.RebuildMesh();
					}
				}
				else if (m_Rot == 3)
				{
					IntVector3 intVector4 = CorrectSizeZY(size);
					int num4 = 0;
					int x4 = intVector4.x;
					int z_2 = 0;
					int z4 = intVector4.z;
					int y4 = intVector4.y;
					if (z4 < 1 || y4 < 1 || x4 <= 0)
					{
						return;
					}
					for (int l = num4; l < x4; l++)
					{
						IntVector3 up4 = new IntVector3(l, y4, z_2);
						IntVector3 dn4 = new IntVector3(l, 0, z4);
						ComputerPreview(up4, dn4, upVSeal, upBSeal, materialType);
						m_Computer.RebuildMesh();
					}
				}
			}
		}
		else
		{
			DestroyPreviewMesh();
		}
		if (!(meshMat != null))
		{
			return;
		}
		meshMat.renderQueue = 3000;
		Renderer[] componentsInChildren = m_Computer.GetComponentsInChildren<Renderer>(includeInactive: true);
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			if (meshMat != null)
			{
				renderer.material = meshMat;
			}
		}
	}

	private void DestroyPreviewMesh()
	{
		for (int i = 0; i < m_Computer.transform.childCount; i++)
		{
			Object.Destroy(m_Computer.transform.GetChild(i).gameObject);
		}
	}

	protected override void Do()
	{
		Vector3 min = base.Min;
		Vector3 size = base.Size;
		List<BSVoxel> list = new List<BSVoxel>();
		List<IntVector3> list2 = new List<IntVector3>();
		List<BSVoxel> list3 = new List<BSVoxel>();
		Dictionary<IntVector3, int> refMap = new Dictionary<IntVector3, int>();
		if (m_Rot == 0)
		{
			IntVector3 intVector = CorrectSizeXY(size);
			int num = Mathf.FloorToInt(min.x * (float)BSBlock45Data.s_ScaleInverted);
			int num2 = num + intVector.x;
			int num3 = Mathf.FloorToInt(min.z * (float)BSBlock45Data.s_ScaleInverted);
			int num4 = num3 + intVector.z;
			int num5 = Mathf.FloorToInt(min.y * (float)BSBlock45Data.s_ScaleInverted);
			int num6 = num5 + intVector.y;
			if (num2 < num + 1 || num6 < num5 + 1 || num4 <= num3)
			{
				return;
			}
			for (int i = num3; i < num4; i++)
			{
				IntVector3 up = new IntVector3(num2, num6, i);
				IntVector3 dn = new IntVector3(num, num5, i);
				ApplyBevel2_10(up, dn, upVSeal, upBSeal, BuildingMan.Blocks, materialType, list, list3, list2, refMap);
			}
		}
		else if (m_Rot == 1)
		{
			IntVector3 intVector2 = CorrectSizeZY(size);
			int num7 = Mathf.FloorToInt(min.x * (float)BSBlock45Data.s_ScaleInverted);
			int num8 = num7 + intVector2.x;
			int num9 = Mathf.FloorToInt(min.z * (float)BSBlock45Data.s_ScaleInverted);
			int num10 = num9 + intVector2.z;
			int num11 = Mathf.FloorToInt(min.y * (float)BSBlock45Data.s_ScaleInverted);
			int num12 = num11 + intVector2.y;
			if (num8 < num7 + 1 || num12 < num11 + 1 || num10 <= num9)
			{
				return;
			}
			for (int j = num7; j < num8; j++)
			{
				IntVector3 up2 = new IntVector3(j, num12, num10);
				IntVector3 dn2 = new IntVector3(j, num11, num9);
				ApplyBevel2_10(up2, dn2, upVSeal, upBSeal, BuildingMan.Blocks, materialType, list, list3, list2, refMap);
			}
		}
		else if (m_Rot == 2)
		{
			IntVector3 intVector3 = CorrectSizeXY(size);
			int num13 = Mathf.FloorToInt(min.x * (float)BSBlock45Data.s_ScaleInverted);
			int num14 = num13 + intVector3.x;
			int num15 = Mathf.FloorToInt(min.z * (float)BSBlock45Data.s_ScaleInverted);
			int num16 = num15 + intVector3.z;
			int num17 = Mathf.FloorToInt(min.y * (float)BSBlock45Data.s_ScaleInverted);
			int num18 = num17 + intVector3.y;
			if (num14 < num13 + 1 || num18 < num17 + 1 || num16 <= num15)
			{
				return;
			}
			for (int k = num15; k < num16; k++)
			{
				IntVector3 up3 = new IntVector3(num13, num18, k);
				IntVector3 dn3 = new IntVector3(num14, num17, k);
				ApplyBevel2_10(up3, dn3, upVSeal, upBSeal, BuildingMan.Blocks, materialType, list, list3, list2, refMap);
			}
		}
		else if (m_Rot == 3)
		{
			IntVector3 intVector4 = CorrectSizeZY(size);
			int num19 = Mathf.FloorToInt(min.x * (float)BSBlock45Data.s_ScaleInverted);
			int num20 = num19 + intVector4.x;
			int num21 = Mathf.FloorToInt(min.z * (float)BSBlock45Data.s_ScaleInverted);
			int num22 = num21 + intVector4.z;
			int num23 = Mathf.FloorToInt(min.y * (float)BSBlock45Data.s_ScaleInverted);
			int num24 = num23 + intVector4.y;
			if (num20 < num19 + 1 || num24 < num23 + 1 || num22 <= num21)
			{
				return;
			}
			for (int l = num19; l < num20; l++)
			{
				IntVector3 up4 = new IntVector3(l, num24, num21);
				IntVector3 dn4 = new IntVector3(l, num23, num22);
				ApplyBevel2_10(up4, dn4, upVSeal, upBSeal, BuildingMan.Blocks, materialType, list, list3, list2, refMap);
			}
		}
		BSBrush.FindExtraExtendableVoxels(dataSource, list, list3, list2, refMap);
		if (list2.Count != 0)
		{
			BSAction bSAction = new BSAction();
			BSVoxelModify modify = new BSVoxelModify(list2.ToArray(), list3.ToArray(), list.ToArray(), dataSource, EBSBrushMode.Add);
			bSAction.AddModify(modify);
			if (bSAction.Do())
			{
				BSHistory.AddAction(bSAction);
			}
		}
	}

	private Vector3 CalcuSizeY(IntVector3 begin, IntVector3 end, ECalcuHeight type)
	{
		Vector3 result = new Vector3(end.x - begin.x, end.y - begin.y, end.z - begin.z);
		Vector3 vector = new Vector3(Mathf.Abs(result.x), Mathf.Abs(result.y), Mathf.Abs(result.z));
		float num = 0f;
		switch (type)
		{
		case ECalcuHeight.XDir:
			num = vector.x;
			break;
		case ECalcuHeight.ZDir:
			num = vector.z;
			break;
		default:
			return result;
		}
		float num2 = Mathf.Abs(num / result.y);
		if (num2 < 1f)
		{
			if (num2 < 0.3334f)
			{
				result.y = Mathf.Sign(result.y) * num * 3f;
			}
			else if (num2 > 0.3333f && num2 <= 0.5f)
			{
				result.y = Mathf.Sign(result.y) * num * 2f;
			}
			else
			{
				result.y = Mathf.Sign(result.y) * num;
			}
		}
		else if (num2 > 1f)
		{
			if (num2 < 3f)
			{
				float v = num % 2f;
				if (EqualsZero(v))
				{
					result.y = Mathf.Sign(result.y) * num * 0.5f;
				}
				else
				{
					v = num % 3f;
					if (EqualsZero(v))
					{
						result.y = Mathf.Sign(result.y) * num / 3f;
					}
					else
					{
						result.y = 1f;
					}
				}
			}
			else if (num2 >= 3f)
			{
				float v2 = num % 3f;
				if (EqualsZero(v2))
				{
					result.y = Mathf.Sign(result.y) * num / 3f;
				}
				else
				{
					result.y = 1f;
				}
			}
		}
		return result;
	}

	private Vector3 CalcuSizeXDirOfY(IntVector3 begin, IntVector3 end)
	{
		Vector3 result = new Vector3(end.x - begin.x, end.y - begin.y, end.z - begin.z);
		Vector3 vector = new Vector3(Mathf.Abs(result.x), Mathf.Abs(result.y), Mathf.Abs(result.z));
		float num = Mathf.Abs(result.x / result.y);
		if (num < 1f)
		{
			if (num < 0.3334f)
			{
				result.y = Mathf.Sign(result.y) * (float)((int)vector.x * 3);
			}
			else if (num > 0.3333f && num <= 0.5f)
			{
				result.y = Mathf.Sign(result.y) * (float)((int)vector.x * 2);
			}
			else
			{
				result.y = Mathf.Sign(result.y) * vector.x;
			}
		}
		else if (num > 1f)
		{
			if (num < 3f)
			{
				float num2 = vector.x % 2f;
				if (num2 > -0.0001f && num2 < 0.0001f)
				{
					result.y = Mathf.Sign(result.y) * vector.x * 0.5f;
				}
				else
				{
					num2 = vector.x % 3f;
					if (num2 > -0.0001f && num2 < 0.0001f)
					{
						result.y = Mathf.Sign(result.y) * vector.x / 3f;
					}
					else
					{
						result.y = 1f;
					}
				}
			}
			else if (num >= 3f)
			{
				float num3 = vector.x % 3f;
				if (num3 > -0.0001f && num3 < 0.0001f)
				{
					result.y = Mathf.Sign(result.y) * vector.x / 3f;
				}
				else
				{
					result.y = 1f;
				}
			}
		}
		return result;
	}

	private Vector3 CalcuSizeX(IntVector3 begin, IntVector3 end, ECalcuHeight type)
	{
		Vector3 result = new Vector3(end.x - begin.x, end.y - begin.y, end.z - begin.z);
		Vector3 vector = new Vector3(Mathf.Abs(result.x), Mathf.Abs(result.y), Mathf.Abs(result.z));
		float num = 0f;
		float num2 = 1f;
		switch (type)
		{
		case ECalcuHeight.XDir:
			num = vector.x;
			num2 = Mathf.Sign(result.x);
			break;
		case ECalcuHeight.ZDir:
			num = vector.z;
			num2 = Mathf.Sign(result.z);
			break;
		default:
			return result;
		}
		float num3 = Mathf.Abs(num / result.y);
		if (num3 < 1f)
		{
			if (num3 < 0.5f)
			{
				float v = vector.y % 3f;
				num = ((!EqualsZero(v)) ? 1f : (num2 * vector.y / 3f));
			}
			else if (num3 >= 0.5f)
			{
				float v2 = vector.y % 2f;
				if (EqualsZero(v2))
				{
					num = num2 * vector.y * 0.5f;
				}
				else
				{
					v2 = vector.y % 3f;
					num = ((!EqualsZero(v2)) ? 1f : (num2 * vector.y / 3f));
				}
			}
		}
		else if (num3 > 1f)
		{
			if (num3 < 2f)
			{
				num = num2 * vector.y;
			}
			else if (num3 >= 2f && num3 < 3f)
			{
				num = num2 * vector.y * 2f;
			}
			else if (num3 >= 3f)
			{
				num = num2 * vector.y * 3f;
			}
		}
		switch (type)
		{
		case ECalcuHeight.XDir:
			result.x = num;
			break;
		case ECalcuHeight.ZDir:
			result.z = num;
			break;
		}
		return result;
	}

	private Vector3 CalcuSizeXY(IntVector3 begin, IntVector3 end)
	{
		Vector3 result = new Vector3(end.x - begin.x, end.y - begin.y, end.z - begin.z);
		Vector3 vector = new Vector3(Mathf.Abs(result.x), Mathf.Abs(result.y), Mathf.Abs(result.z));
		float num = Mathf.Abs(result.x / result.y);
		if (num < 1f)
		{
			if (num < 0.3334f)
			{
				result.y = Mathf.Sign(result.y) * (float)((int)vector.x * 3);
			}
			else if (num > 0.3333f && num <= 0.5f)
			{
				result.y = Mathf.Sign(result.y) * (float)((int)vector.x * 2);
			}
			else
			{
				result.y = Mathf.Sign(result.y) * vector.x;
			}
		}
		else if (num > 1f)
		{
			if (num < 2f)
			{
				result.x = Mathf.Sign(result.x) * vector.y;
			}
			else if (num >= 2f && num < 3f)
			{
				result.x = Mathf.Sign(result.x) * vector.y * 2f;
			}
			else if (num >= 3f)
			{
				result.x = Mathf.Sign(result.x) * vector.y * 3f;
			}
		}
		return result;
	}

	private Vector3 CalcuSizeYZ(IntVector3 begin, IntVector3 end)
	{
		Vector3 result = new Vector3(end.x - begin.x, end.y - begin.y, end.z - begin.z);
		Vector3 vector = new Vector3(Mathf.Abs(result.x), Mathf.Abs(result.y), Mathf.Abs(result.z));
		float num = Mathf.Abs(result.z / result.y);
		if (num < 1f)
		{
			if (num < 0.3334f)
			{
				result.y = Mathf.Sign(result.y) * (float)((int)vector.z * 3);
			}
			else if (num > 0.3333f && num <= 0.5f)
			{
				result.y = Mathf.Sign(result.y) * (float)((int)vector.z * 2);
			}
			else
			{
				result.y = Mathf.Sign(result.y) * vector.z;
			}
		}
		else if (num > 1f)
		{
			if (num < 2f)
			{
				result.z = Mathf.Sign(result.z) * vector.y;
			}
			else if (num >= 2f && num < 3f)
			{
				result.z = Mathf.Sign(result.z) * vector.y * 2f;
			}
			else if (num >= 3f)
			{
				result.z = Mathf.Sign(result.z) * vector.y * 3f;
			}
		}
		return result;
	}

	private IntVector3 CorrectSizeXY(Vector3 size)
	{
		IntVector3 intVector = new IntVector3((int)size.x, (int)size.y, (int)size.z);
		int num = 0;
		if (size.x >= size.y)
		{
			num = Mathf.RoundToInt(size.x / size.y);
			intVector.x--;
			intVector.y = Mathf.FloorToInt(intVector.x / num);
			intVector.x = num * intVector.y;
		}
		else
		{
			num = Mathf.RoundToInt(size.y / size.x);
			intVector.y--;
			intVector.x = Mathf.FloorToInt(intVector.y / num);
			intVector.y = num * intVector.x;
		}
		return intVector;
	}

	private IntVector3 CorrectSizeZY(Vector3 size)
	{
		IntVector3 intVector = new IntVector3((int)size.x, (int)size.y, (int)size.z);
		int num = 0;
		if (size.z >= size.y)
		{
			num = (int)(size.z / size.y);
			intVector.z--;
			intVector.y = Mathf.FloorToInt(intVector.z / num);
			intVector.z = num * intVector.y;
		}
		else
		{
			num = (int)(size.y / size.z);
			intVector.y--;
			intVector.z = Mathf.FloorToInt(intVector.y / num);
			intVector.y = num * intVector.z;
		}
		return intVector;
	}

	public static void MakeExtendableBS(int primitiveType, int rotation, int extendDir, int length, int materialType, out BSVoxel b0, out BSVoxel b1)
	{
		B45Block.MakeExtendableBlock(primitiveType, rotation, extendDir, length, materialType, out var block, out var block2);
		b0 = new BSVoxel(block);
		b1 = new BSVoxel(block2);
	}

	public static void MakeExtendableBS(int primitiveType, int rotation, int extendDir, int length, int materialType, out B45Block b0, out B45Block b1)
	{
		B45Block.MakeExtendableBlock(primitiveType, rotation, extendDir, length, materialType, out var block, out var block2);
		b0 = block;
		b1 = block2;
	}

	private void WriteBSVoxel(IBSDataSource ds, List<BSVoxel> new_voxels, List<BSVoxel> old_voxels, List<IntVector3> indexes, Dictionary<IntVector3, int> refMap, BSVoxel voxel, int x, int y, int z)
	{
		IntVector3 intVector = new IntVector3(x, y, z);
		BSVoxel item = ds.SafeRead(intVector.x, intVector.y, intVector.z);
		old_voxels.Add(item);
		new_voxels.Add(voxel);
		indexes.Add(intVector);
		refMap[intVector] = 0;
	}

	public void ApplyBevel2_10(IntVector3 up, IntVector3 dn, bool upVSeal, bool dnVSeal, IBSDataSource ds, byte matType, List<BSVoxel> new_voxels, List<BSVoxel> old_voxels, List<IntVector3> indexes, Dictionary<IntVector3, int> refMap)
	{
		int num = up.y - dn.y;
		int num2 = up.x - dn.x;
		int num3 = up.z - dn.z;
		int num4 = Mathf.Abs(num2);
		int num5 = Mathf.Abs(num3);
		int num6 = 2;
		int num7 = 10;
		int num8 = 0;
		int num9 = 0;
		int num10 = num4;
		int num11 = ((num2 > 0) ? 1 : (-1));
		int num12 = 0;
		int num13 = 0;
		int num14 = 0;
		int num15 = 1;
		int num16 = 0;
		int num17 = 2;
		int num18 = 0;
		int num19 = ((up.x > dn.x) ? 2 : 0);
		int extendDir = 0;
		if (num4 < num5)
		{
			num10 = num5;
			num11 = 0;
			num12 = ((num3 > 0) ? 1 : (-1));
			num15 = 0;
			num16 = 1;
			num17 = 0;
			num18 = 2;
			num19 = ((up.z > dn.z) ? 1 : 3);
			extendDir = 1;
		}
		if (num < 2 * num10 && num10 < 2 * num)
		{
			num9 = Mathf.Min(num, num10);
			BSVoxel voxel = new BSVoxel((byte)((num6 << 2) | num19), matType);
			BSVoxel voxel2 = new BSVoxel((byte)((num7 << 2) | ((num19 + 2) & 3)), matType);
			if (dnVSeal)
			{
				WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, voxel2, dn.x, dn.y - 1, dn.z);
			}
			for (int i = 0; i < num9; i++)
			{
				WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, voxel, dn.x + i * num11, dn.y + i, dn.z + i * num12);
				WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, voxel2, dn.x + (i + 1) * num11, dn.y + i, dn.z + (i + 1) * num12);
			}
			if (upVSeal)
			{
				WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, voxel, dn.x + num9 * num11, dn.y + num9, dn.z + num9 * num12);
			}
			return;
		}
		BSVoxel b;
		BSVoxel b2;
		BSVoxel b3;
		BSVoxel b4;
		BSVoxel voxel3;
		BSVoxel voxel4;
		if (num > num10)
		{
			extendDir = 2;
			num8 = num / num10;
			if (num8 > 3)
			{
				num8 = 3;
			}
			num9 = num10;
			MakeExtendableBS(num6, num19, extendDir, num8, (int)matType, out b, out b2);
			MakeExtendableBS(num7, (num19 + 2) & 3, extendDir, num8, (int)matType, out b3, out b4);
			voxel3 = b2;
			voxel4 = b4;
			if (dnVSeal)
			{
				WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, b3, dn.x, dn.y - num8, dn.z);
				WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, b4, dn.x, dn.y - num8 + 1, dn.z);
				if (num8 == 3)
				{
					WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, voxel4, dn.x, dn.y - num8 + 2, dn.z);
				}
			}
			for (int j = 0; j < num9; j++)
			{
				WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, b, dn.x + j * num11, dn.y + j * num8, dn.z + j * num12);
				WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, b2, dn.x + j * num11, dn.y + j * num8 + 1, dn.z + j * num12);
				if (num8 == 3)
				{
					WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, voxel3, dn.x + j * num11, dn.y + j * num8 + 2, dn.z + j * num12);
				}
				WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, b3, dn.x + (j + 1) * num11, dn.y + j * num8, dn.z + (j + 1) * num12);
				WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, b4, dn.x + (j + 1) * num11, dn.y + j * num8 + 1, dn.z + (j + 1) * num12);
				if (num8 == 3)
				{
					WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, voxel4, dn.x + (j + 1) * num11, dn.y + j * num8 + 2, dn.z + (j + 1) * num12);
				}
			}
			if (upVSeal)
			{
				WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, b, dn.x + num9 * num11, dn.y + num9 * num8, dn.z + num9 * num12);
				WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, b2, dn.x + num9 * num11, dn.y + num9 * num8 + 1, dn.z + num9 * num12);
				if (num8 == 3)
				{
					WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, voxel3, dn.x + num9 * num11, dn.y + num9 * num8 + 2, dn.z + num9 * num12);
				}
			}
			return;
		}
		num8 = num10 / num;
		if (num8 > 3)
		{
			num8 = 3;
		}
		num9 = num;
		MakeExtendableBS(num6, num19, extendDir, num8, (int)matType, out b, out b2);
		MakeExtendableBS(num7, (num19 + 2) & 3, extendDir, num8, (int)matType, out b3, out b4);
		voxel3 = b2;
		voxel4 = b4;
		if (dnVSeal)
		{
			WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, b3, dn.x + num13, dn.y - 1, dn.z + num14);
			WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, b4, dn.x + num15, dn.y - 1, dn.z + num16);
			if (num8 == 3)
			{
				WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, voxel4, dn.x + num17, dn.y - 1, dn.z + num18);
			}
		}
		for (int k = 0; k < num; k++)
		{
			WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, b, dn.x + k * num8 * num11 + num13, dn.y + k, dn.z + k * num8 * num12 + num14);
			WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, b2, dn.x + k * num8 * num11 + num15, dn.y + k, dn.z + k * num8 * num12 + num16);
			if (num8 == 3)
			{
				WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, voxel3, dn.x + k * num8 * num11 + num17, dn.y + k, dn.z + k * num8 * num12 + num18);
			}
			WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, b3, dn.x + (k + 1) * num8 * num11 + num13, dn.y + k, dn.z + (k + 1) * num8 * num12 + num14);
			WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, b4, dn.x + (k + 1) * num8 * num11 + num15, dn.y + k, dn.z + (k + 1) * num8 * num12 + num16);
			if (num8 == 3)
			{
				WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, voxel4, dn.x + (k + 1) * num8 * num11 + num17, dn.y + k, dn.z + (k + 1) * num8 * num12 + num18);
			}
		}
		if (upVSeal)
		{
			WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, b, dn.x + num9 * num8 * num11 + num13, dn.y + num9, dn.z + num9 * num8 * num12 + num14);
			WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, b2, dn.x + num9 * num8 * num11 + num15, dn.y + num9, dn.z + num9 * num8 * num12 + num16);
			if (num8 == 3)
			{
				WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, voxel3, dn.x + num9 * num8 * num11 + num17, dn.y + num9, dn.z + num9 * num8 * num12 + num18);
			}
		}
	}

	public void ComputerPreview(IntVector3 up, IntVector3 dn, bool upVSeal, bool dnVSeal, byte matType)
	{
		int num = up.y - dn.y;
		int num2 = up.x - dn.x;
		int num3 = up.z - dn.z;
		int num4 = Mathf.Abs(num2);
		int num5 = Mathf.Abs(num3);
		int num6 = 2;
		int num7 = 10;
		int num8 = 0;
		int num9 = 0;
		int num10 = num4;
		int num11 = ((num2 > 0) ? 1 : (-1));
		int num12 = 0;
		int num13 = 0;
		int num14 = 0;
		int num15 = 1;
		int num16 = 0;
		int num17 = 2;
		int num18 = 0;
		int num19 = ((up.x > dn.x) ? 2 : 0);
		int extendDir = 0;
		if (num4 < num5)
		{
			num10 = num5;
			num11 = 0;
			num12 = ((num3 > 0) ? 1 : (-1));
			num15 = 0;
			num16 = 1;
			num17 = 0;
			num18 = 2;
			num19 = ((up.z > dn.z) ? 1 : 3);
			extendDir = 1;
		}
		if (num < 2 * num10 && num10 < 2 * num)
		{
			num9 = Mathf.Min(num, num10);
			B45Block blk = new B45Block((byte)((num6 << 2) | num19), matType);
			B45Block blk2 = new B45Block((byte)((num7 << 2) | ((num19 + 2) & 3)), matType);
			if (dnVSeal)
			{
				m_Computer.AlterBlockInBuild(dn.x, dn.y - 1, dn.z, blk2);
			}
			for (int i = 0; i < num9; i++)
			{
				m_Computer.AlterBlockInBuild(dn.x + i * num11, dn.y + i, dn.z + i * num12, blk);
				m_Computer.AlterBlockInBuild(dn.x + (i + 1) * num11, dn.y + i, dn.z + (i + 1) * num12, blk2);
			}
			if (upVSeal)
			{
				m_Computer.AlterBlockInBuild(dn.x + num9 * num11, dn.y + num9, dn.z + num9 * num12, blk);
			}
			return;
		}
		B45Block b;
		B45Block b2;
		B45Block b3;
		B45Block b4;
		B45Block blk3;
		B45Block blk4;
		if (num > num10)
		{
			extendDir = 2;
			num8 = num / num10;
			if (num8 > 3)
			{
				num8 = 3;
			}
			num9 = num10;
			MakeExtendableBS(num6, num19, extendDir, num8, (int)matType, out b, out b2);
			MakeExtendableBS(num7, (num19 + 2) & 3, extendDir, num8, (int)matType, out b3, out b4);
			blk3 = b2;
			blk4 = b4;
			if (dnVSeal)
			{
				m_Computer.AlterBlockInBuild(dn.x, dn.y - num8, dn.z, b3);
				m_Computer.AlterBlockInBuild(dn.x, dn.y - num8 + 1, dn.z, b4);
				if (num8 == 3)
				{
					m_Computer.AlterBlockInBuild(dn.x, dn.y - num8 + 2, dn.z, blk4);
				}
			}
			for (int j = 0; j < num9; j++)
			{
				m_Computer.AlterBlockInBuild(dn.x + j * num11, dn.y + j * num8, dn.z + j * num12, b);
				m_Computer.AlterBlockInBuild(dn.x + j * num11, dn.y + j * num8 + 1, dn.z + j * num12, b2);
				if (num8 == 3)
				{
					m_Computer.AlterBlockInBuild(dn.x + j * num11, dn.y + j * num8 + 2, dn.z + j * num12, blk3);
				}
				m_Computer.AlterBlockInBuild(dn.x + (j + 1) * num11, dn.y + j * num8, dn.z + (j + 1) * num12, b3);
				m_Computer.AlterBlockInBuild(dn.x + (j + 1) * num11, dn.y + j * num8 + 1, dn.z + (j + 1) * num12, b4);
				if (num8 == 3)
				{
					m_Computer.AlterBlockInBuild(dn.x + (j + 1) * num11, dn.y + j * num8 + 2, dn.z + (j + 1) * num12, blk4);
				}
			}
			if (upVSeal)
			{
				m_Computer.AlterBlockInBuild(dn.x + num9 * num11, dn.y + num9 * num8, dn.z + num9 * num12, b);
				m_Computer.AlterBlockInBuild(dn.x + num9 * num11, dn.y + num9 * num8 + 1, dn.z + num9 * num12, b2);
				if (num8 == 3)
				{
					m_Computer.AlterBlockInBuild(dn.x + num9 * num11, dn.y + num9 * num8 + 2, dn.z + num9 * num12, blk3);
				}
			}
			return;
		}
		num8 = num10 / num;
		if (num8 > 3)
		{
			num8 = 3;
		}
		num9 = num;
		MakeExtendableBS(num6, num19, extendDir, num8, (int)matType, out b, out b2);
		MakeExtendableBS(num7, (num19 + 2) & 3, extendDir, num8, (int)matType, out b3, out b4);
		blk3 = b2;
		blk4 = b4;
		if (dnVSeal)
		{
			m_Computer.AlterBlockInBuild(dn.x + num13, dn.y - 1, dn.z + num14, b3);
			m_Computer.AlterBlockInBuild(dn.x + num15, dn.y - 1, dn.z + num16, b4);
			if (num8 == 3)
			{
				m_Computer.AlterBlockInBuild(dn.x + num17, dn.y - 1, dn.z + num18, blk4);
			}
		}
		for (int k = 0; k < num; k++)
		{
			m_Computer.AlterBlockInBuild(dn.x + k * num8 * num11 + num13, dn.y + k, dn.z + k * num8 * num12 + num14, b);
			m_Computer.AlterBlockInBuild(dn.x + k * num8 * num11 + num15, dn.y + k, dn.z + k * num8 * num12 + num16, b2);
			if (num8 == 3)
			{
				m_Computer.AlterBlockInBuild(dn.x + k * num8 * num11 + num17, dn.y + k, dn.z + k * num8 * num12 + num18, blk3);
			}
			m_Computer.AlterBlockInBuild(dn.x + (k + 1) * num8 * num11 + num13, dn.y + k, dn.z + (k + 1) * num8 * num12 + num14, b3);
			m_Computer.AlterBlockInBuild(dn.x + (k + 1) * num8 * num11 + num15, dn.y + k, dn.z + (k + 1) * num8 * num12 + num16, b4);
			if (num8 == 3)
			{
				m_Computer.AlterBlockInBuild(dn.x + (k + 1) * num8 * num11 + num17, dn.y + k, dn.z + (k + 1) * num8 * num12 + num18, blk4);
			}
		}
		if (upVSeal)
		{
			m_Computer.AlterBlockInBuild(dn.x + num9 * num8 * num11 + num13, dn.y + num9, dn.z + num9 * num8 * num12 + num14, b);
			m_Computer.AlterBlockInBuild(dn.x + num9 * num8 * num11 + num15, dn.y + num9, dn.z + num9 * num8 * num12 + num16, b2);
			if (num8 == 3)
			{
				m_Computer.AlterBlockInBuild(dn.x + num9 * num8 * num11 + num17, dn.y + num9, dn.z + num9 * num8 * num12 + num18, blk3);
			}
		}
	}
}
