namespace Pathea;

public class DragEntityLodCmpt : PeCmpt
{
	private BiologyViewCmpt mView;

	private MotionMgrCmpt mMotionMgr;

	public override void Start()
	{
		base.Start();
		mView = base.Entity.biologyViewCmpt;
		mMotionMgr = base.Entity.motionMgr;
	}

	public void Activate()
	{
		mView.Build();
		if (mMotionMgr != null)
		{
			mMotionMgr.FreezePhySteateForSystem(v: false);
		}
		base.Entity.SendMsg(EMsg.Lod_Collider_Created);
	}

	public void Deactivate()
	{
		if (mMotionMgr != null)
		{
			mMotionMgr.FreezePhySteateForSystem(v: true);
		}
		mView.Destroy();
		base.Entity.SendMsg(EMsg.Lod_Collider_Destroying);
	}
}
