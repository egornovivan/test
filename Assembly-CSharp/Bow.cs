using ItemAsset;
using SkillAsset;
using UnityEngine;
using WhiteCat;

public class Bow : ShootEquipment
{
	public Transform mBowLine;

	public Transform mShootPos;

	public int mArrowId;

	public Transform mArrowBag;

	private Animator mAnimator;

	private Animator _Animator
	{
		get
		{
			if (null == mAnimator)
			{
				mAnimator = GetComponent<Animator>();
			}
			return mAnimator;
		}
	}

	public override void InitEquipment(SkillRunner runner, ItemObject item)
	{
		base.InitEquipment(runner, item);
		Transform[] componentsInChildren = runner.GetComponentsInChildren<Transform>();
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			if (transform.name == "Bow_box")
			{
				mArrowBag.transform.parent = transform;
				mArrowBag.transform.localPosition = Vector3.zero;
				mArrowBag.transform.localScale = Vector3.one;
				mArrowBag.transform.localRotation = Quaternion.identity;
				break;
			}
		}
	}

	public override void RemoveEquipment()
	{
		base.RemoveEquipment();
		if (null != mArrowBag)
		{
			Object.Destroy(mArrowBag.gameObject);
		}
	}

	public override bool CostSkill(ISkillTarget target, int sex = 2, bool buttonDown = false, bool buttonPressed = false)
	{
		if (!base.CostSkill(target, sex, buttonDown, buttonPressed))
		{
			return false;
		}
		if (buttonDown && mShootState == ShootState.Aim && mHuman.CheckAmmoCost(EArmType.Ammo, mArrowId))
		{
			if (mSkillMaleId.Count == 0 || mSkillFemaleId.Count == 0)
			{
				return false;
			}
			EffSkillInstance effSkillInstance = null;
			switch (sex)
			{
			case 1:
				effSkillInstance = CostSkill(mSkillRunner, mSkillFemaleId[0], target);
				break;
			case 2:
				effSkillInstance = CostSkill(mSkillRunner, mSkillMaleId[0], target);
				break;
			}
			if (effSkillInstance != null)
			{
				mHuman.ApplyDurabilityReduce(0);
				mHuman.ApplyAmmoCost(EArmType.Ammo, mArrowId);
				_Animator.SetBool("Fire", value: true);
				mShootState = ShootState.Fire;
				return true;
			}
		}
		return false;
	}

	public void SetAnimatorState(string name, bool state)
	{
		_Animator.SetBool(name, state);
	}

	protected override void Update()
	{
		switch (mShootState)
		{
		case ShootState.Null:
			_Animator.SetBool("Fire", value: false);
			_Animator.SetBool("Hold", value: false);
			break;
		case ShootState.PutOn:
			_Animator.SetBool("Fire", value: false);
			_Animator.SetBool("Hold", value: true);
			break;
		case ShootState.Aim:
			_Animator.SetBool("Fire", value: false);
			_Animator.SetBool("Hold", value: true);
			break;
		case ShootState.Fire:
			_Animator.SetBool("Fire", value: true);
			_Animator.SetBool("Hold", value: false);
			break;
		case ShootState.Reload:
			_Animator.SetBool("Fire", value: false);
			_Animator.SetBool("Hold", value: true);
			break;
		case ShootState.PutOff:
			_Animator.SetBool("Fire", value: false);
			_Animator.SetBool("Hold", value: false);
			break;
		}
	}
}
