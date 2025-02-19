using UnityEngine;

public class LifeShowItem_N : MonoBehaviour
{
	public UISprite mHeadSpr;

	public UISlider mLife;

	public MonoBehaviour mShowObj;

	public void InitItem(AiObject enemy)
	{
		mShowObj = enemy;
		mHeadSpr.MakePixelPerfect();
		mHeadSpr.transform.localScale = new Vector3(32f, 32f, 1f);
	}

	private void Update()
	{
		if (mShowObj == null)
		{
			mLife.sliderValue = 0f;
			Object.Destroy(base.gameObject);
		}
		else if (mShowObj is AiObject)
		{
			mLife.sliderValue = ((AiObject)mShowObj).lifePercent;
		}
	}
}
