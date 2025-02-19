using SkillAsset;
using UnityEngine;

public class FlameGun : ShootEquipment
{
	public float coolDown;

	public Transform muzzle;

	public Transform muzzleParticle;

	private float lastFireTime;

	private void PlayMuzzleEffect()
	{
		if (muzzleParticle != null)
		{
			ParticleSystem[] componentsInChildren = muzzleParticle.GetComponentsInChildren<ParticleSystem>();
			ParticleSystem[] array = componentsInChildren;
			foreach (ParticleSystem particleSystem in array)
			{
				particleSystem.Play();
			}
		}
	}

	private bool CanFire()
	{
		return Time.time - lastFireTime >= coolDown;
	}

	public void Fire()
	{
		lastFireTime = Time.time;
		PlayMuzzleEffect();
	}

	public override bool CostSkill(ISkillTarget target = null, int sex = 2, bool buttonDown = false, bool buttonPressed = false)
	{
		if (!base.CostSkill(target, sex, buttonDown, buttonPressed))
		{
			return false;
		}
		if (mSkillMaleId.Count == 0 || mSkillFemaleId.Count == 0)
		{
			return false;
		}
		int id = 0;
		switch (sex)
		{
		case 1:
		{
			id = mSkillFemaleId[0];
			for (int j = 0; j < mSkillFemaleId.Count - 1; j++)
			{
				if (mSkillRunner.IsEffRunning(mSkillFemaleId[j]))
				{
					id = mSkillFemaleId[j + 1];
				}
			}
			break;
		}
		case 2:
		{
			id = mSkillMaleId[0];
			for (int i = 0; i < mSkillMaleId.Count - 1; i++)
			{
				if (mSkillRunner.IsEffRunning(mSkillMaleId[i]))
				{
					id = mSkillMaleId[i + 1];
				}
			}
			break;
		}
		}
		EffSkillInstance effSkillInstance = CostSkill(mSkillRunner, id, target);
		if (effSkillInstance != null)
		{
			lastFireTime = Time.time;
			PlayMuzzleEffect();
		}
		return effSkillInstance != null;
	}
}
