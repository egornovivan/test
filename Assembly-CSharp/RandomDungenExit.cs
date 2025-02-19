using Pathea;
using UnityEngine;

public class RandomDungenExit : MonoBehaviour
{
	private bool isShow;

	private void OnTriggerEnter(Collider target)
	{
		Debug.Log("Exit dungen");
		if (!(null == target.GetComponentInParent<MainPlayerCmpt>()) && !isShow)
		{
			isShow = true;
			MessageBox_N.ShowYNBox(PELocalization.GetString(8000186), SceneTranslate, SetFalse);
		}
	}

	public void SceneTranslate()
	{
		RandomDungenMgr.Instance.ExitDungen();
	}

	public void SetFalse()
	{
		isShow = false;
	}
}
