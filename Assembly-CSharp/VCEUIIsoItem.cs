using System.IO;
using UnityEngine;

public class VCEUIIsoItem : MonoBehaviour
{
	public VCEUIIsoList m_ParentList;

	public bool m_IsFolder;

	public string m_FilePath;

	public UISprite m_IsoDefaultSprite;

	public UISprite m_FolderSprite;

	public UITexture m_IsoIcon;

	public Material m_IsoIconMatRes;

	private Material m_IsoIconMat;

	private Texture2D m_IsoIconTex;

	public UILabel m_NameLabel;

	public GameObject m_HoverBtn;

	public GameObject m_SelectedSign;

	public GameObject m_DelBtn;

	private string m_Extension;

	public static string s_IsoToLoad = string.Empty;

	public static string s_IsoToDelete = string.Empty;

	private void Start()
	{
		m_IsoDefaultSprite.gameObject.SetActive(!m_IsFolder);
		m_FolderSprite.gameObject.SetActive(m_IsFolder);
		m_SelectedSign.SetActive(value: false);
		if (m_IsFolder)
		{
			m_NameLabel.text = new DirectoryInfo(m_FilePath).Name + "\r\n[808080]Folder[-]";
			m_Extension = string.Empty;
			return;
		}
		string text = new FileInfo(m_FilePath).Name;
		string text2 = (m_Extension = new FileInfo(m_FilePath).Extension);
		int num = text.LastIndexOf(".");
		text = text.Substring(0, (num < 0) ? text.Length : num);
		string text3 = ((!string.IsNullOrEmpty(text2) && text2.Length > 1) ? text2.Substring(1, text2.Length - 1) : "null");
		m_NameLabel.text = text + "\r\n[808080]" + text3.ToUpper() + " File[-]";
		if (text2 == VCConfig.s_IsoFileExt)
		{
			m_IsoDefaultSprite.color = Color.white;
		}
		else if (text2 == VCConfig.s_ObsoleteIsoFileExt)
		{
			m_IsoDefaultSprite.color = new Color(1f, 0.5f, 0.5f);
		}
		else
		{
			m_IsoDefaultSprite.gameObject.SetActive(value: false);
		}
	}

	private void OnDestroy()
	{
		if (m_IsoIconMat != null)
		{
			Object.Destroy(m_IsoIconMat);
			m_IsoIconMat = null;
		}
		if (m_IsoIconTex != null)
		{
			Object.Destroy(m_IsoIconTex);
			m_IsoIconTex = null;
		}
	}

	private void Update()
	{
		m_SelectedSign.SetActive(m_ParentList.m_SelectedItem == this);
		if (m_IsFolder)
		{
			m_DelBtn.SetActive(value: false);
		}
		else
		{
			m_DelBtn.SetActive(m_ParentList.m_SelectedItem == this);
		}
	}

	private void OnSelectClick()
	{
		m_ParentList.m_SelectedItem = this;
		if (m_Extension == VCConfig.s_IsoFileExt || m_Extension.Length == 0)
		{
			VCEStatusBar.ShowText("Double-click to open".ToLocalizationString(), 2f);
		}
		else if (m_Extension == VCConfig.s_ObsoleteIsoFileExt)
		{
			VCEStatusBar.ShowText("This ISO version is NOT compatible with the current build, please download the converter from our website".ToLocalizationString() + "!", Color.red, 20f);
		}
		else
		{
			VCEStatusBar.ShowText("Corrupt file".ToLocalizationString(), 2f);
		}
	}

	public static void DoLoadFromMsgBox()
	{
		VCEditor.LoadIso(s_IsoToLoad);
	}

	private void OnSelectDbClick()
	{
		if (m_IsFolder)
		{
			if (Directory.Exists(m_FilePath))
			{
				m_ParentList.m_Path = m_FilePath;
				m_ParentList.RefreshIsoList();
			}
			else
			{
				VCEStatusBar.ShowText("This folder does not exist".ToLocalizationString() + "!", 2f);
				m_ParentList.RefreshIsoList();
			}
		}
		else if (File.Exists(m_FilePath))
		{
			if (VCEHistory.s_Modified)
			{
				s_IsoToLoad = m_FilePath.Substring(VCConfig.s_IsoPath.Length);
				VCEMsgBox.Show(VCEMsgBoxType.LOAD_QUERY);
			}
			else
			{
				VCEditor.LoadIso(m_FilePath.Substring(VCConfig.s_IsoPath.Length));
			}
		}
		else
		{
			VCEMsgBox.Show(VCEMsgBoxType.MISSING_ISO);
			VCEStatusBar.ShowText("ISO file is missing".ToLocalizationString() + "!", 2f);
			m_ParentList.RefreshIsoList();
		}
	}

	public static void DoDeleteFromMsgBox()
	{
		try
		{
			File.Delete(s_IsoToDelete);
		}
		catch
		{
		}
		VCEditor.Instance.m_UI.m_IsoList.RefreshIsoList();
	}

	private void OnDelClick()
	{
		s_IsoToDelete = m_FilePath;
		VCEMsgBox.Show(VCEMsgBoxType.DELETE_ISO);
	}
}
