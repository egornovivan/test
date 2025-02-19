using System;
using System.Collections.Generic;
using UnityEngine;

public class VCEDecalBrush : VCEBrush
{
	public GameObject m_DecalPrefab;

	[NonSerialized]
	public VCDecalHandler m_DecalInst;

	[NonSerialized]
	public VCEComponentTool m_Tool;

	private float m_RotateAngle;

	private void OnEnable()
	{
		float voxelSize = VCEditor.s_Scene.m_Setting.m_VoxelSize;
		if (m_DecalInst != null)
		{
			UnityEngine.Object.Destroy(m_DecalInst.gameObject);
			m_DecalInst = null;
		}
		m_DecalInst = UnityEngine.Object.Instantiate(m_DecalPrefab).GetComponent<VCDecalHandler>();
		m_DecalInst.gameObject.SetActive(value: false);
		m_DecalInst.transform.parent = base.transform;
		m_DecalInst.transform.localPosition = Vector3.zero;
		m_DecalInst.transform.localScale = Vector3.one;
		m_DecalInst.m_Guid = VCEditor.SelectedDecalGUID;
		m_DecalInst.m_Depth = voxelSize;
		m_DecalInst.m_Size = voxelSize * 10f;
		m_DecalInst.m_Mirrored = false;
		m_DecalInst.m_ShaderIndex = 0;
		m_Tool = m_DecalInst.GetComponent<VCEComponentTool>();
		m_Tool.m_IsBrush = true;
		m_Tool.m_InEditor = true;
		m_Tool.m_ToolGroup.SetActive(value: true);
	}

	private void Update()
	{
		if (VCEditor.SelectedDecalGUID == 0L || VCEditor.SelectedDecalIndex < 0 || VCEditor.SelectedDecal == null)
		{
			return;
		}
		if (VCEInput.s_Cancel)
		{
			Cancel();
			return;
		}
		m_DecalInst.m_Guid = VCEditor.SelectedDecalGUID;
		float voxelSize = VCEditor.s_Scene.m_Setting.m_VoxelSize;
		if (!VCEInput.s_MouseOnUI && VCEMath.RayCastVoxel(VCEInput.s_PickRay, out var rch, 128))
		{
			AdjustBrush();
			m_DecalInst.gameObject.SetActive(value: true);
			Vector3 xyz = rch.point * 2f;
			IntVector3 intVector = new IntVector3(xyz);
			m_Tool.SetPivotPos(intVector.ToVector3() * 0.5f * voxelSize);
			m_DecalInst.transform.localRotation = Quaternion.LookRotation(-rch.normal);
			m_DecalInst.transform.Rotate(-rch.normal, m_RotateAngle, Space.World);
			bool flag = true;
			List<VCComponentData> list = VCEditor.s_Scene.m_IsoData.FindComponentsAtPos(m_DecalInst.transform.localPosition, VCDecalData.s_ComponentId);
			foreach (VCComponentData item in list)
			{
				if (item is VCDecalData vCDecalData && vCDecalData.m_Guid == m_DecalInst.m_Guid)
				{
					flag = false;
					break;
				}
			}
			if (VCEditor.s_Mirror.Enabled_Masked)
			{
				VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
				if (m_DecalInst.transform.localPosition.x == VCEditor.s_Mirror.WorldPos.x && VCEditor.s_Mirror.XPlane_Masked)
				{
					flag = false;
				}
				if (m_DecalInst.transform.localPosition.y == VCEditor.s_Mirror.WorldPos.y && VCEditor.s_Mirror.YPlane_Masked)
				{
					flag = false;
				}
				if (m_DecalInst.transform.localPosition.z == VCEditor.s_Mirror.WorldPos.z && VCEditor.s_Mirror.ZPlane_Masked)
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
			m_DecalInst.gameObject.SetActive(value: false);
		}
	}

	private void AdjustBrush()
	{
		float voxelSize = VCEditor.s_Scene.m_Setting.m_VoxelSize;
		if (VCEInput.s_Increase && m_DecalInst.m_Size < voxelSize * 99.5f)
		{
			m_DecalInst.m_Size += voxelSize;
		}
		if (VCEInput.s_Decrease && m_DecalInst.m_Size > voxelSize * 1.5f)
		{
			m_DecalInst.m_Size -= voxelSize;
		}
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			m_RotateAngle -= 45f;
			if (m_RotateAngle < 0f)
			{
				m_RotateAngle += 360f;
			}
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			m_RotateAngle += 45f;
			if (m_RotateAngle > 360f)
			{
				m_RotateAngle -= 360f;
			}
		}
	}

