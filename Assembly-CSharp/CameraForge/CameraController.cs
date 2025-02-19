using UnityEngine;

namespace CameraForge;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Camera Forge/Camera Controller")]
public class CameraController : MonoBehaviour
{
	public delegate void DEventNotify(CameraController camc);

	public const int maxHistoryPoses = 128;

	[Header(">> Controllers")]
	[SerializeField]
	private ControllerAsset startControllerAsset;

	[SerializeField]
	private ControllerAsset updateControllerAsset;

	[SerializeField]
	private ControllerAsset[] userControllerAsset = new ControllerAsset[0];

	public Controller start_controller;

	public Controller update_controller;

	public Controller[] user_controllers;

	private Camera _camera;

	private Pose _pose;

	[Header(">> Settings")]
	public bool manualUpdate;

	public bool manualCursor;

	private bool ignoreLockCursor;

	private float _last_playtime = -1f;

	public UserVarManager uservars;

	public Pose[] history;

	private int history_count;

	public Camera camera => _camera;

	public Pose pose => _pose;

	public static event DEventNotify OnControllerCreate;

	public static event DEventNotify OnControllerDestroy;

	public static event DEventNotify BeforeControllerPlay;

	public static event DEventNotify AfterControllerPlay;

	private void Awake()
	{
		_camera = GetComponent<Camera>();
		uservars = new UserVarManager();
		history = new Pose[128];
		if (startControllerAsset != null)
		{
			startControllerAsset.Load();
			start_controller = startControllerAsset.controller;
			start_controller.executor = this;
		}
		if (updateControllerAsset != null)
		{
			updateControllerAsset.Load();
			update_controller = updateControllerAsset.controller;
			update_controller.executor = this;
		}
		user_controllers = new Controller[userControllerAsset.Length];
		for (int i = 0; i < userControllerAsset.Length; i++)
		{
			userControllerAsset[i].Load();
			user_controllers[i] = userControllerAsset[i].controller;
			user_controllers[i].executor = this;
		}
		if (CameraController.OnControllerCreate != null)
		{
			CameraController.OnControllerCreate(this);
		}
	}

	private void OnDestroy()
	{
		if (CameraController.OnControllerDestroy != null)
		{
			CameraController.OnControllerDestroy(this);
		}
	}

	private void Start()
	{
		if (start_controller != null)
		{
			PlayController(start_controller);
		}
	}

	private void LateUpdate()
	{
		if (update_controller != null && !manualUpdate)
		{
			PlayController(update_controller);
		}
	}

	public void ManualUpdate()
	{
		PlayController(update_controller);
	}

	public void RecordHistory()
	{
		Pose pose = this.pose;
		pose.position = _camera.transform.position;
		pose.rotation = _camera.transform.rotation;
		pose.fov = _camera.fieldOfView;
		pose.nearClip = _camera.nearClipPlane;
		RecordHistory(pose);
	}

	private void PlayController(Controller c)
	{
		if (CameraController.BeforeControllerPlay != null)
		{
			CameraController.BeforeControllerPlay(this);
		}
		Pose pose = c.final.Calculate();
		_camera.transform.position = pose.position;
		_camera.transform.rotation = pose.rotation;
		_camera.fieldOfView = pose.fov;
		_camera.nearClipPlane = pose.nearClip;
		_pose = pose;
		if (ignoreLockCursor)
		{
			Cursor.lockState = (Screen.fullScreen ? CursorLockMode.Confined : CursorLockMode.None);
			Cursor.visible = true;
		}
		else if (!manualCursor)
		{
			Cursor.lockState = (_pose.lockCursor ? CursorLockMode.Locked : (Screen.fullScreen ? CursorLockMode.Confined : CursorLockMode.None));
			Cursor.visible = !_pose.lockCursor;
		}
		RecordHistory(pose);
		float deltaTime = 0f;
		if (_last_playtime >= 0f)
		{
			deltaTime = Time.time - _last_playtime;
			_last_playtime = Time.time;
		}
		c.Tick(deltaTime);
		if (CameraController.AfterControllerPlay != null)
		{
			CameraController.AfterControllerPlay(this);
		}
	}

	public void PlayUserController(int index)
	{
		if (index >= 0 && index < user_controllers.Length && user_controllers[index] != null)
		{
			PlayController(user_controllers[index]);
		}
	}

	private void RecordHistory(Pose p)
	{
		for (int num = 127; num > 0; num--)
		{
			history[num] = history[num - 1];
		}
		history[0] = p;
		history_count++;
		if (history_count > 128)
		{
			history_count = 128;
		}
	}

	public Pose GetHistoryPose(int index)
	{
		if (index < 0)
		{
			index = 0;
		}
		if (index > history_count - 1)
		{
			index = history_count - 1;
		}
		Pose pose = null;
		if (history != null && history_count != 0)
		{
			pose = history[index];
			if (pose != null)
			{
				return pose;
			}
		}
		pose = Pose.Default;
		pose.position = base.transform.position;
		pose.rotation = base.transform.rotation;
		pose.fov = _camera.fieldOfView;
		pose.nearClip = _camera.nearClipPlane;
		return pose;
	}

	public void SetBool(string name, bool value)
	{
		SetVar(name, value);
	}

	public void SetInt(string name, int value)
	{
		SetVar(name, value);
	}

	public void SetFloat(string name, float value)
	{
		SetVar(name, value);
	}

	public void SetVector(string name, Vector2 value)
	{
		SetVar(name, value);
	}

	public void SetVector(string name, Vector3 value)
	{
		SetVar(name, value);
	}

	public void SetVector(string name, Vector4 value)
	{
		SetVar(name, value);
	}

	public void SetQuaternion(string name, Quaternion value)
	{
		SetVar(name, value);
	}

	public void SetColor(string name, Color value)
	{
		SetVar(name, value);
	}

	public void SetString(string name, string value)
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

	public void UnsetVar(string name)
	{
		SetVar(name, Var.Null);
	}

	public void SetVar(string name, Var value)
	{
		uservars.SetVar(name, value);
	}

	public void SetPose(string name, Pose pose)
	{
		uservars.SetPose(name, pose);
	}

	public void UnsetPose(string name)
	{
		uservars.UnsetPose(name);
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

	public Var GetVar(string name)
	{
		return uservars.GetVar(name);
	}

	public static Var GetGlobalVar(string name)
	{
		return UserVarManager.GetGlobalVar(name);
	}

	public static Transform GetTransform(string name)
	{
		return UserVarManager.GetTransform(name);
	}

	public Pose GetPose(string name)
	{
		return uservars.GetPose(name);
	}

	public void CrossFade(string blender, int index, float speed = 0.3f)
	{
		foreach (PoseNode posenode in update_controller.posenodes)
		{
			if (posenode is PoseBlend)
			{
				PoseBlend poseBlend = posenode as PoseBlend;
				if (poseBlend.Name.value.value_str == blender)
				{
					poseBlend.CrossFade(index, speed);
				}
			}
		}
	}
}
