using PETools;
using UnityEngine;

public class AudioListenerController : MonoBehaviour
{
	private Transform mainPlayerTrans;

	private void Update()
	{
		if (PEUtil.MainCamTransform != null)
		{
			Transform mainCamTransform = PEUtil.MainCamTransform;
			base.transform.position = mainCamTransform.position;
			base.transform.rotation = mainCamTransform.rotation;
		}
	}
}
