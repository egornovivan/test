using UnityEngine;

public class VCEUISceneGizmoCam : MonoBehaviour
{
	public GUISkin m_GUISkin;

	public Material m_FocusMaterial;

	public Material m_OriginMaterial;

	public Material m_XAxisMaterial;

	public Material m_YAxisMaterial;

	public Material m_ZAxisMaterial;

	public Material m_NXAxisMaterial;

	public Material m_NYAxisMaterial;

	public Material m_NZAxisMaterial;

	public float m_TextOffset = 2.9f;

	public Renderer m_mrXAxis;

	public Renderer m_mrYAxis;

	public Renderer m_mrZAxis;

	public Renderer m_mrNXAxis;

	public Renderer m_mrNYAxis;

	public Renderer m_mrNZAxis;

	public Renderer m_mrOrigin;

	public Rect m_RenderRect = new Rect(0f, 0f, 1f, 1f);

	private Ray m_MouseRay;

	private float m_alpha_pos_x = 1f;

	private float m_alpha_pos_y = 1f;

	private float m_alpha_pos_z = 1f;

	private float m_alpha_neg_x = 1f;

	private float m_alpha_neg_y = 1f;

	private float m_alpha_neg_z = 1f;

	private float m_alpha_origin = 1f;

	private float m_alpha_threshold = 0.2f;

	private VCECamera m_MainCameraBehaviour;

	private bool xShow;

	private bool yShow;

	private bool zShow;

	private Vector3 xpoint;

	private Vector3 ypoint;

	private Vector3 zpoint;

	public static float AxisAlpha(float angle)
	{
		float num = angle / 25f;
		if (num > 1f)
		{
			num = 1f;
		}
		return num * num;
	}

	private void Start()
	{
		m_MainCameraBehaviour = VCEditor.Instance.m_MainCamera.GetComponent<VCECamera>();
		m_FocusMaterial = Object.Instantiate(m_FocusMaterial);
		m_OriginMaterial = Object.Instantiate(m_OriginMaterial);
		m_XAxisMaterial = Object.Instantiate(m_XAxisMaterial);
		m_YAxisMaterial = Object.Instantiate(m_YAxisMaterial);
		m_ZAxisMaterial = Object.Instantiate(m_ZAxisMaterial);
		m_NXAxisMaterial = Object.Instantiate(m_NXAxisMaterial);
		m_NYAxisMaterial = Object.Instantiate(m_NYAxisMaterial);
		m_NZAxisMaterial = Object.Instantiate(m_NZAxisMaterial);
	}

