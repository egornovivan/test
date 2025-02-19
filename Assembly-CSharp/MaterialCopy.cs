using UnityEngine;

public class MaterialCopy : MonoBehaviour
{
	private void Awake()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>(includeInactive: true);
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			Material[] array2 = new Material[renderer.materials.Length];
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = Object.Instantiate(renderer.materials[j]);
			}
			renderer.materials = array2;
		}
	}
}
