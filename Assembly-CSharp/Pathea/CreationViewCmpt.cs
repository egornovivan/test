using UnityEngine;
using WhiteCat;

namespace Pathea;

public class CreationViewCmpt : ViewCmpt
{
	private CreationController _creationController;

	public override bool hasView => _creationController.collidable;

	public override Transform centerTransform => (!_creationController.collidable) ? null : _creationController.centerObject;

	public void Init(CreationController creationController)
	{
		_creationController = creationController;
	}
}
