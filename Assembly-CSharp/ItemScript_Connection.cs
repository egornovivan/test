using System.Collections.Generic;
using UnityEngine;

public class ItemScript_Connection : ItemScript
{
	public List<Vector3> mConnectionPoint;

	public override void OnConstruct()
	{
		base.OnConstruct();
		base.gameObject.layer = 12;
	}
}
