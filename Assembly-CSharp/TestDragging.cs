using ItemAsset;
using Pathea;
using UnityEngine;

public class TestDragging : MonoBehaviour
{
	private void Update()
	{
		if (PeInput.Get(PeInput.LogicFunction.Item_CancelDrag))
		{
			PeSingleton<DraggingMgr>.Instance.Cancel();
			Debug.Log("cancel drag");
		}
		if (PeInput.Get(PeInput.LogicFunction.Item_Drag))
		{
			if (PeSingleton<DraggingMgr>.Instance.IsDragging())
			{
				bool flag = PeSingleton<DraggingMgr>.Instance.End();
				Debug.Log("drag success:" + flag);
			}
			else
			{
				ItemObject itemObject = PeSingleton<ItemMgr>.Instance.CreateItem(283);
				Drag cmpt = itemObject.GetCmpt<Drag>();
				if (cmpt != null)
				{
					ItemObjDragging dragable = new ItemObjDragging(cmpt);
					PeSingleton<DraggingMgr>.Instance.Begin(dragable);
					Debug.Log("begin drag");
				}
			}
		}
		if (PeInput.Get(PeInput.LogicFunction.Item_RotateItem))
		{
			PeSingleton<DraggingMgr>.Instance.Rotate();
		}
		PeSingleton<DraggingMgr>.Instance.UpdateRay();
	}
}
