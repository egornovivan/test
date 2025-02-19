using UnityEngine;

public class GameUIMode : MonoBehaviour
{
	public enum UIMode
	{
		um_base,
		um_building,
		um_driving
	}

	private static GameUIMode mInstance;

	private bool _init;

	public static GameUIMode Instance => mInstance;

	public UIMode curUIMode { get; private set; }

	private void Awake()
	{
		mInstance = this;
		curUIMode = UIMode.um_base;
	}

	private void Start()
	{
	}

	private void Init()
	{
		if (!_init)
		{
			GameUI.Instance.mUIMainMidCtrl.onTweenFinished += OnMidCtrlTweenFinished;
			GameUI.Instance.mBuildBlock.onTweenFinished += OnBuildTweenFinished;
			_init = true;
		}
	}

	public void GotoBaseMode()
	{
		ChangeMode(UIMode.um_base);
	}

	public void GotoBuildMode()
	{
		ChangeMode(UIMode.um_building);
	}

	public void GotoVehicleMode()
	{
		ChangeMode(UIMode.um_driving);
	}

	private void ChangeMode(UIMode target)
	{
		Init();
		if (curUIMode != target)
		{
			if (curUIMode == UIMode.um_base)
			{
				GameUI.Instance.mUIMainMidCtrl.PlayTween(forward: true);
			}
			else if (curUIMode == UIMode.um_building)
			{
				GameUI.Instance.mBuildBlock.PlayTween(forward: false);
			}
			curUIMode = target;
		}
	}

	private void OnMidCtrlTweenFinished(bool forward)
	{
		if (forward && curUIMode == UIMode.um_building)
		{
			GameUI.Instance.mBuildBlock.EnterBlock();
			GameUI.Instance.mBuildBlock.PlayTween(forward: true);
		}
	}

	private void PlayBuildTweenDelay()
	{
		GameUI.Instance.mBuildBlock.PlayTween(forward: false);
	}

	private void OnBuildTweenFinished(bool forward)
	{
		if (forward && curUIMode == UIMode.um_base)
		{
			GameUI.Instance.mBuildBlock.QuitBlock();
			GameUI.Instance.mUIMainMidCtrl.PlayTween(forward: false);
		}
	}

	private void HideMode()
	{
		if (curUIMode == UIMode.um_base)
		{
			GameUI.Instance.mUIMainMidCtrl.Hide();
		}
		else if (curUIMode == UIMode.um_building)
		{
			GameUI.Instance.mBuildBlock.QuitBlock();
		}
	}

	private void ShowMode()
	{
		if (curUIMode == UIMode.um_base)
		{
			GameUI.Instance.mUIMainMidCtrl.Show();
		}
		else if (curUIMode == UIMode.um_building)
		{
			GameUI.Instance.mBuildBlock.EnterBlock();
		}
	}
}
