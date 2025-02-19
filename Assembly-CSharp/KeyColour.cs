using DunGen;
using UnityEngine;

public class KeyColour : MonoBehaviour, IKeyLock
{
	public void OnKeyAssigned(Key key, KeyManager manager)
	{
		SetColour(key.Colour);
	}

	private void SetColour(Color colour)
	{
		if (Application.isPlaying)
		{
			Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				renderer.material.color = colour;
			}
		}
	}
}
