public static class VCEMsgBoxResponse
{
	public static void Response(VCEMsgBoxType type, VCEMsgBoxButton button, int frameindex)
	{
		switch (type)
		{
		case VCEMsgBoxType.CLOSE_QUERY:
			switch (button)
			{
			case VCEMsgBoxButton.L:
				VCEditor.Instance.m_UI.OnSaveClick();
				break;
			case VCEMsgBoxButton.C:
				VCEditor.Quit();
				break;
			}
			break;
		case VCEMsgBoxType.CLOSE_QUERY_NOSAVE:
			if (button == VCEMsgBoxButton.L)
			{
				VCEditor.Quit();
			}
			break;
		case VCEMsgBoxType.SWITCH_QUERY:
			switch (button)
			{
			case VCEMsgBoxButton.L:
				VCEditor.Instance.m_UI.OnSaveClick();
				break;
			case VCEMsgBoxButton.R:
				VCEUISceneMenuItem.DoCreateSceneFromMsgBox();
				break;
			}
			VCEUISceneMenuItem.s_SceneToCreate = null;
			break;
		case VCEMsgBoxType.LOAD_QUERY:
			switch (button)
			{
			case VCEMsgBoxButton.L:
				VCEditor.Instance.m_UI.OnSaveClick();
				break;
			case VCEMsgBoxButton.R:
				VCEUIIsoItem.DoLoadFromMsgBox();
				break;
			}
			VCEUIIsoItem.s_IsoToLoad = string.Empty;
			break;
		case VCEMsgBoxType.MATERIAL_DEL_QUERY:
			if (button == VCEMsgBoxButton.L)
			{
				VCEUIMaterialItem.DoDeleteFromMsgBox();
			}
			VCEUIMaterialItem.s_CurrentDelMat = null;
			break;
		case VCEMsgBoxType.DECAL_DEL_QUERY:
			if (button == VCEMsgBoxButton.L)
			{
				VCEUIDecalItem.DoDeleteFromMsgBox();
			}
			VCEUIDecalItem.s_CurrentDelDecal = null;
			break;
		case VCEMsgBoxType.DELETE_ISO:
			if (button == VCEMsgBoxButton.L)
			{
				VCEUIIsoItem.DoDeleteFromMsgBox();
			}
			VCEUIIsoItem.s_IsoToDelete = string.Empty;
			break;
		case VCEMsgBoxType.MISSING_ISO:
			break;
		case VCEMsgBoxType.CORRUPT_ISO:
			break;
		case VCEMsgBoxType.CANNOT_SAVE_NONAME:
			break;
		case VCEMsgBoxType.REPLACE_QUERY:
			if (button == VCEMsgBoxButton.L)
			{
				VCEUISaveWnd.DoSaveForOverwrite();
			}
			VCEUISaveWnd.s_SaveTargetForOverwrite = string.Empty;
			break;
		case VCEMsgBoxType.SAVE_OK:
			break;
		case VCEMsgBoxType.SAVE_FAILED:
			break;
		case VCEMsgBoxType.ISO_INCOMPLETE:
			break;
		case VCEMsgBoxType.ISO_INVALID:
			break;
		case VCEMsgBoxType.EXPORT_OK:
			VCEditor.Instance.m_UI.m_ExportWindow.Hide();
			break;
		case VCEMsgBoxType.EXPORT_NETWORK:
			VCEditor.Instance.m_UI.m_ExportWindow.Hide();
			break;
		case VCEMsgBoxType.EXPORT_FAILED:
			break;
		case VCEMsgBoxType.EXPORT_NOTSAVED:
			VCEditor.Instance.m_UI.OnSaveAsClick();
			break;
		case VCEMsgBoxType.EXPORT_FULL:
			break;
		case VCEMsgBoxType.CANNOT_EXPORT_NOW:
			break;
		case VCEMsgBoxType.CANNOT_EXTRUDE:
			break;
		case VCEMsgBoxType.MATERIAL_NOT_SAVED:
		case VCEMsgBoxType.DECAL_NOT_SAVED:
		case VCEMsgBoxType.EXPORT_NETWORK_FAILED:
		case VCEMsgBoxType.EXPORT_EMPTY_NAME:
		case VCEMsgBoxType.EXPORT_EXISTNAME:
			break;
		}
	}
}
