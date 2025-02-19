using System.Collections.Generic;
using UnityEngine;

public class VCESelectVoxel : VCESelect
{
	public VCEMath.DrawTarget m_Target;

	public VCEVoxelSelection m_SelectionMgr;

	public VCEUIBoxMethodInspector m_BoxMethodInspectorRes;

	private EVCESelectMethod m_Method;

	private VCESelectMethod m_MethodExec;

	private float tips_counter = 8f;

	public EVCESelectMethod SelectMethod
	{
		get
		{
			return m_Method;
		}
		set
		{
			if (value != m_Method)
			{
				if (m_MethodExec != null)
				{
					Object.Destroy(m_MethodExec);
					m_MethodExec = null;
				}
				m_Method = value;
				EVCESelectMethod method = m_Method;
				if (method == EVCESelectMethod.Box)
				{
					m_MethodExec = base.gameObject.AddComponent<VCESelectMethod_Box>();
				}
				else
				{
					m_MethodExec = null;
				}
				if (m_MethodExec != null)
				{
					m_MethodExec.Init(this);
				}
			}
		}
	}

	private void Start()
	{
		if (VCEditor.DocumentOpen())
		{
			m_SelectionMgr = VCEditor.Instance.m_VoxelSelection;
			CreateMainInspector();
			VCEditor.Instance.m_NearVoxelIndicator.enabled = true;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (m_MainInspector != null)
		{
			Object.Destroy(m_MainInspector);
			m_MainInspector = null;
		}
	}

	private void CreateMainInspector()
	{
		m_MainInspector = Object.Instantiate(m_MainInspectorRes);
		m_MainInspector.transform.parent = VCEditor.Instance.m_UI.m_InspectorGroup;
		m_MainInspector.transform.localPosition = Vector3.zero;
		m_MainInspector.transform.localScale = Vector3.one;
		m_MainInspector.SetActive(value: false);
		VCEUISelectVoxelInspector component = m_MainInspector.GetComponent<VCEUISelectVoxelInspector>();
		component.m_SelectBrush = this;
	}

	private void ShowMainInspector()
	{
		m_MainInspector.SetActive(value: true);
	}

	private void HideMainInspector()
	{
		m_MainInspector.SetActive(value: false);
	}

	public override void ClearSelection()
	{
		m_SelectionMgr.ClearSelection();
	}

	public override void Cancel()
	{
	}

	private void OnGUI()
	{
		GUI.skin = VCEditor.Instance.m_GUISkin;
		if (!Input.GetMouseButton(0) && VCEditor.s_Scene.m_IsoData.IsPointIn(m_Target.snapto))
		{
			string text = "Press     you can set\r\nview center here!";
			GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(tips_counter));
			GUI.Label(new Rect(Input.mousePosition.x + 24f, (float)Screen.height - Input.mousePosition.y + 26f, 100f, 100f), text, "CursorText2");
			GUI.color = new Color(1f, 1f, 0f, Mathf.Clamp01(tips_counter));
			GUI.Label(new Rect(Input.mousePosition.x + 24f, (float)Screen.height - Input.mousePosition.y + 26f, 100f, 100f), "          F", "CursorText2");
		}
	}

	private void Update()
	{
		VCEMath.RayCastDrawTarget(VCEInput.s_PickRay, out m_Target, 1, voxelbest: true);
		if (VCEditor.s_Scene.m_IsoData.m_Voxels.Count > 0)
		{
			ShowMainInspector();
		}
		else
		{
			HideMainInspector();
		}
		if (VCEInput.s_Cancel)
		{
			m_SelectionMgr.ClearSelection();
		}
		if (m_MethodExec != null)
		{
			m_MethodExec.MainMethod();
			if (m_MethodExec.m_NeedUpdate)
			{
				m_SelectionMgr.RebuildSelectionBoxes();
				m_MethodExec.m_NeedUpdate = false;
			}
		}
		else
		{
			VCEditor.Instance.m_NearVoxelIndicator.enabled = false;
		}
		if (!m_MainInspector.activeInHierarchy)
		{
			m_MainInspector.GetComponent<VCEUISelectVoxelInspector>().Update();
		}
		if (m_SelectionMgr.m_Selection.Count > 0)
		{
			if (VCEInput.s_Shift && VCEInput.s_Left)
			{
				ExtrudeSelection(-1, 0, 0);
				VCEStatusBar.ShowText("Extrude left".ToLocalizationString(), 2f);
			}
			if (VCEInput.s_Shift && VCEInput.s_Right)
			{
				ExtrudeSelection(1, 0, 0);
				VCEStatusBar.ShowText("Extrude right".ToLocalizationString(), 2f);
			}
			if (VCEInput.s_Shift && VCEInput.s_Up)
			{
				ExtrudeSelection(0, 1, 0);
				VCEStatusBar.ShowText("Extrude up".ToLocalizationString(), 2f);
			}
			if (VCEInput.s_Shift && VCEInput.s_Down)
			{
				ExtrudeSelection(0, -1, 0);
				VCEStatusBar.ShowText("Extrude down".ToLocalizationString(), 2f);
			}
			if (VCEInput.s_Shift && VCEInput.s_Forward)
			{
				ExtrudeSelection(0, 0, 1);
				VCEStatusBar.ShowText("Extrude forward".ToLocalizationString(), 2f);
			}
			if (VCEInput.s_Shift && VCEInput.s_Back)
			{
				ExtrudeSelection(0, 0, -1);
				VCEStatusBar.ShowText("Extrude back".ToLocalizationString(), 2f);
			}
		}
		if (!Input.GetMouseButton(0) && VCEditor.s_Scene.m_IsoData.IsPointIn(m_Target.snapto))
		{
			tips_counter -= Time.deltaTime;
		}
	}

