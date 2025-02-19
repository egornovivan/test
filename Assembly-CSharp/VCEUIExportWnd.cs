using UnityEngine;

public class VCEUIExportWnd : MonoBehaviour
{
	public VCEUIStatisticsPanel m_StatPanel;

	public UILabel m_ErrorLabel;

	public UIButton m_ExportButton;

	public UIInput m_InputField;

	public void Init()
	{
		m_StatPanel.Init();
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
		VCEditor.Instance.m_MirrorGroup.SetActive(value: false);
		VCEditor.Instance.m_GLGroup.SetActive(value: false);
		m_StatPanel.OnCreationInfoRefresh();
		m_StatPanel.SetIsoIcon();
	}

	public void Hide()
	{
		VCEditor.Instance.m_MirrorGroup.SetActive(value: true);
		VCEditor.Instance.m_GLGroup.SetActive(value: true);
		base.gameObject.SetActive(value: false);
		m_InputField.gameObject.SetActive(value: false);
		m_InputField.text = string.Empty;
	}

	public void OnCancelClick()
	{
		Hide();
	}

	public void OnExportClick()
	{
		int num = VCEditor.MakeCreation();
		if (num != -4 && !VCEditor.s_MultiplayerMode)
		{
			VCEMsgBox.Show((num != 0) ? VCEMsgBoxType.EXPORT_FAILED : VCEMsgBoxType.EXPORT_OK);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt))
		{
			m_InputField.gameObject.SetActive(!m_InputField.gameObject.activeInHierarchy);
		}
		if (!VCEditor.s_ConnectedToGame)
		{
			m_ErrorLabel.text = "Export cannot be done outside the game".ToLocalizationString() + "!";
			m_ExportButton.gameObject.SetActive(value: false);
		}
		else if (VCEditor.s_Scene.m_CreationAttr.m_Type == ECreation.Null || VCEditor.s_Scene.m_CreationAttr.m_Errors.Count > 0)
		{
			m_ErrorLabel.text = "Your creation has some errors".ToLocalizationString() + "!";
			m_ExportButton.gameObject.SetActive(value: false);
		}
		else if (!m_StatPanel.m_CostList.IsEnough)
		{
			m_ErrorLabel.text = "Not enough items".ToLocalizationString() + "!";
			m_ExportButton.gameObject.SetActive(value: false);
		}
		else if (GameUI.Instance != null && GameUI.Instance.mSkillWndCtrl != null && GameUI.Instance.mSkillWndCtrl._SkillMgr != null)
		{
			if (!GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckUnlockCusProduct((int)VCEditor.s_Scene.m_IsoData.m_HeadInfo.Category))
			{
				m_ErrorLabel.text = "You need to unlock the skill first".ToLocalizationString() + "!";
				m_ExportButton.gameObject.SetActive(value: false);
			}
			else
			{
				m_ErrorLabel.text = string.Empty;
				m_ExportButton.gameObject.SetActive(value: true);
			}
		}
		else
		{
			m_ErrorLabel.text = string.Empty;
			m_ExportButton.gameObject.SetActive(value: true);
		}
	}

	private void OnInputSubmit()
	{
		if (m_InputField.text.ToLower() == "show me the item")
		{
			VCEditor.Instance.m_CheatWhenMakeCreation = !VCEditor.Instance.m_CheatWhenMakeCreation;
		}
		m_InputField.text = string.Empty;
	}
}
