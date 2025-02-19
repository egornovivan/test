using UnityEngine;

public class ItemDraggingTorch : ItemDraggingArticle
{
	public override void OnDragOut()
	{
		base.OnDragOut();
		base.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
	}
}
