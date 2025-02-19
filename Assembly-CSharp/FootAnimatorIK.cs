using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FootAnimatorIK : MonoBehaviour
{
	public Transform LeftFootBone;

	public Transform RightFootBone;

	public float MaxStepHeight = 0.4f;

	public float AnimChangeSpeed = 10f;

	[HideInInspector]
	public bool mGrounded = true;

	public float BodyOffset = 0.05f;

	public float FootOffset;

	private float LeftFootIKHeight;

	private float RightFootIKHeight;

	private float BodyIKHeight;

	public Animator mAnimator;

	private bool UPD;

	private Vector3 leftFootPos = Vector3.zero;

	private Vector3 rightFootPos = Vector3.zero;

	private Vector3 bodyPos = Vector3.zero;

	[HideInInspector]
	public Vector3 FootDir;

	private void Start()
	{
		mAnimator = GetComponent<Animator>();
		LeftFootIKHeight = 0f;
		RightFootIKHeight = 0f;
		BodyIKHeight = 0f;
	}

	private bool CheckBones()
	{
		if (null == LeftFootBone)
		{
			LeftFootBone = AiUtil.GetChild(base.transform, "Bip01 L Foot");
		}
		if (null == RightFootBone)
		{
			RightFootBone = AiUtil.GetChild(base.transform, "Bip01 R Foot");
		}
		return null != LeftFootBone && null != RightFootBone;
	}

	private void Update()
	{
		UPD = true;
	}

	private void OnAnimatorIK()
	{
		if (!mGrounded || !(null != mAnimator) || !CheckBones())
		{
			return;
		}
		if (UPD)
		{
			UPD = false;
			Vector3 zero = Vector3.zero;
			Vector3 vector = Vector3.zero;
			if (Physics.Raycast(LeftFootBone.position + (MaxStepHeight - LeftFootBone.position.y + base.transform.position.y) * Vector3.up, Vector3.down, out var hitInfo, 2f * MaxStepHeight, 71680))
			{
				if (hitInfo.distance < 2f * MaxStepHeight && !hitInfo.collider.isTrigger)
				{
					LeftFootIKHeight = Mathf.Lerp(LeftFootIKHeight, hitInfo.point.y - base.transform.position.y, AnimChangeSpeed * Time.deltaTime);
					zero = hitInfo.point;
				}
				else
				{
					zero = LeftFootBone.position;
				}
			}
			else
			{
				LeftFootIKHeight = Mathf.Lerp(LeftFootIKHeight, 0f, AnimChangeSpeed * Time.deltaTime);
				zero = LeftFootBone.position;
			}
			if (Physics.Raycast(RightFootBone.position + (MaxStepHeight - RightFootBone.position.y + base.transform.position.y) * Vector3.up, Vector3.down, out hitInfo, 2f * MaxStepHeight, 71680))
			{
				if (hitInfo.distance < 2f * MaxStepHeight)
				{
					RightFootIKHeight = Mathf.Lerp(RightFootIKHeight, hitInfo.point.y - base.transform.position.y, AnimChangeSpeed * Time.deltaTime);
					vector = hitInfo.point;
				}
				else
				{
					vector = RightFootBone.position;
				}
			}
			else
			{
				RightFootIKHeight = Mathf.Lerp(RightFootIKHeight, 0f, AnimChangeSpeed * Time.deltaTime);
			}
			FootDir = zero - vector;
			BodyIKHeight = Mathf.Lerp(BodyIKHeight, Mathf.Min(LeftFootIKHeight, RightFootIKHeight) + BodyOffset, AnimChangeSpeed * Time.deltaTime);
			leftFootPos = LeftFootBone.position + (LeftFootIKHeight + FootOffset) * Vector3.up;
			rightFootPos = RightFootBone.position + (RightFootIKHeight + FootOffset) * Vector3.up;
			bodyPos = mAnimator.bodyPosition + BodyIKHeight * Vector3.up;
		}
		mAnimator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
		mAnimator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
		mAnimator.bodyPosition = bodyPos;
		mAnimator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootPos);
		mAnimator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootPos);
	}
}
