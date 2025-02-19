using UnityEngine;

namespace AnimFollow;

public class BoxHitByBullet_AF : MonoBehaviour
{
	private void HitByBullet(BulletHitInfo_AF bulletHitInfo)
	{
		bulletHitInfo.hitTransform.GetComponent<Rigidbody>().AddForceAtPosition(bulletHitInfo.bulletForce, bulletHitInfo.hitPoint);
	}
}
