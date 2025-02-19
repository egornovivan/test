using UnityEngine;

public class UIMissionTrack : MonoBehaviour
{
	[SerializeField]
	private UIMissionTrackWnd missionUI;

	public UIMissionTrackWndInterpreter missionInterpreter;

	public bool isShow => missionUI.isShow;

	public void Show()
	{
		missionUI.Show();
	}

	public void Hide()
	{
		missionUI.Hide();
	}

	public void ChangeWindowShowState()
	{
		missionUI.ChangeWindowShowState();
	}

	public void Init()
	{
		missionInterpreter.Init();
	}

	public void Close()
	{
		missionInterpreter.Close();
	}

	private void OnDestroy()
	{
	}

	public UIBaseWnd GetBaseWnd()
	{
		return missionUI;
	}
}
