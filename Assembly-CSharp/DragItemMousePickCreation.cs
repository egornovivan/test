using WhiteCat;

public class DragItemMousePickCreation : DragItemMousePick
{
	protected override void OnStart()
	{
		GetComponentInParent<CreationController>().AddBuildFinishedListener(base.CollectColliders);
	}
}
