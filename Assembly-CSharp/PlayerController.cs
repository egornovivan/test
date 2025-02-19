using DunGen;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	public float MinYaw = -360f;

	public float MaxYaw = 360f;

	public float MinPitch = -60f;

	public float MaxPitch = 60f;

	public float LookSensitivity = 1f;

	public float MoveSpeed = 10f;

	public float TurnSpeed = 90f;

	public float JumpHeight = 3f;

	protected CharacterController movementController;

	protected Camera playerCamera;

	protected Camera overheadCamera;

	protected bool isControlling;

	protected float yaw;

	protected float pitch;

	protected Generator gen;

	protected Vector3 velocity;

	protected virtual void Start()
	{
		movementController = GetComponent<CharacterController>();
		playerCamera = GetComponentInChildren<Camera>();
		gen = Object.FindObjectOfType<Generator>();
		overheadCamera = GameObject.Find("Overhead Camera").GetComponent<Camera>();
		isControlling = true;
		ToggleControl();
		gen.DungeonGenerator.Generator.OnGenerationStatusChanged += OnGenerationStatusChanged;
	}

	protected virtual void OnGenerationStatusChanged(DungeonGenerator generator, GenerationStatus status)
	{
		if (status == GenerationStatus.Complete)
		{
			FrameObjectWithCamera(generator.Root);
		}
	}

	protected virtual void Update()
	{
		if (Input.GetKeyDown(KeyCode.C))
		{
			ToggleControl();
		}
		if (Input.GetKeyDown(KeyCode.R))
		{
			base.transform.position = new Vector3(3f, 0f, 0f);
		}
		Vector3 zero = Vector3.zero;
		if (Input.GetKeyDown(KeyCode.Space))
		{
			zero += base.transform.up;
			movementController.Move(zero * JumpHeight);
		}
		zero = Vector3.zero;
		zero += base.transform.forward * Input.GetAxisRaw("Vertical");
		zero += base.transform.right * Input.GetAxisRaw("Horizontal");
		zero.Normalize();
		if (movementController.isGrounded)
		{
			velocity = Vector3.zero;
		}
		else
		{
			velocity += -base.transform.up * 98.100006f * Time.deltaTime;
		}
		zero += velocity * Time.deltaTime;
		movementController.Move(zero * Time.deltaTime * MoveSpeed);
		yaw += Input.GetAxisRaw("Mouse X") * LookSensitivity;
		pitch += Input.GetAxisRaw("Mouse Y") * LookSensitivity;
		yaw = ClampAngle(yaw, MinYaw, MaxYaw);
		pitch = ClampAngle(pitch, MinPitch, MaxPitch);
		base.transform.rotation = Quaternion.AngleAxis(yaw, Vector3.up);
		playerCamera.transform.localRotation = Quaternion.AngleAxis(pitch, -Vector3.right);
	}

	protected float ClampAngle(float angle)
	{
		return ClampAngle(angle, 0f, 360f);
	}

	protected float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}

	protected void ToggleControl()
	{
		isControlling = !isControlling;
		overheadCamera.gameObject.SetActive(!isControlling);
		playerCamera.gameObject.SetActive(isControlling);
		overheadCamera.transform.position = new Vector3(base.transform.position.x, overheadCamera.transform.position.y, base.transform.position.z);
		Cursor.lockState = (isControlling ? CursorLockMode.Locked : (Screen.fullScreen ? CursorLockMode.Confined : CursorLockMode.None));
		Cursor.visible = !isControlling;
		if (!isControlling)
		{
			FrameObjectWithCamera(gen.DungeonGenerator.Generator.Root);
		}
	}

	protected void FrameObjectWithCamera(GameObject gameObject)
	{
		if (!(gameObject == null))
		{
			Bounds bounds = UnityUtil.CalculateObjectBounds(gameObject, includeInactive: false, ignoreSpriteRenderers: false);
			float num = Mathf.Max(bounds.size.x, bounds.size.z);
			float f = num / Mathf.Sin(overheadCamera.fieldOfView / 2f);
			f = Mathf.Abs(f);
			Vector3 position = new Vector3(bounds.center.x, bounds.center.y, bounds.center.z);
			position += gen.DungeonGenerator.Generator.UpVector * f;
			overheadCamera.transform.position = position;
		}
	}
}
