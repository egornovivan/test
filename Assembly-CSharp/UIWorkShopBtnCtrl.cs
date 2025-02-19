using UnityEngine;

public class UIWorkShopBtnCtrl : MonoBehaviour
{
	private UIWorkShopCtrl mWorkShopCtrl;

	public GameObject mWorkShopPrefab;

	public GameObject mCenterAuthor;

	private void Start()
	{
		if (!GameConfig.IsMultiMode)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
	}

	private void BtnWorkShopOnClick()
	{
		if (mWorkShopCtrl == null && GameConfig.IsMultiMode)
		{
			GameObject gameObject = Object.Instantiate(mWorkShopPrefab);
			gameObject.transform.transform.parent = mCenterAuthor.transform;
			gameObject.transform.localPosition = new Vector3(0f, 40f, 0f);
			gameObject.transform.localScale = Vector3.one;
			mWorkShopCtrl = gameObject.GetComponent<UIWorkShopCtrl>();
			if (!(mWorkShopCtrl == null))
			{
				mWorkShopCtrl.e_BtnClose += WorkShopOnClose;
				gameObject.SetActive(value: true);
			}
		}
		else
		{
			mWorkShopCtrl.gameObject.SetActive(value: true);
		}
	}

	private void WorkShopOnClose()
	{
		if (!(mWorkShopCtrl == null))
		{
			Object.Destroy(mWorkShopCtrl.gameObject);
			mWorkShopCtrl = null;
		}
	}
}
