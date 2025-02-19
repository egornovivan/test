using UnityEngine;

public class ArmAimer : MonoBehaviour
{
	public Transform aimPivot;

	public Transform aimWeapon;

	public Transform aimWeaponModel;

	public Vector3 aimTarget;

	public float effect;

	public Transform IKLeft;

	public Transform IKRight;

	private Vector3 aimDirection = Vector3.zero;

	private LayerMask mask;

	private Transform armIKSrcLeft;

	private Transform armIKSrcRight;

	private Vector3 localPosition;

	private Quaternion localRotation;

	private float distance;

	private void Start()
	{
		mask = 1 << base.gameObject.layer;
		mask = (int)mask | (1 << (LayerMask.NameToLayer("Ignore Raycast") & 0x1F));
		mask = ~(int)mask;
		armIKSrcLeft = AiUtil.GetChild(base.transform, "IKHandLeft");
		armIKSrcRight = AiUtil.GetChild(base.transform, "IKHandRight");
		localPosition = aimWeapon.localPosition;
		localRotation = aimWeapon.localRotation;
		distance = Vector3.Distance(aimPivot.position, aimWeapon.position);
	}

	private void Update()
	{
		if (effect <= float.Epsilon)
		{
			aimWeapon.localPosition = localPosition;
			aimWeapon.localRotation = localRotation;
			return;
		}
		aimWeapon.position = aimWeaponModel.position;
		aimWeapon.rotation = aimWeaponModel.rotation;
		Vector3 position = aimWeapon.position;
		Quaternion rotation = aimWeapon.rotation;
		Vector3 position2 = aimPivot.position;
		Transform transform = base.transform;
		Vector3 vector = Quaternion.Inverse(transform.rotation) * (aimWeapon.position - position2);
		Vector3 zero = Vector3.zero;
		zero = (aimDirection = Vector3.Slerp(b: (!(aimTarget == Vector3.zero)) ? (Quaternion.Inverse(transform.rotation) * (aimTarget - position2)) : (Quaternion.Inverse(transform.rotation) * transform.forward), a: aimDirection, t: 15f * Time.deltaTime));
		Quaternion rotation2 = Quaternion.FromToRotation(vector, aimDirection);
		RotateTransformAroundPointInOtherTransformSpace(aimWeapon, rotation2, position2, transform);
		Vector3 vector2 = vector;
		Vector3 vector3 = zero;
		vector2.y = 0f;
		vector3.y = 0f;
		aimWeapon.position = position2 + (aimWeapon.position - position2).normalized * distance;
		if (effect <= 1f)
		{
			aimWeapon.position = Vector3.Lerp(position, aimWeapon.position, effect);
			aimWeapon.rotation = Quaternion.Slerp(rotation, aimWeapon.rotation, effect);
		}
	}

	private void LateUpdate()
	{
		if (!(effect <= float.Epsilon) && aimWeapon != null && aimWeaponModel != null)
		{
			aimWeaponModel.position = aimWeapon.position;
			aimWeaponModel.rotation = aimWeapon.rotation;
		}
	}

	private void RotateTransformAroundPointInOtherTransformSpace(Transform toRotate, Quaternion rotation, Vector3 pivot, Transform space)
	{
		Vector3 vector = toRotate.position - pivot;
		Vector3 vector2 = -vector + space.rotation * (rotation * (Quaternion.Inverse(space.rotation) * vector));
		toRotate.position += vector2;
		toRotate.rotation = space.rotation * rotation * Quaternion.Inverse(space.rotation) * toRotate.rotation;
	}

	private void RecordIKPosition()
	{
		if (IKLeft != null && armIKSrcLeft != null)
		{
			IKLeft.position = armIKSrcLeft.position;
			IKLeft.rotation = armIKSrcLeft.rotation;
		}
		if (IKRight != null && armIKSrcRight != null)
		{
			IKRight.position = armIKSrcRight.position;
			IKRight.rotation = armIKSrcRight.rotation;
		}
	}

	private void OnAnimatorIK(int layerIndex)
	{
		AiObject component = GetComponent<AiObject>();
		if (component == null || (GameConfig.IsMultiMode && !component.IsController))
		{
			return;
		}
		if (effect <= float.Epsilon)
		{
			component.LookAtWeight(0f);
			component.SetLeftHandIKWeight(0f);
			component.SetRightHandIKWeight(0f);
			return;
		}
		component.LookAtWeight(effect);
		Vector3 zero = Vector3.zero;
		if (aimTarget != Vector3.zero)
		{
			zero = aimTarget;
		}
		else
		{
			Transform boneTransform = component.GetBoneTransform(HumanBodyBones.Head);
			Vector3 vector = base.transform.InverseTransformPoint(boneTransform.position);
			zero = base.transform.TransformPoint(new Vector3(0f, vector.y, 1f));
		}
		component.LookAtPosition(zero);
		component.SetLeftHandIKWeight(1f);
		component.SetRightHandIKWeight(1f);
		component.SetLeftHandIKPosition(IKLeft.position);
		component.SetRightHandIKPosition(IKRight.position);
	}
}
