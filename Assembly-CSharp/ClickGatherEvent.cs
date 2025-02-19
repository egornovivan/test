public class ClickGatherEvent : MousePickableChildCollider
{
	protected override void OnStart()
	{
		operateDistance = 2f;
		base.OnStart();
	}

	protected override void CheckOperate()
	{
	}
}
