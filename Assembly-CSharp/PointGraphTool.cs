using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class PointGraphTool : MonoBehaviour
{
	public string caveName;

	private Transform player;

	private void Start()
	{
		if (player == null && PeSingleton<PeCreature>.Instance.mainPlayer != null)
		{
			player = PeSingleton<PeCreature>.Instance.mainPlayer.GetComponent<PeTrans>().trans;
		}
		AddMesh();
	}

	private void Update()
	{
		if (Application.isEditor && Input.GetKeyUp(KeyCode.O) && player != null)
		{
			GameObject gameObject = CreatePoint(player.position + Vector3.up, player.rotation);
			if (gameObject != null)
			{
				gameObject.transform.parent = GetCaveTransform(caveName);
			}
		}
	}

	private void AddMesh()
	{
		foreach (Transform item in base.transform)
		{
			List<GameObject> list = new List<GameObject>();
			foreach (Transform item2 in item)
			{
				list.Add(CreatePoint(item2.position, item2.rotation));
				Object.Destroy(item2.gameObject);
			}
			foreach (GameObject item3 in list)
			{
				item3.transform.parent = GetCaveTransform(item.name);
			}
		}
	}

	private GameObject CreatePoint(Vector3 position, Quaternion rotation)
	{
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		Object.Destroy(gameObject.GetComponent<BoxCollider>());
		gameObject.transform.position = position;
		gameObject.transform.rotation = rotation;
		gameObject.transform.localScale = Vector3.one * 0.5f;
		return gameObject;
	}

	private Transform GetCaveTransform(string name)
	{
		Transform transform = base.transform.FindChild(name);
		if (transform == null)
		{
			transform = new GameObject(name).transform;
			transform.parent = base.transform;
		}
		return transform;
	}
}
