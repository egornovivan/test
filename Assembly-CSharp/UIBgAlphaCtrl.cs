using UnityEngine;

public class UIBgAlphaCtrl : MonoBehaviour
{
	[SerializeField]
	private BoxCollider mCollider;

	[SerializeField]
	private UISprite mBg_1;

	[SerializeField]
	private UISprite mBg_2;

	[SerializeField]
	private UISprite mBtnClose;

	[SerializeField]
	private GameObject mSpecular;

	private void Start()
	{
	}

	private void Update()
	{
		if (!(mCollider == null) && !(UICamera.currentCamera == null) && !(mBg_1 == null) && !(mBg_2 == null))
		{
			Ray ray = UICamera.currentCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo;
			bool active = mCollider.Raycast(ray, out hitInfo, 100f);
			mBg_1.enabled = active;
			mBg_2.enabled = active;
			mBtnClose.enabled = active;
			mSpecular.SetActive(active);
		}
	}
}
