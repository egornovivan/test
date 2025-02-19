using UnityEngine;

public abstract class VEPoint : VEObject
{
	[XMLIO(Attr = "pos", Necessary = true)]
	public Vector3 Position { get; set; }
}
