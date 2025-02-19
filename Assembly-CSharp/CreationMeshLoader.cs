using System.Collections.Generic;
using UnityEngine;
using WhiteCat;

public class CreationMeshLoader : MonoBehaviour
{
	public delegate void DNotify();

	private int m_CreationID;

	private CreationData m_CreationData;

	private VCMCComputer m_Computer;

	private VCMeshMgr m_MeshMgr;

	private CreationController creationController;

	public string m_DebugCommand = string.Empty;

	private bool m_lastVisible;

	private bool m_Visible;

	private bool m_Loading;

	public bool m_ShowNow;

	public bool m_HideNow;

	public bool m_Meshdagger;

	private int CreationID
	{
		set
		{
			if (m_Computer != null)
			{
				m_Computer.Destroy();
			}
			CreationData creation = CreationMgr.GetCreation(value);
			if (creation != null)
			{
				m_CreationID = value;
				m_CreationData = creation;
				m_MeshMgr = GetComponent<VCMeshMgr>();
				m_Computer = new VCMCComputer();
				m_Computer.Init(new IntVector3(creation.m_IsoData.m_HeadInfo.xSize, creation.m_IsoData.m_HeadInfo.ySize, creation.m_IsoData.m_HeadInfo.zSize), m_MeshMgr, for_editor: false);
				if (creation.m_Attribute.m_Type == ECreation.Vehicle || creation.m_Attribute.m_Type == ECreation.Aircraft || creation.m_Attribute.m_Type == ECreation.Boat || creation.m_Attribute.m_Type == ECreation.SimpleObject || creation.m_Attribute.m_Type == ECreation.AITurret)
				{
					m_Computer.m_CreateBoxCollider = true;
				}
			}
			else
			{
				m_CreationID = 0;
				m_CreationData = null;
				m_Computer = null;
				m_MeshMgr = null;
			}
		}
	}

	public bool Valid
	{
		get
		{
			if (m_CreationID == 0)
			{
				return false;
			}
			if (m_CreationData == null)
			{
				return false;
			}
			if (m_Computer == null)
			{
				return false;
			}
			if (m_MeshMgr == null)
			{
				return false;
			}
			return true;
		}
	}

	public event DNotify OnLoadMeshComplete;

	public event DNotify OnFreeMesh;

	private void InitCreationID(int creationId)
	{
		if (m_Computer != null)
		{
			m_Computer.Destroy();
		}
		CreationData creation = CreationMgr.GetCreation(creationId);
		if (creation != null)
		{
			m_CreationID = creationId;
			m_CreationData = creation;
			m_MeshMgr = GetComponent<VCMeshMgr>();
			m_Computer = new VCMCComputer();
			m_Computer.Init(new IntVector3(creation.m_IsoData.m_HeadInfo.xSize, creation.m_IsoData.m_HeadInfo.ySize, creation.m_IsoData.m_HeadInfo.zSize), m_MeshMgr, for_editor: false);
			if (creation.m_Attribute.m_Type == ECreation.Vehicle || creation.m_Attribute.m_Type == ECreation.Aircraft || creation.m_Attribute.m_Type == ECreation.Boat || creation.m_Attribute.m_Type == ECreation.SimpleObject || creation.m_Attribute.m_Type == ECreation.AITurret)
			{
				m_Computer.m_CreateBoxCollider = true;
			}
		}
		else
		{
			m_CreationID = 0;
			m_CreationData = null;
			m_Computer = null;
			m_MeshMgr = null;
		}
	}

	private void InitCreationIDClone(int creationId, VFCreationDataSource dataSource, VCMeshMgr mesh_mgr)
	{
		if (m_Computer != null)
		{
			m_Computer.Destroy();
		}
		CreationData creation = CreationMgr.GetCreation(creationId);
		if (creation != null)
		{
			m_CreationID = creationId;
			m_CreationData = creation;
			m_MeshMgr = mesh_mgr;
			m_MeshMgr.m_LeftSidePos = !mesh_mgr.m_LeftSidePos;
			m_Computer = new VCMCComputer();
			m_Computer.InitClone(dataSource, m_MeshMgr, for_editor: false);
			if (creation.m_Attribute.m_Type == ECreation.Vehicle || creation.m_Attribute.m_Type == ECreation.Aircraft || creation.m_Attribute.m_Type == ECreation.Boat || creation.m_Attribute.m_Type == ECreation.SimpleObject || creation.m_Attribute.m_Type == ECreation.AITurret)
			{
				m_Computer.m_CreateBoxCollider = true;
			}
		}
		else
		{
			m_CreationID = 0;
			m_CreationData = null;
			m_Computer = null;
			m_MeshMgr = null;
		}
	}

