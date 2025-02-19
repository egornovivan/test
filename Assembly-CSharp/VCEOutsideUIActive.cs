using UnityEngine;

public class VCEOutsideUIActive : MonoBehaviour
{
	private bool lastVCEditorActive;

	public bool revertEnable;

	private void Start()
	{
		lastVCEditorActive = false;
	}

	private void LateUpdate()
	{
		if (VCEditor.s_Active && !lastVCEditorActive)
		{
			revertEnable = GetComponent<Camera>().enabled;
			GetComponent<Camera>().enabled = false;
			GetComponent<UICamera>().enabled = false;
		}
		if (!VCEditor.s_Active && lastVCEditorActive)
		{
			GetComponent<Camera>().enabled = revertEnable;
			GetComponent<UICamera>().enabled = revertEnable;
		}
		lastVCEditorActive = VCEditor.s_Active;
	}
}
