using UnityEngine;

public class VCEDiagonalBrush : VCEFreeSizeBrush
{
	private const float ISOVAL = 127.5f;

	private const float MAXVAL = 255f;

	private static int m_Direction;

	private static int m_Thickness;

	private static int m_Offset;

	protected override void ExtraAdjust()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			m_Direction++;
			m_Direction %= 12;
		}
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			m_Offset++;
		}
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			m_Offset--;
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			m_Thickness--;
		}
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			m_Thickness++;
		}
		Transform transform = null;
		if (m_GizmoCube.m_ShapeGizmo != null)
		{
			transform = m_GizmoCube.m_ShapeGizmo.GetChild(0);
		}
		if (transform != null)
		{
			switch (m_Direction)
			{
			case 0:
				transform.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
				break;
			case 1:
				transform.transform.localEulerAngles = new Vector3(180f, 0f, 0f);
				break;
			case 2:
				transform.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
				break;
			case 3:
				transform.transform.localEulerAngles = new Vector3(270f, 0f, 0f);
				break;
			case 4:
				transform.transform.localEulerAngles = new Vector3(0f, 90f, 90f);
				break;
			case 5:
				transform.transform.localEulerAngles = new Vector3(0f, 270f, 90f);
				break;
			case 6:
				transform.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
				break;
			case 7:
				transform.transform.localEulerAngles = new Vector3(0f, 180f, 90f);
				break;
			case 8:
				transform.transform.localEulerAngles = new Vector3(0f, 270f, 180f);
				break;
			case 9:
				transform.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
				break;
			case 10:
				transform.transform.localEulerAngles = new Vector3(90f, 90f, 0f);
				break;
			case 11:
				transform.transform.localEulerAngles = new Vector3(270f, 90f, 0f);
				break;
			default:
				transform.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
				break;
			}
		}
	}

	protected override string BrushDesc()
	{
		return "Diagonal Brush - Draw free size voxel diagonal";
	}

	protected override void Do()
	{
		m_Action = new VCEAction();
		ulong num = VCEditor.s_Scene.m_IsoData.MaterialGUID(VCEditor.SelectedVoxelType);
		ulong guid = VCEditor.SelectedMaterial.m_Guid;
		if (num != guid)
		{
			VCEAlterMaterialMap item = new VCEAlterMaterialMap(VCEditor.SelectedVoxelType, num, guid);
			m_Action.Modifies.Add(item);
		}
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		Vector3 zero3 = Vector3.zero;
		switch (m_Direction)
		{
		default:
			zero = new Vector3(base.Min.x, base.Min.y, (float)base.Min.z - 0.5f);
			zero2 = new Vector3(base.Max.x + 1, (float)base.Max.y + 1.5f, base.Max.z + 1);
			zero3 = new Vector3(base.Max.x + 1, base.Min.y, (float)base.Min.z - 0.5f);
			break;
		case 1:
			zero = new Vector3(base.Max.x + 1, base.Max.y + 1, (float)base.Max.z + 1.5f);
			zero2 = new Vector3(base.Min.x, (float)base.Min.y - 0.5f, base.Min.z);
			zero3 = new Vector3(base.Max.x + 1, (float)base.Min.y - 0.5f, base.Min.z);
			break;
		case 2:
			zero = new Vector3(base.Min.x, (float)base.Max.y + 1.5f, base.Min.z);
			zero2 = new Vector3(base.Max.x + 1, base.Min.y, (float)base.Max.z + 1.5f);
			zero3 = new Vector3(base.Max.x + 1, (float)base.Max.y + 1.5f, base.Min.z);
			break;
		case 3:
			zero = new Vector3(base.Max.x + 1, (float)base.Min.y - 0.5f, base.Max.z + 1);
			zero2 = new Vector3(base.Min.x, base.Max.y + 1, (float)base.Min.z - 0.5f);
			zero3 = new Vector3(base.Max.x + 1, base.Max.y + 1, (float)base.Min.z - 0.5f);
			break;
		case 4:
			zero = new Vector3((float)base.Min.x - 0.5f, base.Min.y, base.Min.z);
			zero2 = new Vector3(base.Max.x + 1, base.Max.y + 1, (float)base.Max.z + 1.5f);
			zero3 = new Vector3((float)base.Min.x - 0.5f, base.Max.y + 1, base.Min.z);
			break;
		case 5:
			zero = new Vector3((float)base.Max.x + 1.5f, base.Max.y + 1, base.Max.z + 1);
			zero2 = new Vector3(base.Min.x, base.Min.y, (float)base.Min.z - 0.5f);
			zero3 = new Vector3(base.Min.x, base.Max.y + 1, (float)base.Min.z - 0.5f);
			break;
		case 6:
			zero = new Vector3(base.Max.x + 1, base.Min.y, (float)base.Min.z - 0.5f);
			zero2 = new Vector3((float)base.Min.x - 0.5f, base.Max.y + 1, base.Max.z + 1);
			zero3 = new Vector3(base.Max.x + 1, base.Max.y + 1, (float)base.Min.z - 0.5f);
			break;
		case 7:
			zero = new Vector3(base.Min.x, base.Max.y + 1, (float)base.Max.z + 1.5f);
			zero2 = new Vector3((float)base.Max.x + 1.5f, base.Min.y, base.Min.z);
			zero3 = new Vector3((float)base.Max.x + 1.5f, base.Max.y + 1, base.Min.z);
			break;
		case 8:
			zero = new Vector3(base.Min.x, (float)base.Min.y - 0.5f, base.Min.z);
			zero2 = new Vector3((float)base.Max.x + 1.5f, base.Max.y + 1, base.Max.z + 1);
			zero3 = new Vector3(base.Min.x, (float)base.Min.y - 0.5f, base.Max.z + 1);
			break;
		case 9:
			zero = new Vector3(base.Max.x + 1, (float)base.Max.y + 1.5f, base.Max.z + 1);
			zero2 = new Vector3((float)base.Min.x - 0.5f, base.Min.y, base.Min.z);
			zero3 = new Vector3((float)base.Min.x - 0.5f, base.Min.y, base.Max.z + 1);
			break;
		case 10:
			zero = new Vector3((float)base.Max.x + 1.5f, base.Min.y, base.Min.z);
			zero2 = new Vector3(base.Min.x, (float)base.Max.y + 1.5f, base.Max.z + 1);
			zero3 = new Vector3((float)base.Max.x + 1.5f, base.Min.y, base.Max.z + 1);
			break;
		case 11:
			zero = new Vector3((float)base.Min.x - 0.5f, base.Max.y + 1, base.Max.z + 1);
			zero2 = new Vector3(base.Max.x + 1, (float)base.Min.y - 0.5f, base.Min.z);
			zero3 = new Vector3(base.Max.x + 1, (float)base.Min.y - 0.5f, base.Max.z + 1);
			break;
		}
		Plane p = new Plane(zero, zero2, zero3);
		VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
		IntVector3 min = base.Min;
		IntVector3 max = base.Max;
		for (int i = min.x; i <= max.x; i++)
		{
			for (int j = min.y; j <= max.y; j++)
			{
				for (int k = min.z; k <= max.z; k++)
				{
					float f = VCEMath.DetermineVolume(i, j, k, p);
					int num2 = Mathf.RoundToInt(f);
					if (num2 == 0)
					{
						continue;
					}
					if (VCEditor.s_Mirror.Enabled_Masked)
					{
						VCEditor.s_Mirror.MirrorVoxel(new IntVector3(i, j, k));
						for (int l = 0; l < VCEditor.s_Mirror.OutputCnt; l++)
						{
							if (VCEditor.s_Scene.m_IsoData.IsPointIn(VCEditor.s_Mirror.Output[l]))
							{
								int num3 = VCIsoData.IPosToKey(VCEditor.s_Mirror.Output[l]);
								VCVoxel voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(num3);
								VCVoxel vCVoxel = new VCVoxel((byte)num2, (byte)VCEditor.SelectedVoxelType);
								if ((int)voxel != (int)vCVoxel)
								{
									VCEAlterVoxel item2 = new VCEAlterVoxel(num3, voxel, vCVoxel);
									m_Action.Modifies.Add(item2);
								}
							}
						}
					}
					else
					{
						int num4 = VCIsoData.IPosToKey(new IntVector3(i, j, k));
						VCVoxel voxel2 = VCEditor.s_Scene.m_IsoData.GetVoxel(num4);
						VCVoxel vCVoxel2 = new VCVoxel((byte)num2, (byte)VCEditor.SelectedVoxelType);
						if ((int)voxel2 != (int)vCVoxel2)
						{
							VCEAlterVoxel item3 = new VCEAlterVoxel(num4, voxel2, vCVoxel2);
							m_Action.Modifies.Add(item3);
						}
					}
				}
			}
		}
		if (m_Action.Modifies.Count > 0)
		{
			m_Action.Do();
		}
		ResetDrawing();
	}

	protected override void ExtraGUI()
	{
		if (m_Phase != 0)
		{
			string text = "Use          to change direction";
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			GUI.Label(new Rect(Input.mousePosition.x + 24f, (float)Screen.height - Input.mousePosition.y + 56f, 100f, 100f), text, "CursorText2");
			GUI.color = new Color(1f, 1f, 0f, 0.9f);
			GUI.Label(new Rect(Input.mousePosition.x + 24f, (float)Screen.height - Input.mousePosition.y + 56f, 100f, 100f), "       TAB", "CursorText2");
		}
	}
}
