using UnityEngine;

public class BuildBlockCamera : MonoBehaviour
{
	public float m_camSpeed = 20f;

	public float m_accRatio = 2f;

	public float m_sensitivityX = 5f;

	public float m_sensitivityY = 5f;

	private float mRotX;

	private float mRotY;

	private Vector3 DefaultPos;

	private Quaternion DefaultRot;

	private void Start()
	{
		mRotX = base.transform.eulerAngles.y;
		mRotY = 0f - base.transform.eulerAngles.x;
		DefaultPos = base.transform.position;
		DefaultRot = base.transform.rotation;
	}

	private void Update()
	{
		SceneMan.SetObserverTransform(base.transform);
		Debug.DrawLine(base.transform.position, base.transform.position + base.transform.forward * 100f);
		float num = m_camSpeed;
		if (Input.GetKey(KeyCode.LeftShift))
		{
			num /= m_accRatio;
		}
		if (Input.GetKey(KeyCode.LeftAlt))
		{
			num *= m_accRatio;
		}
		if (Input.GetKey(KeyCode.W))
		{
			base.transform.position += base.transform.forward * num * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.S))
		{
			base.transform.position += -base.transform.forward * num * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.A))
		{
			base.transform.position += -base.transform.right * num * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.D))
		{
			base.transform.position += base.transform.right * num * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.Q))
		{
			base.transform.position += base.transform.up * num * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.Z))
		{
			base.transform.position += -base.transform.up * num * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.R))
		{
			base.transform.position = DefaultPos;
			base.transform.rotation = DefaultRot;
		}
		if (Input.GetKey(KeyCode.Mouse1))
		{
			Ray ray = GetComponent<Camera>().ScreenPointToRay(new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, 0f));
			if (Physics.Raycast(ray, out var hitInfo, 1000f))
			{
				mRotX += Input.GetAxis("Mouse X") * m_sensitivityX;
				mRotY += Input.GetAxis("Mouse Y") * m_sensitivityY;
				mRotY = Mathf.Clamp(mRotY, -89.9f, -5f);
				base.transform.localEulerAngles = new Vector3(0f - mRotY, mRotX, 0f);
				base.transform.position = hitInfo.point - Vector3.Distance(GetComponent<Camera>().transform.position, hitInfo.point) * base.transform.forward;
			}
			else
			{
				ray = GetComponent<Camera>().ScreenPointToRay(new Vector3((float)Screen.width / 2f, (float)Screen.height / 3f, 0f));
				if (Physics.Raycast(ray, out hitInfo, 1000f))
				{
					mRotX += Input.GetAxis("Mouse X") * m_sensitivityX;
					mRotY += Input.GetAxis("Mouse Y") * m_sensitivityY;
					mRotY = Mathf.Clamp(mRotY, -89.9f, -5f);
					base.transform.localEulerAngles = new Vector3(0f - mRotY, mRotX, 0f);
					base.transform.position = hitInfo.point - Vector3.Distance(GetComponent<Camera>().transform.position, hitInfo.point) * base.transform.forward;
				}
			}
		}
		if (Input.GetKey(KeyCode.Mouse2))
		{
			Vector3 localPosition = base.transform.localPosition;
			localPosition += base.transform.right * Input.GetAxis("Mouse X") * 10f;
			localPosition += base.transform.forward * Input.GetAxis("Mouse Y") * 10f;
			base.transform.localPosition = localPosition;
		}
	}
}
