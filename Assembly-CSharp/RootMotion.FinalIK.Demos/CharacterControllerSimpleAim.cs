using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class CharacterControllerSimpleAim : MonoBehaviour
{
	public SimpleAimingSystem aimingSystem;

	public Transform target;

	private void LateUpdate()
	{
		aimingSystem.targetPosition = target.position;
	}
}
