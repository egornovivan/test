using UnityEngine;

public class LightControl : MonoBehaviour
{
	private static readonly Color eCol = new Color(0.388f, 0.749f, 1f, 1f);

	private bool isO;

	private void Start()
	{
		if (GameTime.Timer.CycleInDay > 0.0)
		{
			TurnOff();
		}
		else
		{
			TurnOn();
		}
	}

	private void Update()
	{
		if (GameTime.Timer.CycleInDay > 0.0)
		{
			if (isO)
			{
				TurnOff();
			}
		}
		else if (!isO)
		{
			TurnOn();
		}
	}

	private void TurnOn()
	{
		isO = true;
		ParticleSystem[] componentsInChildren = base.gameObject.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem particleSystem in array)
		{
			particleSystem.gameObject.SetActive(value: true);
		}
		Light[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<Light>();
		Light[] array2 = componentsInChildren2;
		foreach (Light light in array2)
		{
			light.enabled = true;
		}
		if (GetComponent<MeshRenderer>() != null && GetComponent<MeshRenderer>().materials.Length > 1)
		{
			Material material = GetComponent<MeshRenderer>().materials[1];
			material.SetColor("_EmissionColor", 6f * eCol);
		}
		if (GetComponent<SkinnedMeshRenderer>() != null && GetComponent<SkinnedMeshRenderer>().materials.Length > 0)
		{
			Material material2 = GetComponent<SkinnedMeshRenderer>().materials[0];
			material2.SetColor("_EmissionColor", 6f * eCol);
		}
	}

	private void TurnOff()
	{
		isO = false;
		ParticleSystem[] componentsInChildren = base.gameObject.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem particleSystem in array)
		{
			particleSystem.gameObject.SetActive(value: false);
		}
		Light[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<Light>();
		Light[] array2 = componentsInChildren2;
		foreach (Light light in array2)
		{
			light.enabled = false;
		}
		if (GetComponent<MeshRenderer>() != null && GetComponent<MeshRenderer>().materials.Length > 1)
		{
			Material material = GetComponent<MeshRenderer>().materials[1];
			material.SetColor("_EmissionColor", 0f * eCol);
		}
		if (GetComponent<SkinnedMeshRenderer>() != null && GetComponent<SkinnedMeshRenderer>().materials.Length > 0)
		{
			Material material2 = GetComponent<SkinnedMeshRenderer>().materials[0];
			material2.SetColor("_EmissionColor", 0f * eCol);
		}
	}
}
