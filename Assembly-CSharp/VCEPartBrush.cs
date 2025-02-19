using System;
using System.Collections.Generic;
using UnityEngine;

public class VCEPartBrush : VCEBrush
{
	[NonSerialized]
	public GameObject m_PartInst;

	[NonSerialized]
	public VCEComponentTool m_Tool;

	private VCEMath.DrawTarget m_Target;

	private VCPartInfo m_LastSelectedPart;

	private void Update()
	{
		if (VCEditor.SelectedPart == null)
		{
			return;
		}
		if (VCEInput.s_Cancel)
		{
			Cancel();
			return;
		}
		if (VCEditor.SelectedPart != m_LastSelectedPart)
		{
			if (m_PartInst != null)
			{
				UnityEngine.Object.Destroy(m_PartInst);
			}
			if (VCEditor.SelectedPart.m_ResObj == null)
			{
				Debug.LogError("This part has no prefab resource: " + VCEditor.SelectedPart.m_Name);
				Cancel();
				return;
			}
			m_PartInst = UnityEngine.Object.Instantiate(VCEditor.SelectedPart.m_ResObj);
			m_PartInst.SetActive(value: false);
			m_PartInst.transform.parent = base.transform;
			m_PartInst.transform.localPosition = Vector3.zero;
			m_Tool = m_PartInst.GetComponent<VCEComponentTool>();
			m_Tool.m_IsBrush = true;
			m_Tool.m_InEditor = true;
			m_Tool.m_ToolGroup.SetActive(value: true);
			Collider[] componentsInChildren = m_PartInst.GetComponentsInChildren<Collider>(includeInactive: true);
			Collider[] array = componentsInChildren;
			foreach (Collider collider in array)
			{
				if (collider.gameObject != m_Tool.m_ToolGroup.gameObject)
				{
					UnityEngine.Object.Destroy(collider);
				}
				else
				{
					collider.enabled = false;
				}
			}
			m_LastSelectedPart = VCEditor.SelectedPart;
		}
		float voxelSize = VCEditor.s_Scene.m_Setting.m_VoxelSize;
		if (!VCEInput.s_MouseOnUI && VCEMath.RayCastDrawTarget(VCEInput.s_PickRay, out m_Target, 128) && VCEditor.s_Scene.m_IsoData.IsPointIn(m_Target.cursor))
		{
			m_PartInst.SetActive(value: true);
			Vector3 xyz = m_Target.rch.point * 2f;
			IntVector3 intVector = new IntVector3(xyz);
			m_Tool.SetPivotPos(intVector.ToVector3() * 0.5f * voxelSize);
			bool flag = true;
			if (VCEditor.s_Scene.m_IsoData.FindComponentsAtPos(m_PartInst.transform.localPosition).Count > 0)
			{
				flag = false;
			}
			if (VCEditor.s_Mirror.Enabled_Masked)
			{
				VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
				if (m_PartInst.transform.localPosition.x == VCEditor.s_Mirror.WorldPos.x && VCEditor.s_Mirror.XPlane_Masked)
				{
					flag = false;
				}
				if (m_PartInst.transform.localPosition.y == VCEditor.s_Mirror.WorldPos.y && VCEditor.s_Mirror.YPlane_Masked)
				{
					flag = false;
				}
				if (m_PartInst.transform.localPosition.z == VCEditor.s_Mirror.WorldPos.z && VCEditor.s_Mirror.ZPlane_Masked)
				{
					flag = false;
				}
			}
			if (flag)
			{
				m_Tool.m_SelBound.m_BoundColor = GLComponentBound.s_Green;
				m_Tool.m_SelBound.m_Highlight = false;
			}
			else
			{
				m_Tool.m_SelBound.m_BoundColor = GLComponentBound.s_Red;
				m_Tool.m_SelBound.m_Highlight = true;
			}
			if (Input.GetMouseButtonDown(0) && flag)
			{
				Do();
			}
		}
		else
		{
			m_PartInst.SetActive(value: false);
		}
	}

	protected override void Do()
	{
		m_Action = new VCEAction();
		VCComponentData vCComponentData = VCComponentData.Create(VCEditor.SelectedPart);
		if (vCComponentData != null)
		{
			vCComponentData.m_ComponentId = VCEditor.SelectedPart.m_ID;
			vCComponentData.m_Type = VCEditor.SelectedPart.m_Type;
			vCComponentData.m_Position = m_PartInst.transform.localPosition;
			vCComponentData.m_Rotation = Vector3.zero;
			vCComponentData.m_Scale = Vector3.one;
			vCComponentData.m_Visible = true;
			vCComponentData.m_CurrIso = VCEditor.s_Scene.m_IsoData;
			vCComponentData.Validate();
			VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
			if (VCEditor.s_Mirror.Enabled_Masked)
			{
				VCEditor.s_Mirror.MirrorComponent(vCComponentData);
				for (int num = VCEditor.s_Mirror.OutputCnt - 1; num >= 0; num--)
				{
					VCComponentData vCComponentData2 = VCEditor.s_Mirror.ComponentOutput[num];
					if (VCEditor.s_Scene.m_IsoData.IsComponentIn(vCComponentData2.m_Position))
					{
						List<VCComponentData> list = VCEditor.s_Scene.m_IsoData.FindComponentsAtPos(vCComponentData2.m_Position, vCComponentData2.m_ComponentId);
						foreach (VCComponentData item2 in list)
						{
							if (!(item2.m_Scale != vCComponentData2.m_Scale) && item2.m_Visible == vCComponentData2.m_Visible && (!(vCComponentData2 is IVCMultiphaseComponentData) || !(item2 is IVCMultiphaseComponentData) || (vCComponentData2 as IVCMultiphaseComponentData).Phase == (item2 as IVCMultiphaseComponentData).Phase))
							{
								int componentIndex = VCEditor.s_Scene.m_IsoData.GetComponentIndex(item2);
								if (componentIndex >= 0)
								{
									VCEDelComponent vCEDelComponent = new VCEDelComponent(componentIndex, item2);
									vCEDelComponent.Redo();
									m_Action.Modifies.Add(vCEDelComponent);
								}
							}
						}
						VCEAddComponent vCEAddComponent = new VCEAddComponent(VCEditor.s_Scene.m_IsoData.m_Components.Count, vCComponentData2);
						vCEAddComponent.Redo();
						m_Action.Modifies.Add(vCEAddComponent);
					}
				}
				m_Action.Register();
			}
			else
			{
				if (vCComponentData is VCFixedHandPartData vCFixedHandPartData)
				{
					vCFixedHandPartData.m_LeftHand = vCComponentData.m_Position.x < (float)VCEditor.s_Scene.m_IsoData.m_HeadInfo.xSize * VCEditor.s_Scene.m_Setting.m_VoxelSize * 0.5f;
				}
				VCEAddComponent item = new VCEAddComponent(VCEditor.s_Scene.m_IsoData.m_Components.Count, vCComponentData);
				m_Action.Modifies.Add(item);
				m_Action.Do();
			}
		}
		else
		{
			Debug.LogWarning("Forget to add this part's implementation ?");
		}
	}

	public override void Cancel()
	{
		VCEditor.SelectedPart = null;
	}
}
