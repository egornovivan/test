using UnityEngine;

public class VCEPointBrush : VCEBrush
{
	public VCEMath.DrawTarget m_Target;

	public IntVector3 m_Offset = new IntVector3();

	public GameObject m_GizmoGroup;

	public VCEGizmoCubeMesh m_CursorGizmoCube;

	public VCEGizmoCubeMesh m_OffsetGizmoCube;

	private void Start()
	{
		m_Target = default(VCEMath.DrawTarget);
		m_CursorGizmoCube.m_Shrink = -0.03f;
		m_OffsetGizmoCube.m_Shrink = 0.03f;
		VCEStatusBar.ShowText("Pencil - Dot single voxel".ToLocalizationString(), 7f, typeeffect: true);
	}

	private void Update()
	{
		if (!VCEditor.DocumentOpen() || VCEditor.SelectedVoxelType < 0 || VCEInput.s_MouseOnUI)
		{
			m_GizmoGroup.gameObject.SetActive(value: false);
			return;
		}
		if (VCEInput.s_Cancel)
		{
			Cancel();
		}
		if (VCEInput.s_Shift && VCEInput.s_Left)
		{
			m_Offset -= IntVector3.UnitX;
		}
		if (VCEInput.s_Shift && VCEInput.s_Right)
		{
			m_Offset += IntVector3.UnitX;
		}
		if (VCEInput.s_Shift && VCEInput.s_Up)
		{
			m_Offset += IntVector3.UnitY;
		}
		if (VCEInput.s_Shift && VCEInput.s_Down)
		{
			m_Offset -= IntVector3.UnitY;
		}
		if (VCEInput.s_Shift && VCEInput.s_Forward)
		{
			m_Offset += IntVector3.UnitZ;
		}
		if (VCEInput.s_Shift && VCEInput.s_Back)
		{
			m_Offset -= IntVector3.UnitZ;
		}
		float voxelSize = VCEditor.s_Scene.m_Setting.m_VoxelSize;
		m_CursorGizmoCube.m_VoxelSize = voxelSize;
		m_OffsetGizmoCube.m_VoxelSize = voxelSize;
		if (VCEMath.RayCastDrawTarget(VCEInput.s_PickRay, out m_Target, 128))
		{
			m_GizmoGroup.gameObject.SetActive(value: true);
			IntVector3 intVector = m_Target.cursor + m_Offset;
			m_CursorGizmoCube.transform.position = m_Target.cursor.ToVector3() * voxelSize;
			m_OffsetGizmoCube.transform.position = intVector.ToVector3() * voxelSize;
			bool flag = VCEditor.s_Scene.m_IsoData.IsPointIn(m_Target.cursor);
			bool flag2 = VCEditor.s_Scene.m_IsoData.IsPointIn(intVector);
			bool flag3 = m_Offset.ToVector3().magnitude > 0.1f;
			m_CursorGizmoCube.gameObject.SetActive(flag && flag3);
			m_OffsetGizmoCube.gameObject.SetActive(flag2);
			if (flag && flag2 && Input.GetMouseButtonDown(0))
			{
				Do();
			}
		}
		else
		{
			m_GizmoGroup.gameObject.SetActive(value: false);
		}
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
		IntVector3 intVector = m_Target.cursor + m_Offset;
		if (VCEditor.s_Mirror.Enabled_Masked)
		{
			VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
			VCEditor.s_Mirror.MirrorVoxel(intVector);
			for (int i = 0; i < VCEditor.s_Mirror.OutputCnt; i++)
			{
				if (VCEditor.s_Scene.m_IsoData.IsPointIn(VCEditor.s_Mirror.Output[i]))
				{
					int num2 = VCIsoData.IPosToKey(VCEditor.s_Mirror.Output[i]);
					VCVoxel voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(num2);
					VCVoxel vCVoxel = new VCVoxel(byte.MaxValue, (byte)VCEditor.SelectedVoxelType);
					if ((int)voxel != (int)vCVoxel)
					{
						VCEAlterVoxel item2 = new VCEAlterVoxel(num2, voxel, vCVoxel);
						m_Action.Modifies.Add(item2);
					}
				}
			}
		}
		else
		{
			int num3 = VCIsoData.IPosToKey(intVector);
			VCVoxel voxel2 = VCEditor.s_Scene.m_IsoData.GetVoxel(num3);
			VCVoxel vCVoxel2 = new VCVoxel(byte.MaxValue, (byte)VCEditor.SelectedVoxelType);
			if ((int)voxel2 != (int)vCVoxel2)
			{
				VCEAlterVoxel item3 = new VCEAlterVoxel(num3, voxel2, vCVoxel2);
				m_Action.Modifies.Add(item3);
			}
		}
		if (m_Action.Modifies.Count > 0)
		{
			m_Action.Do();
		}
		VCEditor.Instance.m_MainCamera.GetComponent<VCECamera>().SetTarget((intVector.ToVector3() + Vector3.one * 0.5f) * VCEditor.s_Scene.m_Setting.m_VoxelSize);
	}

	public override void Cancel()
	{
		if (VCEditor.DocumentOpen())
		{
			VCEditor.Instance.m_UI.m_DefaultBrush_Material.isChecked = true;
		}
	}
}
