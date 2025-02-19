using UnityEngine;

public class UINpcSpeech : MonoBehaviour
{
	[SerializeField]
	private UINpcSpeechBox speechUI;

	public UINpcSpeechBoxInterpreter speechInterpreter;

	public bool isShow => speechUI.isShow;

	public bool isChoice => speechUI.IsChoice;

	public void Show()
	{
		speechUI.Show();
	}

	public void Hide()
	{
		speechUI.Hide();
	}

	public void Init()
	{
		speechInterpreter.Init();
	}

	public void Close()
	{
		speechInterpreter.Init();
	}

	private void OnDestroy()
	{
	}
}
