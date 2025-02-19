using System;
using CameraForge;
using Pathea;
using UnityEngine;
using WhiteCat;

public static class PeCamera
{
	public enum ControlMode
	{
		ThirdPerson,
		MMOControl,
		FirstPerson
	}

	private static CameraController camc = null;

	private static Camera nearCamera;

	public static Transform cutsceneTransform;

	public static PeCameraImageEffect imageEffect;

	public static Action<ControlMode> onControlModeChange;

	private static bool shakeEnabled0 = false;

	private static float shakeDuration0 = 0f;

	private static float shakeTime0 = 0f;

	private static bool lockCursorMode = false;

	private static HistoryPosTracker mainPlayerPosTracker = new HistoryPosTracker();

	private static HistoryPosTracker drivePosTracker = new HistoryPosTracker();

	private static int shootModeIndex = 0;

	private static float shootModeTime = 0f;

	private static CameraModeData camModeData = CameraModeData.DefaultCameraData;

	private static bool isFirstPerson = false;

	private static bool freeLook = false;

	private static LayerMask obstacle_layermask = 8468481;

	public static bool inited => camc != null;

	public static bool cursorLocked => camc.pose.lockCursor;

	public static bool arouseCursor
	{
		get
		{
			if (UIStateMgr.Instance != null && UIStateMgr.Instance.CurShowGui)
			{
				return true;
			}
			if (Input.GetKey(KeyCode.Tab))
			{
				return true;
			}
			return false;
		}
	}

	public static Vector2 cursorPos
	{
		get
		{
			if (Check())
			{
				return camc.pose.cursorPos;
			}
			return Vector2.one * 0.5f;
		}
	}

	public static Vector3 mousePos
	{
		get
		{
			if (camc == null)
			{
				return Input.mousePosition;
			}
			if (camc.pose.lockCursor)
			{
				return new Vector3(camc.pose.cursorPos.x * (float)Screen.width, camc.pose.cursorPos.y * (float)Screen.height, 0f);
			}
			return Input.mousePosition;
		}
	}

	public static Ray mouseRay => Camera.main.ScreenPointToRay(mousePos);

	public static Ray cursorRay => Camera.main.ScreenPointToRay(new Vector3(camc.pose.cursorPos.x * (float)Screen.width, camc.pose.cursorPos.y * (float)Screen.height, 0f));

	public static float activitySpaceSize { get; private set; }

	public static CameraModeData cameraModeData
	{
		get
		{
			return camModeData;
		}
		set
		{
			camModeData = value;
			if (camModeData != null)
			{
				shootModeIndex = camModeData.camModeIndex3rd;
				if (camModeData.camModeIndex3rd > 0)
				{
					shootModeTime = 0.7f;
					camc.CrossFade("3rd Person Blend", camModeData.camModeIndex3rd, 0.23f);
					camc.CrossFade("1st Person Blend", camModeData.camModeIndex1st, 0.23f);
				}
				SetVar("1st Offset Up", camModeData.offsetUp);
				SetVar("1st Offset", camModeData.offset);
				SetVar("1st Offset Down", camModeData.offsetDown);
			}
		}
	}

	public static bool is1stPerson
	{
		get
		{
			return isFirstPerson;
		}
		set
		{
			isFirstPerson = value;
			if (isFirstPerson)
			{
				camc.CrossFade("1/3 Person Blend", 1, 0.23f);
			}
			else
			{
				camc.CrossFade("1/3 Person Blend", 0, 0.23f);
			}
		}
	}

	public static bool isFreeLook => freeLook;

	public static bool fpCameraCanRotate
	{
		get
		{
			return GetVar("FirstPersonCameraLock").value_b;
		}
		set
		{
			SetBool("FirstPersonCameraLock", value);
		}
	}

	public static void Init()
	{
		CameraController.OnControllerCreate += OnControllerCreate;
		CameraController.OnControllerDestroy += OnControllerDestroy;
		CameraController.AfterControllerPlay += AfterControllerPlay;
	}

	private static bool Check()
	{
		return inited;
	}

