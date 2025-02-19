using UnityEngine;

public class VCEUIDecalWnd : MonoBehaviour
{
	public GameObject m_Window;

	public UILabel m_UIDLabel;

	public UITexture m_DecalUITex;

	private Material m_DecalUIMat;

	public UIInput m_PathInput;

	private string m_LastPath;

	public UILabel m_ErrorLabel;

	public GameObject m_CreateBtnGO;

	private VCDecalAsset m_TmpDecal;

	private void Start()
	{
		m_DecalUIMat = Object.Instantiate(m_DecalUITex.material);
		m_DecalUITex.material = m_DecalUIMat;
		m_LastPath = string.Empty;
	}

	private void Update()
	{
		if (!WindowVisible())
		{
			return;
		}
		if (VCEditor.SelectedDecal != null)
		{
			HideWindow();
		}
		if (m_PathInput.text.Length > 0)
		{
			string text = m_PathInput.text.Replace("\\", "/");
			if (m_PathInput.text != text)
			{
				m_PathInput.text = text;
			}
		}
		if (m_LastPath.Length < 4 && m_TmpDecal != null)
		{
			m_TmpDecal.Destroy();
			m_TmpDecal = null;
		}
		if (m_LastPath != m_PathInput.text)
		{
			m_LastPath = m_PathInput.text;
			if (m_TmpDecal != null)
			{
				m_TmpDecal.Destroy();
				m_TmpDecal = null;
			}
			Texture2D texture2D = VCUtils.LoadTextureFromFile(m_PathInput.text);
			if (texture2D != null)
			{
				m_TmpDecal = new VCDecalAsset();
				m_TmpDecal.Import(texture2D.EncodeToPNG());
				Object.Destroy(texture2D);
			}
		}
		if (m_TmpDecal != null && m_TmpDecal.m_Tex != null)
		{
			m_UIDLabel.text = m_TmpDecal.GUIDString;
			m_DecalUIMat.mainTexture = m_TmpDecal.m_Tex;
			m_DecalUITex.gameObject.SetActive(value: true);
			if (VCEAssetMgr.GetDecal(m_TmpDecal.m_Guid) != null)
			{
				m_ErrorLabel.text = "The same decal image already exist".ToLocalizationString() + " !";
			}
			else if (m_TmpDecal.m_Tex.width > 512 || m_TmpDecal.m_Tex.height > 512)
			{
				m_ErrorLabel.text = "Decal size must smaller than 512px".ToLocalizationString() + " !";
			}
			else
			{
				m_ErrorLabel.text = string.Empty;
			}
		}
		else
		{
			m_UIDLabel.text = "0000000000000000";
			m_DecalUITex.gameObject.SetActive(value: false);
			m_DecalUIMat.mainTexture = null;
			m_ErrorLabel.text = "Please specify a decal image".ToLocalizationString() + " (*.png)";
		}
		m_CreateBtnGO.SetActive(m_ErrorLabel.text.Trim().Length < 1);
	}

	private void OnDestroy()
	{
		if (m_TmpDecal != null)
		{
			m_TmpDecal.Destroy();
			m_TmpDecal = null;
		}
	}

	public bool WindowVisible()
	{
		return m_Window.activeInHierarchy;
	}

	public void ShowWindow()
	{
		m_TmpDecal = new VCDecalAsset();
		m_Window.SetActive(value: true);
	}

	public void HideWindow()
	{
		m_PathInput.text = string.Empty;
		if (m_TmpDecal != null)
		{
			m_TmpDecal.Destroy();
			m_TmpDecal = null;
		}
		m_Window.SetActive(value: false);
	}

	public void OnOKClick()
	{
		if (m_ErrorLabel.text.Trim().Length < 1 && m_TmpDecal != null && m_TmpDecal.m_Tex != null)
		{
			VCDecalAsset vCDecalAsset = new VCDecalAsset();
			vCDecalAsset.Import(m_TmpDecal.Export());
			VCEAssetMgr.s_Decals.Add(vCDecalAsset.m_Guid, vCDecalAsset);
			if (!VCEAssetMgr.CreateDecalDataFile(vCDecalAsset))
			{
				VCEMsgBox.Show(VCEMsgBoxType.DECAL_NOT_SAVED);
			}
			VCEditor.SelectedDecal = vCDecalAsset;
			VCEditor.Instance.m_UI.m_DecalList.RefreshDecalList();
			VCEditor.SelectedDecal = vCDecalAsset;
			VCEStatusBar.ShowText("Added new decal".ToLocalizationString() + " !", 4f, typeeffect: true);
		}
	}

	public void OnCloseClick()
	{
		HideWindow();
	}
}
