using System;
using System.IO;
using UnityEngine;

public class VCEUISaveWnd : MonoBehaviour
{
	public VCIsoData m_Iso;

	public UIInput m_NameInput;

	public UIInput m_AuthorInput;

	public UILabel m_AuthorLabel;

	public UIInput m_DescriptionInput;

	public UIInput m_SaveLocInput;

	public UITexture m_IconUITex;

	public UITexture m_CaptureUITex;

	public UISprite m_ColorRect;

	public UIButton m_SaveBtn;

	public UIButton m_CancelBtn;

	public UILabel m_ErrorLabel;

	public Material m_MatRes;

	public Transform m_ColorWndParent;

	public Vector3 m_ColorWndPos;

	public Camera m_CaptureCamera;

	private Material m_IconMat;

	private Texture2D m_IconTex;

	private Material m_CaptureMat;

	private RenderTexture m_CaptureTex;

	private VCEUIColorPickWnd m_PopupColorWnd;

	private string m_last_input_name = string.Empty;

	public static string s_SaveTargetForOverwrite = string.Empty;

	public void Init()
	{
		m_IconMat = UnityEngine.Object.Instantiate(m_MatRes);
		m_CaptureMat = UnityEngine.Object.Instantiate(m_MatRes);
		m_IconUITex.material = m_IconMat;
		m_CaptureUITex.material = m_CaptureMat;
		m_CaptureCamera = VCEditor.Instance.m_CaptureCamera;
	}

	public void DestroyTex()
	{
		if (m_IconTex != null)
		{
			UnityEngine.Object.Destroy(m_IconTex);
			m_IconTex = null;
			if (m_IconMat != null)
			{
				m_IconMat.mainTexture = null;
			}
		}
		if (m_CaptureTex != null)
		{
			UnityEngine.Object.Destroy(m_CaptureTex);
			m_CaptureTex = null;
			if (m_CaptureMat != null)
			{
				m_CaptureMat.mainTexture = null;
			}
		}
	}

	private void OnDestroy()
	{
		DestroyTex();
		if (m_IconMat != null)
		{
			UnityEngine.Object.Destroy(m_IconMat);
			m_IconMat = null;
		}
		if (m_CaptureMat != null)
		{
			UnityEngine.Object.Destroy(m_CaptureMat);
			m_CaptureMat = null;
		}
	}

	public void Show(VCIsoData iso)
	{
		DestroyTex();
		m_Iso = iso;
		ReadIso();
		m_CaptureTex = new RenderTexture(256, 256, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
		m_CaptureMat.mainTexture = m_CaptureTex;
		m_CaptureCamera.targetTexture = m_CaptureTex;
		m_CaptureCamera.enabled = true;
		VCEditor.Instance.m_MainCamera.GetComponent<VCECamera>().ControlMode = 2;
		VCEditor.Instance.m_MirrorGroup.SetActive(value: false);
		VCEditor.Instance.m_GLGroup.SetActive(value: false);
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		VCEditor.Instance.m_MainCamera.GetComponent<VCECamera>().ControlMode = 1;
		VCEditor.Instance.m_MirrorGroup.SetActive(value: true);
		VCEditor.Instance.m_GLGroup.SetActive(value: true);
		base.gameObject.SetActive(value: false);
		m_Iso = null;
		m_CaptureCamera.enabled = false;
		m_CaptureCamera.targetTexture = null;
		DestroyTex();
		if (VCEditor.Instance.m_UI.m_ISOTab.isChecked)
		{
			VCEditor.Instance.m_UI.m_IsoList.RefreshIsoList();
		}
	}

	public void ReadIso()
	{
		if (m_Iso != null)
		{
			m_NameInput.text = m_Iso.m_HeadInfo.Name;
			if (m_Iso.m_HeadInfo.Author.Trim().Length > 0)
			{
				m_AuthorInput.gameObject.SetActive(value: false);
				m_AuthorInput.text = m_Iso.m_HeadInfo.Author.Trim();
				m_AuthorLabel.text = m_Iso.m_HeadInfo.Author.Trim();
			}
			else
			{
				m_AuthorInput.gameObject.SetActive(value: true);
				m_AuthorInput.text = string.Empty;
				m_AuthorLabel.text = string.Empty;
			}
			m_last_input_name = m_NameInput.text;
			m_DescriptionInput.text = m_Iso.m_HeadInfo.Desc;
			m_SaveLocInput.text = VCEditor.s_Scene.m_DocumentPath.Substring(0, VCEditor.s_Scene.m_DocumentPath.Length - VCConfig.s_IsoFileExt.Length);
			m_IconTex = new Texture2D(2, 2);
			m_IconTex.LoadImage(m_Iso.m_HeadInfo.IconTex);
			m_IconMat.mainTexture = m_IconTex;
			GuessFilepath();
		}
	}

	public void GuessFilepath()
	{
		string text = m_NameInput.text;
		if (text.Trim().Length < 1)
		{
			text = "Untitled";
		}
		string text2 = VCUtils.MakeFileName(text);
		string text3 = m_SaveLocInput.text;
		int num = Mathf.Max(text3.LastIndexOf("\\"), text3.LastIndexOf("/"));
		if (num >= 0)
		{
			m_SaveLocInput.text = text3.Substring(0, num + 1) + text2;
		}
		else
		{
			m_SaveLocInput.text = text2;
		}
		m_SaveLocInput.text = m_SaveLocInput.text.Trim();
		string text4 = m_SaveLocInput.text;
		int num2 = text4.LastIndexOf(" (");
		int num3 = text4.LastIndexOf(")");
		int num4 = 2;
		if (num2 > 0 && num3 == text4.Length - 1)
		{
			try
			{
				num4 = Convert.ToInt32(text4.Substring(num2 + 2, num3 - num2 - 2));
				text4 = text4.Substring(0, num2);
			}
			catch (Exception)
			{
				num4 = 2;
			}
		}
		while (File.Exists(VCConfig.s_IsoPath + m_SaveLocInput.text + VCConfig.s_IsoFileExt) && m_SaveLocInput.text + VCConfig.s_IsoFileExt != VCEditor.s_Scene.m_DocumentPath)
		{
			m_SaveLocInput.text = text4 + " (" + num4 + ")";
			num4++;
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F10))
		{
			OnCaptureClick();
		}
		if (m_last_input_name != m_NameInput.text)
		{
			m_last_input_name = m_NameInput.text;
			GuessFilepath();
		}
		if (m_PopupColorWnd != null)
		{
			m_ColorRect.color = m_PopupColorWnd.m_ColorPick.FinalColor;
		}
		string text = m_SaveLocInput.text.Trim();
		if (text.Length > 0)
		{
			if (text[0] == '/' || text[0] == '\\' || text[0] == '.')
			{
				m_SaveBtn.gameObject.SetActive(value: false);
				m_ErrorLabel.text = "Invalid file save path".ToLocalizationString() + " !";
			}
			else
			{
				FileInfo fileInfo = new FileInfo(VCConfig.s_IsoPath + m_SaveLocInput.text);
				string text2 = fileInfo.Name.Trim();
				if (text2.Length > 0)
				{
					if (text2[0] == '.')
					{
						m_SaveBtn.gameObject.SetActive(value: false);
						m_ErrorLabel.text = "Invalid file save path".ToLocalizationString() + " !";
					}
					else
					{
						m_SaveBtn.gameObject.SetActive(value: true);
						m_ErrorLabel.text = string.Empty;
					}
				}
				else
				{
					m_SaveBtn.gameObject.SetActive(value: false);
					m_ErrorLabel.text = "Invalid file save path".ToLocalizationString() + " !";
				}
			}
		}
		else
		{
			m_SaveBtn.gameObject.SetActive(value: false);
			m_ErrorLabel.text = "No save path specified".ToLocalizationString() + " !";
		}
		m_Iso.m_HeadInfo.Name = m_NameInput.text;
		m_Iso.m_HeadInfo.Desc = m_DescriptionInput.text;
		m_CaptureCamera.transform.position = VCEditor.Instance.m_MainCamera.transform.position;
		m_CaptureCamera.transform.rotation = VCEditor.Instance.m_MainCamera.transform.rotation;
		m_CaptureCamera.backgroundColor = m_ColorRect.color;
		m_CaptureCamera.aspect = 1f;
		m_CaptureCamera.nearClipPlane = VCEditor.Instance.m_MainCamera.nearClipPlane;
		m_CaptureCamera.farClipPlane = VCEditor.Instance.m_MainCamera.farClipPlane;
		m_CaptureCamera.fieldOfView = VCEditor.Instance.m_MainCamera.fieldOfView;
	}