	public void FreeMesh()
	{
		StopAllCoroutines();
		RevertMat();
		m_Loading = false;
		if (Valid)
		{
			MeshFilter[] componentsInChildren = GetComponentsInChildren<MeshFilter>(includeInactive: true);
			MeshFilter[] array = componentsInChildren;
			foreach (MeshFilter meshFilter in array)
			{
				if (meshFilter.mesh != null)
				{
					Object.Destroy(meshFilter.mesh);
					meshFilter.mesh = null;
				}
			}
			m_Computer.Clear();
		}
		if (this.OnFreeMesh != null)
		{
			this.OnFreeMesh();
		}
	}

	public void LoadMesh()
	{
		if (m_Loading)
		{
			FreeMesh();
		}
		base.gameObject.layer = VCConfig.s_ProductLayer;
		DoLoadMesh();
	}

	private void DoLoadMesh()
	{
		m_Loading = true;
		SetLoadingMat();
		if (Valid)
		{
			int count = m_CreationData.m_IsoData.m_Voxels.Count;
			int[] array = new int[count];
			m_CreationData.m_IsoData.m_Voxels.Keys.CopyTo(array, 0);
			Dictionary<int, VCVoxel> voxels = m_CreationData.m_IsoData.m_Voxels;
			if (m_Computer != null)
			{
				for (int i = 0; i < count; i++)
				{
					int num = array[i];
					if (m_Meshdagger && m_Computer != null && m_Computer.m_MeshMgr != null)
					{
						if (voxels != null && voxels.ContainsKey(num) && m_Computer.InSide(num, m_CreationData.m_IsoData.m_HeadInfo.xSize, m_Computer.m_MeshMgr.m_LeftSidePos))
						{
							m_Computer.AlterVoxel(num, voxels[num]);
						}
					}
					else if (m_Computer != null && voxels != null && voxels.ContainsKey(num))
					{
						m_Computer.AlterVoxel(num, voxels[num]);
					}
				}
				if (m_Computer != null)
				{
					m_Computer.ReqMesh();
				}
			}
			creationController.AddBuildFinishedListener(RevertMat);
		}
		m_Loading = false;
	}

	public void Init(CreationController controller)
	{
		m_lastVisible = false;
		m_Visible = false;
		InitCreationID(controller.creationData.m_ObjectID);
		creationController = controller;
		controller.onUpdate += UpdateMeshLoader;
	}

	public void InitClone(CreationController controller, VFCreationDataSource dataSource, VCMeshMgr mesh)
	{
		m_lastVisible = false;
		m_Visible = false;
		InitCreationIDClone(controller.creationData.m_ObjectID, dataSource, mesh);
		creationController = controller;
		controller.onUpdate += UpdateMeshLoader;
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
		m_Loading = false;
		if (m_Computer != null)
		{
			m_Computer.Destroy();
			m_Computer = null;
		}
	}

	private void UpdateMeshLoader()
	{
		if (!Valid)
		{
			return;
		}
		if (m_ShowNow)
		{
			m_ShowNow = false;
			m_DebugCommand = "show";
		}
		if (m_HideNow)
		{
			m_HideNow = false;
			m_DebugCommand = "hide";
		}
		if (m_DebugCommand.ToLower().Trim() == "show")
		{
			m_DebugCommand = string.Empty;
			LoadMesh();
		}
		else if (m_DebugCommand.ToLower().Trim() == "hide")
		{
			m_DebugCommand = string.Empty;
			FreeMesh();
		}
		m_Visible = VCGameMediator.MeshVisible(this);
		if (m_lastVisible != m_Visible)
		{
			m_lastVisible = m_Visible;
			if (m_Visible)
			{
				LoadMesh();
			}
			else
			{
				FreeMesh();
			}
		}
	}

	private void SetLoadingMat()
	{
		if (Valid && m_Loading)
		{
			m_MeshMgr.m_MeshMat = VCEditor.Instance.m_HolographicMat;
		}
	}

	private void RevertMat()
	{
		if (Valid && !m_Loading)
		{
			m_MeshMgr.m_MeshMat = m_CreationData.m_MeshMgr.m_MeshMat;
			m_MeshMgr.SetMeshMat(m_MeshMgr.m_MeshMat);
			OutlineObject componentOrOnParent = VCUtils.GetComponentOrOnParent<OutlineObject>(base.gameObject);
			if (componentOrOnParent != null)
			{
				componentOrOnParent.ReplaceInCache(VCEditor.Instance.m_HolographicMat, m_MeshMgr.m_MeshMat);
			}
			if (this.OnLoadMeshComplete != null)
			{
				this.OnLoadMeshComplete();
			}
		}
	}
}