	public void ExtrudeSelection(int x, int y, int z)
	{
		if (x == 0 && y == 0 && z == 0)
		{
			return;
		}
		m_Action = new VCEAction();
		VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		foreach (KeyValuePair<int, byte> item3 in m_SelectionMgr.m_Selection)
		{
			float num = (float)(int)item3.Value / 255f;
			IntVector3 intVector = VCIsoData.KeyToIPos(item3.Key);
			intVector.x += x;
			intVector.y += y;
			intVector.z += z;
			if (!VCEditor.s_Scene.m_IsoData.IsPointIn(intVector))
			{
				continue;
			}
			int num2 = VCIsoData.IPosToKey(intVector);
			list.Add(num2);
			list2.Add(item3.Value);
			if (VCEditor.s_Mirror.Enabled_Masked)
			{
				IntVector3 intVector2 = intVector;
				VCEditor.s_Mirror.MirrorVoxel(intVector);
				for (int i = 0; i < VCEditor.s_Mirror.OutputCnt; i++)
				{
					IntVector3 pos = VCEditor.s_Mirror.Output[i];
					if (VCEditor.s_Scene.m_IsoData.IsPointIn(pos))
					{
						num2 = VCIsoData.IPosToKey(pos);
						VCVoxel voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(item3.Key);
						VCVoxel voxel2 = VCEditor.s_Scene.m_IsoData.GetVoxel(num2);
						VCVoxel vCVoxel = voxel;
						if (num == 1f)
						{
							vCVoxel.VolumeF = voxel2.VolumeF + voxel.VolumeF;
						}
						else
						{
							vCVoxel.VolumeF = voxel2.VolumeF + voxel.VolumeF * Mathf.Lerp(Mathf.Pow(num, 0.3f), 1f, 0.3f);
						}
						if ((int)voxel2 != (int)vCVoxel)
						{
							VCEAlterVoxel item = new VCEAlterVoxel(num2, voxel2, vCVoxel);
							m_Action.Modifies.Add(item);
						}
					}
				}
			}
			else
			{
				VCVoxel voxel3 = VCEditor.s_Scene.m_IsoData.GetVoxel(item3.Key);
				VCVoxel voxel4 = VCEditor.s_Scene.m_IsoData.GetVoxel(num2);
				VCVoxel vCVoxel2 = voxel3;
				if (num == 1f)
				{
					vCVoxel2.VolumeF = voxel4.VolumeF + voxel3.VolumeF;
				}
				else
				{
					vCVoxel2.VolumeF = voxel4.VolumeF + voxel3.VolumeF * Mathf.Lerp(Mathf.Pow(num, 0.3f), 1f, 0.3f);
				}
				if ((int)voxel4 != (int)vCVoxel2)
				{
					VCEAlterVoxel item2 = new VCEAlterVoxel(num2, voxel4, vCVoxel2);
					m_Action.Modifies.Add(item2);
				}
			}
		}
		ColorSelection(VCIsoData.BLANK_COLOR, consider_strength: false, action_segment: true);
		if (m_Action.Modifies.Count > 0)
		{
			m_Action.Do();
		}
		m_SelectionMgr.m_Selection.Clear();
		for (int j = 0; j < list.Count; j++)
		{
			if (!VCEditor.s_Scene.m_IsoData.IsGarbageVoxel(list[j]))
			{
				m_SelectionMgr.m_Selection.Add(list[j], (byte)list2[j]);
			}
		}
		m_SelectionMgr.RebuildSelectionBoxes();
	}

