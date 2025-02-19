using UnityEngine;

public class FileNameSelItem_N : MonoBehaviour
{
	public UILabel mTextLabel;

	private GameObject mEnventReceiver;

	public void SetText(string fileName, GameObject eventReceiver)
	{
		mTextLabel.text = fileName;
		mEnventReceiver = eventReceiver;
		GetComponent<UICheckbox>().radioButtonRoot = base.transform.parent;
	}

	private void OnSelected(bool selected)
	{
		if (selected && null != mEnventReceiver)
		{
			mEnventReceiver.SendMessage("OnFileSelected", mTextLabel.text, SendMessageOptions.DontRequireReceiver);
		}
	}
}
