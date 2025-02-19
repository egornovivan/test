using System;
using System.Collections.Generic;
using UnityEngine;

public class VCEUIMatWnd : MonoBehaviour
{
	public GameObject m_Window;

	public VCMaterial m_Target;

	private VCMaterial m_TempMaterial;

	public UILabel m_TitleUI;

	public UILabel m_UIDUI;

	public UIInput m_NameUI;

	public UIPopupList m_MatterUI;

	public UICheckbox m_CustomizeUI;

	public List<UILabel> m_CustomizeLabels;

	public GameObject m_CustomizePropertyMask;

	public UISlider m_BumpStrengthUI;

	public UISlider m_SpecStrengthUI;

	public UISlider m_SpecPowerUI;

	public UISprite m_SpecColorUI;

	public UISprite m_EmsvColorUI;

	public UISlider m_TileUI;

	public UILabel m_BumpStrengthValueUI;

	public UILabel m_SpecStrengthValueUI;

	public UILabel m_SpecPowerValueUI;

	public UILabel m_TileValueUI;

	public VCEUIColorPickWnd m_SpecColorWnd;

	public VCEUIColorPickWnd m_EmsvColorWnd;

	public Transform m_ColorWndParent;

	public UITexture m_DiffuseMapUI;

	public UITexture m_BumpMapUI;

	public UIInput m_DiffusePathUI;

	public UIInput m_BumpPathUI;

	public GameObject m_CreateAsNewBtn;

	public GameObject m_OKBtn;

	public UILabel m_ErrUI;

	public UILabel m_OKUI;

	public Material m_UITextureSrcMat;

	private Material m_DiffuseMat;

	private Material m_BumpMat;

	private Texture2D m_DiffuseTex;

	private Texture2D m_BumpTex;

	private VCMatterInfo m_SelectedMatter;

	private string lastDiffPath = string.Empty;

	private string lastBumpPath = string.Empty;

	private bool loadedDiff;

	private bool loadedBump;

	private void Start()
	{
	}

	public bool WindowVisible()
	{
		return m_Window.activeInHierarchy;
	}

	public void ShowWindow(VCMaterial target)
	{
		m_Window.SetActive(value: true);
		Reset(target);
		Update();
	}

	public void HideWindow()
	{
		Reset(null);
		m_Window.SetActive(value: false);
	}

	private void ResetMaterial()
	{
		if (m_TempMaterial != null)
		{
			m_TempMaterial.Destroy();
			m_TempMaterial = null;
		}
		m_TempMaterial = new VCMaterial();
	}

	private void InitMatterList()
	{
		m_MatterUI.items.Clear();
		foreach (KeyValuePair<int, VCMatterInfo> s_Matter in VCConfig.s_Matters)
		{
			m_MatterUI.items.Add(VCUtils.Capital(s_Matter.Value.Name, everyword: true).ToLocalizationString());
		}
	}

	private void OnDestroy()
	{
		if (m_DiffuseMat != null)
		{
			UnityEngine.Object.Destroy(m_DiffuseMat);
			m_DiffuseMat = null;
		}
		if (m_BumpMat != null)
		{
			UnityEngine.Object.Destroy(m_BumpMat);
			m_BumpMat = null;
		}
		if (m_DiffuseTex != null)
		{
			UnityEngine.Object.Destroy(m_DiffuseTex);
			m_DiffuseTex = null;
		}
		if (m_BumpTex != null)
		{
			UnityEngine.Object.Destroy(m_BumpTex);
			m_BumpTex = null;
		}
		if (m_TempMaterial != null)
		{
			m_TempMaterial.Destroy();
			m_TempMaterial = null;
		}
	}

	private void Update()
	{
		if (WindowVisible())
		{
			m_TitleUI.text = ((m_Target != null) ? "EDIT MATERIAL".ToLocalizationString() : "NEW MATERIAL".ToLocalizationString());
			m_UIDUI.text = ((m_TempMaterial != null) ? m_TempMaterial.GUIDString : string.Empty);
			UpdateValue();
		}
	}

	private void QuerySelectedMatter()
	{
		m_SelectedMatter = null;
		foreach (KeyValuePair<int, VCMatterInfo> s_Matter in VCConfig.s_Matters)
		{
			if (m_MatterUI.selection.ToLower() == s_Matter.Value.Name.ToLower())
			{
				m_SelectedMatter = s_Matter.Value;
				break;
			}
		}
	}

	private void UpdateValue()
	{
		UpdateCustomize();
		UpdateProperties();
		UpdateTextures();
		UpdateButtons();
		UpdateTempMaterial();
	}

