using UnityEngine;

public class UICompoundBtnEffct : MonoBehaviour
{
	[SerializeField]
	private UISprite mSpr;

	[SerializeField]
	private float dtTime = 0.3f;

	private N_ImageButton mBtn;

	private UICompoundWndControl comCtrl;

	private float time;

	private void Start()
	{
		mBtn = GetComponent<N_ImageButton>();
		if (GameUI.Instance != null)
		{
			comCtrl = GameUI.Instance.mCompoundWndCtrl;
		}
	}

	private void Update()
	{
		if (!(mBtn != null) || !(mSpr != null) || !(comCtrl != null))
		{
			return;
		}
		if (comCtrl.IsCompounding)
		{
			time += Time.deltaTime;
			if (time > dtTime)
			{
				mSpr.spriteName = ((!(mSpr.spriteName == "Craft1")) ? "Craft1" : "Craft2");
				time = 0f;
			}
		}
		else
		{
			mSpr.spriteName = "Craft1";
		}
	}
}
