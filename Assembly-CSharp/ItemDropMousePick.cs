using UnityEngine;

[RequireComponent(typeof(MousePickableChildCollider))]
public class ItemDropMousePick : ItemDrop
{
	private void Start()
	{
		MousePickableChildCollider component = GetComponent<MousePickableChildCollider>();
		if (component != null)
		{
			component.eventor.Subscribe(delegate(object sender, MousePickable.RMouseClickEvent e)
			{
				OpenGui(e.mousePickable.transform.position);
			});
		}
	}
}
