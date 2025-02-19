using UnityEngine;

public class PeViewController : ViewController
{
	public MovingGizmo moveHandle;

	public RotatingGizmo rotateHandle;

	public ScalingGizmo scaleHandle;

	public override void SetTarget(Transform target)
	{
		base.SetTarget(target);
		target.transform.localPosition = new Vector3(ID * 100, 0f, 0f);
	}
}
