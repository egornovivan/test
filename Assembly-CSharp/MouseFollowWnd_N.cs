using UnityEngine;

public class MouseFollowWnd_N : MonoBehaviour
{
	private static MouseFollowWnd_N mInstance;

	public Transform mWnd;

	public UILabel mText;

	public static MouseFollowWnd_N Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
	}

	private void Update()
	{
		mWnd.localPosition = PeCamera.mousePos + 10f * Vector3.forward;
	}

	public void SetText(string content)
	{
		mText.text = content;
	}
}
