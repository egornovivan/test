using System.IO;
using UnityEngine;

public class VCEUIIsoHeaderInspector : VCEUIInspector
{
	public GameObject m_HeaderGroup;

	public UILabel m_ErrorMessageLabel;

	public UILabel m_NameLabel;

	public UILabel m_CategoryLabel;

	public UILabel m_SizeLabel;

	public UITexture m_IconTexture;

	public UILabel m_DescriptionLabel;

	public UILabel m_FileSizeLabel;

	public Material m_IconMatRes;

	private Material m_IconMat;

	private Texture2D m_IconTex;

	private string m_FilePath = string.Empty;

	public string FilePath
	{
		get
		{
			return m_FilePath;
		}
		set
		{
			if (m_FilePath != value)
			{
				m_FilePath = value;
				Refresh();
			}
		}
	}

	private void OnDestroy()
	{
		DestroyIconMat();
	}

	private void CreateIconMat(byte[] texbuf)
	{
		m_IconMat = Object.Instantiate(m_IconMatRes);
		m_IconTex = new Texture2D(2, 2);
		m_IconTex.LoadImage(texbuf);
		m_IconMat.mainTexture = m_IconTex;
		m_IconTexture.material = m_IconMat;
	}

	private void DestroyIconMat()
	{
		m_IconTexture.material = null;
		if (m_IconMat != null)
		{
			Object.Destroy(m_IconMat);
			m_IconMat = null;
		}
		if (m_IconTex != null)
		{
			Object.Destroy(m_IconTex);
			m_IconTex = null;
		}
	}

	private void Refresh()
	{
		if (m_FilePath.Length < 1)
		{
			m_HeaderGroup.SetActive(value: false);
			m_ErrorMessageLabel.gameObject.SetActive(value: false);
			DestroyIconMat();
			return;
		}
		VCIsoHeadData iso_header;
		int num = VCIsoData.ExtractHeader(m_FilePath, out iso_header);
		if (num > 0)
		{
			m_ErrorMessageLabel.gameObject.SetActive(value: false);
			m_NameLabel.color = new Color(1f, 1f, 1f, 1f);
			m_NameLabel.text = iso_header.Name;
			if (iso_header.Name.Trim().Length < 1)
			{
				m_NameLabel.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
				m_NameLabel.text = "< No name >".ToLocalizationString();
			}
			m_CategoryLabel.text = VCConfig.s_Categories[iso_header.Category].m_Name.ToLocalizationString();
			m_SizeLabel.text = "Width".ToLocalizationString() + ": " + iso_header.xSize + "\r\n" + "Depth".ToLocalizationString() + ": " + iso_header.zSize + "\r\n" + "Height".ToLocalizationString() + ": " + iso_header.ySize;
			m_DescriptionLabel.color = new Color(1f, 1f, 1f, 1f);
			m_DescriptionLabel.text = iso_header.Desc;
			if (iso_header.Desc.Trim().Length < 1)
			{
				m_DescriptionLabel.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
				m_DescriptionLabel.text = "< No description >".ToLocalizationString();
			}
			string text = "v";
			text += (iso_header.Version >> 24) & 0xFF;
			text += ".";
			text += (iso_header.Version >> 16) & 0xFF;
			m_FileSizeLabel.text = string.Empty;
			if (iso_header.Author != null && iso_header.Author.Trim().Length > 0)
			{
				m_FileSizeLabel.text = "Author".ToLocalizationString() + ": [00FFFF]" + iso_header.Author.Trim() + "[-]\r\n";
			}
			string text2;
			if (iso_header.Version > 33751041)
			{
				UILabel fileSizeLabel = m_FileSizeLabel;
				text2 = fileSizeLabel.text;
				fileSizeLabel.text = text2 + "ISO " + "Version".ToLocalizationString() + ": [FF0000]" + text + "[-]\r\n";
			}
			else
			{
				UILabel fileSizeLabel2 = m_FileSizeLabel;
				text2 = fileSizeLabel2.text;
				fileSizeLabel2.text = text2 + "ISO " + "Version".ToLocalizationString() + ": [00FFFF]" + text + "[-]\r\n";
			}
			UILabel fileSizeLabel3 = m_FileSizeLabel;
			text2 = fileSizeLabel3.text;
			fileSizeLabel3.text = text2 + "File Size".ToLocalizationString() + ": [00FFFF]" + (num >> 10).ToString("#,##0") + " KB[-]";
			CreateIconMat(iso_header.IconTex);
			m_HeaderGroup.SetActive(value: true);
			return;
		}
		m_HeaderGroup.SetActive(value: false);
		DestroyIconMat();
		VCEUIIsoItem selectedItem = VCEditor.Instance.m_UI.m_IsoList.m_SelectedItem;
		if (selectedItem != null)
		{
			FileInfo fileInfo = new FileInfo(selectedItem.m_FilePath);
			if (fileInfo.Extension == VCConfig.s_IsoFileExt)
			{
				m_ErrorMessageLabel.text = "Corrupt file data".ToLocalizationString();
			}
			else if (fileInfo.Extension == VCConfig.s_ObsoleteIsoFileExt)
			{
				m_ErrorMessageLabel.text = "Obsoleted ISO format".ToLocalizationString() + "\r\n\r\n[FF8080]" + "This ISO version is NOT compatible with the current build, please download the converter from our website".ToLocalizationString() + "![-]";
			}
			else
			{
				m_ErrorMessageLabel.text = "Unknown file format".ToLocalizationString();
			}
		}
		else
		{
			Debug.LogError("This cannot happen");
		}
		m_ErrorMessageLabel.gameObject.SetActive(value: true);
	}
}
