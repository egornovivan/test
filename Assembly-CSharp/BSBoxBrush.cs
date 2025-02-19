using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BSBoxBrush : BSFreeSizeBrush
{
	public Color addModeGizmoColor = Color.white;

	public Color removeModeGizmoColor = Color.white;

	public bool forceShowRemoveColor;

	private BSPattern m_PrevPattern;

	private GameObject modelGizmo;

	private int m_Rot;

	public bool AllowRemoveVoxel;

	protected override bool ExtraAdjust()
	{
		if (BSInput.s_Alt)
		{
			mode = EBSBrushMode.Subtract;
			gizmoCube.color = removeModeGizmoColor;
		}
		else
		{
			mode = EBSBrushMode.Add;
			gizmoCube.color = addModeGizmoColor;
		}
		if (!AllowRemoveVoxel && pattern.type == EBSVoxelType.Voxel && mode == EBSBrushMode.Subtract)
		{
			if (modelGizmo != null && modelGizmo.gameObject.activeSelf)
			{
				modelGizmo.gameObject.SetActive(value: false);
			}
			if (modelGizmo != null && !modelGizmo.gameObject.activeSelf)
			{
				gizmoCube.gameObject.SetActive(value: false);
			}
			ResetDrawing();
			return false;
		}
		if (gizmoCube != null && !gizmoCube.gameObject.activeSelf)
		{
			gizmoCube.gameObject.SetActive(value: true);
		}
		if (BSInput.s_MouseOnUI)
		{
			if (modelGizmo != null && modelGizmo.gameObject.activeSelf)
			{
				modelGizmo.gameObject.SetActive(value: false);
			}
			return true;
		}
		if (modelGizmo != null && !modelGizmo.gameObject.activeSelf)
		{
			modelGizmo.gameObject.SetActive(value: true);
		}
		if (pattern != m_PrevPattern)
		{
			if (modelGizmo != null)
			{
				Object.Destroy(modelGizmo.gameObject);
				modelGizmo = null;
			}
			if (pattern.MeshPath != null && pattern.MeshPath != string.Empty)
			{
				modelGizmo = Object.Instantiate(Resources.Load(pattern.MeshPath)) as GameObject;
				modelGizmo.transform.parent = base.transform;
				modelGizmo.name = "Model Gizmo";
				Renderer component = modelGizmo.GetComponent<Renderer>();
				component.shadowCastingMode = ShadowCastingMode.Off;
				component.receiveShadows = false;
			}
			m_PrevPattern = pattern;
			m_Rot = 0;
		}
		if (pattern.MeshMat != null && modelGizmo != null)
		{
			Renderer component2 = modelGizmo.GetComponent<Renderer>();
			if (component2 != null)
			{
				component2.material = pattern.MeshMat;
			}
		}
		if (modelGizmo != null)
		{
			modelGizmo.transform.localScale = new Vector3(pattern.size, pattern.size, pattern.size) * dataSource.Scale;
		}
		if (pattern.type == EBSVoxelType.Block && Input.GetKeyDown(KeyCode.T))
		{
			m_Rot = ((++m_Rot <= 3) ? m_Rot : 0);
		}
		if (gizmoTrigger.RayCast || forceShowRemoveColor)
		{
			if (gizmoTrigger.RayCast)
			{
				ExtraTips = PELocalization.GetString(8000686);
			}
			if (forceShowRemoveColor)
			{
				ExtraTips = PELocalization.GetString(821000001);
			}
			gizmoCube.color = removeModeGizmoColor;
		}
		else
		{
			ExtraTips = string.Empty;
		}
		return true;
	}

	protected override void AfterDo()
	{
		if (modelGizmo != null)
		{
			Quaternion quaternion = Quaternion.Euler(0f, 90 * m_Rot, 0f);
			modelGizmo.transform.rotation = Quaternion.Euler(0f, 90 * m_Rot, 0f);
			float num = (float)pattern.size * 0.5f;
			Vector3 vector = (quaternion * new Vector3(0f - num, 0f - num, 0f - num) + new Vector3(num, num, num)) * dataSource.Scale;
			modelGizmo.transform.position = m_GizmoCursor + dataSource.Offset + vector;
		}
	}

	protected override void Do()
	{
		if (gizmoTrigger.RayCast)
		{
			return;
		}
		Vector3 vector = base.Min * dataSource.ScaleInverted;
		Vector3 vector2 = base.Max * dataSource.ScaleInverted;
		int size = pattern.size;
		Vector3 vector3 = new Vector3((float)(size - 1) / 2f, 0f, (float)(size - 1) / 2f);
		if (mode == EBSBrushMode.Add)
		{
			List<BSVoxel> list = new List<BSVoxel>();
			List<IntVector3> list2 = new List<IntVector3>();
			List<BSVoxel> list3 = new List<BSVoxel>();
			Dictionary<IntVector3, int> dictionary = new Dictionary<IntVector3, int>();
			for (int i = (int)vector.x; i < (int)vector2.x; i += size)
			{
				for (int j = (int)vector.y; j < (int)vector2.y; j += size)
				{
					for (int k = (int)vector.z; k < (int)vector2.z; k += size)
					{
						for (int l = 0; l < size; l++)
						{
							for (int m = 0; m < size; m++)
							{
								for (int n = 0; n < size; n++)
								{
									Vector3 vector4 = Quaternion.Euler(0f, 90 * m_Rot, 0f) * new Vector3((float)l - vector3.x, (float)m - vector3.y, (float)n - vector3.z) + vector3;
									IntVector3 intVector = new IntVector3((float)Mathf.FloorToInt(i) + vector4.x, (float)Mathf.FloorToInt(j) + vector4.y, (float)Mathf.FloorToInt(k) + vector4.z);
									BSVoxel item = pattern.voxelList[l, m, n];
									item.materialType = materialType;
									item.blockType = BSVoxel.MakeBlockType(item.blockType >> 2, ((item.blockType & 3) + m_Rot) % 4);
									BSVoxel item2 = dataSource.SafeRead(intVector.x, intVector.y, intVector.z);
									list.Add(item);
									list3.Add(item2);
									list2.Add(intVector);
									dictionary.Add(intVector, 0);
								}
							}
						}
					}
				}
			}
			BSBrush.FindExtraExtendableVoxels(dataSource, list, list3, list2, dictionary);
			if (list2.Count != 0)
			{
				BSAction bSAction = new BSAction();
				BSVoxelModify modify = new BSVoxelModify(list2.ToArray(), list3.ToArray(), list.ToArray(), dataSource, mode);
				bSAction.AddModify(modify);
				if (bSAction.Do())
				{
					BSHistory.AddAction(bSAction);
				}
			}
		}
		else
		{
			if (mode != EBSBrushMode.Subtract)
			{
				return;
			}
			List<BSVoxel> list4 = new List<BSVoxel>();
			List<IntVector3> list5 = new List<IntVector3>();
			List<BSVoxel> list6 = new List<BSVoxel>();
			Dictionary<IntVector3, int> dictionary2 = new Dictionary<IntVector3, int>();
			for (int num = (int)vector.x; num < (int)vector2.x; num += size)
			{
				for (int num2 = (int)vector.y; num2 < (int)vector2.y; num2 += size)
				{
					for (int num3 = (int)vector.z; num3 < (int)vector2.z; num3 += size)
					{
						for (int num4 = 0; num4 < size; num4++)
						{
							for (int num5 = 0; num5 < size; num5++)
							{
								for (int num6 = 0; num6 < size; num6++)
								{
									IntVector3 intVector2 = new IntVector3(Mathf.FloorToInt(num) + num4, Mathf.FloorToInt(num2) + num5, Mathf.FloorToInt(num3) + num6);
									BSVoxel item3 = dataSource.Read(intVector2.x, intVector2.y, intVector2.z);
									list4.Add(default(BSVoxel));
									list5.Add(intVector2);
									list6.Add(item3);
									dictionary2[intVector2] = 0;
									List<IntVector4> posList = null;
									List<BSVoxel> voxels = null;
									if (!dataSource.ReadExtendableBlock(new IntVector4(intVector2, 0), out posList, out voxels))
									{
										continue;
									}
									for (int num7 = 0; num7 < voxels.Count; num7++)
									{
										IntVector3 intVector3 = new IntVector3(posList[num7].x, posList[num7].y, posList[num7].z);
										if (intVector3 != intVector2 && !dictionary2.ContainsKey(intVector3))
										{
											BSVoxel item4 = dataSource.Read(intVector3.x, intVector3.y, intVector3.z);
											list6.Add(item4);
											list5.Add(intVector3);
											list4.Add(default(BSVoxel));
											dictionary2.Add(intVector3, 0);
										}
									}
								}
							}
						}
					}
				}
			}
			BSBrush.FindExtraExtendableVoxels(dataSource, list4, list6, list5, dictionary2);
			if (list5.Count != 0)
			{
				BSAction bSAction2 = new BSAction();
				BSVoxelModify modify2 = new BSVoxelModify(list5.ToArray(), list6.ToArray(), list4.ToArray(), dataSource, mode);
				bSAction2.AddModify(modify2);
				if (bSAction2.Do())
				{
					BSHistory.AddAction(bSAction2);
				}
			}
		}
	}
}
