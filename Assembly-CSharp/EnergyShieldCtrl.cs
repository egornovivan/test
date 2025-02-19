using SkillAsset;
using UnityEngine;

public class EnergyShieldCtrl : MonoBehaviour
{
	private EnergySheildHandler mSheild;

	public SkillRunner mSkillRunner;

	public bool mActive;

	private void Start()
	{
		mSheild = GetComponent<EnergySheildHandler>();
	}

	private void OnTriggerEnter(Collider other)
	{
		Projectile component = other.gameObject.GetComponent<Projectile>();
		if (null != component)
		{
			HitShield(component);
		}
	}

	public void HitShield(Projectile proj)
	{
		if (!proj.ShieldHasBeenHitted(this) && mActive)
		{
			Vector3 vector = proj.transform.position - base.transform.position;
			mSheild.Impact(base.transform.position + vector.normalized);
			proj.ApplyDamageReduce(mSkillRunner.ApplyEnergyShieldAttack(proj), this);
		}
	}
}
