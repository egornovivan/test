using UnityEngine;

public class PathPoints : MonoBehaviour
{
	private static GameObject obj;

	public bool drawGizmos;

	public bool isCube;

	private void Awake()
	{
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!(componentsInChildren[i] == base.transform))
			{
				componentsInChildren[i].tag = "PathPoint";
			}
		}
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		MeshFilter meshFilter = null;
		MeshRenderer meshRenderer = null;
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		if (isCube)
		{
			if (obj == null)
			{
				obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
			}
			meshFilter = obj.GetComponent<MeshFilter>();
			meshRenderer = obj.GetComponent<MeshRenderer>();
		}
		else if (obj != null)
		{
			Object.DestroyImmediate(obj);
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i] == base.transform)
			{
				continue;
			}
			MeshFilter component = componentsInChildren[i].gameObject.GetComponent<MeshFilter>();
			MeshRenderer component2 = componentsInChildren[i].gameObject.GetComponent<MeshRenderer>();
			if (isCube)
			{
				if (meshFilter != null && component == null)
				{
					component = componentsInChildren[i].gameObject.AddComponent<MeshFilter>();
					component.sharedMesh = meshFilter.sharedMesh;
				}
				if (meshRenderer != null && component2 == null)
				{
					component2 = componentsInChildren[i].gameObject.AddComponent<MeshRenderer>();
					component2.sharedMaterials = meshRenderer.sharedMaterials;
				}
			}
			else
			{
				if (component != null)
				{
					Object.DestroyImmediate(component);
				}
				if (component2 != null)
				{
					Object.DestroyImmediate(component2);
				}
			}
			if (drawGizmos)
			{
				Gizmos.DrawCube(componentsInChildren[i].position, Vector3.one * 0.5f);
			}
		}
	}
}