	protected override void Do()
	{
		if (VCEditor.SelectedDecalGUID == 0L || VCEditor.SelectedDecalIndex < 0 || VCEditor.SelectedDecal == null)
		{
			return;
		}
		m_Action = new VCEAction();
		if (VCComponentData.CreateDecal() is VCDecalData vCDecalData)
		{
			vCDecalData.m_ComponentId = VCDecalData.s_ComponentId;
			vCDecalData.m_Type = EVCComponent.cpDecal;
			vCDecalData.m_Position = m_DecalInst.transform.localPosition;
			vCDecalData.m_Rotation = VCEMath.NormalizeEulerAngle(m_DecalInst.transform.localEulerAngles);
			vCDecalData.m_Scale = Vector3.one;
			vCDecalData.m_Visible = true;
			vCDecalData.m_CurrIso = VCEditor.s_Scene.m_IsoData;
			vCDecalData.m_Guid = VCEditor.SelectedDecalGUID;
			vCDecalData.m_AssetIndex = VCEditor.SelectedDecalIndex;
			vCDecalData.m_Size = m_DecalInst.m_Size;
			vCDecalData.m_Depth = m_DecalInst.m_Depth;
			vCDecalData.m_Mirrored = m_DecalInst.m_Mirrored;
			vCDecalData.m_ShaderIndex = m_DecalInst.m_ShaderIndex;
			vCDecalData.m_Color = m_DecalInst.m_Color;
			vCDecalData.Validate();
			ulong num = VCEditor.s_Scene.m_IsoData.DecalGUID(VCEditor.SelectedDecalIndex);
			ulong selectedDecalGUID = VCEditor.SelectedDecalGUID;
			if (num != selectedDecalGUID)
			{
				VCEAlterDecalMap vCEAlterDecalMap = new VCEAlterDecalMap(VCEditor.SelectedDecalIndex, num, selectedDecalGUID);
				vCEAlterDecalMap.Redo();
				m_Action.Modifies.Add(vCEAlterDecalMap);
			}
			VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
			if (VCEditor.s_Mirror.Enabled_Masked)
			{
				VCEditor.s_Mirror.MirrorComponent(vCDecalData);
				for (int num2 = VCEditor.s_Mirror.OutputCnt - 1; num2 >= 0; num2--)
				{
					VCComponentData vCComponentData = VCEditor.s_Mirror.ComponentOutput[num2];
					if (VCEditor.s_Scene.m_IsoData.IsComponentIn(vCComponentData.m_Position))
					{
						VCEAddComponent vCEAddComponent = new VCEAddComponent(VCEditor.s_Scene.m_IsoData.m_Components.Count, vCComponentData);
						vCEAddComponent.Redo();
						m_Action.Modifies.Add(vCEAddComponent);
					}
				}
			}
			else
			{
				VCEAddComponent vCEAddComponent2 = new VCEAddComponent(VCEditor.s_Scene.m_IsoData.m_Components.Count, vCDecalData);
				vCEAddComponent2.Redo();
				m_Action.Modifies.Add(vCEAddComponent2);
			}
			m_Action.Register();
		}
		else
		{
			Debug.LogWarning("Decal data create failed");
		}
	}

	public override void Cancel()
	{
		VCEditor.SelectedDecal = null;
	}
}