	private void UpdateCustomize()
	{
		m_CustomizePropertyMask.SetActive(!m_CustomizeUI.isChecked);
		Color color = ((!m_CustomizeUI.isChecked) ? new Color(0.5f, 0.5f, 0.5f, 0.5f) : Color.white);
		m_BumpStrengthUI.foreground.GetComponent<UISprite>().color = color;
		m_SpecStrengthUI.foreground.GetComponent<UISprite>().color = color;
		m_SpecPowerUI.foreground.GetComponent<UISprite>().color = color;
		m_TileUI.foreground.GetComponent<UISprite>().color = color;
		foreach (UILabel customizeLabel in m_CustomizeLabels)
		{
			customizeLabel.color = color;
		}
		if (!m_CustomizeUI.isChecked)
		{
			loadedDiff = true;
			loadedBump = true;
		}
	}

	private void UpdateProperties()
	{
		m_BumpStrengthValueUI.text = (m_BumpStrengthUI.sliderValue * 100f).ToString("0") + "%";
		m_SpecStrengthValueUI.text = (m_SpecStrengthUI.sliderValue * 200f).ToString("0") + "%";
		m_SpecPowerValueUI.text = Mathf.RoundToInt(m_SpecPowerUI.sliderValue * 254f + 1f).ToString("0");
		if (m_SpecColorWnd != null && m_CustomizeUI.isChecked)
		{
			m_SpecColorUI.color = m_SpecColorWnd.m_ColorPick.FinalColor;
		}
		if (m_EmsvColorWnd != null && m_CustomizeUI.isChecked)
		{
			m_EmsvColorUI.color = m_EmsvColorWnd.m_ColorPick.FinalColor;
		}
		m_TileValueUI.text = (m_TileUI.sliderValue * 3.75f + 0.25f).ToString("0.00");
	}

	private void UpdateButtons()
	{
		string err = string.Empty;
		if (CheckForm(out err))
		{
			m_ErrUI.color = Color.yellow;
			if (CheckExist(out err))
			{
				m_CreateAsNewBtn.SetActive(value: false);
				m_OKBtn.SetActive(value: false);
			}
			else
			{
				m_CreateAsNewBtn.SetActive(m_Target != null);
				m_OKBtn.SetActive(value: true);
			}
			m_ErrUI.text = err.ToLocalizationString();
		}
		else
		{
			m_ErrUI.color = Color.red;
			m_ErrUI.text = err.ToLocalizationString();
			m_CreateAsNewBtn.SetActive(value: false);
			m_OKBtn.SetActive(value: false);
		}
	}

	private void UpdateTempMaterial()
	{
		m_TempMaterial.m_Name = m_NameUI.text.Trim();
		m_TempMaterial.m_MatterId = m_SelectedMatter.ItemIndex;
		m_TempMaterial.ItemId = m_SelectedMatter.ItemId;
		m_TempMaterial.m_UseDefault = !m_CustomizeUI.isChecked;
		if (!m_TempMaterial.m_UseDefault)
		{
			m_TempMaterial.m_BumpStrength = m_BumpStrengthUI.sliderValue;
			m_TempMaterial.m_SpecularStrength = m_SpecStrengthUI.sliderValue * 2f;
			m_TempMaterial.m_SpecularPower = Mathf.Round(m_SpecPowerUI.sliderValue * 254f + 1f);
			m_TempMaterial.m_SpecularColor = m_SpecColorUI.color;
			m_TempMaterial.m_EmissiveColor = m_EmsvColorUI.color;
			m_TempMaterial.m_Tile = m_TileUI.sliderValue * 3.75f + 0.25f;
		}
		m_TempMaterial.CalcGUID();
	}

