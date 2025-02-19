using UnityEngine;

public class SimplePlayerCtrl : MonoBehaviour
{
	private Animator mAnim;

	public float TurnSpeed = 540f;

	public float MoveSpeed = 5f;

	public float Acc = 10f;

	private Vector3 CurrentVec = Vector3.zero;

	private void Start()
	{
		mAnim = GetComponent<Animator>();
	}

	private void Update()
	{
		Vector3 zero = Vector3.zero;
		if (Input.GetKey(KeyCode.W))
		{
			zero += Vector3.forward;
		}
		if (Input.GetKey(KeyCode.S))
		{
			zero += Vector3.back;
		}
		zero *= MoveSpeed;
		CurrentVec = Vector3.Lerp(CurrentVec, zero, Time.deltaTime * Acc);
		base.transform.position += CurrentVec * Time.deltaTime;
		Vector3 vector = base.transform.TransformDirection(CurrentVec) / MoveSpeed;
		mAnim.SetFloat("ForwardSpeed", vector.z);
	}
}
