using System;
using System.Collections.Generic;
using UnityEngine;
using WhiteCat;

public class VCESelectComponent : VCESelect
{
	[Serializable]
	public class SelectInfo
	{
		public VCEComponentTool m_Component;

		public Vector3 m_OldPosition = Vector3.zero;

		public Vector3 m_OldRotation = Vector3.zero;

		public Vector3 m_OldScale = Vector3.one;

		public Vector3 m_DragPosition = Vector3.zero;

		public Vector3 m_DragRotation = Vector3.zero;

		public Vector3 m_DragScale = Vector3.one;

		public Vector3 m_NewPosition = Vector3.zero;

		public Vector3 m_NewRotation = Vector3.zero;

		public Vector3 m_NewScale = Vector3.one;
	}

	public GameObject m_DataInspector;

	public List<SelectInfo> m_Selection;

	public static VCEComponentTool s_LastCreate;

	public VCEHoloBoard m_DescBoard;

	public VCEMovingGizmo m_MovingGizmo;

	public VCERotatingGizmo m_RotatingGizmo;

	public bool m_MouseOnGizmo;

	public VCEAction m_GizmoAction;

	private int pick_order;

	private Vector3 lastMousePos = Vector3.zero;

	public bool UsingGizmo => m_MovingGizmo.Dragging || m_RotatingGizmo.Dragging;

	public SelectInfo FindSelected(VCEComponentTool ct)
	{
		if (ct == null)
		{
			return null;
		}
		foreach (SelectInfo item in m_Selection)
		{
			if (item.m_Component == ct)
			{
				return item;
			}
		}
		return null;
	}

