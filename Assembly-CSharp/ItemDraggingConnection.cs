using UnityEngine;

public class ItemDraggingConnection : ItemDraggingArticle
{
	protected override bool canPutUp => true;

	public override bool OnDragging(Ray cameraRay)
	{
		if (!base.OnDragging(cameraRay))
		{
			return false;
		}
		AutoSnap();
		return true;
	}

	private void AutoSnap()
	{
		ItemScript_Connection component = fhitInfo.transform.GetComponent<ItemScript_Connection>();
		if (!(null != component) || component.mConnectionPoint.Count <= 0)
		{
			return;
		}
		float num = 10f;
		foreach (Vector3 item in component.mConnectionPoint)
		{
			float magnitude = (fhitInfo.transform.position + fhitInfo.transform.rotation * item - fhitInfo.point).magnitude;
			if (num > magnitude)
			{
				num = magnitude;
				base.rootGameObject.transform.position = fhitInfo.transform.position + fhitInfo.transform.rotation * item;
			}
		}
	}
}
