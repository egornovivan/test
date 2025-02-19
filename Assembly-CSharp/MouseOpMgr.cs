using Pathea;
using UnityEngine;

public class MouseOpMgr : Singleton<MouseOpMgr>
{
	public enum MouseOpCursor
	{
		Null,
		Gather,
		Fell,
		NPCTalk,
		LootCorpse,
		Door,
		Bed,
		Carrier,
		Ladder,
		PowerPlantSolar,
		WareHouse,
		RepairMachine,
		Colony,
		Plant,
		Tower,
		PickUpItem,
		KickStarter,
		Hand,
		Ride
	}

	public MouseOpCursor currentState;

	private void Update()
	{
		UpdateCurrentState();
	}

	private void UpdateCurrentState()
	{
		currentState = MouseOpCursor.Null;
		if (null == MainPlayerCmpt.gMainPlayer)
		{
			return;
		}
		if (MainPlayerCmpt.gMainPlayer.actionOpCursor != 0)
		{
			currentState = MainPlayerCmpt.gMainPlayer.actionOpCursor;
		}
		else
		{
			if (PeSingleton<MousePicker>.Instance.curPickObj == null || PeSingleton<MousePicker>.Instance.curPickObj.Equals(null))
			{
				return;
			}
			MonoBehaviour monoBehaviour = PeSingleton<MousePicker>.Instance.curPickObj as MonoBehaviour;
			if (PeSingleton<MousePicker>.Instance.curPickObj is ClickPEentityLootItem)
			{
				currentState = MouseOpCursor.LootCorpse;
			}
			else if (PeSingleton<MousePicker>.Instance.curPickObj is MousePickableRandomItem)
			{
				currentState = MouseOpCursor.LootCorpse;
			}
			else if (PeSingleton<MousePicker>.Instance.curPickObj is MousePickableNPC)
			{
				MousePickableNPC mousePickableNPC = PeSingleton<MousePicker>.Instance.curPickObj as MousePickableNPC;
				if (null != mousePickableNPC && null != mousePickableNPC.npc)
				{
					if (mousePickableNPC.npc.CanTalk)
					{
						currentState = MouseOpCursor.NPCTalk;
					}
					else if (mousePickableNPC.npc.CanHanded)
					{
						currentState = MouseOpCursor.Hand;
					}
				}
			}
			else if (PeSingleton<MousePicker>.Instance.curPickObj is DragItemMousePickBed)
			{
				currentState = MouseOpCursor.Bed;
			}
			else if (PeSingleton<MousePicker>.Instance.curPickObj is DragItemMousePickDoor)
			{
				currentState = MouseOpCursor.Door;
			}
			else if (PeSingleton<MousePicker>.Instance.curPickObj is DragItemMousePickCarrier)
			{
				currentState = MouseOpCursor.Carrier;
			}
			else if (PeSingleton<MousePicker>.Instance.curPickObj is DragItemMousePickLadder)
			{
				currentState = MouseOpCursor.Ladder;
			}
			else if (PeSingleton<MousePicker>.Instance.curPickObj is OperatableItemPowerPlantSolar)
			{
				currentState = MouseOpCursor.PowerPlantSolar;
			}
			else if (PeSingleton<MousePicker>.Instance.curPickObj is WareHouseObject)
			{
				currentState = MouseOpCursor.WareHouse;
			}
			else if (PeSingleton<MousePicker>.Instance.curPickObj is OperatableItemRepairMachine)
			{
				currentState = MouseOpCursor.RepairMachine;
			}
			else if (PeSingleton<MousePicker>.Instance.curPickObj is DragItemMousePickColony)
			{
				currentState = MouseOpCursor.Colony;
			}
			else if (PeSingleton<MousePicker>.Instance.curPickObj is DragItemMousePickPlant)
			{
				currentState = MouseOpCursor.Plant;
			}
			else if (PeSingleton<MousePicker>.Instance.curPickObj is DragItemMousePickTower)
			{
				currentState = MouseOpCursor.Tower;
			}
			else if (PeSingleton<MousePicker>.Instance.curPickObj is DragItemMousePickKickStarter)
			{
				currentState = MouseOpCursor.KickStarter;
			}
			else if (PeSingleton<MousePicker>.Instance.curPickObj is ClickGatherEvent)
			{
				currentState = MouseOpCursor.Gather;
			}
			else if (PeSingleton<MousePicker>.Instance.curPickObj is ClickFellEvent)
			{
				MotionMgrCmpt motionMgr = PeSingleton<PeCreature>.Instance.mainPlayer.motionMgr;
				Action_Fell action = motionMgr.GetAction<Action_Fell>();
				if ((bool)action.m_Axe && motionMgr.GetMaskState(PEActionMask.EquipmentHold))
				{
					currentState = MouseOpCursor.Fell;
				}
			}
			else if (PeSingleton<MousePicker>.Instance.curPickObj is MousePickRides)
			{
				currentState = MouseOpCursor.Ride;
			}
			else if (null != monoBehaviour && null != monoBehaviour.GetComponent<ItemDropMousePickRandomItem>())
			{
				currentState = MouseOpCursor.PickUpItem;
			}
			else if (null != monoBehaviour && null != monoBehaviour.GetComponent<ItemDropMousePick>())
			{
				currentState = MouseOpCursor.PickUpItem;
			}
		}
	}
}