	private void Start()
	{
		if (VCEditor.DocumentOpen())
		{
			m_Selection = new List<SelectInfo>();
			CreateMainInspector();
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		m_MovingGizmo.OnDragBegin = OnGizmoBegin;
		m_MovingGizmo.OnDrop = OnGizmoEnd;
		m_MovingGizmo.OnMoving = OnMoving;
		m_RotatingGizmo.OnDragBegin = OnGizmoBegin;
		m_RotatingGizmo.OnDrop = OnGizmoEnd;
		m_RotatingGizmo.OnRotating = OnRotating;
	}

	private void CreateMainInspector()
	{
		m_MainInspector = UnityEngine.Object.Instantiate(m_MainInspectorRes);
		m_MainInspector.transform.parent = VCEditor.Instance.m_UI.m_InspectorGroup;
		m_MainInspector.transform.localPosition = new Vector3(0f, -6f, 0f);
		m_MainInspector.transform.localScale = Vector3.one;
		m_MainInspector.SetActive(value: false);
		VCEUISelectComponentInspector component = m_MainInspector.GetComponent<VCEUISelectComponentInspector>();
		component.m_SelectBrush = this;
	}

	private void ShowMainInspector()
	{
		m_MainInspector.SetActive(value: true);
	}

	private void ShowDataInspector(string respath, VCComponentData setdata)
	{
		HideDataInspector();
		m_DataInspector = UnityEngine.Object.Instantiate(Resources.Load(respath) as GameObject);
		m_DataInspector.transform.parent = VCEditor.Instance.m_UI.m_InspectorGroup;
		m_DataInspector.transform.localPosition = new Vector3(0f, -30f, -1f);
		m_DataInspector.transform.localScale = Vector3.one;
		m_DataInspector.SetActive(value: true);
		VCEUIComponentInspector component = m_DataInspector.GetComponent<VCEUIComponentInspector>();
		component.m_SelectBrush = this;
		component.Set(setdata);
		ShowBonePanel();
	}

	private void ShowBonePanel()
	{
		if (m_Selection != null && m_Selection.Count == 1)
		{
			VCPArmorPivot component = m_Selection[0].m_Component.GetComponent<VCPArmorPivot>();
			if (component != null)
			{
				VCEditor.Instance.m_UI.bonePanel.Show(component);
			}
		}
	}

	private void HideBonePanel()
	{
		if ((bool)VCEditor.Instance && (bool)VCEditor.Instance.m_UI && (bool)VCEditor.Instance.m_UI.bonePanel)
		{
			VCEditor.Instance.m_UI.bonePanel.Hide();
		}
	}

	private void HideDataInspector()
	{
		if (m_DataInspector != null)
		{
			HideBonePanel();
			UnityEngine.Object.Destroy(m_DataInspector);
			m_DataInspector = null;
		}
	}

	private void HideInspectors()
	{
		m_MainInspector.SetActive(value: false);
		HideDataInspector();
	}

	private void OnDisable()
	{
		ClearSelection();
		ToggleComponentColliders(_enabled: false);
		VCEditor.s_ProtectLock0 = false;
	}

	private void OnDestroy()
	{
		ClearSelection();
		ToggleComponentColliders(_enabled: false);
		if (m_MainInspector != null)
		{
			UnityEngine.Object.Destroy(m_MainInspector);
			m_MainInspector = null;
		}
		HideDataInspector();
		s_LastCreate = null;
		VCEditor.s_ProtectLock0 = false;
	}

	private void ToggleComponentColliders(bool _enabled)
	{
		if (!VCEditor.DocumentOpen())
		{
			return;
		}
		foreach (VCComponentData component2 in VCEditor.s_Scene.m_IsoData.m_Components)
		{
			VCEComponentTool component = component2.m_Entity.GetComponent<VCEComponentTool>();
			component.m_SelBound.enabled = _enabled;
			component.m_SelBound.GetComponent<Collider>().enabled = _enabled;
		}
	}

	private void OnSelectionChange()
	{
		s_LastCreate = null;
		if (m_Selection.Count == 1)
		{
			VCComponentData data = m_Selection[0].m_Component.m_Data;
			VCPartData vCPartData = data as VCPartData;
			VCDecalData vCDecalData = data as VCDecalData;
			if (vCPartData != null)
			{
				ShowDataInspector(VCConfig.s_PartTypes[vCPartData.m_Type].m_InspectorRes, data);
			}
			else if (vCDecalData != null)
			{
				ShowDataInspector("GUI/Prefabs/Inspectors/Components Inspectors/inspector decal image", data);
			}
		}
		if (m_Selection.Count > 0)
		{
			VCEStatusBar.ShowText(m_Selection.Count + " " + "component(s)".ToLocalizationString() + " " + "selected".ToLocalizationString(), 4f);
		}
	}

	public List<VCEComponentTool> MirrorImage(VCEComponentTool c)
	{
		VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
		if (VCEditor.s_Mirror.Enabled_Masked)
		{
			List<VCEComponentTool> list = new List<VCEComponentTool>();
			VCEditor.s_Mirror.MirrorComponent(c.m_Data);
			for (int i = 1; i < VCEditor.s_Mirror.OutputCnt; i++)
			{
				VCComponentData vCComponentData = VCEditor.s_Mirror.ComponentOutput[i];
				List<VCComponentData> list2 = VCEditor.s_Scene.m_IsoData.FindComponentsAtPos(vCComponentData.m_Position, vCComponentData.m_ComponentId);
				foreach (VCComponentData item in list2)
				{
					if (VCEMath.IsEqualRotation(item.m_Rotation, vCComponentData.m_Rotation) && !(item.m_Scale != vCComponentData.m_Scale) && item.m_Visible == vCComponentData.m_Visible && (!(vCComponentData is IVCMultiphaseComponentData) || !(item is IVCMultiphaseComponentData) || (vCComponentData as IVCMultiphaseComponentData).Phase == (item as IVCMultiphaseComponentData).Phase))
					{
						VCEComponentTool component = item.m_Entity.GetComponent<VCEComponentTool>();
						if (!(component == null) && !(component == c))
						{
							list.Add(component);
						}
					}
				}
			}
			return list;
		}
		return new List<VCEComponentTool>();
	}

	private void NormalizeSelection()
	{
		SelectInfo selectInfo = null;
		bool flag = false;
		do
		{
			foreach (SelectInfo item in m_Selection)
			{
				VCEComponentTool component = item.m_Component;
				List<VCEComponentTool> list = MirrorImage(component);
				VCEComponentTool image;
				foreach (VCEComponentTool item2 in list)
				{
					image = item2;
					selectInfo = m_Selection.Find((SelectInfo iter) => iter.m_Component == image);
					if (selectInfo != null)
					{
						flag = true;
						goto end_IL_0096;
					}
				}
				continue;
				end_IL_0096:
				break;
			}
			m_Selection.Remove(selectInfo);
		}
		while (selectInfo != null);
		List<SelectInfo> list2 = new List<SelectInfo>();
		foreach (SelectInfo item3 in m_Selection)
		{
			if (item3.m_Component == null)
			{
				list2.Add(item3);
				flag = true;
			}
		}
		foreach (SelectInfo item4 in list2)
		{
			m_Selection.Remove(item4);
		}
		if (flag)
		{
			OnSelectionChange();
		}
	}

	private void Update()
	{
		if (!VCEditor.DocumentOpen())
		{
			return;
		}
		if (s_LastCreate != null)
		{
			ClearSelection();
			SelectInfo selectInfo = new SelectInfo();
			selectInfo.m_Component = s_LastCreate;
			m_Selection.Add(selectInfo);
			OnSelectionChange();
		}
		VCEditor.s_ProtectLock0 = UsingGizmo;
		NormalizeSelection();
		Vector3 mousePosition = Input.mousePosition;
		if ((mousePosition - lastMousePos).magnitude > 3.1f)
		{
			pick_order = 0;
			lastMousePos = mousePosition;
		}
		if (m_DescBoard != null)
		{
			m_DescBoard.m_Position = Vector3.Lerp(m_DescBoard.m_Position, VCEInput.s_PickRay.GetPoint(1.5f), 0.5f);
			List<VCEComponentTool> list = new List<VCEComponentTool>();
			if (!UsingGizmo && !VCEInput.s_MouseOnUI)
			{
				list = VCEMath.RayPickComponents(VCEInput.s_PickRay);
			}
			List<VCPart> list2 = new List<VCPart>();
			Vector3 b = Vector3.zero;
			if (list.Count > 0)
			{
				b = list[0].transform.localPosition;
			}
			foreach (VCEComponentTool item in list)
			{
				if (!(Vector3.Distance(item.transform.localPosition, b) > VCEditor.s_Scene.m_Setting.m_VoxelSize * 2f))
				{
					VCPart component = item.GetComponent<VCPart>();
					if (component != null)
					{
						list2.Add(component);
					}
				}
			}
			if (list.Count > 0)
			{
				m_DescBoard.FadeIn();
				VCEditor.Instance.m_UI.m_HoloBoardCamera.enabled = true;
			}
			else
			{
				VCEditor.Instance.m_UI.m_HoloBoardCamera.enabled = false;
				m_DescBoard.FadeOut();
			}
			list.Clear();
			VCEditor.Instance.m_UI.m_PartDescList.SyncList(list2);
			list2.Clear();
			list = null;
			list2 = null;
		}
		VCEComponentTool vCEComponentTool = null;
		if (!VCEInput.s_MouseOnUI && !m_MouseOnGizmo)
		{
			vCEComponentTool = VCEMath.RayPickComponent(VCEInput.s_PickRay, pick_order);
		}
		if (Input.GetMouseButtonDown(0) && !VCEInput.s_MouseOnUI && !m_MouseOnGizmo)
		{
			if (!VCEInput.s_Shift && !VCEInput.s_Control && !VCEInput.s_Alt)
			{
				m_Selection.Clear();
			}
			if (vCEComponentTool != null)
			{
				SelectInfo selectInfo2 = FindSelected(vCEComponentTool);
				if (selectInfo2 != null)
				{
					m_Selection.Remove(selectInfo2);
				}
				else
				{
					selectInfo2 = new SelectInfo();
					selectInfo2.m_Component = vCEComponentTool;
					List<SelectInfo> list3 = new List<SelectInfo>();
					foreach (SelectInfo item2 in m_Selection)
					{
						VCEComponentTool component2 = item2.m_Component;
						List<VCEComponentTool> list4 = MirrorImage(component2);
						if (list4.Contains(selectInfo2.m_Component))
						{
							list3.Add(item2);
						}
					}
					m_Selection.Add(selectInfo2);
					foreach (SelectInfo item3 in list3)
					{
						m_Selection.Remove(item3);
					}
				}
				pick_order++;
			}
			OnSelectionChange();
		}
		foreach (VCComponentData component8 in VCEditor.s_Scene.m_IsoData.m_Components)
		{
			VCEComponentTool component3 = component8.m_Entity.GetComponent<VCEComponentTool>();
			component3.m_SelBound.enabled = true;
			component3.m_SelBound.GetComponent<Collider>().enabled = true;
			component3.m_SelBound.m_Highlight = component3 == vCEComponentTool;
			component3.m_SelBound.m_BoundColor = GLComponentBound.s_Blue;
		}
		if (m_Selection.Count > 0)
		{
			VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
			foreach (SelectInfo item4 in m_Selection)
			{
				VCEComponentTool component4 = item4.m_Component;
				List<VCEComponentTool> list5 = MirrorImage(component4);
				foreach (VCEComponentTool item5 in list5)
				{
					item5.m_SelBound.m_BoundColor = GLComponentBound.s_Orange;
				}
			}
			foreach (SelectInfo item6 in m_Selection)
			{
				VCEComponentTool component5 = item6.m_Component;
				component5.m_SelBound.m_BoundColor = GLComponentBound.s_Yellow;
			}
		}
		if (m_Selection.Count < 1)
		{
			HideInspectors();
		}
		else if (m_Selection.Count == 1)
		{
			ShowMainInspector();
			VCEUISelectComponentInspector component6 = m_MainInspector.GetComponent<VCEUISelectComponentInspector>();
			if (m_Selection[0].m_Component != null)
			{
				component6.m_SelectInfo.text = m_Selection[0].m_Component.gameObject.name;
			}
			else
			{
				component6.m_SelectInfo.text = "< Deleted >";
			}
		}
		else
		{
			ShowMainInspector();
			HideDataInspector();
			VCEUISelectComponentInspector component7 = m_MainInspector.GetComponent<VCEUISelectComponentInspector>();
			component7.m_SelectInfo.text = m_Selection.Count + " " + "objects".ToLocalizationString() + " " + "selected".ToLocalizationString();
		}
		if (m_Selection.Count > 0)
		{
			Vector3 zero = Vector3.zero;
			int num = 7;
			foreach (SelectInfo item7 in m_Selection)
			{
				zero += item7.m_Component.transform.position;
				if (item7.m_Component.m_Data is VCPartData)
				{
					num &= VCConfig.s_PartTypes[item7.m_Component.m_Data.m_Type].m_RotateMask;
				}
			}
			zero /= (float)m_Selection.Count;
			m_MovingGizmo.transform.position = zero;
			m_RotatingGizmo.transform.position = zero;
			m_MovingGizmo.gameObject.SetActive(VCEditor.TransformType == EVCETransformType.Move);
			m_RotatingGizmo.gameObject.SetActive(VCEditor.TransformType == EVCETransformType.Rotate);
			m_RotatingGizmo.m_AxisMask = num;
		}
		else
		{
			m_MovingGizmo.gameObject.SetActive(value: false);
			m_RotatingGizmo.gameObject.SetActive(value: false);
		}
	}

	private void OnGizmoBegin()
	{
		m_GizmoAction = new VCEAction();
		foreach (SelectInfo item in m_Selection)
		{
			item.m_OldPosition = (item.m_DragPosition = (item.m_NewPosition = item.m_Component.m_Data.m_Position));
			item.m_OldRotation = (item.m_DragRotation = (item.m_NewRotation = item.m_Component.m_Data.m_Rotation));
			item.m_OldScale = (item.m_DragScale = (item.m_NewScale = item.m_Component.m_Data.m_Scale));
		}
	}

	private void OnGizmoEnd()
	{
		if (m_GizmoAction != null)
		{
			VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
			foreach (SelectInfo item3 in m_Selection)
			{
				bool flag = false;
				if (!VCEMath.IsEqualVector(item3.m_NewPosition, item3.m_OldPosition))
				{
					flag = true;
				}
				if (!VCEMath.IsEqualRotation(item3.m_NewRotation, item3.m_OldRotation))
				{
					flag = true;
				}
				if (!VCEMath.IsEqualVector(item3.m_NewScale, item3.m_OldScale))
				{
					flag = true;
				}
				if (!flag)
				{
					continue;
				}
				VCEAlterComponentTransform item = new VCEAlterComponentTransform(VCEditor.s_Scene.m_IsoData.GetComponentIndex(item3.m_Component.m_Data), item3.m_OldPosition, item3.m_OldRotation, item3.m_OldScale, item3.m_NewPosition, item3.m_NewRotation, item3.m_NewScale);
				m_GizmoAction.Modifies.Add(item);
				if (!VCEditor.s_Mirror.Enabled_Masked)
				{
					continue;
				}
				Vector3[] array = new Vector3[8];
				Vector3[] array2 = new Vector3[8];
				Vector3[] array3 = new Vector3[8];
				VCEditor.s_Mirror.MirrorComponent(item3.m_Component.m_Data);
				for (int i = 0; i < VCEditor.s_Mirror.OutputCnt; i++)
				{
					ref Vector3 reference = ref array[i];
					reference = VCEditor.s_Mirror.ComponentOutput[i].m_Position;
					ref Vector3 reference2 = ref array2[i];
					reference2 = VCEditor.s_Mirror.ComponentOutput[i].m_Rotation;
					ref Vector3 reference3 = ref array3[i];
					reference3 = VCEditor.s_Mirror.ComponentOutput[i].m_Scale;
				}
				VCComponentData vCComponentData = item3.m_Component.m_Data.Copy();
				vCComponentData.m_Position = item3.m_NewPosition;
				vCComponentData.m_Rotation = item3.m_NewRotation;
				vCComponentData.m_Scale = item3.m_NewScale;
				VCEditor.s_Mirror.MirrorComponent(vCComponentData);
				for (int j = 1; j < VCEditor.s_Mirror.OutputCnt; j++)
				{
					VCComponentData vCComponentData2 = VCEditor.s_Mirror.ComponentOutput[j];
					vCComponentData2.Validate();
					List<VCComponentData> list = VCEditor.s_Scene.m_IsoData.FindComponentsAtPos(array[j], vCComponentData2.m_ComponentId);
					VCComponentData iter;
					foreach (VCComponentData item4 in list)
					{
						iter = item4;
						if (VCEMath.IsEqualRotation(iter.m_Rotation, array2[j]) && !(iter.m_Scale != array3[j]) && iter.m_Visible == vCComponentData2.m_Visible && (!(vCComponentData2 is IVCMultiphaseComponentData) || !(iter is IVCMultiphaseComponentData) || (vCComponentData2 as IVCMultiphaseComponentData).Phase == (iter as IVCMultiphaseComponentData).Phase) && m_Selection.Find((SelectInfo it) => it.m_Component.m_Data == iter) == null)
						{
							VCEAlterComponentTransform item2 = new VCEAlterComponentTransform(VCEditor.s_Scene.m_IsoData.GetComponentIndex(iter), iter.m_Position, iter.m_Rotation, iter.m_Scale, vCComponentData2.m_Position, vCComponentData2.m_Rotation, vCComponentData2.m_Scale);
							m_GizmoAction.Modifies.Add(item2);
						}
					}
				}
			}
			if (m_GizmoAction.Modifies.Count > 0)
			{
				m_GizmoAction.Do();
				m_GizmoAction = null;
			}
		}
		else
		{
			Debug.LogWarning("Must be some problem here!");
		}
	}

	private void OnMoving(Vector3 offset)
	{
		float num = VCEditor.s_Scene.m_Setting.m_VoxelSize * 0.5f;
		VCEditor.s_Mirror.CalcPrepare(num * 2f);
		foreach (SelectInfo item in m_Selection)
		{
			item.m_DragPosition += offset;
			item.m_NewPosition.x = Mathf.Round(item.m_DragPosition.x / num) * num;
			item.m_NewPosition.y = Mathf.Round(item.m_DragPosition.y / num) * num;
			item.m_NewPosition.z = Mathf.Round(item.m_DragPosition.z / num) * num;
			Vector3 position = item.m_Component.m_Data.m_Position;
			item.m_Component.m_Data.m_Position = item.m_NewPosition;
			item.m_Component.m_Data.Validate();
			Vector3 position2 = item.m_Component.m_Data.m_Position;
			item.m_Component.transform.position = position2;
			item.m_NewPosition = position2;
			if (m_DataInspector != null)
			{
				m_DataInspector.GetComponent<VCEUIComponentInspector>().Set(item.m_Component.m_Data);
			}
			item.m_Component.m_Data.m_Position = position;
			if (!VCEditor.s_Mirror.Enabled_Masked)
			{
				continue;
			}
			Vector3[] array = new Vector3[8];
			VCEditor.s_Mirror.MirrorComponent(item.m_Component.m_Data);
			for (int i = 0; i < VCEditor.s_Mirror.OutputCnt; i++)
			{
				ref Vector3 reference = ref array[i];
				reference = VCEditor.s_Mirror.ComponentOutput[i].m_Position;
			}
			VCComponentData vCComponentData = item.m_Component.m_Data.Copy();
			vCComponentData.m_Position = item.m_NewPosition;
			VCEditor.s_Mirror.MirrorComponent(vCComponentData);
			for (int j = 1; j < VCEditor.s_Mirror.OutputCnt; j++)
			{
				VCComponentData vCComponentData2 = VCEditor.s_Mirror.ComponentOutput[j];
				List<VCComponentData> list = VCEditor.s_Scene.m_IsoData.FindComponentsAtPos(array[j], vCComponentData2.m_ComponentId);
				VCComponentData iter;
				foreach (VCComponentData item2 in list)
				{
					iter = item2;
					if (VCEMath.IsEqualRotation(iter.m_Rotation, vCComponentData2.m_Rotation) && !(iter.m_Scale != vCComponentData2.m_Scale) && iter.m_Visible == vCComponentData2.m_Visible && (!(vCComponentData2 is IVCMultiphaseComponentData) || !(iter is IVCMultiphaseComponentData) || (vCComponentData2 as IVCMultiphaseComponentData).Phase == (iter as IVCMultiphaseComponentData).Phase) && m_Selection.Find((SelectInfo it) => it.m_Component.m_Data == iter) == null)
					{
						iter.m_Entity.transform.position = vCComponentData2.m_Position;
					}
				}
			}
		}
	}

	private void OnRotating(Vector3 axis, float angle)
	{
		float num = VCEditor.s_Scene.m_Setting.m_VoxelSize * 0.5f;
		VCEditor.s_Mirror.CalcPrepare(num * 2f);
		foreach (SelectInfo item in m_Selection)
		{
			Vector3 eulerAngles = item.m_Component.transform.eulerAngles;
			item.m_Component.transform.eulerAngles = item.m_OldRotation;
			item.m_Component.transform.Rotate(axis, angle, Space.World);
			item.m_DragRotation = item.m_Component.transform.eulerAngles;
			item.m_DragRotation = VCEMath.NormalizeEulerAngle(item.m_DragRotation);
			item.m_NewRotation = item.m_DragRotation;
			item.m_Component.transform.eulerAngles = eulerAngles;
			Vector3 rotation = item.m_Component.m_Data.m_Rotation;
			item.m_Component.m_Data.m_Rotation = item.m_NewRotation;
			item.m_Component.m_Data.Validate();
			if (item.m_Component.m_Data.m_Rotation != item.m_NewRotation)
			{
				item.m_NewRotation = eulerAngles;
				item.m_Component.m_Data.m_Rotation = item.m_NewRotation;
				item.m_Component.m_Data.Validate();
			}
			Vector3 rotation2 = item.m_Component.m_Data.m_Rotation;
			item.m_Component.transform.eulerAngles = rotation2;
			item.m_NewRotation = rotation2;
			if (m_DataInspector != null)
			{
				m_DataInspector.GetComponent<VCEUIComponentInspector>().Set(item.m_Component.m_Data);
			}
			item.m_Component.m_Data.m_Rotation = rotation;
			if (!VCEditor.s_Mirror.Enabled_Masked)
			{
				continue;
			}
			Vector3[] array = new Vector3[8];
			VCEditor.s_Mirror.MirrorComponent(item.m_Component.m_Data);
			for (int i = 0; i < VCEditor.s_Mirror.OutputCnt; i++)
			{
				ref Vector3 reference = ref array[i];
				reference = VCEditor.s_Mirror.ComponentOutput[i].m_Rotation;
			}
			VCComponentData vCComponentData = item.m_Component.m_Data.Copy();
			vCComponentData.m_Rotation = item.m_NewRotation;
			VCEditor.s_Mirror.MirrorComponent(vCComponentData);
			for (int j = 1; j < VCEditor.s_Mirror.OutputCnt; j++)
			{
				VCComponentData vCComponentData2 = VCEditor.s_Mirror.ComponentOutput[j];
				List<VCComponentData> list = VCEditor.s_Scene.m_IsoData.FindComponentsAtPos(vCComponentData2.m_Position, vCComponentData2.m_ComponentId);
				VCComponentData iter;
				foreach (VCComponentData item2 in list)
				{
					iter = item2;
					if (VCEMath.IsEqualRotation(iter.m_Rotation, array[j]) && !(iter.m_Scale != vCComponentData2.m_Scale) && iter.m_Visible == vCComponentData2.m_Visible && (!(vCComponentData2 is IVCMultiphaseComponentData) || !(iter is IVCMultiphaseComponentData) || (vCComponentData2 as IVCMultiphaseComponentData).Phase == (iter as IVCMultiphaseComponentData).Phase) && m_Selection.Find((SelectInfo it) => it.m_Component.m_Data == iter) == null)
					{
						iter.m_Entity.transform.eulerAngles = vCComponentData2.m_Rotation;
					}
				}
			}
		}
	}

	public override void ClearSelection()
	{
		m_Selection.Clear();
	}

	public void DeleteSelection()
	{
		if (UsingGizmo)
		{
			return;
		}
		VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
		m_Action = new VCEAction();
		int num = 0;
		foreach (SelectInfo item in m_Selection)
		{
			VCEComponentTool component = item.m_Component;
			List<VCEComponentTool> list = MirrorImage(component);
			list.Add(component);
			foreach (VCEComponentTool item2 in list)
			{
				int componentIndex = VCEditor.s_Scene.m_IsoData.GetComponentIndex(item2.m_Data);
				if (componentIndex >= 0)
				{
					VCEDelComponent vCEDelComponent = new VCEDelComponent(componentIndex, item2.m_Data);
					vCEDelComponent.Redo();
					m_Action.Modifies.Add(vCEDelComponent);
					num++;
				}
			}
		}
		if (m_Action.Modifies.Count > 0)
		{
			m_Action.Register();
			ClearSelection();
		}
		if (num > 0)
		{
			VCEStatusBar.ShowText(num + " " + "component(s) have been removed".ToLocalizationString(), 4f);
		}
		m_MouseOnGizmo = false;
	}

	public void ApplyInspectorChange()
	{
		if (m_Selection.Count != 1 || m_DataInspector == null)
		{
			return;
		}
		VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
		m_Action = new VCEAction();
		VCComponentData data = m_Selection[0].m_Component.m_Data;
		VCComponentData vCComponentData = m_DataInspector.GetComponent<VCEUIComponentInspector>().Get();
		if (VCEditor.s_Mirror.Enabled_Masked)
		{
			VCComponentData[] array = new VCComponentData[8];
			VCEditor.s_Mirror.MirrorComponent(data);
			for (int i = 0; i < VCEditor.s_Mirror.OutputCnt; i++)
			{
				array[i] = VCEditor.s_Mirror.ComponentOutput[i].Copy();
			}
			VCEditor.s_Mirror.MirrorComponent(vCComponentData);
			for (int j = 0; j < VCEditor.s_Mirror.OutputCnt; j++)
			{
				VCComponentData vCComponentData2 = VCEditor.s_Mirror.ComponentOutput[j];
				List<VCComponentData> list = VCEditor.s_Scene.m_IsoData.FindComponentsAtPos(array[j].m_Position, vCComponentData2.m_ComponentId);
				VCComponentData iter;
				foreach (VCComponentData item3 in list)
				{
					iter = item3;
					if ((j > 0 && m_Selection.Find((SelectInfo it) => it.m_Component.m_Data == iter) != null) || !VCEMath.IsEqualRotation(iter.m_Rotation, array[j].m_Rotation) || !VCEMath.IsEqualVector(iter.m_Scale, array[j].m_Scale) || iter.m_Visible != array[j].m_Visible || (array[j] is IVCMultiphaseComponentData && iter is IVCMultiphaseComponentData && (array[j] as IVCMultiphaseComponentData).Phase != (iter as IVCMultiphaseComponentData).Phase))
					{
						continue;
					}
					int componentIndex = VCEditor.s_Scene.m_IsoData.GetComponentIndex(iter);
					if (componentIndex >= 0)
					{
						if (j != 0 && vCComponentData2 is VCQuadphaseFixedPartData)
						{
							(vCComponentData2 as VCQuadphaseFixedPartData).m_Phase = ((vCComponentData2 as VCQuadphaseFixedPartData).m_Phase & 1) | ((iter as VCQuadphaseFixedPartData).m_Phase & 2);
						}
						VCEAlterComponent item = new VCEAlterComponent(componentIndex, iter, vCComponentData2);
						m_Action.Modifies.Add(item);
					}
				}
			}
		}
		else
		{
			int componentIndex2 = VCEditor.s_Scene.m_IsoData.GetComponentIndex(data);
			if (componentIndex2 < 0)
			{
				return;
			}
			VCEAlterComponent item2 = new VCEAlterComponent(componentIndex2, data, vCComponentData);
			m_Action.Modifies.Add(item2);
		}
		VCEStatusBar.ShowText("Changes applied".ToLocalizationString(), 2f);
		m_Action.Do();
	}

	public VCPArmorPivot GetVCPArmorPivotByIndex(int index)
	{
		return (m_Selection.Count <= 0 || m_Selection.Count < index + 1) ? null : m_Selection[index].m_Component.GetComponent<VCPArmorPivot>();
	}
}
