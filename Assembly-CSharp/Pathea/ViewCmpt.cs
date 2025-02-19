using UnityEngine;

namespace Pathea;

public abstract class ViewCmpt : PeCmpt
{
	public abstract bool hasView { get; }

	public abstract Transform centerTransform { get; }

	public Vector3 centerPosition => (!(centerTransform != null)) ? base.transform.position : centerTransform.position;
}