	private void UpdateTextures()
	{
		if (m_DiffusePathUI.text.Length > 0)
		{
			string text = m_DiffusePathUI.text.Replace("\\", "/");
			if (m_DiffusePathUI.text != text)
			{
				m_DiffusePathUI.text = text;
			}
		}
		if (m_BumpPathUI.text.Length > 0)
		{
			string text2 = m_BumpPathUI.text.Replace("\\", "/");
			if (m_BumpPathUI.text != text2)
			{
				m_BumpPathUI.text = text2;
			}
		}
		if (!m_DiffusePathUI.selected)
		{
			lastDiffPath = string.Empty;
			m_DiffusePathUI.text = string.Empty;
		}
		if (!m_BumpPathUI.selected)
		{
			lastBumpPath = string.Empty;
			m_BumpPathUI.text = string.Empty;
		}
		if (lastDiffPath != m_DiffusePathUI.text)
		{
			lastDiffPath = m_DiffusePathUI.text;
			if (lastDiffPath.Length > 0)
			{
				loadedDiff = false;
				if (m_DiffuseTex != null)
				{
					UnityEngine.Object.Destroy(m_DiffuseTex);
				}
				m_DiffuseTex = VCUtils.LoadTextureFromFile(m_DiffusePathUI.text);
				if (m_DiffuseTex == null)
				{
					m_DiffuseTex = UnityEngine.Object.Instantiate(Resources.Load(VCMaterial.s_BlankDiffuseRes) as Texture2D);
					if (!loadedBump)
					{
						m_BumpTex = UnityEngine.Object.Instantiate(Resources.Load(VCMaterial.s_BlankBumpRes) as Texture2D);
						m_BumpMat.mainTexture = m_BumpTex;
					}
				}
				else
				{
					loadedDiff = true;
					if (!loadedBump)
					{
						m_BumpTex = UnityEngine.Object.Instantiate(Resources.Load(VCMaterial.s_BlankBumpRes_) as Texture2D);
						m_BumpMat.mainTexture = m_BumpTex;
					}
				}
				m_DiffuseMat.mainTexture = m_DiffuseTex;
				m_TempMaterial.m_DiffuseData = m_DiffuseTex.EncodeToPNG();
				m_TempMaterial.m_BumpData = m_BumpTex.EncodeToPNG();
				m_TempMaterial.CalcGUID();
			}
		}
		if (!(lastBumpPath != m_BumpPathUI.text))
		{
			return;
		}
		lastBumpPath = m_BumpPathUI.text;
		if (lastBumpPath.Length <= 0)
		{
			return;
		}
		loadedBump = false;
		if (m_BumpTex != null)
		{
			UnityEngine.Object.Destroy(m_BumpTex);
		}
		m_BumpTex = VCUtils.LoadTextureFromFile(m_BumpPathUI.text);
		if (m_BumpTex == null)
		{
			if (loadedDiff)
			{
				m_BumpTex = UnityEngine.Object.Instantiate(Resources.Load(VCMaterial.s_BlankBumpRes_) as Texture2D);
			}
			else
			{
				m_BumpTex = UnityEngine.Object.Instantiate(Resources.Load(VCMaterial.s_BlankBumpRes) as Texture2D);
			}
		}
		else
		{
			loadedBump = true;
		}
		m_BumpMat.mainTexture = m_BumpTex;
		m_TempMaterial.m_DiffuseData = m_DiffuseTex.EncodeToPNG();
		m_TempMaterial.m_BumpData = m_BumpTex.EncodeToPNG();
		m_TempMaterial.CalcGUID();
	}

	private void SetDefaultProperties()
	{
		if (m_SelectedMatter != null)
		{
			m_BumpStrengthUI.sliderValue = m_SelectedMatter.DefaultBumpStrength;
			m_SpecStrengthUI.sliderValue = m_SelectedMatter.DefaultSpecularStrength * 0.5f;
			m_SpecPowerUI.sliderValue = (m_SelectedMatter.DefaultSpecularPower - 1f) / 254f;
			m_SpecColorUI.color = m_SelectedMatter.DefaultSpecularColor;
			m_EmsvColorUI.color = m_SelectedMatter.DefaultEmissiveColor;
			m_TileUI.sliderValue = (m_SelectedMatter.DefaultTile - 0.25f) / 3.75f;
			if (m_DiffuseTex != null)
			{
				UnityEngine.Object.Destroy(m_DiffuseTex);
			}
			m_DiffuseTex = UnityEngine.Object.Instantiate(Resources.Load(m_SelectedMatter.DefaultDiffuseRes) as Texture2D);
			if (m_BumpTex != null)
			{
				UnityEngine.Object.Destroy(m_BumpTex);
			}
			m_BumpTex = UnityEngine.Object.Instantiate(Resources.Load(m_SelectedMatter.DefaultBumpRes) as Texture2D);
			m_DiffuseMat.mainTexture = m_DiffuseTex;
			m_BumpMat.mainTexture = m_BumpTex;
			m_TempMaterial.m_DiffuseData = m_DiffuseTex.EncodeToPNG();
			m_TempMaterial.m_BumpData = m_BumpTex.EncodeToPNG();
			m_TempMaterial.CalcGUID();
		}
	}

