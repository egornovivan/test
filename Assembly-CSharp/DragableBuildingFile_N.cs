using UnityEngine;

public class DragableBuildingFile_N : MonoBehaviour
{
	public UILabel mFileName;

	private GameObject mEventReceiver;

	public string FileName => mFileName.text;

	public void SetFile(string fileName, GameObject eventRecv)
	{
		mFileName.text = fileName;
		mEventReceiver = eventRecv;
	}

	private void OnDrag(Vector2 delta)
	{
		if ((bool)mEventReceiver)
		{
			mEventReceiver.SendMessage("OnFileDrag", this, SendMessageOptions.DontRequireReceiver);
		}
	}
}
