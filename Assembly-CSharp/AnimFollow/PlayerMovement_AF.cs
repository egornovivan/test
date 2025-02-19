using UnityEngine;

namespace AnimFollow;

public class PlayerMovement_AF : MonoBehaviour
{
	public readonly int version = 4;

	private Animator anim;

	private HashIDs_AF hash;

	public float animatorSpeed = 1.3f;

	public float speedDampTime = 0.1f;

	private float mouseInput;

	public float mouseSensitivityX = 100f;

	public bool inhibitMove;

	[HideInInspector]
	public Vector3 glideFree = Vector3.zero;

	private Vector3 glideFree2 = Vector3.zero;

	[HideInInspector]
	public bool inhibitRun;

	private void Awake()
	{
		if (!(anim = GetComponent<Animator>()))
		{
			Debug.LogWarning("Missing Animator on " + base.name);
			inhibitMove = true;
		}
		if (!(hash = GetComponent<HashIDs_AF>()))
		{
			Debug.LogWarning("Missing Script: HashIDs on " + base.name);
			inhibitMove = true;
		}
		if ((bool)anim.avatar && !anim.avatar.isValid)
		{
			Debug.LogWarning("Animator avatar is not valid");
		}
	}

	private void OnAnimatorMove()
	{
		glideFree2 = Vector3.Lerp(glideFree2, glideFree, 0.05f);
		base.transform.position += anim.deltaPosition + glideFree2;
	}

	private void FixedUpdate()
	{
		if (!inhibitMove)
		{
			base.transform.Rotate(0f, Input.GetAxis("Mouse X") * mouseSensitivityX * Time.fixedDeltaTime, 0f);
			MovementManagement(Input.GetAxis("Vertical"), Input.GetKey(KeyCode.LeftShift), Input.GetKey(KeyCode.LeftControl));
		}
	}

	public void MovementManagement(float vertical, bool walk, bool sneaking)
	{
		walk = walk || inhibitRun;
		anim.SetBool(hash.sneakingBool, sneaking);
		if (vertical >= 0.1f && !walk)
		{
			anim.SetFloat(hash.speedFloat, 5.5f, speedDampTime, Time.fixedDeltaTime);
		}
		else if (vertical >= 0.1f && walk)
		{
			anim.SetFloat(hash.speedFloat, 2.5f, speedDampTime, Time.fixedDeltaTime);
		}
		else if (vertical <= -0.1f)
		{
			anim.SetFloat(hash.speedFloat, -3f, speedDampTime, Time.fixedDeltaTime);
		}
		else
		{
			anim.SetFloat(hash.speedFloat, 0f, speedDampTime, Time.fixedDeltaTime);
		}
	}
}