	private void SetProperties(VCMaterial mat)
	{
		if (m_SelectedMatter != null)
		{
			m_BumpStrengthUI.sliderValue = mat.m_BumpStrength;
			m_SpecStrengthUI.sliderValue = mat.m_SpecularStrength * 0.5f;
			m_SpecPowerUI.sliderValue = (mat.m_SpecularPower - 1f) / 254f;
			m_SpecColorUI.color = mat.m_SpecularColor;
			m_EmsvColorUI.color = mat.m_EmissiveColor;
			m_TileUI.sliderValue = (mat.m_Tile - 0.25f) / 3.75f;
			if (m_DiffuseTex != null)
			{
				UnityEngine.Object.Destroy(m_DiffuseTex);
			}
			m_DiffuseTex = UnityEngine.Object.Instantiate(mat.m_DiffuseTex);
			if (m_BumpTex != null)
			{
				UnityEngine.Object.Destroy(m_BumpTex);
			}
			m_BumpTex = UnityEngine.Object.Instantiate(mat.m_BumpTex);
			m_DiffuseMat.mainTexture = m_DiffuseTex;
			m_BumpMat.mainTexture = m_BumpTex;
			m_TempMaterial.m_DiffuseData = new byte[mat.m_DiffuseData.Length];
			m_TempMaterial.m_BumpData = new byte[mat.m_BumpData.Length];
			Array.Copy(mat.m_DiffuseData, m_TempMaterial.m_DiffuseData, mat.m_DiffuseData.Length);
			Array.Copy(mat.m_BumpData, m_TempMaterial.m_BumpData, mat.m_BumpData.Length);
			m_TempMaterial.CalcGUID();
		}
	}

	public void Reset(VCMaterial target)
	{
		ResetMaterial();
		InitMatterList();
		if (m_DiffuseMat == null)
		{
			m_DiffuseMat = UnityEngine.Object.Instantiate(m_UITextureSrcMat);
			m_DiffuseMapUI.material = m_DiffuseMat;
		}
		if (m_BumpMat == null)
		{
			m_BumpMat = UnityEngine.Object.Instantiate(m_UITextureSrcMat);
			m_BumpMapUI.material = m_BumpMat;
		}
		m_Target = target;
		m_TitleUI.text = ((m_Target != null) ? "EDIT MATERIAL".ToLocalizationString() : "NEW MATERIAL".ToLocalizationString());
		m_OKUI.text = ((m_Target != null) ? "Apply".ToLocalizationString() : "Create".ToLocalizationString());
		if (m_Target != null)
		{
			m_UIDUI.text = target.GUIDString;
			m_NameUI.text = target.m_Name;
			m_MatterUI.selection = VCUtils.Capital(VCConfig.s_Matters[target.m_MatterId].Name, everyword: true);
			m_CustomizeUI.isChecked = !target.m_UseDefault;
			QuerySelectedMatter();
			if (!m_CustomizeUI.isChecked)
			{
				SetDefaultProperties();
			}
			else
			{
				SetProperties(target);
			}
		}
		else
		{
			m_UIDUI.text = "0000000000000000";
			m_CustomizeUI.isChecked = false;
			if (VCEditor.Instance.m_UI.m_MatterPopupList.selection != "All".ToLocalizationString())
			{
				m_MatterUI.selection = VCEditor.Instance.m_UI.m_MatterPopupList.selection;
			}
			else
			{
				m_MatterUI.selection = m_MatterUI.items[0];
			}
			m_NameUI.text = "New Material".ToLocalizationString();
			QuerySelectedMatter();
			SetDefaultProperties();
		}
		Update();
	}

	private bool CheckForm(out string err)
	{
		err = string.Empty;
		if (m_NameUI.text.Length < 1)
		{
			err = "You must specify a material name".ToLocalizationString() + " !";
			return false;
		}
		if (m_NameUI.text.Length > 20)
		{
			err = "Material name is too long".ToLocalizationString() + " !";
			return false;
		}
		if (m_DiffuseTex.height > 128 || m_DiffuseTex.width > 128)
		{
			err = "Map size can not be larger than 128".ToLocalizationString() + " !";
			return false;
		}
		if (m_BumpTex.height > 128 || m_BumpTex.width > 128)
		{
			err = "Map size can not be larger than 128".ToLocalizationString() + " !";
			return false;
		}
		return true;
	}

	private bool CheckExist(out string warning)
	{
		warning = string.Empty;
		if (m_Target != null && m_Target.m_Guid == m_TempMaterial.m_Guid)
		{
			warning = string.Empty;
			return true;
		}
		if (VCEAssetMgr.GetMaterial(m_TempMaterial.m_Guid) != null)
		{
			warning = "The same material already exist".ToLocalizationString() + " !";
			return true;
		}
		return false;
	}

