using UnityEngine;

public class VCEUISelectedItem : MonoBehaviour
{
	public GameObject m_UIGroup;

	public UISprite m_ComponentIcon;

	public UITexture m_MaterialIcon;

	public UISprite m_ColorIcon;

	public UILabel m_ItemNameLabel;

	public UITweener m_GlowIcon;

	public GameObject m_IconMask;

	public Material m_SourceIconTexture;

	private Material m_MaterialIconTexture;

	private VCMaterial m_lastMat;

	private VCPartInfo m_lastPart;

	private VCDecalAsset m_lastDecal;

	private Color m_lastColor = Color.clear;

	private void Start()
	{
		m_MaterialIconTexture = Object.Instantiate(m_SourceIconTexture);
	}

	private void Update()
	{
		if (!VCEditor.DocumentOpen())
		{
			return;
		}
		if (VCEditor.Instance.m_UI.m_MaterialTab.isChecked)
		{
			if (VCEditor.SelectedMaterial != null)
			{
				if (m_lastMat != VCEditor.SelectedMaterial)
				{
					m_UIGroup.SetActive(value: true);
					m_MaterialIconTexture.mainTexture = VCEditor.SelectedMaterial.m_DiffuseTex;
					m_MaterialIcon.material = m_MaterialIconTexture;
					m_IconMask.GetComponent<UITweener>().Reset();
					m_IconMask.GetComponent<UITweener>().Play(forward: true);
					m_GlowIcon.Reset();
					m_GlowIcon.Play(forward: true);
					m_MaterialIcon.gameObject.SetActive(value: true);
					m_ComponentIcon.gameObject.SetActive(value: false);
					m_ColorIcon.gameObject.SetActive(value: false);
					m_ItemNameLabel.text = VCEditor.SelectedMaterial.m_Name;
					m_IconMask.SetActive(value: true);
				}
			}
			else
			{
				m_UIGroup.SetActive(value: false);
			}
			m_lastMat = VCEditor.SelectedMaterial;
			m_lastDecal = null;
			m_lastPart = null;
			m_lastColor = Color.clear;
		}
		else if (VCEditor.Instance.m_UI.m_DecalTab.isChecked)
		{
			if (VCEditor.SelectedDecal != null)
			{
				if (m_lastDecal != VCEditor.SelectedDecal)
				{
					m_UIGroup.SetActive(value: true);
					m_MaterialIconTexture.mainTexture = VCEditor.SelectedDecal.m_Tex;
					m_MaterialIcon.material = m_MaterialIconTexture;
					m_IconMask.GetComponent<UITweener>().Reset();
					m_IconMask.GetComponent<UITweener>().Play(forward: true);
					m_GlowIcon.Reset();
					m_GlowIcon.Play(forward: true);
					m_MaterialIcon.gameObject.SetActive(value: true);
					m_ComponentIcon.gameObject.SetActive(value: false);
					m_ColorIcon.gameObject.SetActive(value: false);
					m_ItemNameLabel.text = "1 " + "decal".ToLocalizationString() + "  [999999](UID = " + VCEditor.SelectedDecal.GUIDString + ")[-]";
					m_IconMask.SetActive(value: true);
				}
			}
			else
			{
				m_UIGroup.SetActive(value: false);
			}
			m_lastDecal = VCEditor.SelectedDecal;
			m_lastMat = null;
			m_lastPart = null;
			m_lastColor = Color.clear;
		}
		else if (VCEditor.Instance.m_UI.m_PartTab.isChecked)
		{
			if (VCEditor.SelectedPart != null)
			{
				if (m_lastPart != VCEditor.SelectedPart)
				{
					m_ComponentIcon.spriteName = VCEditor.SelectedPart.m_IconPath.Split(',')[0];
					m_IconMask.GetComponent<UITweener>().Reset();
					m_IconMask.GetComponent<UITweener>().Play(forward: true);
					m_GlowIcon.Reset();
					m_GlowIcon.Play(forward: true);
					m_ComponentIcon.gameObject.SetActive(value: true);
					m_UIGroup.SetActive(value: true);
					m_MaterialIcon.gameObject.SetActive(value: false);
					m_ColorIcon.gameObject.SetActive(value: false);
					m_ItemNameLabel.text = VCEditor.SelectedPart.m_Name;
					m_IconMask.SetActive(value: true);
				}
			}
			else
			{
				m_UIGroup.SetActive(value: false);
			}
			m_lastMat = null;
			m_lastDecal = null;
			m_lastPart = VCEditor.SelectedPart;
			m_lastColor = Color.clear;
		}
		else if (VCEditor.Instance.m_UI.m_PaintTab.isChecked)
		{
			Color color = VCEditor.SelectedColor;
			color.a = 1f;
			m_ColorIcon.color = color;
			if (m_lastColor != color)
			{
				m_IconMask.GetComponent<UITweener>().Reset();
				m_IconMask.GetComponent<UITweener>().Play(forward: true);
				m_GlowIcon.Reset();
				m_GlowIcon.Play(forward: true);
			}
			m_ColorIcon.gameObject.SetActive(value: true);
			m_UIGroup.SetActive(value: true);
			m_MaterialIcon.gameObject.SetActive(value: false);
			m_ComponentIcon.gameObject.SetActive(value: false);
			m_ItemNameLabel.text = "RGB ( " + (color.r * 100f).ToString("0") + "%, " + (color.g * 100f).ToString("0") + "%, " + (color.b * 100f).ToString("0") + "% )";
			m_IconMask.SetActive(value: true);
			m_lastMat = null;
			m_lastDecal = null;
			m_lastPart = null;
			m_lastColor = color;
		}
		else
		{
			m_UIGroup.SetActive(value: false);
			m_lastMat = null;
			m_lastDecal = null;
			m_lastPart = null;
			m_lastColor = Color.clear;
		}
	}

	private void OnSelectedMatClick()
	{
		if (VCEditor.Instance.m_UI.m_MaterialTab.isChecked && VCEditor.SelectedMaterial != null)
		{
			VCEditor.DeselectBrushes();
			VCEditor.Instance.m_UI.m_MaterialWindow.ShowWindow(VCEditor.SelectedMaterial);
		}
	}
}