	public void DeleteSelection()
	{
		m_Action = new VCEAction();
		VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
		foreach (KeyValuePair<int, byte> item2 in m_SelectionMgr.m_Selection)
		{
			if (VCEditor.s_Mirror.Enabled_Masked)
			{
				IntVector3 ipos = VCIsoData.KeyToIPos(item2.Key);
				VCEditor.s_Mirror.MirrorVoxel(ipos);
				float num = (float)(int)item2.Value / 255f;
				for (int i = 0; i < VCEditor.s_Mirror.OutputCnt; i++)
				{
					if (VCEditor.s_Scene.m_IsoData.IsPointIn(VCEditor.s_Mirror.Output[i]))
					{
						int num2 = VCIsoData.IPosToKey(VCEditor.s_Mirror.Output[i]);
						VCVoxel voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(num2);
						VCVoxel vCVoxel = voxel;
						vCVoxel.VolumeF *= 1f - num;
						if ((int)voxel != (int)vCVoxel)
						{
							VCEAlterVoxel vCEAlterVoxel = new VCEAlterVoxel(num2, voxel, vCVoxel);
							vCEAlterVoxel.Redo();
							m_Action.Modifies.Add(vCEAlterVoxel);
						}
					}
				}
			}
			else
			{
				float num3 = (float)(int)item2.Value / 255f;
				int key = item2.Key;
				VCVoxel voxel2 = VCEditor.s_Scene.m_IsoData.GetVoxel(key);
				VCVoxel vCVoxel2 = voxel2;
				vCVoxel2.VolumeF *= 1f - num3;
				if ((int)voxel2 != (int)vCVoxel2)
				{
					VCEAlterVoxel item = new VCEAlterVoxel(key, voxel2, vCVoxel2);
					m_Action.Modifies.Add(item);
				}
			}
		}
		ColorSelection(VCIsoData.BLANK_COLOR, consider_strength: false, action_segment: true);
		if (m_Action.Modifies.Count > 0)
		{
			m_Action.DoButNotRegister();
			VCUtils.ISOCut(VCEditor.s_Scene.m_IsoData, m_Action);
			m_Action.Register();
			VCEStatusBar.ShowText("Selected voxels have been removed".ToLocalizationString(), 2f);
		}
		ClearSelection();
	}

	public void TextureSelection()
	{
		if (!VCEditor.Instance.m_UI.m_MaterialTab.isChecked || VCEditor.SelectedVoxelType < 0)
		{
			return;
		}
		m_Action = new VCEAction();
		ulong num = VCEditor.s_Scene.m_IsoData.MaterialGUID(VCEditor.SelectedVoxelType);
		ulong guid = VCEditor.SelectedMaterial.m_Guid;
		if (num != guid)
		{
			VCEAlterMaterialMap item = new VCEAlterMaterialMap(VCEditor.SelectedVoxelType, num, guid);
			m_Action.Modifies.Add(item);
		}
		VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
		foreach (KeyValuePair<int, byte> item3 in m_SelectionMgr.m_Selection)
		{
			if (VCEditor.s_Mirror.Enabled_Masked)
			{
				IntVector3 ipos = VCIsoData.KeyToIPos(item3.Key);
				VCEditor.s_Mirror.MirrorVoxel(ipos);
				float num2 = (float)(int)item3.Value / 255f;
				if (num2 < 0.5f)
				{
					continue;
				}
				for (int i = 0; i < VCEditor.s_Mirror.OutputCnt; i++)
				{
					if (VCEditor.s_Scene.m_IsoData.IsPointIn(VCEditor.s_Mirror.Output[i]))
					{
						int num3 = VCIsoData.IPosToKey(VCEditor.s_Mirror.Output[i]);
						VCVoxel voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(num3);
						VCVoxel vCVoxel = new VCVoxel(voxel.Volume, (byte)VCEditor.SelectedVoxelType);
						if ((int)voxel != (int)vCVoxel)
						{
							VCEAlterVoxel vCEAlterVoxel = new VCEAlterVoxel(num3, voxel, vCVoxel);
							vCEAlterVoxel.Redo();
							m_Action.Modifies.Add(vCEAlterVoxel);
						}
					}
				}
				continue;
			}
			float num4 = (float)(int)item3.Value / 255f;
			if (!(num4 < 0.5f))
			{
				int key = item3.Key;
				VCVoxel voxel2 = VCEditor.s_Scene.m_IsoData.GetVoxel(key);
				VCVoxel vCVoxel2 = new VCVoxel(voxel2.Volume, (byte)VCEditor.SelectedVoxelType);
				if ((int)voxel2 != (int)vCVoxel2)
				{
					VCEAlterVoxel item2 = new VCEAlterVoxel(key, voxel2, vCVoxel2);
					m_Action.Modifies.Add(item2);
				}
			}
		}
		if (m_Action.Modifies.Count > 0)
		{
			m_Action.Do();
			VCEStatusBar.ShowText("Selected voxels have been textured".ToLocalizationString(), 2f);
		}
	}

