using UnityEngine;

public class UISpritEabled : MonoBehaviour
{
	public float mColorAl = 0.1f;

	public UISlicedSprite mSprite1;

	public UISlicedSprite mSprite2;

	private bool IsShow;

	private void Start()
	{
	}

	private void Update()
	{
		if (!(mSprite2 == null) && !(mSprite1 == null))
		{
			if (IsShow && !mSprite1.enabled)
			{
				mSprite1.enabled = true;
				mSprite2.enabled = true;
			}
			else if (!IsShow && mSprite1.enabled)
			{
				mSprite1.enabled = false;
				mSprite2.enabled = false;
			}
		}
	}

	private void SpritOnMouseOver()
	{
		IsShow = true;
	}

	private void SpritOnMouseOut()
	{
		IsShow = false;
	}
}