	private static void OnControllerCreate(CameraController c)
	{
		if (c.camera == Camera.main)
		{
			camc = c;
			Start();
		}
	}

	private static void OnControllerDestroy(CameraController c)
	{
		if (c.camera == Camera.main)
		{
			UserVarManager.ResetAllGlobalVars();
			camc = null;
		}
	}

	private static void AfterControllerPlay(CameraController c)
	{
		SyncNearCamera();
		if (UISightingTelescope.Instance != null)
		{
			UISightingTelescope.Instance.UpdateType();
		}
		if (null != MainPlayerCmpt.gMainPlayer)
		{
			MainPlayerCmpt.gMainPlayer.UpdateCamDirection(c.transform.forward);
		}
	}

	public static void PlayUserController(int index)
	{
		if (Check())
		{
			camc.PlayUserController(index);
		}
	}

	public static void SetBool(string name, bool value)
	{
		SetVar(name, value);
	}

	public static void SetInt(string name, int value)
	{
		SetVar(name, value);
	}

	public static void SetFloat(string name, float value)
	{
		SetVar(name, value);
	}

	public static void SetVector(string name, Vector2 value)
	{
		SetVar(name, value);
	}

	public static void SetVector(string name, Vector3 value)
	{
		SetVar(name, value);
	}

	public static void SetVector(string name, Vector4 value)
	{
		SetVar(name, value);
	}

	public static void SetQuaternion(string name, Quaternion value)
	{
		SetVar(name, value);
	}

	public static void SetColor(string name, Color value)
	{
		SetVar(name, value);
	}

	public static void SetString(string name, string value)
	{
		SetVar(name, value);
	}

	public static void SetGlobalBool(string name, bool value)
	{
		SetGlobalVar(name, value);
	}

	public static void SetGlobalInt(string name, int value)
	{
		SetGlobalVar(name, value);
	}

	public static void SetGlobalFloat(string name, float value)
	{
		SetGlobalVar(name, value);
	}

	public static void SetGlobalVector(string name, Vector2 value)
	{
		SetGlobalVar(name, value);
	}

	public static void SetGlobalVector(string name, Vector3 value)
	{
		SetGlobalVar(name, value);
	}

	public static void SetGlobalVector(string name, Vector4 value)
	{
		SetGlobalVar(name, value);
	}

	public static void SetGlobalQuaternion(string name, Quaternion value)
	{
		SetGlobalVar(name, value);
	}

	public static void SetGlobalColor(string name, Color value)
	{
		SetGlobalVar(name, value);
	}

	public static void SetGlobalString(string name, string value)
	{
		SetGlobalVar(name, value);
	}

	public static void UnsetVar(string name)
	{
		SetVar(name, Var.Null);
	}

	public static void SetVar(string name, Var value)
	{
		if (Check())
		{
			camc.SetVar(name, value);
		}
	}

	public static void SetPose(string name, Pose pose)
	{
		if (Check())
		{
			camc.SetPose(name, pose);
		}
	}

	public static void UnsetPose(string name)
	{
		if (Check())
		{
			camc.UnsetPose(name);
		}
	}

	public static void SetTransform(string name, Transform t)
	{
		UserVarManager.SetTransform(name, t);
	}

	public static void UnsetTransform(string name)
	{
		UserVarManager.UnsetTransform(name);
	}

	public static void UnsetGlobalVar(string name)
	{
		SetGlobalVar(name, Var.Null);
	}

	public static void SetGlobalVar(string name, Var value)
	{
		UserVarManager.SetGlobalVar(name, value);
	}

	public static Var GetVar(string name)
	{
		if (!Check())
		{
			return Var.Null;
		}
		return camc.GetVar(name);
	}

	public static Var GetGlobalVar(string name)
	{
		return UserVarManager.GetGlobalVar(name);
	}

	public static Transform GetTransform(string name)
	{
		return UserVarManager.GetTransform(name);
	}

	public static Pose GetPose(string name)
	{
		if (!Check())
		{
			return Pose.Default;
		}
		return camc.GetPose(name);
	}

