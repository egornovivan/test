using UnityEngine;

public class FogController : MonoBehaviour
{
	public Camera mCam;

	public GameObject mWaterObj;

	private void Update()
	{
		if (mWaterObj != null)
		{
			base.transform.position = new Vector3(mCam.transform.position.x, mWaterObj.transform.position.y, mCam.transform.position.z);
		}
		else
		{
			base.transform.position = new Vector3(mCam.transform.position.x, 0f, mCam.transform.position.z);
		}
	}
}
