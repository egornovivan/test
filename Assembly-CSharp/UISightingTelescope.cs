using System;
using UnityEngine;

public class UISightingTelescope : UIBaseWidget
{
	public enum SightingType
	{
		Default,
		ShotGun,
		Bow,
		Null
	}

	private static UISightingTelescope mInstance;

	[SerializeField]
	private UIDefaultSighting mDefault;

	[SerializeField]
	private UIShotGunSighting mShotGun;

	[SerializeField]
	private UIBowSighting mBow;

	[SerializeField]
	private SightingType currentType;

	[SerializeField]
	private float mValue;

	[SerializeField]
	private GameObject m_OrthoAimGoDefault;

	[SerializeField]
	private GameObject m_OrthoAimGoShotGun;

	[SerializeField]
	private GameObject m_OrthoAimGoBow;

	private GameObject m_OrthoAimGo;

	private SightingType m_CurOrthoAimType;

	public int MaxOrthoAimAlpha = 1;

	public int MinOrthoAimAlpha;

	public int MaxDistance = 30;

	public int MinDistance;

	private bool m_UpdateUIWnd;

	public static UISightingTelescope Instance => mInstance;

	private Vector2 viewPos => PeCamera.cursorPos;

	public float Scale
	{
		set
		{
			mValue = Mathf.Clamp01(value);
		}
	}

	public SightingType CurType => currentType;

	public override void OnCreate()
	{
		base.OnCreate();
		mInstance = this;
		currentType = SightingType.Null;
	}

	public void Show(SightingType type)
	{
		currentType = type;
		UpdateType();
		m_UpdateUIWnd = true;
		if (GameUI.Instance.mMissionTrackWnd.isShow)
		{
			GameUI.Instance.mMissionTrackWnd.EnableWndDrag(enable: false);
		}
		if (GameUI.Instance.mItemsTrackWnd.isShow)
		{
			GameUI.Instance.mItemsTrackWnd.EnableWndDrag(enable: false);
		}
	}

	public void ExitShootMode()
	{
		EnableOrthoAimPoint(enabel: false);
		currentType = SightingType.Null;
		UpdateType();
		m_UpdateUIWnd = true;
		if (GameUI.Instance.mMissionTrackWnd.isShow)
		{
			GameUI.Instance.mMissionTrackWnd.EnableWndDrag(enable: true);
		}
		if (GameUI.Instance.mItemsTrackWnd.isShow)
		{
			GameUI.Instance.mItemsTrackWnd.EnableWndDrag(enable: true);
		}
	}

	public void UpdateType()
	{
		mDefault.gameObject.SetActive(currentType == SightingType.Default);
		mShotGun.gameObject.SetActive(currentType == SightingType.ShotGun);
		mBow.gameObject.SetActive(currentType == SightingType.Bow);
	}

	public void EnableOrthoAimPoint(bool enabel)
	{
		if (currentType != SightingType.Null)
		{
			if (enabel)
			{
				ShowOrthoAimByCurType();
			}
			else
			{
				HideOrthoAim();
			}
		}
	}

	public void SetOrthoAimPointPos(Vector3 pointPos)
	{
		if (currentType != SightingType.Null)
		{
			if (m_CurOrthoAimType != currentType || null == m_OrthoAimGo)
			{
				ShowOrthoAimByCurType();
			}
			m_OrthoAimGo.transform.localPosition = pointPos - new Vector3(0.5f * (float)Screen.width, 0.5f * (float)Screen.height, 0f);
			ChangeAlphaByDistance();
		}
	}

