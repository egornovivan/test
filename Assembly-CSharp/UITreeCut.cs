using Pathea;

public class UITreeCut : UIStaticWnd
{
	private static UITreeCut mInstance;

	public UISlider mProgressBar;

	private bool mAttached;

	private TreeInfo mOpTree;

	public static UITreeCut Instance => mInstance;

	public override void OnCreate()
	{
		base.OnCreate();
		mInstance = this;
	}

	public void SetSliderValue(TreeInfo treeInfo, float sValue)
	{
		if (treeInfo == mOpTree)
		{
			mProgressBar.sliderValue = sValue;
		}
	}

	private void AttachEvent()
	{
		Action_Fell action = PeSingleton<PeCreature>.Instance.mainPlayer.motionMgr.GetAction<Action_Fell>();
		if (action != null)
		{
			mAttached = true;
			action.startFell += StartFell;
			action.endFell += EndFell;
			action.hpChange += SetSliderValue;
		}
		ServantLeaderCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();
		if (null != cmpt)
		{
			cmpt.changeEventor.Subscribe(OnServantChange);
		}
	}

	public void StartFell(TreeInfo treeInfo)
	{
		mOpTree = treeInfo;
		SetSliderValue(mOpTree, 1f);
		Show();
	}

	private void EndFell()
	{
		mOpTree = null;
		Hide();
	}

	private void Update()
	{
		if (!mAttached)
		{
			TryAttachEvent();
		}
	}

	private void TryAttachEvent()
	{
		PeSingleton<MainPlayer>.Instance.mainPlayerCreatedEventor.Subscribe(delegate
		{
			AttachEvent();
		});
		if (null != PeSingleton<MainPlayer>.Instance.entity)
		{
			AttachEvent();
		}
	}

	private void OnServantChange(object sender, ServantLeaderCmpt.ServantChanged arg)
	{
		Action_Fell action = arg.servant.motionMgr.GetAction<Action_Fell>();
		if (action != null)
		{
			if (arg.isAdd)
			{
				action.hpChange += SetSliderValue;
			}
			else
			{
				action.hpChange -= SetSliderValue;
			}
		}
	}
}
