using UnityEngine;

public class VCEUIMaterialItem : MonoBehaviour
{
	public VCEUIMaterialList m_ParentList;

	public VCMaterial m_Material;

	public UILabel m_NameLabel;

	public UILabel m_IndexLabel;

	public Material m_SourceIconTexture;

	public UITexture m_MaterialIcon;

	private Material m_MaterialIconTexture;

	public GameObject m_SelectedGlow;

	public GameObject m_AddBtn;

	public GameObject m_DelBtn;

	public static VCMaterial s_CurrentDelMat;

	private void Start()
	{
		m_NameLabel.text = m_Material.m_Name;
		m_MaterialIconTexture = Object.Instantiate(m_SourceIconTexture);
		m_MaterialIconTexture.mainTexture = m_Material.m_DiffuseTex;
		m_MaterialIcon.material = m_MaterialIconTexture;
	}

	private void Update()
	{
		if (!VCEditor.DocumentOpen())
		{
			return;
		}
		int num = VCEditor.s_Scene.m_IsoData.QueryMaterialIndex(m_Material);
		if (num >= 0 && VCEditor.s_Scene.m_IsoData.m_Materials[num] == m_Material)
		{
			m_IndexLabel.text = "No." + (num + 1);
			m_IndexLabel.color = Color.white;
		}
		else if (VCEditor.SelectedMaterial == m_Material)
		{
			if (VCEditor.SelectedVoxelType < 0)
			{
				m_IndexLabel.text = "ISO FULL";
				m_IndexLabel.color = Color.red;
			}
			else
			{
				m_IndexLabel.text = "No." + (VCEditor.SelectedVoxelType + 1);
				m_IndexLabel.color = Color.white;
			}
		}
		else
		{
			m_IndexLabel.text = string.Empty;
			m_IndexLabel.color = Color.white;
		}
		bool flag = VCEAssetMgr.s_TempMaterials.ContainsKey(m_Material.m_Guid);
		bool flag2 = VCEAssetMgr.s_Materials.ContainsKey(m_Material.m_Guid);
		m_AddBtn.SetActive(flag && !flag2);
		m_DelBtn.SetActive(!flag && flag2 && num < 0 && VCEditor.SelectedMaterial == m_Material);
	}

	private void OnDestroy()
	{
		Object.Destroy(m_MaterialIconTexture);
	}

	public void OnItemClick()
	{
		VCEditor.SelectedMaterial = m_Material;
		VCEditor.Instance.m_UI.m_MaterialWindow.Reset(m_Material);
		if (!VCEditor.SelectedGeneralBrush && VCEditor.VoxelSelection.Count == 0 && VCEditor.Instance.m_UI.m_VoxelBrushItemGroup.m_LastGeneralBrush != null)
		{
			VCEditor.Instance.m_UI.m_VoxelBrushItemGroup.m_LastGeneralBrush.GetComponent<UICheckbox>().isChecked = true;
		}
		s_CurrentDelMat = null;
	}

	public void OnItemDbClick()
	{
		if (!VCEditor.Instance.m_UI.m_MaterialWindow.WindowVisible())
		{
			VCEditor.Instance.m_UI.m_MaterialWindow.ShowWindow(m_Material);
		}
		s_CurrentDelMat = null;
	}

	public void OnAddClick()
	{
		if (VCEAssetMgr.AddMaterialFromTemp(m_Material.m_Guid))
		{
			VCEditor.SelectedMaterial = m_Material;
			VCEditor.Instance.m_UI.m_MaterialWindow.Reset(m_Material);
			VCEditor.Instance.m_UI.m_MaterialList.RefreshMaterialList(VCEditor.Instance.m_UI.m_MatterPopupList.selection);
			VCEStatusBar.ShowText("Add material".ToLocalizationString() + " [" + m_Material.m_Name + "] " + "from the current ISO".ToLocalizationString() + " !", 6f, typeeffect: true);
		}
		else
		{
			VCEMsgBox.Show(VCEMsgBoxType.MATERIAL_NOT_SAVED);
		}
		s_CurrentDelMat = null;
	}

	public static void DoDeleteFromMsgBox()
	{
		if (VCEAssetMgr.DeleteMaterial(s_CurrentDelMat.m_Guid))
		{
			VCEditor.SelectedMaterial = null;
			if (VCEditor.Instance.m_UI.m_MaterialWindow.WindowVisible())
			{
				VCEditor.Instance.m_UI.m_MaterialWindow.HideWindow();
			}
			VCEditor.Instance.m_UI.m_MaterialList.RefreshMaterialList(VCEditor.Instance.m_UI.m_MatterPopupList.selection);
			VCEStatusBar.ShowText("Material".ToLocalizationString() + " [" + s_CurrentDelMat.m_Name + "] " + "has been deleted".ToLocalizationString() + " !", 6f, typeeffect: true);
		}
		else
		{
			VCEMsgBox.Show(VCEMsgBoxType.MATERIAL_NOT_SAVED);
		}
	}

	public void OnDelClick()
	{
		s_CurrentDelMat = m_Material;
		VCEMsgBox.Show(VCEMsgBoxType.MATERIAL_DEL_QUERY);
	}
}