	private void ShowOrthoAimByCurType()
	{
		if (m_CurOrthoAimType != currentType || null == m_OrthoAimGo)
		{
			HideAllOrthoAim();
			switch (currentType)
			{
			case SightingType.Default:
				m_OrthoAimGo = m_OrthoAimGoDefault;
				break;
			case SightingType.ShotGun:
				m_OrthoAimGo = m_OrthoAimGoShotGun;
				break;
			case SightingType.Bow:
				m_OrthoAimGo = m_OrthoAimGoBow;
				break;
			case SightingType.Null:
				m_OrthoAimGo = null;
				break;
			}
			m_CurOrthoAimType = currentType;
		}
		if (m_OrthoAimGo != null)
		{
			m_OrthoAimGo.transform.localPosition = Vector3.zero;
			m_OrthoAimGo.SetActive(value: true);
		}
	}

	private void HideOrthoAim()
	{
		if (m_CurOrthoAimType == currentType && null != m_OrthoAimGo)
		{
			m_OrthoAimGo.SetActive(value: false);
		}
		else
		{
			HideAllOrthoAim();
		}
	}

	private void HideAllOrthoAim()
	{
		if (m_OrthoAimGoDefault.activeSelf)
		{
			m_OrthoAimGoDefault.SetActive(value: false);
		}
		if (m_OrthoAimGoShotGun.activeSelf)
		{
			m_OrthoAimGoShotGun.SetActive(value: false);
		}
		if (m_OrthoAimGoBow.activeSelf)
		{
			m_OrthoAimGoBow.SetActive(value: false);
		}
	}

	public void OnShoot()
	{
		if (currentType != SightingType.Null)
		{
			GetCurSightingByCurType().OnShoot();
		}
	}

	private void ChangeAlphaByDistance()
	{
		if (!(null != m_OrthoAimGo) || currentType == SightingType.Null)
		{
			return;
		}
		UISprite[] componentsInChildren = m_OrthoAimGo.GetComponentsInChildren<UISprite>();
		if (componentsInChildren.Length > 0)
		{
			Vector2 vector = GetCurSightingByCurType().transform.localPosition;
			Vector2 vector2 = m_OrthoAimGo.transform.localPosition;
			float magnitude = (vector2 - vector).magnitude;
			magnitude = Mathf.Clamp(magnitude, MinDistance, MaxDistance);
			float alpha = Mathf.Lerp(MinOrthoAimAlpha, MaxOrthoAimAlpha, magnitude / (float)(MaxDistance - MinDistance));
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].alpha = alpha;
			}
		}
	}

	private void InstantiateCurSighting()
	{
		GameObject original = GetCurSightingByCurType().gameObject;
		m_OrthoAimGo = UnityEngine.Object.Instantiate(original);
		m_OrthoAimGo.GetComponent<UIBaseSighting>().Value = 0f;
		m_OrthoAimGo.transform.parent = base.transform;
		m_OrthoAimGo.name = "OrthoAimGo";
		m_OrthoAimGo.transform.localPosition = Vector3.zero;
		m_OrthoAimGo.transform.localScale = Vector3.one;
		m_OrthoAimGo.SetActive(value: true);
		m_CurOrthoAimType = currentType;
	}

	private UIBaseSighting GetCurSightingByCurType()
	{
		return currentType switch
		{
			SightingType.Default => mDefault, 
			SightingType.ShotGun => mShotGun, 
			SightingType.Bow => mBow, 
			_ => null, 
		};
	}

	private void Update()
	{
		if (currentType != SightingType.Null)
		{
			Vector2 vector = viewPos;
			try
			{
				int num = Convert.ToInt32((vector.x - 0.5f) * (float)Screen.width);
				int num2 = Convert.ToInt32((vector.y - 0.5f) * (float)Screen.height);
				GetCurSightingByCurType().transform.localPosition = new Vector3(num, num2, 0f);
				GetCurSightingByCurType().Value = mValue;
			}
			catch
			{
			}
		}
	}

	private void LateUpdate()
	{
		if (m_UpdateUIWnd)
		{
			if (currentType != SightingType.Null)
			{
				GameUI.Instance.HideGameWnd();
			}
			else
			{
				GameUI.Instance.ShowGameWndAll();
			}
			m_UpdateUIWnd = false;
		}
	}
}
