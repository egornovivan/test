using UnityEngine;

public class LocalSpaceGraph : MonoBehaviour
{
	protected Matrix4x4 originalMatrix;

	private void Start()
	{
		originalMatrix = base.transform.localToWorldMatrix;
	}

	public Matrix4x4 GetMatrix()
	{
		return base.transform.worldToLocalMatrix * originalMatrix;
	}
}
