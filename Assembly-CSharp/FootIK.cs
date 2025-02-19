using UnityEngine;

public class FootIK : MonoBehaviour
{
	public float ikHeight;

	private Transform leftFoot;

	private Transform rightFoot;

	private float leftFootHeight;

	private float rightFootHeight;

	private Animator animator;

	private void Start()
	{
		animator = GetComponent<Animator>();
		if (GetComponent<Animation>() != null)
		{
			leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
			rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
			if (leftFoot != null)
			{
				leftFootHeight = base.transform.InverseTransformPoint(leftFoot.position).y;
			}
			if (rightFoot != null)
			{
				rightFootHeight = base.transform.InverseTransformPoint(rightFoot.position).y;
			}
		}
	}

	private void OnAnimatorIK(int layerIndex)
	{
		if (layerIndex == 0 && !(animator == null))
		{
			animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
			animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);
			if (leftFoot != null && Physics.Raycast(leftFoot.position + Vector3.up * ikHeight, -Vector3.up, out var hitInfo, 2f * ikHeight, AiUtil.groundedLayer) && Mathf.Abs(hitInfo.point.y + leftFootHeight - leftFoot.position.y) > 0.5f)
			{
				animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0.5f);
				animator.SetIKPosition(AvatarIKGoal.LeftFoot, hitInfo.point + Vector3.up * leftFootHeight);
			}
			if (rightFoot != null && Physics.Raycast(rightFoot.position + Vector3.up * ikHeight, -Vector3.up, out hitInfo, 2f * ikHeight, AiUtil.groundedLayer) && Mathf.Abs(hitInfo.point.y + rightFootHeight - rightFoot.position.y) > 0.5f)
			{
				animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0.5f);
				animator.SetIKPosition(AvatarIKGoal.RightFoot, hitInfo.point + Vector3.up * rightFootHeight);
			}
		}
	}
}
