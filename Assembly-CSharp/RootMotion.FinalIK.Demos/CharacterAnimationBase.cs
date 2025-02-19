using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public abstract class CharacterAnimationBase : MonoBehaviour
{
	[SerializeField]
	protected CharacterBase character;

	[SerializeField]
	protected CameraController cameraController;

	[SerializeField]
	private bool smoothFollow = true;

	[SerializeField]
	private float smoothFollowSpeed = 30f;

	public bool animationGrounded { get; protected set; }

	public abstract Vector3 GetPivotPoint();

	public float GetAngleFromForward(Vector3 worldDirection)
	{
		Vector3 vector = base.transform.InverseTransformDirection(worldDirection);
		return Mathf.Atan2(vector.x, vector.z) * 57.29578f;
	}

	protected virtual void Start()
	{
		cameraController.enabled = false;
	}

	protected virtual void LateUpdate()
	{
		Follow();
		UpdateCamera();
	}

	protected void Follow()
	{
		if (smoothFollow)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, character.transform.position, Time.deltaTime * smoothFollowSpeed);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, character.transform.rotation, Time.deltaTime * smoothFollowSpeed);
		}
		else
		{
			base.transform.position = character.transform.position;
			base.transform.rotation = character.transform.rotation;
		}
	}

	protected void UpdateCamera()
	{
		cameraController.UpdateInput();
		cameraController.UpdateTransform();
	}
}
