using Pathea.Operate;

public class ItemScript_Bed : ItemScript
{
	private PESleep mSleep;

	public PESleep peSleep => mSleep;

	public override void OnConstruct()
	{
		base.OnConstruct();
		mSleep = GetComponentInChildren<PESleep>();
		if (mSleep == null)
		{
			PEBed componentInParent = GetComponentInParent<PEBed>();
			if (!(componentInParent == null) && componentInParent.sleeps != null && componentInParent.sleeps.Length != 0)
			{
				mSleep = componentInParent.sleeps[0];
			}
		}
	}
}
