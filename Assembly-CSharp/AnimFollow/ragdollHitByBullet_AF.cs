using System.Collections;
using UnityEngine;

namespace AnimFollow;

public class ragdollHitByBullet_AF : MonoBehaviour
{
	private RagdollControl_AF ragdollControl;

	public ParticleSystem blood;

	public ParticleSystem bloodClone;

	private bool userNeedsToFixStuff;

	private void Awake()
	{
		if (!blood)
		{
			Debug.LogWarning("You need to assign blood prefab in the ragdollHitByBullet script on " + base.name);
			userNeedsToFixStuff = true;
		}
		if (!(ragdollControl = GetComponentInChildren<RagdollControl_AF>()))
		{
			Debug.LogWarning("The ragdollHitByBullet script on " + base.name + " requires a RagdollControl script to work");
			userNeedsToFixStuff = true;
		}
	}

	private void HitByBullet(BulletHitInfo_AF bulletHitInfo)
	{
		if (!userNeedsToFixStuff)
		{
			ragdollControl.shotByBullet = true;
			StartCoroutine(AddForceToLimb(bulletHitInfo));
			bloodClone = Object.Instantiate(blood, bulletHitInfo.hitPoint, Quaternion.LookRotation(bulletHitInfo.hitNormal)) as ParticleSystem;
			bloodClone.transform.parent = bulletHitInfo.hitTransform;
			bloodClone.Play();
			Object.Destroy(bloodClone.gameObject, 1f);
		}
	}

	private IEnumerator AddForceToLimb(BulletHitInfo_AF bulletHitInfo)
	{
		yield return new WaitForFixedUpdate();
		bulletHitInfo.hitTransform.GetComponent<Rigidbody>().AddRelativeForce(bulletHitInfo.hitTransform.InverseTransformDirection(bulletHitInfo.bulletForce));
	}
}
