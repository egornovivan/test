using ItemAsset;
using SkillAsset;
using UnityEngine;

public class WaterPump : Gun
{
	public delegate void DirtyVoxelEvent(Vector3 pos, byte terrainType);

	public byte TerrainType = 19;

	public int TansRadius = 2;

	public int Height = 3;

	public float MaxOpDistance = 10f;

	public Transform mBackPack;

	public Transform mMuzzle;

	public int mFireSound;

	public int mPumpSkillId;

	public override void InitEquipment(SkillRunner runner, ItemObject item)
	{
		base.InitEquipment(runner, item);
		Transform[] componentsInChildren = runner.GetComponentsInChildren<Transform>();
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			if (transform.name == "Bow_box")
			{
				mBackPack.transform.parent = transform;
				mBackPack.transform.localPosition = Vector3.zero;
				mBackPack.transform.localScale = Vector3.one;
				mBackPack.transform.localRotation = Quaternion.identity;
				break;
			}
		}
	}

	public override void RemoveEquipment()
	{
		base.RemoveEquipment();
		if (null != mBackPack)
		{
			Object.Destroy(mBackPack.gameObject);
		}
	}

	public override bool CostSkill(ISkillTarget target, int sex = 2, bool buttonDown = false, bool buttonPressed = false)
	{
		if (!base.CostSkill(target, sex, buttonDown, buttonPressed))
		{
			return false;
		}
		if (VFVoxelWater.self.IsInWater(mMuzzle.position) && mShootState == ShootState.Aim && buttonPressed)
		{
			DefaultPosTarget target2 = new DefaultPosTarget(mSkillRunner.transform.position + mSkillRunner.transform.forward);
			if (CostSkill(mSkillRunner, mPumpSkillId, target2) != null)
			{
				AudioManager.instance.Create(base.transform.position, mFireSound);
				if (!GameConfig.IsMultiMode || mMainPlayerEquipment)
				{
				}
				return true;
			}
		}
		return false;
	}
}
