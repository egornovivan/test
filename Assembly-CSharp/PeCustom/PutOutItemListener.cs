using ItemAsset;
using Pathea;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("PUT OUT ITEM")]
public class PutOutItemListener : EventListener
{
	private OBJECT item;

	protected override void OnCreate()
	{
		item = Utility.ToObject(base.parameters["item"]);
	}

	public override void Listen()
	{
		if (PeGameMgr.IsSingle)
		{
			if (PeSingleton<DraggingMgr>.Instance != null && PeSingleton<DraggingMgr>.Instance.eventor != null)
			{
				PeSingleton<DraggingMgr>.Instance.eventor.Subscribe(OnResponse);
			}
		}
		else if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.mainPlayer.OnCustomPutOutItemEventHandler += OnNetResponse;
		}
	}

	public override void Close()
	{
		if (PeGameMgr.IsSingle)
		{
			if (PeSingleton<DraggingMgr>.Instance != null && PeSingleton<DraggingMgr>.Instance.eventor != null)
			{
				PeSingleton<DraggingMgr>.Instance.eventor.Unsubscribe(OnResponse);
			}
			else
			{
				Debug.LogError("Try to close eventlistener, but source has been destroyed");
			}
		}
		else if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.mainPlayer.OnCustomPutOutItemEventHandler -= OnNetResponse;
		}
	}

	private void OnResponse(object sender, DraggingMgr.EventArg e)
	{
		if (item.type != OBJECT.OBJECTTYPE.ItemProto)
		{
			return;
		}
		ItemObjDragging itemObjDragging = e.dragable as ItemObjDragging;
		PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
		if (cmpt == null)
		{
			return;
		}
		if (item.isAnyPrototype)
		{
			Post();
		}
		else if (item.isAnyPrototypeInCategory)
		{
			if (PeSingleton<ItemProto.Mgr>.Instance != null)
			{
				ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(itemObjDragging.GetItemProtoId());
				if (itemProto != null && item.Group == itemProto.editorTypeId)
				{
					Post();
				}
			}
		}
		else if (item.isSpecificPrototype && item.Id == itemObjDragging.GetItemProtoId())
		{
			Post();
		}
	}

	private void OnNetResponse(ItemObject itemObj)
	{
		if (item.type != OBJECT.OBJECTTYPE.ItemProto)
		{
			return;
		}
		if (item.isAnyPrototype)
		{
			Post();
		}
		else if (item.isAnyPrototypeInCategory)
		{
			if (PeSingleton<ItemProto.Mgr>.Instance != null)
			{
				ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(itemObj.protoId);
				if (itemProto != null && item.Group == itemProto.editorTypeId)
				{
					Post();
				}
			}
		}
		else if (item.isSpecificPrototype && item.Id == itemObj.protoId)
		{
			Post();
		}
	}
}
