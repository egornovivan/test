using SkillAsset;
using UnityEngine;

public class ShootEquipment : Equipment
{
	[HideInInspector]
	public Vector3 mTarget;

	[HideInInspector]
	protected ShootState mShootState;

	public float mRange = 100f;

	public void SetShootState(ShootState ss)
	{
		mShootState = ss;
	}

	public virtual DefaultPosTarget GetShootTargetByMouse()
	{
		Ray mouseRay = PeCamera.mouseRay;
		mTarget = mouseRay.origin + mouseRay.direction * mRange;
		if (Physics.Raycast(mouseRay, out var hitInfo, mRange, 8392960) && !hitInfo.collider.isTrigger && Vector3.Distance(hitInfo.point, mouseRay.origin) > Vector3.Distance(mSkillRunner.transform.position + 1f * Vector3.up, mouseRay.origin) && Vector3.Angle(mSkillRunner.transform.forward, hitInfo.point - mSkillRunner.transform.position) < 90f)
		{
			mTarget = hitInfo.point;
		}
		return new DefaultPosTarget(mTarget);
	}
}