	private void Apply()
	{
		if (m_Target != null)
		{
			ulong num = 0uL;
			ulong num2 = 0uL;
			bool flag = VCEditor.s_Scene.m_IsoData.QueryMaterial(m_Target.m_Guid) != null;
			if (VCEAssetMgr.s_Materials.ContainsKey(m_Target.m_Guid))
			{
				num = m_Target.m_Guid;
				if (!VCEAssetMgr.DeleteMaterialDataFile(m_Target.m_Guid))
				{
					VCEMsgBox.Show(VCEMsgBoxType.MATERIAL_NOT_SAVED);
				}
				VCEAssetMgr.s_Materials.Remove(m_Target.m_Guid);
				m_Target.Import(m_TempMaterial.Export());
				num2 = m_Target.m_Guid;
				VCEAssetMgr.s_Materials.Add(m_Target.m_Guid, m_Target);
				if (!VCEAssetMgr.CreateMaterialDataFile(m_Target))
				{
					VCEMsgBox.Show(VCEMsgBoxType.MATERIAL_NOT_SAVED);
				}
			}
			else if (VCEAssetMgr.s_TempMaterials.ContainsKey(m_Target.m_Guid))
			{
				num = m_Target.m_Guid;
				VCEAssetMgr.s_TempMaterials.Remove(m_Target.m_Guid);
				m_Target.Import(m_TempMaterial.Export());
				num2 = m_Target.m_Guid;
				VCEAssetMgr.s_TempMaterials.Add(m_Target.m_Guid, m_Target);
			}
			else
			{
				Debug.LogError("What the hell is that ?!");
				num = m_Target.m_Guid;
				m_Target.Import(m_TempMaterial.Export());
				num2 = m_Target.m_Guid;
				VCEAssetMgr.s_Materials.Add(m_Target.m_Guid, m_Target);
				if (!VCEAssetMgr.CreateMaterialDataFile(m_Target))
				{
					VCEMsgBox.Show(VCEMsgBoxType.MATERIAL_NOT_SAVED);
				}
			}
			VCEAlterMaterialMap.MatChange(num, num2);
			if (flag)
			{
				VCEditor.s_Scene.GenerateIsoMat();
			}
			VCEditor.SelectedMaterial = m_Target;
			VCEditor.Instance.m_UI.m_MaterialList.RefreshMaterialListThenFocusOnSelected();
			VCEditor.SelectedMaterial = m_Target;
		}
		else
		{
			Debug.LogError("No target material, Create new material instead !");
			CreateAsNew();
		}
	}

	private void CreateAsNew()
	{
		VCMaterial vCMaterial = new VCMaterial();
		vCMaterial.Import(m_TempMaterial.Export());
		VCEAssetMgr.s_Materials.Add(vCMaterial.m_Guid, vCMaterial);
		if (!VCEAssetMgr.CreateMaterialDataFile(vCMaterial))
		{
			VCEMsgBox.Show(VCEMsgBoxType.MATERIAL_NOT_SAVED);
		}
		VCEditor.SelectedMaterial = vCMaterial;
		VCEditor.Instance.m_UI.m_MaterialList.RefreshMaterialListThenFocusOnSelected();
		VCEditor.SelectedMaterial = vCMaterial;
		Reset(vCMaterial);
		VCEStatusBar.ShowText("New Material".ToLocalizationString() + " [" + vCMaterial.m_Name + "]", 6f, typeeffect: true);
	}

	private void OnSpecColorClick()
	{
		m_SpecColorWnd = VCEditor.Instance.m_UI.ShowColorPickWindow(m_ColorWndParent, new Vector3(405f, 0f, 0f), m_SpecColorUI.color);
	}

	private void OnEmsvColorClick()
	{
		m_EmsvColorWnd = VCEditor.Instance.m_UI.ShowColorPickWindow(m_ColorWndParent, new Vector3(405f, 0f, 0f), m_EmsvColorUI.color);
	}

	private void OnUseDefault(bool active)
	{
		if (!active)
		{
			SetDefaultProperties();
		}
	}

	private void OnMatterChange()
	{
		QuerySelectedMatter();
		if (!m_CustomizeUI.isChecked)
		{
			SetDefaultProperties();
		}
	}

	private void OnCreateAsNewClick()
	{
		string err = string.Empty;
		if (CheckForm(out err) && !CheckExist(out err))
		{
			CreateAsNew();
		}
	}

	private void OnOKClick()
	{
		string err = string.Empty;
		if (CheckForm(out err) && !CheckExist(out err))
		{
			if (m_Target == null)
			{
				CreateAsNew();
			}
			else
			{
				Apply();
			}
		}
	}

	private void OnCloseClick()
	{
		HideWindow();
	}
}
