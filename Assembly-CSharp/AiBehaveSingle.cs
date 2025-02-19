using UnityEngine;

public class AiBehaveSingle : AiBehave
{
	public AiObject aiObject;

	public override bool isPause
	{
		set
		{
			base.isPause = value;
			if (value && aiObject != null)
			{
				aiObject.desiredLookAtTransform = null;
				aiObject.desiredFaceDirection = Vector3.zero;
				aiObject.desiredMoveDestination = Vector3.zero;
				aiObject.desiredMovementDirection = Vector3.zero;
			}
		}
	}

	public override bool isMember
	{
		get
		{
			Transform parent = base.transform.parent;
			while (parent != null)
			{
				if (parent.GetComponent<SPGroup>() != null)
				{
					return true;
				}
				parent = parent.parent;
			}
			return false;
		}
	}

	public override bool isSingle
	{
		get
		{
			Transform parent = base.transform.parent;
			while (parent != null)
			{
				if (parent.GetComponent<SPGroup>() != null)
				{
					return false;
				}
				parent = parent.parent;
			}
			return true;
		}
	}

	public override bool isActive => base.isActive && (!GameConfig.IsMultiMode || aiObject.IsController) && aiObject != null && aiObject.CanAiWorking();

	public void RegisterAiObject(AiObject aiObj)
	{
		aiObject = aiObj;
	}
}