	public static void CrossFade(string blender, int index, float speed = 0.3f)
	{
		if (Check())
		{
			camc.CrossFade(blender, index, speed);
		}
	}

	private static void SyncNearCamera()
	{
		if (!(camc != null))
		{
			return;
		}
		if (nearCamera == null)
		{
			GameObject gameObject = Resources.Load("Near Camera") as GameObject;
			if (gameObject != null)
			{
				nearCamera = UnityEngine.Object.Instantiate(gameObject).GetComponent<Camera>();
				nearCamera.transform.parent = camc.camera.transform;
				nearCamera.transform.localPosition = Vector3.zero;
				nearCamera.transform.localRotation = Quaternion.identity;
				nearCamera.transform.localScale = Vector3.one;
				nearCamera.depth = camc.camera.depth + 1f;
			}
		}
		if (nearCamera != null)
		{
			nearCamera.farClipPlane = camc.camera.nearClipPlane + 0.01f;
			nearCamera.nearClipPlane = 0.03f;
			nearCamera.fieldOfView = camc.camera.fieldOfView;
		}
	}

	public static void Start()
	{
		SetVar("Obstacle LayerMask", (int)obstacle_layermask);
		SetVar("Build Mode", false);
		SetVar("Roll Mode", false);
		lockCursorMode = !SystemSettingData.Instance.mMMOControlType || SystemSettingData.Instance.FirstPersonCtrl;
		isFirstPerson = SystemSettingData.Instance.FirstPersonCtrl;
		GameObject gameObject = new GameObject("Cutscene Transform");
		cutsceneTransform = gameObject.transform;
		SetTransform("Cutscene Camera", cutsceneTransform);
		imageEffect = Camera.main.gameObject.GetComponent<PeCameraImageEffect>();
	}

