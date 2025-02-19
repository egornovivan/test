using UnityEngine;

public class UINpcDialog : MonoBehaviour
{
	[SerializeField]
	private UINpcDialogWnd dialogUI;

	public UINpcDialogWndInterpreter dialogInterpreter;

	public bool allowShow = true;

	public bool isShow => dialogUI.isShow;

	public void Show()
	{
		if (allowShow)
		{
			dialogUI.Show();
		}
	}

	public void Hide()
	{
		dialogUI.Hide();
	}

	public void Init()
	{
		dialogInterpreter.Init();
	}

	public void Close()
	{
		dialogInterpreter.Close();
	}

	private void OnDestroy()
	{
	}
}
