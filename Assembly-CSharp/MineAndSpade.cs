using ItemAsset;
using SkillAsset;
using UnityEngine;

public class MineAndSpade : Equipment
{
	public OpCubeCtrl mOpCube;

	private float mUnActiveTime = 0.5f;

	private VFTerrainTarget mTarget;

	public override void InitEquipment(SkillRunner runner, ItemObject item)
	{
		base.InitEquipment(runner, item);
		if (mMainPlayerEquipment)
		{
			mOpCube.Active = true;
			mOpCube.Enable = true;
		}
		else
		{
			mOpCube.Active = false;
		}
	}

	public override void RemoveEquipment()
	{
		base.RemoveEquipment();
		Object.Destroy(mOpCube.gameObject);
	}

	public override bool CostSkill(ISkillTarget target, int sex = 2, bool buttonDown = false, bool buttonPressed = false)
	{
		if (mMainPlayerEquipment)
		{
			mUnActiveTime = 0.5f;
			mOpCube.Active = true;
		}
		if (mSkillMaleId.Count == 0 || mSkillFemaleId.Count == 0)
		{
			return false;
		}
		if (!base.CostSkill(target, sex, buttonDown, buttonPressed))
		{
			return false;
		}
		if (mOpCube.Active && mOpCube.Enable)
		{
			int num = 0;
			switch (sex)
			{
			case 1:
				num = mSkillFemaleId[0];
				break;
			case 2:
				num = mSkillMaleId[0];
				break;
			}
			EffSkillInstance effSkillInstance = mSkillRunner.GetRunningEff(num);
			if (effSkillInstance != null && buttonPressed)
			{
				effSkillInstance.mSkillCostTimeAdd = true;
				effSkillInstance.mNextTarget = mTarget;
			}
			else if (effSkillInstance == null && buttonDown)
			{
				effSkillInstance = CostSkill(mSkillRunner, num, mTarget);
			}
			return null != effSkillInstance;
		}
		return false;
	}

	private void LateUpdate()
	{
		if (mMainPlayerEquipment)
		{
			CheckTerrain();
			mUnActiveTime -= Time.deltaTime;
			if (mUnActiveTime < 0f)
			{
				mOpCube.Active = false;
			}
		}
	}

	private void CheckTerrain()
	{
		Ray mouseRay = PeCamera.mouseRay;
		mOpCube.Enable = false;
		if (!Physics.Raycast(mouseRay, out var hitInfo, 100f, 4096))
		{
			return;
		}
		VFVoxelChunkGo component = hitInfo.collider.gameObject.GetComponent<VFVoxelChunkGo>();
		if (component != null)
		{
			mOpCube.Enable = true;
			IntVector3 intVector = new IntVector3();
			if (hitInfo.normal.x == 0f || hitInfo.point.x - (float)(int)hitInfo.point.x > 0.5f)
			{
				intVector.x = Mathf.RoundToInt(hitInfo.point.x);
			}
			else
			{
				intVector.x = ((!(hitInfo.normal.x > 0f)) ? Mathf.FloorToInt(hitInfo.point.x) : Mathf.CeilToInt(hitInfo.point.x));
			}
			if (hitInfo.normal.y == 0f || hitInfo.point.y - (float)(int)hitInfo.point.y > 0.5f)
			{
				intVector.y = Mathf.RoundToInt(hitInfo.point.y);
			}
			else
			{
				intVector.y = ((!(hitInfo.normal.y > 0f)) ? Mathf.FloorToInt(hitInfo.point.y) : Mathf.CeilToInt(hitInfo.point.y));
			}
			if (hitInfo.normal.z == 0f || hitInfo.point.z - (float)(int)hitInfo.point.z > 0.5f)
			{
				intVector.z = Mathf.RoundToInt(hitInfo.point.z);
			}
			else
			{
				intVector.z = ((!(hitInfo.normal.z > 0f)) ? Mathf.FloorToInt(hitInfo.point.z) : Mathf.CeilToInt(hitInfo.point.z));
			}
			IntVector3 intVector2 = intVector;
			float num = 100f;
			if (VFVoxelTerrain.self.Voxels.SafeRead(intVector.x, intVector.y, intVector.z).Volume < 127)
			{
				for (int i = -1; i <= 1; i++)
				{
					for (int j = -1; j <= 1; j++)
					{
						for (int k = -1; k <= 1; k++)
						{
							IntVector3 intVector3 = new IntVector3(intVector.x + i, intVector.y + j, intVector.z + k);
							if (VFVoxelTerrain.self.Voxels.SafeRead(intVector3.x, intVector3.y, intVector3.z).Volume > 127)
							{
								float magnitude = (intVector3.ToVector3() - hitInfo.point).magnitude;
								if (num >= magnitude)
								{
									intVector2 = intVector3;
									num = magnitude;
								}
							}
						}
					}
				}
			}
			mOpCube.transform.position = intVector2.ToVector3();
			VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead(intVector2.x, intVector2.y, intVector2.z);
			mOpCube.Enable = Vector3.Distance(mSkillRunner.transform.position + Vector3.up, hitInfo.point) < 3f && voxel.Volume > 0 && mSkillRunner.GetComponent<Rigidbody>().velocity.sqrMagnitude < 9f;
			if (mOpCube.Enable)
			{
				mTarget = new VFTerrainTarget(hitInfo.point, intVector2, ref voxel);
			}
		}
		else
		{
			mOpCube.Enable = false;
		}
	}
}
