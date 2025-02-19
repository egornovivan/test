using ItemAsset;
using Pathea;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("USE ITEM")]
public class UseItemListener : EventListener
{
	private OBJECT item;

	private UseItemCmpt uic;

	protected override void OnCreate()
	{
		item = Utility.ToObject(base.parameters["item"]);
	}

	public override void Listen()
	{
		if (PeSingleton<PeCreature>.Instance == null || PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			return;
		}
		if (PeGameMgr.IsSingle)
		{
			uic = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<UseItemCmpt>();
			if (!(uic == null))
			{
				uic.eventor.Subscribe(OnResponse);
			}
		}
		else if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.mainPlayer.OnCustomUseItemEventHandler += OnNetResponse;
		}
	}

	public override void Close()
	{
		if (PeGameMgr.IsSingle)
		{
			if (uic != null && uic.eventor != null)
			{
				uic.eventor.Unsubscribe(OnResponse);
			}
			else
			{
				Debug.LogError("Try to close eventlistener, but source has been destroyed");
			}
		}
		else if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.mainPlayer.OnCustomUseItemEventHandler -= OnNetResponse;
		}
	}

	private void OnResponse(object sender, UseItemCmpt.EventArg e)
	{
		if (item.type != OBJECT.OBJECTTYPE.ItemProto)
		{
			return;
		}
		if (item.isSpecificPrototype)
		{
			if (item.Id == e.itemObj.protoId)
			{
				Post();
			}
		}
		else if (item.isAnyPrototypeInCategory)
		{
			if (PeSingleton<ItemProto.Mgr>.Instance != null)
			{
				ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(e.itemObj.protoId);
				if (itemProto != null && item.Group == itemProto.editorTypeId)
				{
					Post();
				}
			}
		}
		else if (item.isAnyPrototype && e.itemObj != null)
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
		if (item.isSpecificPrototype)
		{
			if (item.Id == itemObj.protoId)
			{
				Post();
			}
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
		else if (item.isAnyPrototype && itemObj != null)
		{
			Post();
		}
	}
}
