using Pathea;
using UnityEngine;

public class PlayerSynAttribute
{
	public const float SyncMovePrecision = 0.1f;

	public Vector3 mv3Postion;

	public Vector3 mv3shootTarget;

	public float mfRotationY;

	public SpeedState mnPlayerState;

	public bool mbGrounded;

	public bool mbJumpFlag;
}
