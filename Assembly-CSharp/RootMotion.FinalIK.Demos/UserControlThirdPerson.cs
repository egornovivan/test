using UnityEngine;

namespace RootMotion.FinalIK.Demos;

[RequireComponent(typeof(CharacterBase))]
public class UserControlThirdPerson : MonoBehaviour
{
	public struct State
	{
		public Vector3 move;

		public Vector3 lookPos;

		public bool crouch;

		public bool jump;
	}

	[SerializeField]
	private bool walkByDefault;

	[SerializeField]
	private bool canCrouch = true;

	[SerializeField]
	private bool canJump = true;

	protected Transform cam;

	protected CharacterThirdPerson character;

	protected State state = default(State);

	private void Start()
	{
		cam = Camera.main.transform;
		character = GetComponent<CharacterThirdPerson>();
	}

	protected virtual void FixedUpdate()
	{
		state.crouch = canCrouch && Input.GetKey(KeyCode.C);
		state.jump = canJump && Input.GetButton("Jump");
		float axis = Input.GetAxis("Horizontal");
		float axis2 = Input.GetAxis("Vertical");
		Vector3 normalized = Vector3.Scale(cam.forward, new Vector3(1f, 0f, 1f)).normalized;
		state.move = (axis2 * normalized + axis * cam.right).normalized;
		bool key = Input.GetKey(KeyCode.LeftShift);
		float num = (walkByDefault ? ((!key) ? 0.5f : 1f) : ((!key) ? 1f : 0.5f));
		state.move *= num;
		state.lookPos = base.transform.position + cam.forward * 100f;
		character.UpdateState(state);
	}
}
