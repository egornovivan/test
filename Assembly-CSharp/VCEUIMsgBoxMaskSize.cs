using UnityEngine;

public class VCEUIMsgBoxMaskSize : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		base.transform.localScale = new Vector3((Screen.width / 64 + 1) * 64, (Screen.height / 64 + 1) * 64, 1f);
	}
}
