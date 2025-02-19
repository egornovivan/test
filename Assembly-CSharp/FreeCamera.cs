using CameraForge;
using UnityEngine;

public class FreeCamera : MonoBehaviour
{
	public float m_camSpeed = 20f;

	public float m_accRatio = 2f;

	public float m_sensitivityX = 5f;

	public float m_sensitivityY = 5f;

	private float mRotX;

	private float mRotY;

	private static bool _freeCameraMode;

	public static bool FreeCameraMode => _freeCameraMode;

	private void Start()
	{
		mRotX = base.transform.eulerAngles.y;
		mRotY = 0f - base.transform.eulerAngles.x;
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
		if (Input.GetKey(KeyCode.Mouse1))
		{
			mRotX += Input.GetAxis("Mouse X") * m_sensitivityX;
			mRotY += Input.GetAxis("Mouse Y") * m_sensitivityY;
			if (mRotY > 89.9f)
			{
				mRotY = 89.9f;
			}
			if (mRotY < -89.9f)
			{
				mRotY = -89.9f;
			}
			base.transform.localEulerAngles = new Vector3(0f - mRotY, mRotX, 0f);
		}
		if (Input.GetKey(KeyCode.Mouse2))
		{
			Vector3 localPosition = base.transform.localPosition;
			localPosition += base.transform.right * Input.GetAxis("Mouse X") * 10f;
			localPosition += base.transform.forward * Input.GetAxis("Mouse Y") * 10f;
			base.transform.localPosition = localPosition;
		}
	}

	public static void SetFreeCameraMode()
	{
		if (SceneMan.self == null)
		{
			Debug.LogError("[SetFreeCameraMode]Invalid scene");
			return;
		}
		_freeCameraMode = !_freeCameraMode;
		if (_freeCameraMode)
		{
			CameraController component = Camera.main.GetComponent<CameraController>();
			if (component != null)
			{
				component.enabled = false;
			}
			FreeCamera component2 = Camera.main.GetComponent<FreeCamera>();
			if (component2 != null)
			{
				component.enabled = true;
			}
			else
			{
				Camera.main.gameObject.AddComponent<FreeCamera>();
			}
		}
		else
		{
			FreeCamera component3 = Camera.main.GetComponent<FreeCamera>();
			if (component3 != null)
			{
				Object.Destroy(component3);
			}
			CameraController component4 = Camera.main.GetComponent<CameraController>();
			if (component4 != null)
			{
				component4.enabled = true;
			}
		}
		Debug.LogError("[SetFreeCameraMode]FreeCameraMode:" + _freeCameraMode);
	}
}
