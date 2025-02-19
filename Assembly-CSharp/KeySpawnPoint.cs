using DunGen;
using UnityEngine;

public class KeySpawnPoint : MonoBehaviour, IKeySpawnable
{
	public bool SetColourOnSpawn = true;

	public void SpawnKey(Key key, KeyManager manager)
	{
		GameObject gameObject = Object.Instantiate(key.Prefab);
		gameObject.transform.parent = base.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		if (SetColourOnSpawn && Application.isPlaying)
		{
			Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				renderer.material.color = key.Colour;
			}
		}
	}
}