	public void ColorSelection(Color32 color, bool consider_strength = true, bool action_segment = false)
	{
		if (!VCEditor.Instance.m_UI.m_PaintTab.isChecked)
		{
			return;
		}
		if (!action_segment)
		{
			m_Action = new VCEAction();
		}
		bool flag = false;
		VCEUpdateColorSign item = new VCEUpdateColorSign(forward: false, back: true);
		m_Action.Modifies.Add(item);
		VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
		foreach (SelBox box in m_SelectionMgr.m_GL.m_Boxes)
		{
			float t = (float)(int)box.m_Val / 255f;
			float num = box.m_Box.xMin;
			byte b = 0;
			while (num <= (float)box.m_Box.xMax + 1.01f)
			{
				float num2 = box.m_Box.yMin;
				b &= 1;
				while (num2 <= (float)box.m_Box.yMax + 1.01f)
				{
					if (b != 0 && b != 4)
					{
						float num3 = box.m_Box.zMin;
						b &= 3;
						while (num3 <= (float)box.m_Box.zMax + 1.01f)
						{
							if (b != 1 && b != 2)
							{
								if (VCEditor.s_Mirror.Enabled_Masked)
								{
									IntVector3 ipos = VCIsoData.IPosToColorPos(new Vector3(num, num2, num3));
									VCEditor.s_Mirror.MirrorColor(ipos);
									for (int i = 0; i < VCEditor.s_Mirror.OutputCnt; i++)
									{
										if (VCEditor.s_Scene.m_IsoData.IsColorPosIn(VCEditor.s_Mirror.Output[i]))
										{
											int num4 = VCIsoData.ColorPosToColorKey(VCEditor.s_Mirror.Output[i]);
											Color32 color2 = VCEditor.s_Scene.m_IsoData.GetColor(num4);
											Color32 new_color = ((!consider_strength) ? color : Color32.Lerp(color2, color, t));
											if (color2.r != new_color.r || color2.g != new_color.g || color2.b != new_color.b || color2.a != new_color.a)
											{
												VCEAlterColor vCEAlterColor = new VCEAlterColor(num4, color2, new_color);
												vCEAlterColor.Redo();
												m_Action.Modifies.Add(vCEAlterColor);
												flag = true;
											}
										}
									}
								}
								else
								{
									int num5 = VCIsoData.IPosToColorKey(new Vector3(num, num2, num3));
									Color32 color3 = VCEditor.s_Scene.m_IsoData.GetColor(num5);
									Color32 new_color2 = ((!consider_strength) ? color : Color32.Lerp(color3, color, t));
									if (color3.r != new_color2.r || color3.g != new_color2.g || color3.b != new_color2.b || color3.a != new_color2.a)
									{
										VCEAlterColor item2 = new VCEAlterColor(num5, color3, new_color2);
										m_Action.Modifies.Add(item2);
										flag = true;
									}
								}
							}
							num3 += 0.5f;
							b ^= 4;
						}
					}
					num2 += 0.5f;
					b ^= 2;
				}
				num += 0.5f;
				b ^= 1;
			}
		}
		VCEUpdateColorSign item3 = new VCEUpdateColorSign(forward: true, back: false);
		m_Action.Modifies.Add(item3);
		if (action_segment && !flag)
		{
			m_Action.Modifies.RemoveRange(m_Action.Modifies.Count - 2, 2);
		}
		if (!action_segment && m_Action.Modifies.Count > 2)
		{
			m_Action.Do();
			if (color.r == VCIsoData.BLANK_COLOR.r && color.g == VCIsoData.BLANK_COLOR.g && color.b == VCIsoData.BLANK_COLOR.b && color.a == VCIsoData.BLANK_COLOR.a)
			{
				VCEStatusBar.ShowText("Selection color have been erased".ToLocalizationString(), 2f);
			}
			else
			{
				VCEStatusBar.ShowText("Selected voxels have been painted".ToLocalizationString(), 2f);
			}
		}
	}
}
