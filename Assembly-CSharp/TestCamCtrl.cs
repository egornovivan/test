using UnityEngine;

public class TestCamCtrl : MonoBehaviour
{
	public Transform m_Target;

	public float m_MoveSpeed;

	private void Start()
	{
		Invoke("AttachCam", 0.5f);
	}

	private void AttachCam()
	{
		if (null != m_Target)
		{
			Transform child = AiUtil.GetChild(m_Target, "Female");
			if (null != child)
			{
				ZXCameraCtrl component = GetComponent<ZXCameraCtrl>();
				component.Following = child.transform;
			}
			else
			{
				Invoke("AttachCam", 0.5f);
			}
		}
	}

	private void Update()
	{
		base.transform.position += m_MoveSpeed * Vector3.forward * Time.deltaTime;
	}
}