	public void OnCancelClick()
	{
		Hide();
	}

	public static void DoSaveForOverwrite()
	{
		VCEMsgBox.Show((!VCEditor.s_Scene.SaveIsoAs(s_SaveTargetForOverwrite)) ? VCEMsgBoxType.SAVE_FAILED : VCEMsgBoxType.SAVE_OK);
	}

	public void OnSaveClick()
	{
		m_Iso.m_HeadInfo.Author = m_AuthorInput.text;
		string fileName = VCConfig.s_IsoPath + m_SaveLocInput.text + VCConfig.s_IsoFileExt;
		FileInfo fileInfo = new FileInfo(VCConfig.s_IsoPath + VCEditor.s_Scene.m_DocumentPath);
		FileInfo fileInfo2 = new FileInfo(fileName);
		bool flag = false;
		if (fileInfo.FullName.ToLower() == fileInfo2.FullName.ToLower())
		{
			VCEMsgBox.Show((!VCEditor.s_Scene.SaveIso()) ? VCEMsgBoxType.SAVE_FAILED : VCEMsgBoxType.SAVE_OK);
		}
		else if (File.Exists(fileInfo2.FullName))
		{
			s_SaveTargetForOverwrite = m_SaveLocInput.text + VCConfig.s_IsoFileExt;
			VCEMsgBox.Show(VCEMsgBoxType.REPLACE_QUERY);
		}
		else
		{
			VCEMsgBox.Show((!VCEditor.s_Scene.SaveIsoAs(m_SaveLocInput.text + VCConfig.s_IsoFileExt)) ? VCEMsgBoxType.SAVE_FAILED : VCEMsgBoxType.SAVE_OK);
		}
		Hide();
	}

	public void OnCaptureClick()
	{
		if (m_IconTex != null)
		{
			UnityEngine.Object.Destroy(m_IconTex);
			m_IconMat.mainTexture = null;
		}
		m_IconTex = new Texture2D(256, 256, TextureFormat.ARGB32, mipmap: true);
		m_IconMat.mainTexture = m_IconTex;
		RenderTexture.active = m_CaptureTex;
		m_IconTex.ReadPixels(new Rect(0f, 0f, 256f, 256f), 0, 0, recalculateMipMaps: true);
		m_IconTex.Apply();
		RenderTexture.active = null;
		m_IconTex.filterMode = FilterMode.Trilinear;
		m_Iso.m_HeadInfo.IconTex = m_IconTex.EncodeToJPG(25);
		VCEditor.Instance.m_UI.m_StatPanel.SetIsoIcon();
	}

	public void OnBGColorClick()
	{
		m_PopupColorWnd = VCEditor.Instance.m_UI.ShowColorPickWindow(m_ColorWndParent, m_ColorWndPos, m_ColorRect.color);
	}
}
