using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour
{
	public struct CameraParam
	{
		public float fov;

		public float farClip;

		public float nearClip;

		public int cullingMask;
	}

	public delegate void DCamNotify();

	public delegate void DCamModeNotify(CamMode mode);

	public Camera m_TargetCam;

	public Transform m_ModeGroup;

	public Transform m_EffectGroup;

	public Transform m_ConstraintGroup;

	public string m_DebugParam;

	public bool m_PushMode;

	public bool m_AddEffect;

	public bool m_PopMode;

	public bool m_RemoveMode;

	public bool m_AddConstraint;

	private Stack<CamMode> m_Modes;

	private List<CamEffect> m_Effects;

	private List<CamConstraint> m_Constraints;

	public bool m_MouseOnGUI;

	public bool m_MouseOpOnGUI;

	public bool m_MouseOnScroll;

	private CameraParam m_SavedParams;

	public string currentModeName
	{
		get
		{
			CleanUp();
			if (m_Modes.Count > 0)
			{
				CamMode camMode = m_Modes.Peek();
				if (camMode != null)
				{
					return camMode.name;
				}
			}
			return string.Empty;
		}
	}

	public CamMode currentMode
	{
		get
		{
			CleanUp();
			if (m_Modes.Count > 0)
			{
				CamMode camMode = m_Modes.Peek();
				if (camMode != null)
				{
					return camMode;
				}
			}
			return null;
		}
	}

	private CamMode currentModeRaw
	{
		get
		{
			if (m_Modes.Count > 0)
			{
				return m_Modes.Peek();
			}
			return null;
		}
	}

	private bool TargetCameraEnable
	{
		get
		{
			if (m_TargetCam == null)
			{
				return false;
			}
			if (!m_TargetCam.gameObject.activeInHierarchy)
			{
				return false;
			}
			if (!m_TargetCam.enabled)
			{
				return false;
			}
			return true;
		}
	}

	public Vector3 mousePosition
	{
		get
		{
			CamMode camMode = currentMode;
			if (camMode != null && camMode.m_LockCursor)
			{
				return m_TargetCam.ViewportToScreenPoint(new Vector3(camMode.m_TargetViewportPos.x, camMode.m_TargetViewportPos.y, 0f));
			}
			return Input.mousePosition;
		}
	}

	public Vector3 mouseViewportPosition
	{
		get
		{
			CamMode camMode = currentMode;
			if (camMode != null && camMode.m_LockCursor)
			{
				return new Vector3(camMode.m_TargetViewportPos.x, camMode.m_TargetViewportPos.y, 0f);
			}
			return m_TargetCam.ScreenToViewportPoint(Input.mousePosition);
		}
	}

	public Ray mouseRay => m_TargetCam.ScreenPointToRay(mousePosition);

	public Vector3 forward => m_TargetCam.transform.forward;

	public Vector3 horzForward
	{
		get
		{
			Vector3 normalized = Vector3.Cross(Vector3.up, m_TargetCam.transform.forward).normalized;
			return Vector3.Cross(normalized, Vector3.up).normalized;
		}
	}

	public float yaw => Mathf.Atan2(forward.x, forward.z) * 57.29578f;

	public Transform character
	{
		get
		{
			CamMode camMode = currentMode;
			if (camMode is CameraThirdPerson)
			{
				return (camMode as CameraThirdPerson).m_Character;
			}
			if (camMode is CameraFirstPerson)
			{
				return (camMode as CameraFirstPerson).m_Character;
			}
			return null;
		}
	}

	public event DCamNotify AfterInit;

	public event DCamNotify BeforeDestroy;

	public event DCamNotify BeforeAllModifier;

	public event DCamNotify AfterAllMode;

	public event DCamNotify AfterAllEffect;

	public event DCamNotify AfterAllModifier;

	public event DCamModeNotify OnSwitchMode;

	public void SaveParams()
	{
		m_SavedParams = default(CameraParam);
		if (TargetCameraEnable)
		{
			m_SavedParams.cullingMask = m_TargetCam.cullingMask;
			m_SavedParams.fov = m_TargetCam.fieldOfView;
			m_SavedParams.farClip = m_TargetCam.farClipPlane;
			m_SavedParams.nearClip = m_TargetCam.nearClipPlane;
		}
	}

	public void LoadParam()
	{
		if (TargetCameraEnable)
		{
			m_TargetCam.cullingMask = m_SavedParams.cullingMask;
			m_TargetCam.fieldOfView = m_SavedParams.fov;
			m_TargetCam.farClipPlane = m_SavedParams.farClip;
			m_TargetCam.nearClipPlane = m_SavedParams.nearClip;
		}
	}

	public void Init()
	{
		m_Modes = new Stack<CamMode>();
		m_Effects = new List<CamEffect>();
		m_Constraints = new List<CamConstraint>();
		if (this.AfterInit != null)
		{
			this.AfterInit();
		}
	}

	public void Destroy()
	{
		if (this.BeforeDestroy != null)
		{
			this.BeforeDestroy();
		}
		ClearAllModifiers();
		m_Modes = null;
		m_Effects = null;
		m_Constraints = null;
	}

	public CamMode PushMode(string mode_name)
	{
		CamMode camMode = CreateModifier("CamModes/" + mode_name) as CamMode;
		if (camMode != null)
		{
			camMode.m_Controller = this;
			CamMode camMode2 = currentMode;
			if (camMode2 != null)
			{
				camMode2.enabled = false;
			}
			m_Modes.Push(camMode);
			camMode.m_TargetCam = m_TargetCam;
			if (this.OnSwitchMode != null)
			{
				this.OnSwitchMode(camMode);
			}
		}
		return camMode;
	}

	public CamMode ReplaceMode(CamMode search, string replace_name)
	{
		if (search == null)
		{
			return null;
		}
		CleanUp();
		CamMode result = search;
		CamMode[] array = m_Modes.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (!(array[i] == search))
			{
				continue;
			}
			CamMode camMode = CreateModifier("CamModes/" + replace_name) as CamMode;
			if (camMode != null)
			{
				camMode.m_Controller = this;
				camMode.m_TargetCam = m_TargetCam;
				array[i] = camMode;
				Object.Destroy(search.gameObject);
				if (i == array.Length - 1 && this.OnSwitchMode != null)
				{
					this.OnSwitchMode(camMode);
				}
				result = camMode;
				break;
			}
		}
		m_Modes.Clear();
		for (int num = array.Length - 1; num >= 0; num--)
		{
			m_Modes.Push(array[num]);
		}
		return result;
	}

	public CamEffect AddEffect(string eff_name)
	{
		CamEffect camEffect = CreateModifier("CamEffects/" + eff_name) as CamEffect;
		camEffect.m_Controller = this;
		if (camEffect != null)
		{
			m_Effects.Add(camEffect);
		}
		return camEffect;
	}

	public CamConstraint AddConstraint(string cons_name)
	{
		CamConstraint camConstraint = CreateModifier("CamConstraints/" + cons_name) as CamConstraint;
		camConstraint.m_Controller = this;
		if (camConstraint != null)
		{
			m_Constraints.Add(camConstraint);
		}
		return camConstraint;
	}

	public CamMode FindMode(string mode_name)
	{
		foreach (CamMode mode in m_Modes)
		{
			if (mode == null || !(mode.name.ToLower() == mode_name.ToLower()))
			{
				continue;
			}
			return mode;
		}
		return null;
	}

	public CamEffect FindEffect(string eff_name)
	{
		return m_Effects.Find((CamEffect iter) => CamModifier.MatchName(iter, eff_name));
	}

	public CamConstraint FindConstraint(string cons_name)
	{
		return m_Constraints.Find((CamConstraint iter) => CamModifier.MatchName(iter, cons_name));
	}

	public void PopMode()
	{
		CleanUp();
		if (m_Modes.Count <= 0)
		{
			return;
		}
		CamMode camMode = m_Modes.Pop();
		if (camMode != null)
		{
			Object.Destroy(camMode.gameObject);
		}
		CamMode camMode2 = currentMode;
		if (camMode2 != null)
		{
			camMode2.enabled = true;
			camMode2.m_TargetCam = m_TargetCam;
			camMode2.ModeEnter();
			if (this.OnSwitchMode != null)
			{
				this.OnSwitchMode(camMode2);
			}
		}
	}

	public void RemoveMode(string mode_name)
	{
		CamMode camMode = FindMode(mode_name);
		if (camMode != null)
		{
			Object.Destroy(camMode.gameObject);
		}
	}

	public void RemoveEffect(string eff_name)
	{
		CamEffect camEffect = FindEffect(eff_name);
		if (camEffect != null)
		{
			Object.Destroy(camEffect.gameObject);
		}
	}

	public void RemoveConstraint(string cons_name)
	{
		CamConstraint camConstraint = FindConstraint(cons_name);
		if (camConstraint != null)
		{
			Object.Destroy(camConstraint.gameObject);
		}
	}

	public void ClearModes()
	{
		if (m_Modes == null)
		{
			return;
		}
		foreach (CamMode mode in m_Modes)
		{
			if (mode != null)
			{
				Object.Destroy(mode.gameObject);
			}
		}
		m_Modes.Clear();
	}

	public void ClearEffects()
	{
		if (m_Effects == null)
		{
			return;
		}
		foreach (CamEffect effect in m_Effects)
		{
			if (effect != null)
			{
				Object.Destroy(effect.gameObject);
			}
		}
		m_Effects.Clear();
	}

	public void ClearConstraints()
	{
		if (m_Constraints == null)
		{
			return;
		}
		foreach (CamConstraint constraint in m_Constraints)
		{
			if (constraint != null)
			{
				Object.Destroy(constraint.gameObject);
			}
		}
		m_Constraints.Clear();
	}

	public void ClearAllModifiers()
	{
		ClearConstraints();
		ClearEffects();
		ClearModes();
		Cursor.lockState = (Screen.fullScreen ? CursorLockMode.Confined : CursorLockMode.None);
		Cursor.visible = true;
	}

	private CamModifier CreateModifier(string respath)
	{
		GameObject gameObject = Resources.Load(respath) as GameObject;
		if (gameObject == null)
		{
			return null;
		}
		GameObject gameObject2 = Object.Instantiate(gameObject);
		if (gameObject2 == null)
		{
			return null;
		}
		gameObject2.name = gameObject.name;
		CamModifier component = gameObject2.GetComponent<CamModifier>();
		component.name = gameObject.name;
		if (component == null)
		{
			Object.Destroy(gameObject2);
			return null;
		}
		if (component is CamMode)
		{
			gameObject2.transform.parent = m_ModeGroup;
		}
		else if (component is CamEffect)
		{
			gameObject2.transform.parent = m_EffectGroup;
		}
		else if (component is CamConstraint)
		{
			gameObject2.transform.parent = m_ConstraintGroup;
		}
		else
		{
			gameObject2.transform.parent = base.transform;
		}
		return component;
	}

	private void CleanUp()
	{
		bool flag = false;
		while (m_Modes.Count > 0 && m_Modes.Peek() == null)
		{
			m_Modes.Pop();
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		CamMode camMode = currentModeRaw;
		if (camMode != null)
		{
			camMode.enabled = true;
			camMode.m_TargetCam = m_TargetCam;
			camMode.ModeEnter();
			if (this.OnSwitchMode != null)
			{
				this.OnSwitchMode(camMode);
			}
		}
	}

	private void UpdateModes()
	{
		CleanUp();
		if (m_Modes.Count > 0)
		{
			CamMode camMode = m_Modes.Peek();
			{
				foreach (CamMode mode in m_Modes)
				{
					if (!(mode != null))
					{
						continue;
					}
					if (mode != camMode)
					{
						if (mode.enabled)
						{
							mode.enabled = false;
						}
					}
					else if (!mode.enabled)
					{
						mode.enabled = true;
					}
				}
				return;
			}
		}
		Cursor.lockState = (Screen.fullScreen ? CursorLockMode.Confined : CursorLockMode.None);
		Cursor.visible = true;
	}

	private void Awake()
	{
		Init();
	}

	private void Start()
	{
		SaveParams();
	}

	private void OnDestroy()
	{
		Destroy();
	}

	private void Update()
	{
		UpdateDebugTools();
		if (!TargetCameraEnable)
		{
			return;
		}
		UpdateModes();
		if (m_Modes.Count > 0)
		{
			CamMode camMode = m_Modes.Peek();
			if (camMode != null)
			{
				camMode.UserInput();
			}
		}
	}

	private void LateUpdate()
	{
		if (!TargetCameraEnable)
		{
			return;
		}
		LoadParam();
		if (this.BeforeAllModifier != null)
		{
			this.BeforeAllModifier();
		}
		if (m_Modes.Count > 0)
		{
			CamMode camMode = m_Modes.Peek();
			if (camMode != null)
			{
				camMode.m_TargetCam = m_TargetCam;
				camMode.Do();
			}
		}
		if (this.AfterAllMode != null)
		{
			this.AfterAllMode();
		}
		foreach (CamEffect effect in m_Effects)
		{
			if (effect != null)
			{
				effect.m_TargetCam = m_TargetCam;
				effect.Do();
			}
		}
		if (this.AfterAllEffect != null)
		{
			this.AfterAllEffect();
		}
		foreach (CamConstraint constraint in m_Constraints)
		{
			if (constraint != null)
			{
				constraint.m_TargetCam = m_TargetCam;
				constraint.Do();
			}
		}
		if (this.AfterAllModifier != null)
		{
			this.AfterAllModifier();
		}
		m_MouseOnGUI = false;
		m_MouseOpOnGUI = false;
		m_MouseOnScroll = false;
	}

	private void UpdateDebugTools()
	{
		if (Application.isEditor)
		{
			if (m_PushMode)
			{
				m_PushMode = false;
				PushMode(m_DebugParam);
			}
			if (m_AddEffect)
			{
				m_AddEffect = false;
				AddEffect(m_DebugParam);
			}
			if (m_RemoveMode)
			{
				m_RemoveMode = false;
				RemoveMode(m_DebugParam);
			}
			if (m_PopMode)
			{
				m_PopMode = false;
				PopMode();
			}
			if (m_AddConstraint)
			{
				m_AddConstraint = false;
				AddConstraint(m_DebugParam);
			}
		}
	}
}
