using UnityEngine;

public class VCEUIDecalItem : MonoBehaviour
{
	public VCEUIDecalList m_ParentList;

	public UITexture m_DecalUITex;

	public GameObject m_HoverBtn;

	public GameObject m_SelectedSign;

	private Material m_DecalUITexMat;

	public UILabel m_IndexLabel;

	public GameObject m_AddBtn;

	public GameObject m_DelBtn;

	public ulong m_GUID;

	public static VCDecalAsset s_CurrentDelDecal;

	private void Start()
	{
		m_DecalUITexMat = Object.Instantiate(m_DecalUITex.material);
		m_DecalUITex.material = m_DecalUITexMat;
		m_SelectedSign.SetActive(value: false);
		VCDecalAsset decal = VCEAssetMgr.GetDecal(m_GUID);
		if (decal != null)
		{
			m_DecalUITexMat.SetTexture("_MainTex", decal.m_Tex);
		}
		else
		{
			m_DecalUITexMat.SetTexture("_MainTex", null);
		}
	}

	private void Update()
	{
		if (!VCEditor.DocumentOpen())
		{
			return;
		}
		VCDecalAsset decal = VCEAssetMgr.GetDecal(m_GUID);
		m_SelectedSign.SetActive(VCEditor.SelectedDecalGUID == m_GUID && m_GUID != 0);
		int num = VCEditor.s_Scene.m_IsoData.QueryExistDecalIndex(decal);
		if (num >= 0 && VCEditor.s_Scene.m_IsoData.m_DecalAssets[num] == decal)
		{
			m_IndexLabel.text = num + 1 + " of " + 4;
			m_IndexLabel.color = Color.white;
		}
		else if (VCEditor.SelectedDecal == decal)
		{
			if (VCEditor.SelectedDecalIndex < 0)
			{
				m_IndexLabel.text = "FULL";
				m_IndexLabel.color = Color.red;
			}
			else
			{
				m_IndexLabel.text = VCEditor.SelectedDecalIndex + 1 + " of " + 4;
				m_IndexLabel.color = Color.white;
			}
		}
		else
		{
			m_IndexLabel.text = string.Empty;
			m_IndexLabel.color = Color.white;
		}
		bool flag = VCEAssetMgr.s_TempDecals.ContainsKey(m_GUID);
		bool flag2 = VCEAssetMgr.s_Decals.ContainsKey(m_GUID);
		m_AddBtn.SetActive(flag && !flag2);
		m_DelBtn.SetActive(!flag && flag2 && num < 0 && VCEditor.SelectedDecal == decal);
	}

	private void OnSelectClick()
	{
		VCEditor.SelectedDecalGUID = m_GUID;
	}

	public void OnAddClick()
	{
		if (VCEAssetMgr.AddDecalFromTemp(m_GUID))
		{
			VCDecalAsset decal = VCEAssetMgr.GetDecal(m_GUID);
			VCEditor.SelectedDecalGUID = m_GUID;
			m_ParentList.RefreshDecalList();
			VCEStatusBar.ShowText("Add decal".ToLocalizationString() + " [" + decal.GUIDString + "] " + "from the current ISO".ToLocalizationString() + " !", 6f, typeeffect: true);
		}
		else
		{
			VCEMsgBox.Show(VCEMsgBoxType.DECAL_NOT_SAVED);
		}
		s_CurrentDelDecal = null;
	}

	public static void DoDeleteFromMsgBox()
	{
		string gUIDString = s_CurrentDelDecal.GUIDString;
		if (VCEAssetMgr.DeleteDecal(s_CurrentDelDecal.m_Guid))
		{
			VCEditor.SelectedDecal = null;
			VCEditor.Instance.m_UI.m_DecalList.RefreshDecalList();
			VCEStatusBar.ShowText("Decal".ToLocalizationString() + " [" + gUIDString + "] " + "has been deleted".ToLocalizationString() + " !", 6f, typeeffect: true);
		}
		else
		{
			VCEMsgBox.Show(VCEMsgBoxType.DECAL_NOT_SAVED);
		}
	}

	public void OnDelClick()
	{
		s_CurrentDelDecal = VCEAssetMgr.GetDecal(m_GUID);
		if (s_CurrentDelDecal != null)
		{
			VCEMsgBox.Show(VCEMsgBoxType.DECAL_DEL_QUERY);
		}
	}
}
