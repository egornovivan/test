using UnityEngine;

namespace Pathea;

public class PECapsuleHitResult
{
	public Transform selfTrans;

	public Transform hitTrans;

	public Vector3 hitPos;

	public Vector3 hitDir;

	public float distance;

	public AttackForm selfAttackForm;

	public DefenceType hitDefenceType;

	public float damageScale;
}
