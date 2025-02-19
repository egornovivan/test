using System;
using UnityEngine;
using WhiteCat;

[RequireComponent(typeof(BuildMeshAlongPath))]
public class CreatePathFromCode : MonoBehaviour
{
	[Range(4f, 32f)]
	public int nodesCount = 16;

	[Range(30f, 60f)]
	public float minRadius = 30f;

	[Range(30f, 60f)]
	public float maxRadius = 60f;

	public void CreateRandomPath()
	{
		CardinalPath cardinalPath = base.gameObject.AddComponent<CardinalPath>();
		cardinalPath.isCircular = true;
		for (int i = cardinalPath.nodesCount; i < nodesCount; i++)
		{
			cardinalPath.InsertNode(0);
		}
		Vector3 zero = Vector3.zero;
		for (int j = 0; j < nodesCount; j++)
		{
			float f = (float)Math.PI * 2f / (float)nodesCount * (float)j;
			float num = UnityEngine.Random.Range(minRadius, maxRadius);
			zero.x = Mathf.Cos(f) * num;
			zero.z = Mathf.Sin(f) * num;
			cardinalPath.SetNodeLocalPosition(j, zero);
		}
		BuildMeshAlongPath component = GetComponent<BuildMeshAlongPath>();
		component.path = cardinalPath;
		component.Bulid();
		UnityEngine.Object.Destroy(cardinalPath);
	}
}