	private void LateUpdate()
	{
		if (!VCEditor.DocumentOpen())
		{
			return;
		}
		GetComponent<Camera>().pixelRect = m_RenderRect;
		float num = 7.5f;
		GetComponent<Camera>().transform.localPosition = Vector3.zero;
		GetComponent<Camera>().transform.localRotation = m_MainCameraBehaviour.transform.localRotation;
		GetComponent<Camera>().transform.localPosition -= GetComponent<Camera>().transform.forward * num;
		Vector3 forward = GetComponent<Camera>().transform.forward;
		m_alpha_pos_x = AxisAlpha(Vector3.Angle(forward, -Vector3.right));
		m_alpha_pos_y = AxisAlpha(Vector3.Angle(forward, -Vector3.up));
		m_alpha_pos_z = AxisAlpha(Vector3.Angle(forward, -Vector3.forward));
		m_alpha_neg_x = AxisAlpha(Vector3.Angle(forward, Vector3.right));
		m_alpha_neg_y = AxisAlpha(Vector3.Angle(forward, Vector3.up));
		m_alpha_neg_z = AxisAlpha(Vector3.Angle(forward, Vector3.forward));
		m_alpha_origin = 1f;
		m_XAxisMaterial.color = new Color(m_XAxisMaterial.color.r, m_XAxisMaterial.color.g, m_XAxisMaterial.color.b, m_alpha_pos_x);
		m_YAxisMaterial.color = new Color(m_YAxisMaterial.color.r, m_YAxisMaterial.color.g, m_YAxisMaterial.color.b, m_alpha_pos_y);
		m_ZAxisMaterial.color = new Color(m_ZAxisMaterial.color.r, m_ZAxisMaterial.color.g, m_ZAxisMaterial.color.b, m_alpha_pos_z);
		m_NXAxisMaterial.color = new Color(m_NXAxisMaterial.color.r, m_NXAxisMaterial.color.g, m_NXAxisMaterial.color.b, m_alpha_neg_x);
		m_NYAxisMaterial.color = new Color(m_NYAxisMaterial.color.r, m_NYAxisMaterial.color.g, m_NYAxisMaterial.color.b, m_alpha_neg_y);
		m_NZAxisMaterial.color = new Color(m_NZAxisMaterial.color.r, m_NZAxisMaterial.color.g, m_NZAxisMaterial.color.b, m_alpha_neg_z);
		m_OriginMaterial.color = new Color(m_OriginMaterial.color.r, m_OriginMaterial.color.g, m_OriginMaterial.color.b, m_alpha_origin);
		m_MouseRay = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
		Renderer renderer = null;
		float num2 = 1000f;
		float num3 = 0f;
		float num4 = 0f;
		m_mrXAxis.material = m_XAxisMaterial;
		m_mrYAxis.material = m_YAxisMaterial;
		m_mrZAxis.material = m_ZAxisMaterial;
		m_mrNXAxis.material = m_NXAxisMaterial;
		m_mrNYAxis.material = m_NYAxisMaterial;
		m_mrNZAxis.material = m_NZAxisMaterial;
		m_mrOrigin.material = m_OriginMaterial;
		if (m_alpha_pos_x > m_alpha_threshold && m_mrXAxis.GetComponent<Collider>().Raycast(m_MouseRay, out var hitInfo, 100f) && hitInfo.distance < num2)
		{
			num2 = hitInfo.distance;
			renderer = m_mrXAxis;
			num3 = 0f;
			num4 = 0f;
			VCEStatusBar.ShowText("Right view".ToLocalizationString(), 2f);
		}
		if (m_alpha_pos_y > m_alpha_threshold && m_mrYAxis.GetComponent<Collider>().Raycast(m_MouseRay, out hitInfo, 100f) && hitInfo.distance < num2)
		{
			num2 = hitInfo.distance;
			renderer = m_mrYAxis;
			num3 = 90f;
			num4 = 90f;
			VCEStatusBar.ShowText("Top view".ToLocalizationString(), 2f);
		}
		if (m_alpha_pos_z > m_alpha_threshold && m_mrZAxis.GetComponent<Collider>().Raycast(m_MouseRay, out hitInfo, 100f) && hitInfo.distance < num2)
		{
			num2 = hitInfo.distance;
			renderer = m_mrZAxis;
			num3 = 90f;
			num4 = 0f;
			VCEStatusBar.ShowText("Front view".ToLocalizationString(), 2f);
		}
		if (m_alpha_neg_x > m_alpha_threshold && m_mrNXAxis.GetComponent<Collider>().Raycast(m_MouseRay, out hitInfo, 100f) && hitInfo.distance < num2)
		{
			num2 = hitInfo.distance;
			renderer = m_mrNXAxis;
			num3 = 180f;
			num4 = 0f;
			VCEStatusBar.ShowText("Left view".ToLocalizationString(), 2f);
		}
		if (m_alpha_neg_y > m_alpha_threshold && m_mrNYAxis.GetComponent<Collider>().Raycast(m_MouseRay, out hitInfo, 100f) && hitInfo.distance < num2)
		{
			num2 = hitInfo.distance;
			renderer = m_mrNYAxis;
			num3 = 90f;
			num4 = -90f;
			VCEStatusBar.ShowText("Bottom view".ToLocalizationString(), 2f);
		}
		if (m_alpha_neg_z > m_alpha_threshold && m_mrNZAxis.GetComponent<Collider>().Raycast(m_MouseRay, out hitInfo, 100f) && hitInfo.distance < num2)
		{
			num2 = hitInfo.distance;
			renderer = m_mrNZAxis;
			num3 = -90f;
			num4 = 0f;
			VCEStatusBar.ShowText("Back view".ToLocalizationString(), 2f);
		}
		if (m_mrOrigin.GetComponent<Collider>().Raycast(m_MouseRay, out hitInfo, 100f) && hitInfo.distance < num2)
		{
			num2 = hitInfo.distance;
			renderer = m_mrOrigin;
			num3 = m_MainCameraBehaviour.BeginYaw;
			num4 = m_MainCameraBehaviour.BeginPitch;
			VCEStatusBar.ShowText("Reset camera".ToLocalizationString(), 2f);
		}
		if (num2 < 999f)
		{
			renderer.material = m_FocusMaterial;
			if (Input.GetMouseButtonDown(0))
			{
				for (; num3 < m_MainCameraBehaviour.Yaw - 180f; num3 += 360f)
				{
				}
				while (num3 > m_MainCameraBehaviour.Yaw + 180f)
				{
					num3 -= 360f;
				}
				for (; num4 < m_MainCameraBehaviour.Pitch - 180f; num4 += 360f)
				{
				}
				while (num4 > m_MainCameraBehaviour.Pitch + 180f)
				{
					num4 -= 360f;
				}
				m_MainCameraBehaviour.SetYaw(num3);
				m_MainCameraBehaviour.SetPitch(num4);
			}
		}
		xpoint = GetComponent<Camera>().WorldToScreenPoint(Vector3.right * m_TextOffset + base.transform.parent.position);
		ypoint = GetComponent<Camera>().WorldToScreenPoint(Vector3.up * m_TextOffset + base.transform.parent.position);
		zpoint = GetComponent<Camera>().WorldToScreenPoint(Vector3.forward * m_TextOffset + base.transform.parent.position);
		Ray ray = GetComponent<Camera>().ScreenPointToRay(xpoint);
		Ray ray2 = GetComponent<Camera>().ScreenPointToRay(ypoint);
		Ray ray3 = GetComponent<Camera>().ScreenPointToRay(zpoint);
		xShow = true;
		yShow = true;
		zShow = true;
		if (m_mrXAxis.GetComponent<Collider>().Raycast(ray, out hitInfo, 100f))
		{
			xShow = false;
		}
		if (m_mrYAxis.GetComponent<Collider>().Raycast(ray, out hitInfo, 100f))
		{
			xShow = false;
		}
		if (m_mrZAxis.GetComponent<Collider>().Raycast(ray, out hitInfo, 100f))
		{
			xShow = false;
		}
		if (m_mrNXAxis.GetComponent<Collider>().Raycast(ray, out hitInfo, 100f))
		{
			xShow = false;
		}
		if (m_mrNYAxis.GetComponent<Collider>().Raycast(ray, out hitInfo, 100f))
		{
			xShow = false;
		}
		if (m_mrNZAxis.GetComponent<Collider>().Raycast(ray, out hitInfo, 100f))
		{
			xShow = false;
		}
		if (m_mrOrigin.GetComponent<Collider>().Raycast(ray, out hitInfo, 100f))
		{
			xShow = false;
		}
		if (m_mrXAxis.GetComponent<Collider>().Raycast(ray2, out hitInfo, 100f))
		{
			yShow = false;
		}
		if (m_mrYAxis.GetComponent<Collider>().Raycast(ray2, out hitInfo, 100f))
		{
			yShow = false;
		}
		if (m_mrZAxis.GetComponent<Collider>().Raycast(ray2, out hitInfo, 100f))
		{
			yShow = false;
		}
		if (m_mrNXAxis.GetComponent<Collider>().Raycast(ray2, out hitInfo, 100f))
		{
			yShow = false;
		}
		if (m_mrNYAxis.GetComponent<Collider>().Raycast(ray2, out hitInfo, 100f))
		{
			yShow = false;
		}
		if (m_mrNZAxis.GetComponent<Collider>().Raycast(ray2, out hitInfo, 100f))
		{
			yShow = false;
		}
		if (m_mrOrigin.GetComponent<Collider>().Raycast(ray2, out hitInfo, 100f))
		{
			yShow = false;
		}
		if (m_mrXAxis.GetComponent<Collider>().Raycast(ray3, out hitInfo, 100f))
		{
			zShow = false;
		}
		if (m_mrYAxis.GetComponent<Collider>().Raycast(ray3, out hitInfo, 100f))
		{
			zShow = false;
		}
		if (m_mrZAxis.GetComponent<Collider>().Raycast(ray3, out hitInfo, 100f))
		{
			zShow = false;
		}
		if (m_mrNXAxis.GetComponent<Collider>().Raycast(ray3, out hitInfo, 100f))
		{
			zShow = false;
		}
		if (m_mrNYAxis.GetComponent<Collider>().Raycast(ray3, out hitInfo, 100f))
		{
			zShow = false;
		}
		if (m_mrNZAxis.GetComponent<Collider>().Raycast(ray3, out hitInfo, 100f))
		{
			zShow = false;
		}
		if (m_mrOrigin.GetComponent<Collider>().Raycast(ray3, out hitInfo, 100f))
		{
			zShow = false;
		}
	}

	private void OnGUI()
	{
		if (VCEditor.DocumentOpen())
		{
			GUI.skin = m_GUISkin;
			if (m_alpha_pos_x > m_alpha_threshold && xShow)
			{
				GUI.Label(new Rect(xpoint.x - 10f, (float)Screen.height - xpoint.y - 15f, 20f, 20f), "x", "AxisText");
			}
			if (m_alpha_pos_y > m_alpha_threshold && yShow)
			{
				GUI.Label(new Rect(ypoint.x - 10f, (float)Screen.height - ypoint.y - 15f, 20f, 20f), "y", "AxisText");
			}
			if (m_alpha_pos_z > m_alpha_threshold && zShow)
			{
				GUI.Label(new Rect(zpoint.x - 10f, (float)Screen.height - zpoint.y - 15f, 20f, 20f), "z", "AxisText");
			}
		}
	}
}
