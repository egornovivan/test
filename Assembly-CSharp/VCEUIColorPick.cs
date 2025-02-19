using UnityEngine;

public class VCEUIColorPick : MonoBehaviour
{
	private float m_Hue;

	private float m_Sat;

	private float m_Brt;

	private Color m_FinalColor = new Color(0f, 0f, 0f, 1f);

	private bool m_InitialLock = true;

	public Transform m_HSCircle;

	public UISprite m_HSPad;

	public UISlider m_BrtSlider;

	public UISprite m_BrtSliderColor;

	public UISlider m_RSlider;

	public UISlider m_GSlider;

	public UISlider m_BSlider;

	public UILabel m_RValLabel;

	public UILabel m_GValLabel;

	public UILabel m_BValLabel;

	public UISprite m_ColorRect;

	private bool bFocusedHS;

	public Color FinalColor
	{
		get
		{
			return m_FinalColor;
		}
		set
		{
			m_FinalColor = value;
			m_FinalColor.a = 1f;
			Vector3 vector = VCUtils.RGB2HSB(m_FinalColor, m_Hue, m_Sat);
			m_Hue = vector.x;
			m_Sat = vector.y;
			m_Brt = vector.z;
		}
	}

	public float R
	{
		get
		{
			return m_FinalColor.r;
		}
		set
		{
			m_FinalColor.r = value;
			Vector3 vector = VCUtils.RGB2HSB(m_FinalColor, m_Hue, m_Sat);
			m_Hue = vector.x;
			m_Sat = vector.y;
			m_Brt = vector.z;
		}
	}

	public float G
	{
		get
		{
			return m_FinalColor.g;
		}
		set
		{
			m_FinalColor.g = value;
			Vector3 vector = VCUtils.RGB2HSB(m_FinalColor, m_Hue, m_Sat);
			m_Hue = vector.x;
			m_Sat = vector.y;
			m_Brt = vector.z;
		}
	}

	public float B
	{
		get
		{
			return m_FinalColor.b;
		}
		set
		{
			m_FinalColor.b = value;
			Vector3 vector = VCUtils.RGB2HSB(m_FinalColor, m_Hue, m_Sat);
			m_Hue = vector.x;
			m_Sat = vector.y;
			m_Brt = vector.z;
		}
	}

	public Vector3 HSB
	{
		get
		{
			return VCUtils.RGB2HSB(m_FinalColor, m_Hue, m_Sat);
		}
		set
		{
			m_Hue = value.x;
			m_Sat = value.y;
			m_Brt = value.z;
			m_FinalColor = VCUtils.HSB2RGB(m_Hue, m_Sat, m_Brt);
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		m_InitialLock = false;
		UpdateHSPadLogic();
		UpdateUI();
	}

	public void UpdateUI()
	{
		m_ColorRect.color = m_FinalColor;
		m_HSCircle.localPosition = new Vector3(m_Hue / 360f, m_Sat, 0f);
		m_BrtSlider.sliderValue = m_Brt;
		m_BrtSliderColor.color = VCUtils.HSB2RGB(m_Hue, m_Sat, 1f);
		m_RSlider.sliderValue = m_FinalColor.r;
		m_GSlider.sliderValue = m_FinalColor.g;
		m_BSlider.sliderValue = m_FinalColor.b;
		m_RValLabel.text = (m_FinalColor.r * 100f).ToString("0") + "%";
		m_GValLabel.text = (m_FinalColor.g * 100f).ToString("0") + "%";
		m_BValLabel.text = (m_FinalColor.b * 100f).ToString("0") + "%";
	}

	private void OnRChange(float r)
	{
		if (!m_InitialLock)
		{
			R = r;
		}
	}

	private void OnGChange(float g)
	{
		if (!m_InitialLock)
		{
			G = g;
		}
	}

	private void OnBChange(float b)
	{
		if (!m_InitialLock)
		{
			B = b;
		}
	}

	private void UpdateHSPadLogic()
	{
		if (m_InitialLock)
		{
			return;
		}
		if (Input.GetMouseButtonDown(0))
		{
			if (Physics.Raycast(VCEInput.s_UIRay, out var hitInfo, 100f, VCConfig.s_UILayerMask))
			{
				if (hitInfo.collider.gameObject == m_HSPad.gameObject)
				{
					bFocusedHS = true;
				}
				else
				{
					bFocusedHS = false;
				}
			}
			else
			{
				bFocusedHS = false;
			}
		}
		if (Input.GetMouseButtonUp(0))
		{
			bFocusedHS = false;
		}
		if (Input.GetMouseButton(0) && bFocusedHS && Physics.Raycast(VCEInput.s_UIRay, out var hitInfo2, 100f, VCConfig.s_UILayerMask) && hitInfo2.collider.gameObject == m_HSPad.gameObject)
		{
			Vector3 vector = m_HSPad.transform.InverseTransformPoint(hitInfo2.point);
			vector.x = Mathf.Clamp(vector.x, 0f, 0.9995f);
			vector.y = Mathf.Clamp01(vector.y);
			HSB = new Vector3(vector.x * 360f, vector.y, m_Brt);
		}
	}

	private void OnBrtChange(float brt)
	{
		if (!m_InitialLock)
		{
			HSB = new Vector3(m_Hue, m_Sat, brt);
		}
	}
}
