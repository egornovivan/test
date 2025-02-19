using ItemAsset;
using UnityEngine;

public class GunAimer : MonoBehaviour
{
	public Transform aimPivot;

	public Transform aimWeapon;

	public Transform aimTarget;

	public float effect = 1f;

	public Transform bowPivot;

	public Transform singlePistolPivot;

	public Transform doublePistolPivot;

	public AnimationClip fireBowAnimation;

	public AnimationClip fireSinglePistolAnimation;

	public AnimationClip fireDoublePistolAnimation;

	public Transform reload;

	public AnimationClip reloadAnimation;

	private Vector3 aimDirection = Vector3.zero;

	private LayerMask mask;

	private bool aimEnable;

	public void SetGunAimerEnable(bool isEnable, Transform aimWeaponRelative, EquipType equipType)
	{
		aimEnable = isEnable;
		if (!aimEnable)
		{
			return;
		}
		if (aimWeaponRelative != null)
		{
			aimWeapon.position = aimWeaponRelative.position;
			aimWeapon.rotation = aimWeaponRelative.rotation;
		}
		switch (equipType)
		{
		case EquipType.Null:
			aimEnable = false;
			break;
		case EquipType.Bow:
			if (bowPivot != null)
			{
				aimPivot = bowPivot;
			}
			else
			{
				aimEnable = false;
			}
			break;
		case EquipType.HandGun:
			if (singlePistolPivot != null)
			{
				aimPivot = singlePistolPivot;
			}
			else
			{
				aimEnable = false;
			}
			break;
		case EquipType.Rifle:
			if (doublePistolPivot != null)
			{
				aimPivot = doublePistolPivot;
			}
			else
			{
				aimEnable = false;
			}
			break;
		default:
			aimEnable = false;
			break;
		}
	}

	private void Start()
	{
		mask = 1 << base.gameObject.layer;
		mask = (int)mask | (1 << (LayerMask.NameToLayer("Ignore Raycast") & 0x1F));
		mask = ~(int)mask;
		if (fireBowAnimation != null)
		{
			AnimationState animationState = GetComponent<Animation>()[fireBowAnimation.name];
			animationState.wrapMode = WrapMode.Once;
			animationState.layer = 101;
		}
		if (fireSinglePistolAnimation != null)
		{
			AnimationState animationState2 = GetComponent<Animation>()[fireSinglePistolAnimation.name];
			animationState2.wrapMode = WrapMode.Once;
			animationState2.layer = 101;
		}
		if (fireDoublePistolAnimation != null)
		{
			AnimationState animationState3 = GetComponent<Animation>()[fireDoublePistolAnimation.name];
			animationState3.wrapMode = WrapMode.Once;
			animationState3.layer = 101;
		}
		if (reload != null && reloadAnimation != null)
		{
			AnimationState animationState4 = GetComponent<Animation>()[reloadAnimation.name];
			animationState4.wrapMode = WrapMode.Once;
			animationState4.layer = 101;
		}
	}

	private void LateUpdate()
	{
		if (!(effect <= 0f) && aimEnable && !(aimWeapon == null) && !(aimPivot == null) && !(aimTarget == null))
		{
			Vector3 position = aimWeapon.position;
			Quaternion rotation = aimWeapon.rotation;
			Vector3 position2 = aimPivot.position;
			Transform transform = base.transform;
			Vector3 vector = Quaternion.Inverse(transform.rotation) * (aimWeapon.position - position2);
			Vector3 b = Quaternion.Inverse(transform.rotation) * (aimTarget.position - position2);
			b = (aimDirection = Vector3.Slerp(aimDirection, b, 15f * Time.deltaTime));
			Quaternion rotation2 = Quaternion.FromToRotation(vector, b);
			RotateTransformAroundPointInOtherTransformSpace(aimWeapon, rotation2, position2, transform);
			Vector3 vector2 = vector;
			Vector3 vector3 = b;
			vector2.y = 0f;
			vector3.y = 0f;
			if (effect <= 1f)
			{
				aimWeapon.position = Vector3.Lerp(position, aimWeapon.position, effect);
				aimWeapon.rotation = Quaternion.Slerp(rotation, aimWeapon.rotation, effect);
			}
		}
	}

	private void RotateTransformAroundPointInOtherTransformSpace(Transform toRotate, Quaternion rotation, Vector3 pivot, Transform space)
	{
		Vector3 vector = toRotate.position - pivot;
		Vector3 vector2 = -vector + space.rotation * (rotation * (Quaternion.Inverse(space.rotation) * vector));
		toRotate.position += vector2;
		toRotate.rotation = space.rotation * rotation * Quaternion.Inverse(space.rotation) * toRotate.rotation;
	}
}
