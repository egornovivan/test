using ItemAsset;
using UnityEngine;
using WhiteCat;

public class UIRobotManger : MonoBehaviour
{
	[SerializeField]
	private UIRobotItem mRobot;

	[SerializeField]
	private UIGrid mServantGrid;

	[SerializeField]
	private UIGrid mRobotGrid;

	[SerializeField]
	private Transform Centent;

	private bool mNeedReposition;

	private LifeLimit mLifeCmpt;

	private Energy mEnergyCmpt;

	private bool mShowLifeTip = true;

	private bool mShowEnergyTip = true;

	private void Awake()
	{
		RobotController.onPlayerGetRobot += OnCreatRobot;
		RobotController.onPlayerLoseRobot += OnDeleteRobot;
	}

	private void OnDestroy()
	{
		RobotController.onPlayerGetRobot -= OnCreatRobot;
		RobotController.onPlayerLoseRobot -= OnDeleteRobot;
	}

	private void ResetPostion()
	{
		Vector3 localPosition = mServantGrid.transform.localPosition;
		if (mNeedReposition)
		{
			mServantGrid.transform.localPosition = new Vector3(localPosition.x, -54f, localPosition.z);
		}
		else
		{
			mServantGrid.transform.localPosition = new Vector3(localPosition.x, 0f, localPosition.z);
		}
	}

	private void Update()
	{
		if (mLifeCmpt != null && mShowLifeTip && mLifeCmpt.floatValue.percent == 0f)
		{
			mShowLifeTip = false;
			new PeTipMsg(PELocalization.GetString(8000177), PeTipMsg.EMsgLevel.HighLightRed);
		}
		if (mEnergyCmpt != null && mShowEnergyTip && mEnergyCmpt.floatValue.percent == 0f)
		{
			mShowEnergyTip = false;
			new PeTipMsg(PELocalization.GetString(8000176), PeTipMsg.EMsgLevel.HighLightRed);
			new PeTipMsg(PELocalization.GetString(8000176), PeTipMsg.EMsgLevel.HighLightRed);
			new PeTipMsg(PELocalization.GetString(8000176), PeTipMsg.EMsgLevel.HighLightRed);
		}
		if (!RobotController.playerFollower && mRobot.IsShow)
		{
			OnDeleteRobot();
		}
		Centent.localPosition = new Vector3(0f - UIMinMapCtrl.Instance.GetMinMapWidth() - 20f, -5f, -22f);
	}

	private void OnCreatRobot(ItemObject obj, GameObject gameobj)
	{
		if (obj != null)
		{
			mLifeCmpt = obj.GetCmpt<LifeLimit>();
			mEnergyCmpt = obj.GetCmpt<Energy>();
			mShowLifeTip = true;
			mShowEnergyTip = true;
			mRobot.mItemObj = obj;
			mRobot.mGameobj = gameobj;
			mRobot.Show();
			mNeedReposition = true;
			ResetPostion();
		}
	}

	private void OnDeleteRobot()
	{
		mRobot.mItemObj = null;
		mRobot.mGameobj = null;
		mRobot.Close();
		mNeedReposition = false;
		ResetPostion();
	}
}