	public static void Update()
	{
		if (!inited)
		{
			return;
		}
		SetGlobalFloat("Rotate Sensitivity", SystemSettingData.Instance.CameraSensitivity * 3.5f);
		SetGlobalFloat("Original Fov", SystemSettingData.Instance.CameraFov);
		SetGlobalBool("Inverse X", SystemSettingData.Instance.CameraHorizontalInverse);
		SetGlobalBool("Inverse Y", SystemSettingData.Instance.CameraVerticalInverse);
		if (MainPlayerCmpt.gMainPlayer != null)
		{
			PeEntity entity = MainPlayerCmpt.gMainPlayer.Entity;
			BiologyViewCmpt biologyViewCmpt = entity.viewCmpt as BiologyViewCmpt;
			PeTrans peTrans = entity.peTrans;
			PassengerCmpt cmpt = entity.GetCmpt<PassengerCmpt>();
			SetTransform("Anchor", peTrans.camAnchor);
			if (biologyViewCmpt.monoModelCtrlr != null)
			{
				SetTransform("Character", MainPlayerCmpt.gMainPlayer._camTarget);
				SetTransform("Bone Neck M", MainPlayerCmpt.gMainPlayer._bneckModel);
			}
			if (biologyViewCmpt.monoRagdollCtrlr != null)
			{
				SetTransform("Bone Neck R", MainPlayerCmpt.gMainPlayer._bneckRagdoll);
			}
			bool isRagdoll = biologyViewCmpt.IsRagdoll;
			SetVar("Is Ragdoll", isRagdoll);
			mainPlayerPosTracker.Record(peTrans.position, Time.time);
			SetVar("Character Velocity", mainPlayerPosTracker.aveVelocity);
			drivePosTracker.breakDistance = 10f;
			drivePosTracker.maxRecord = 4;
			drivePosTracker.Record(peTrans.position, Time.time);
			SetVar("Driving Velocity", drivePosTracker.aveVelocity);
			SetVar("Rigidbody Velocity", drivePosTracker.aveVelocity);
			activitySpaceSize = CameraForge.Utils.EvaluateActivitySpaceSize(peTrans.camAnchor.position, 0.5f, 50f, Vector3.up, 4f, obstacle_layermask);
			SetVar("Activity Space Size", activitySpaceSize);
			SetBool("Lock Cursor Mode", lockCursorMode || PeInput.UsingJoyStick);
			SetVar("Arouse Cursor", arouseCursor);
			SetVar("Roll Mode", MainPlayerCmpt.isCameraRollable);
			if (GetVar("Build Mode").value_b)
			{
				if (PeInput.Get(PeInput.LogicFunction.Build_FreeBuildModeOnOff) && !PeGameMgr.IsTutorial)
				{
					freeLook = !freeLook;
					camc.CrossFade("Global Blend", (!freeLook) ? 1 : 0);
				}
			}
			else
			{
				int index = 1;
				if (cmpt != null)
				{
					CarrierController drivingController = cmpt.drivingController;
					if (drivingController != null)
					{
						index = 2;
						SetVar("Vehicle Arm", drivingController.isAttackMode);
					}
				}
				camc.CrossFade("Global Blend", index);
				freeLook = false;
			}
			UpdateShake();
		}
		if (PeInput.Get(PeInput.LogicFunction.ChangeContrlMode))
		{
			if (SystemSettingData.Instance.FirstPersonCtrl)
			{
				lockCursorMode = false;
				SystemSettingData.Instance.mMMOControlType = true;
				SystemSettingData.Instance.FirstPersonCtrl = false;
				SystemSettingData.Instance.dataDirty = true;
				if (onControlModeChange != null)
				{
					onControlModeChange(ControlMode.ThirdPerson);
				}
			}
			else if (SystemSettingData.Instance.mMMOControlType)
			{
				lockCursorMode = true;
				SystemSettingData.Instance.mMMOControlType = false;
				SystemSettingData.Instance.FirstPersonCtrl = false;
				SystemSettingData.Instance.dataDirty = true;
				if (onControlModeChange != null)
				{
					onControlModeChange(ControlMode.MMOControl);
				}
			}
			else
			{
				lockCursorMode = true;
				SystemSettingData.Instance.FirstPersonCtrl = true;
				SystemSettingData.Instance.dataDirty = true;
				if (onControlModeChange != null)
				{
					onControlModeChange(ControlMode.FirstPerson);
				}
			}
		}
		if (shootModeIndex == 0 && shootModeTime > 0f)
		{
			shootModeTime -= Time.deltaTime;
			if (shootModeTime <= 0f)
			{
				camc.CrossFade("3rd Person Blend", 0, 0.05f);
				camc.CrossFade("1st Person Blend", 0, 0.05f);
			}
		}
		if (isFirstPerson)
		{
			SetVar("1st Offset Up", camModeData.offsetUp);
			SetVar("1st Offset", camModeData.offset);
			SetVar("1st Offset Down", camModeData.offsetDown);
		}
		SetGlobalVar("Mouse On Scroll", UIMouseEvent.opAnyScroll);
		SetGlobalVar("Mouse On GUI", UIMouseEvent.onAnyGUI);
		SetGlobalVar("Mouse Op GUI", UIMouseEvent.opAnyGUI);
	}

	public static void RecordHistory()
	{
		if (camc != null)
		{
			camc.RecordHistory();
		}
	}

	public static void ApplySkCameraEffect(int id, PESkEntity skEntity)
	{
		MainPlayerCmpt component = skEntity.GetComponent<MainPlayerCmpt>();
		if (component != null && id == 1)
		{
			PlayAttackShake();
		}
	}

	public static void PlayAttackShake()
	{
		PlayShakeEffect(0, 0.4f, 0.2f);
	}

	public static void PlayShakeEffect(int index, float during, float delay)
	{
		if (index == 0)
		{
			shakeEnabled0 = true;
			shakeDuration0 = during;
			shakeTime0 = 0f - delay;
		}
	}

	public static void UpdateShake()
	{
		if (shakeEnabled0)
		{
			shakeTime0 += Time.deltaTime;
			if (shakeTime0 > shakeDuration0)
			{
				shakeEnabled0 = false;
			}
		}
		SetVar("ShakeEnabled0", shakeEnabled0);
		SetVar("ShakeTime0", shakeTime0);
		SetVar("ShakeDuration0", shakeDuration0);
	}
}
