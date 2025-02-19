using Pathea;

public class DragItemLogicCreation : DragItemLogic
{
	public override void OnActivate()
	{
		DragCreationLodCmpt component = GetComponent<DragCreationLodCmpt>();
		if (null != component)
		{
			component.Activate(this);
		}
	}

	public override void OnDeactivate()
	{
		DragCreationLodCmpt component = GetComponent<DragCreationLodCmpt>();
		if (null != component)
		{
			component.Deactivate(this);
		}
	}

	public override void OnConstruct()
	{
		DragCreationLodCmpt component = GetComponent<DragCreationLodCmpt>();
		if (null != component)
		{
			component.Construct(this);
		}
	}

	public override void OnDestruct()
	{
		DragCreationLodCmpt component = GetComponent<DragCreationLodCmpt>();
		if (null != component)
		{
			component.Destruct(this);
		}
	}
}
