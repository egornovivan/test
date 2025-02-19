using UnityEngine;

public class SceneDoodadDesc
{
	public const int c_neutralCamp = 28;

	public const int c_neutralDamage = 28;

	public const int c_playerAttackableCamp = 0;

	public const int c_playerAttackableDamage = 25;

	public int _id;

	public int _type;

	public int _protoId;

	public bool _isShown;

	public int _campId;

	public int _damageId;

	public Vector3 _pos;

	public Vector3 _scl;

	public Quaternion _rot;

	public static void GetCampDamageId(bool damagable, out int campId, out int damageId)
	{
		if (damagable)
		{
			campId = 0;
			damageId = 25;
		}
		else
		{
			campId = 28;
			damageId = 28;
		}
	}
}
