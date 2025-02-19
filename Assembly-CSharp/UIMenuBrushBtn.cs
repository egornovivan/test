using UnityEngine;

public class UIMenuBrushBtn : MonoBehaviour
{
	public UISprite bgSprite;

	public UISprite checkedSprite;

	public UICheckbox checkBox;

	private BoxCollider _collider;

	public BoxCollider boxCollier
	{
		get
		{
			if (_collider == null)
			{
				_collider = base.gameObject.GetComponent<BoxCollider>();
			}
			return _collider;
		}
	}
}
