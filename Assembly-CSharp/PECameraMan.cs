using PETools;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class PECameraMan : MonoBehaviour
{
	private static PECameraMan s_Instance;

	public CamController m_Controller;

	public GameObject m_AmbientLights;

	private int m_ControlType = 1;

	private CameraYawPitchRoll m_NormalMode;

	private bool _sync_roll;

	public static PECameraMan Instance => s_Instance;

	public int ControlType => m_ControlType;

	public bool SyncRoll
	{
		get
		{
			return _sync_roll;
		}
		set
		{
			_sync_roll = value;
		}
	}

	public Vector3 mousePos => m_Controller.mousePosition;

	public Ray mouseRay
	{
		get
		{
			if (null == m_Controller)
			{
				Camera.main.ScreenPointToRay(Input.mousePosition);
			}
			return m_Controller.mouseRay;
		}
	}

	public Vector3 camPosition => m_Controller.m_TargetCam.transform.position;

	public Vector3 forward => m_Controller.forward;

	public Vector3 horzForward => m_Controller.horzForward;

	public bool underWater
	{
		get
		{
			if (null == m_Controller || null == m_Controller.m_TargetCam)
			{
				return false;
			}
			return PE.PointInWater(m_Controller.m_TargetCam.transform.position) > 0.5f;
		}
	}

	public static void Create()
	{
		Object.Instantiate(Resources.Load("Camera Controller"));
	}

	private void Awake()
	{
		s_Instance = this;
		if (m_Controller.m_TargetCam == null)
		{
			m_Controller.m_TargetCam = CamMediator.MainCamera;
		}
		LoadControlType();
		ChangeNormalMode();
	}

	private void Start()
	{
		ApplySysSetting();
		AddTerrainConstraint();
	}

	private void Update()
	{
		CreateNearCamera();
		CreateAmbientLight();
		UpdateControlType();
		UpdateCameraCursor();
		UpdateSyncRoll();
	}

	private T PushMode<T>(string prefabName) where T : CamMode
	{
		T val = m_Controller.PushMode(prefabName) as T;
		if (null == val)
		{
			Debug.LogError("push camera mode " + prefabName + " error.");
		}
		return val;
	}

	public void RemoveCamMode(CamMode mode)
	{
		if (!(null == mode))
		{
			Object.Destroy(mode.gameObject);
		}
	}

	public CameraYawPitchRoll SetNormalMode(Transform target)
	{
		string prefabName = "Normal Mode F" + m_ControlType;
		CameraYawPitchRoll cameraYawPitchRoll = PushMode<CameraYawPitchRoll>(prefabName);
		if (cameraYawPitchRoll is CameraThirdPerson)
		{
			(cameraYawPitchRoll as CameraThirdPerson).m_Character = target;
		}
		if (cameraYawPitchRoll is CameraFirstPerson)
		{
			(cameraYawPitchRoll as CameraFirstPerson).m_Character = target;
		}
		cameraYawPitchRoll.ModeEnter();
		m_NormalMode = cameraYawPitchRoll;
		return cameraYawPitchRoll;
	}

	private CameraYawPitchRoll ChangeNormalMode()
	{
		if (m_NormalMode != null)
		{
			CameraThirdPerson cameraThirdPerson = m_NormalMode as CameraThirdPerson;
			CameraFirstPerson cameraFirstPerson = m_NormalMode as CameraFirstPerson;
			Transform character = null;
			if (cameraThirdPerson != null)
			{
				character = cameraThirdPerson.m_Character;
			}
			if (cameraFirstPerson != null)
			{
				character = cameraFirstPerson.m_Character;
			}
			string replace_name = "Normal Mode F" + m_ControlType;
			m_NormalMode = m_Controller.ReplaceMode(m_NormalMode, replace_name) as CameraYawPitchRoll;
			if (m_NormalMode is CameraThirdPerson)
			{
				(m_NormalMode as CameraThirdPerson).m_Character = character;
			}
			if (m_NormalMode is CameraFirstPerson)
			{
				(m_NormalMode as CameraFirstPerson).m_Character = character;
			}
			if (m_Controller.currentMode == m_NormalMode)
			{
				m_NormalMode.ModeEnter();
			}
		}
		return m_NormalMode;
	}

	public CameraThirdPerson EnterShoot(Transform target, float curDist)
	{
		string prefabName = "3rd Person Shoot";
		CameraThirdPerson cameraThirdPerson = PushMode<CameraThirdPerson>(prefabName);
		cameraThirdPerson.m_Character = target;
		cameraThirdPerson.m_Distance = curDist;
		cameraThirdPerson.ModeEnter();
		return cameraThirdPerson;
	}

	public CameraThirdPerson EnterVehicle(Transform target)
	{
		string prefabName = "3rd Person Vehicle";
		CameraThirdPerson cameraThirdPerson = PushMode<CameraThirdPerson>(prefabName);
		cameraThirdPerson.m_Character = target;
		cameraThirdPerson.ModeEnter();
		return cameraThirdPerson;
	}

	public CameraThirdPerson EnterVehicleArm(Transform target)
	{
		string prefabName = "3rd Person Vehicle Arm";
		CameraThirdPerson cameraThirdPerson = PushMode<CameraThirdPerson>(prefabName);
		cameraThirdPerson.m_Character = target;
		cameraThirdPerson.ModeEnter();
		return cameraThirdPerson;
	}

	public CameraFreeLook EnterFreeLook()
	{
		string prefabName = "Free Look";
		CameraFreeLook cameraFreeLook = PushMode<CameraFreeLook>(prefabName);
		cameraFreeLook.ModeEnter();
		return cameraFreeLook;
	}

	public CameraThirdPerson EnterBuild(Transform target)
	{
		return null;
	}

	public CameraFreeLook EnterFreeLookBuild()
	{
		string prefabName = "Free Look Build";
		CameraFreeLook cameraFreeLook = PushMode<CameraFreeLook>(prefabName);
		cameraFreeLook.ModeEnter();
		return cameraFreeLook;
	}

	public CameraThirdPerson EnterFollow(Transform target)
	{
		string prefabName = "Follow";
		CameraThirdPerson cameraThirdPerson = PushMode<CameraThirdPerson>(prefabName);
		cameraThirdPerson.m_Character = target;
		cameraThirdPerson.ModeEnter();
		return cameraThirdPerson;
	}

	public CameraYawPitchRoll EnterClimb(Transform target)
	{
		int num = m_ControlType;
		if (num == 3)
		{
			num = 2;
		}
		string prefabName = "Normal Mode F" + num;
		CameraYawPitchRoll cameraYawPitchRoll = PushMode<CameraYawPitchRoll>(prefabName);
		if (cameraYawPitchRoll is CameraThirdPerson)
		{
			(cameraYawPitchRoll as CameraThirdPerson).m_Character = target;
		}
		if (cameraYawPitchRoll is CameraFirstPerson)
		{
			(cameraYawPitchRoll as CameraFirstPerson).m_Character = target;
		}
		cameraYawPitchRoll.ModeEnter();
		return cameraYawPitchRoll;
	}

	public CameraShakeEffect ShakeEffect(string prefabName, Vector3 pos, float strength = 1f, float max_strength = 20f, float delayTime = 0f, float lifeTime = 5f)
	{
		CameraShakeEffect cameraShakeEffect = m_Controller.AddEffect(prefabName) as CameraShakeEffect;
		cameraShakeEffect.m_Multiplier = Mathf.Clamp(strength * 150f / (pos - camPosition).sqrMagnitude, 0f, max_strength);
		cameraShakeEffect.Invoke("Shake", delayTime);
		DestroyTimer component = cameraShakeEffect.GetComponent<DestroyTimer>();
		component.m_LifeTime = lifeTime;
		return cameraShakeEffect;
	}

	public CameraHitEffect HitEffect(Vector3 dir, float delayTime = 0f, float lifeTime = 1f)
	{
		CameraHitEffect cameraHitEffect = m_Controller.FindEffect("Hit Effect") as CameraHitEffect;
		if (cameraHitEffect != null)
		{
			return null;
		}
		cameraHitEffect = m_Controller.AddEffect("Hit Effect") as CameraHitEffect;
		if (cameraHitEffect == null)
		{
			return null;
		}
		cameraHitEffect.m_Dir = dir;
		cameraHitEffect.m_RotDir = Vector3.Cross(dir, m_Controller.m_TargetCam.transform.up);
		return cameraHitEffect;
	}

	public CameraShootEffect ShootEffect(Vector3 dir, float delayTime = 0f, float lifeTime = 1f)
	{
		CameraShootEffect cameraShootEffect = m_Controller.AddEffect("Shoot Effect") as CameraShootEffect;
		if (cameraShootEffect == null)
		{
			return null;
		}
		cameraShootEffect.m_Dir = dir;
		cameraShootEffect.m_RotDir = Vector3.Cross(dir, m_Controller.m_TargetCam.transform.up);
		return cameraShootEffect;
	}

	public CameraShootEffect BowShootEffect(Vector3 dir, float delayTime = 0f, float lifeTime = 1f)
	{
		CameraShootEffect cameraShootEffect = m_Controller.AddEffect("Bow Shoot Effect") as CameraShootEffect;
		if (cameraShootEffect == null)
		{
			return null;
		}
		cameraShootEffect.m_Dir = dir;
		cameraShootEffect.m_RotDir = Vector3.Cross(dir, m_Controller.m_TargetCam.transform.up);
		return cameraShootEffect;
	}

	public CameraShootEffect LaserShootEffect(Vector3 dir, float delayTime = 0f, float lifeTime = 1f)
	{
		CameraShootEffect cameraShootEffect = m_Controller.AddEffect("Laser Shoot Effect") as CameraShootEffect;
		if (cameraShootEffect == null)
		{
			return null;
		}
		cameraShootEffect.m_Dir = dir;
		cameraShootEffect.m_RotDir = Vector3.Cross(dir, m_Controller.m_TargetCam.transform.up);
		return cameraShootEffect;
	}

	public CameraShootEffect ShotgunShootEffect(Vector3 dir, float delayTime = 0f, float lifeTime = 1f)
	{
		CameraShootEffect cameraShootEffect = m_Controller.AddEffect("Shotgun Shoot Effect") as CameraShootEffect;
		if (cameraShootEffect == null)
		{
			return null;
		}
		cameraShootEffect.m_Dir = dir;
		cameraShootEffect.m_RotDir = Vector3.Cross(dir, m_Controller.m_TargetCam.transform.up);
		return cameraShootEffect;
	}

	public CamEffect AddWalkEffect()
	{
		return m_Controller.AddEffect("Walk Effect");
	}

	public CamConstraint AddTerrainConstraint()
	{
		return m_Controller.AddConstraint("Terrain Constraint");
	}

	public static void DepthScEnable(bool enable)
	{
		DepthOfField component = Camera.main.GetComponent<DepthOfField>();
		if (null != component)
		{
			component.enabled = enable;
		}
	}

	public static void SSAOEnable(bool enable)
	{
		SSAOPro component = Camera.main.GetComponent<SSAOPro>();
		if (null != component)
		{
			component.enabled = enable;
		}
	}

	public static void AAEnable(bool enable)
	{
		Antialiasing component = Camera.main.GetComponent<Antialiasing>();
		if (null != component)
		{
			component.enabled = enable;
		}
	}

	public static void ApplySysSetting()
	{
		if (!(Camera.main == null))
		{
			AAEnable(SystemSettingData.Instance.mAntiAliasing > 0);
			DepthScEnable(SystemSettingData.Instance.mDepthBlur);
			SSAOEnable(SystemSettingData.Instance.mSSAO);
			Camera.main.hdr = SystemSettingData.Instance.HDREffect;
			PeCamera.SetVar("Camera Inertia", SystemSettingData.Instance.CamInertia);
			PeCamera.SetVar("Drive Camera Inertia", SystemSettingData.Instance.DriveCamInertia);
		}
	}

	private void UpdateControlType()
	{
		if (Input.GetKeyDown(KeyCode.F1))
		{
			m_ControlType = 1;
			ChangeNormalMode();
			SaveControlType();
		}
		if (Input.GetKeyDown(KeyCode.F2))
		{
			m_ControlType = 2;
			ChangeNormalMode();
			SaveControlType();
		}
	}

	private void SaveControlType()
	{
		PlayerPrefs.SetInt("ControlType", ControlType);
		PlayerPrefs.Save();
	}

	private void LoadControlType()
	{
		if (PlayerPrefs.HasKey("ControlType"))
		{
			m_ControlType = PlayerPrefs.GetInt("ControlType");
		}
		else
		{
			m_ControlType = 1;
		}
		if (m_ControlType < 1)
		{
			m_ControlType = 1;
		}
		if (m_ControlType > 2)
		{
			m_ControlType = 2;
		}
	}

	private void UpdateSyncRoll()
	{
		CameraThirdPerson cameraThirdPerson = m_Controller.currentMode as CameraThirdPerson;
		CameraFirstPerson cameraFirstPerson = m_Controller.currentMode as CameraFirstPerson;
		CameraYawPitchRoll cameraYawPitchRoll = m_Controller.currentMode as CameraYawPitchRoll;
		if (cameraThirdPerson != null)
		{
			cameraThirdPerson.m_SyncRoll = _sync_roll;
		}
		if (cameraFirstPerson != null)
		{
			cameraFirstPerson.m_SyncRoll = _sync_roll;
		}
		if (!_sync_roll && cameraYawPitchRoll != null)
		{
			cameraYawPitchRoll.m_Roll *= 0.85f;
			if (Mathf.Abs(cameraYawPitchRoll.m_Roll) < 0.005f)
			{
				cameraYawPitchRoll.m_Roll = 0f;
			}
		}
	}

	private void CreateNearCamera()
	{
	}

	private void CreateAmbientLight()
	{
		if (m_Controller.m_TargetCam != null && m_AmbientLights == null)
		{
			GameObject gameObject = Resources.Load("Camera ambient lights") as GameObject;
			if (gameObject != null)
			{
				m_AmbientLights = Object.Instantiate(gameObject);
				m_AmbientLights.transform.parent = m_Controller.m_TargetCam.transform;
				m_AmbientLights.transform.localPosition = Vector3.zero;
				m_AmbientLights.transform.localRotation = Quaternion.identity;
				m_AmbientLights.transform.localScale = Vector3.one;
			}
		}
	}

	private void UpdateCameraCursor()
	{
		if (null == m_Controller)
		{
			return;
		}
		CamMode currentMode = m_Controller.currentMode;
		if (!(null == currentMode))
		{
			bool flag = currentMode is CameraFirstPerson;
			if ((currentMode.m_Tag & 1) != 0 && UIStateMgr.Instance != null)
			{
				currentMode.m_LockCursor = !UIStateMgr.Instance.CurShowGui;
				currentMode.m_ShowTarget = !UIStateMgr.Instance.CurShowGui;
			}
		}
	}

	private void SyncNearCam()
	{
	}
}
