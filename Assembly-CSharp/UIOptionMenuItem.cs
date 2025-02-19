using UnityEngine;

public class UIOptionMenuItem : MonoBehaviour
{
	public delegate void BaseMsgEvent(object sender);

	[SerializeField]
	private UILabel mLbText;

	[SerializeField]
	private UISprite mMouseSpr;

	[SerializeField]
	private Collider mCollider;

	private int mIndex = -1;

	public object Data;

	private bool mEnbale = true;

	public int Index => mIndex;

	public string Text => mLbText.text;

	public bool isEnable
	{
		get
		{
			return mEnbale;
		}
		set
		{
			mEnbale = value;
			mCollider.enabled = value;
			mLbText.color = ((!value) ? Color.gray : Color.white);
		}
	}

	public event BaseMsgEvent e_OnClickItem;

	public void Init(string text, int index)
	{
		mLbText.text = text;
		mIndex = index;
	}

	private void OnMouseOver()
	{
		mMouseSpr.enabled = true;
	}

	private void OnMouseOut()
	{
		mMouseSpr.enabled = false;
	}

	private void OnSelectItem()
	{
		if (this.e_OnClickItem != null)
		{
			this.e_OnClickItem(this);
		}
	}
}
