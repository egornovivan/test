using System.Collections.Generic;
using UnityEngine;

public class AircaraftTest : MonoBehaviour
{
	[SerializeField]
	public Rigidbody mRigidbody;

	[SerializeField]
	private Transform mFwordLeft;

	[SerializeField]
	private Transform mFwordRight;

	[SerializeField]
	private Transform mBackLeft;

	[SerializeField]
	private Transform mBackRight;

	[SerializeField]
	private float mGrivate = 9.8f;

	private int UpLevel;

	private float speed;

	[SerializeField]
	private float mUpForce_k = 0.2f;

	[SerializeField]
	private float OtherForceSize = 10f;

	public bool m_ForwardInput;

	public bool m_BackwardInput;

	public bool m_LeftInput;

	public bool m_RightInput;

	public bool m_UpInput;

	public bool m_DownInput;

	public List<AircarftPutForce> forceList;

	private Vector3 MassCenter => mRigidbody.centerOfMass;

	private float Grivate => mRigidbody.mass * mGrivate;

	private Vector3 FwordLeftForce => mFwordLeft.up * OtherForceSize;

	private Vector3 FwordRightForce => mFwordRight.up * OtherForceSize;

	private Vector3 BackLeftForce => mBackLeft.up * OtherForceSize;

	private Vector3 BackRightForce => mBackRight.up * OtherForceSize;

	private void Start()
	{
		forceList.AddRange(GetComponentsInChildren<AircarftPutForce>());
	}

	private void Update()
	{
		m_ForwardInput = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
		m_BackwardInput = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
		m_LeftInput = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
		m_RightInput = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
		m_UpInput = Input.GetKey(KeyCode.Space);
		m_DownInput = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
		if (Input.GetKey(KeyCode.Space) && Time.frameCount % 6 == 0 && UpLevel < 10)
		{
			UpLevel++;
		}
		if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Time.frameCount % 6 == 0 && UpLevel > -10)
		{
			UpLevel--;
		}
	}

	private void FixedUpdate()
	{
		mRigidbody.AddForce(new Vector3(0f, 0f - Grivate, 0f));
		float num = 1f + (float)UpLevel * mUpForce_k;
		mRigidbody.AddForce(new Vector3(0f, Grivate * num, 0f));
		if (m_ForwardInput)
		{
			mRigidbody.AddForceAtPosition(BackLeftForce, mBackLeft.position);
			mRigidbody.AddForceAtPosition(BackRightForce, mBackRight.position);
		}
		if (m_BackwardInput)
		{
			mRigidbody.AddForceAtPosition(FwordLeftForce, mFwordLeft.position);
			mRigidbody.AddForceAtPosition(FwordRightForce, mFwordRight.position);
		}
		if (m_LeftInput)
		{
			mRigidbody.AddForceAtPosition(FwordRightForce, mFwordRight.position);
			mRigidbody.AddForceAtPosition(BackLeftForce, mBackLeft.position);
		}
		if (m_RightInput)
		{
			mRigidbody.AddForceAtPosition(FwordLeftForce, mFwordLeft.position);
			mRigidbody.AddForceAtPosition(BackRightForce, mBackRight.position);
		}
	}

	private void OnGUI()
	{
		GUI.Label(new Rect(50f, 80f, 500f, 20f), "Level: " + UpLevel);
	}
}
