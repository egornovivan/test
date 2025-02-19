using WhiteCat;

namespace Pathea;

public class DragCreationLodCmpt : PeCmpt
{
	public void Construct(DragItemLogicCreation dragLogic)
	{
		CreationController component = GetComponent<CreationController>();
		component.visible = true;
		switch (component.category)
		{
		case EVCCategory.cgAircraft:
		{
			ItemScript component3 = GetComponent<ItemScript>();
			if (null != component3)
			{
				component3.SetItemObject(dragLogic.itemDrag.itemObj);
				component3.InitNetlayer(dragLogic.mNetlayer);
				component3.id = dragLogic.id;
			}
			break;
		}
		case EVCCategory.cgVehicle:
		case EVCCategory.cgBoat:
		case EVCCategory.cgRobot:
		case EVCCategory.cgAITurret:
		{
			ItemScript component2 = GetComponent<ItemScript>();
			if (null != component2)
			{
				component2.SetItemObject(dragLogic.itemDrag.itemObj);
				component2.InitNetlayer(dragLogic.mNetlayer);
				component2.id = dragLogic.id;
			}
			break;
		}
		case EVCCategory.cgBow:
		case EVCCategory.cgAxe:
			break;
		}
	}

	public void Destruct(DragItemLogicCreation dragLogic)
	{
		CreationController component = GetComponent<CreationController>();
		component.visible = false;
	}

	public void Activate(DragItemLogicCreation dragLogic)
	{
		BehaviourController component = GetComponent<BehaviourController>();
		if ((bool)component)
		{
			component.physicsEnabled = true;
		}
		CreationController component2 = GetComponent<CreationController>();
		component2.collidable = true;
		base.Entity.SendMsg(EMsg.Lod_Collider_Created);
	}

	public void Deactivate(DragItemLogicCreation dragLogic)
	{
		CreationController component = GetComponent<CreationController>();
		component.collidable = false;
		BehaviourController component2 = GetComponent<BehaviourController>();
		if ((bool)component2)
		{
			component2.physicsEnabled = false;
		}
		base.Entity.SendMsg(EMsg.Lod_Collider_Destroying);
	}
}
