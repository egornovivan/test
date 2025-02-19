using System.Collections.Generic;
using UnityEngine;
using WhiteCat.UnityExtension;

namespace WhiteCat;

public class ParticlePlayer : MonoBehaviour
{
	[SerializeField]
	private List<ParticleSystem> particles;

	[SerializeField]
	private Light light;

	[SerializeField]
	private float lightDelay = 0.025f;

	[SerializeField]
	private float lightDuration = 0.05f;

	private float time;

	private void Reset()
	{
		base.enabled = false;
		particles = new List<ParticleSystem>(4);
		base.transform.TraverseHierarchy(delegate(Transform trans, int depth)
		{
			ParticleSystem component = trans.GetComponent<ParticleSystem>();
			if ((bool)component)
			{
				particles.Add(component);
			}
			Light component2 = trans.GetComponent<Light>();
			if ((bool)component2)
			{
				light = component2;
			}
		});
	}

	private void Update()
	{
		time += Time.deltaTime;
		if (light.enabled)
		{
			if (time >= lightDuration)
			{
				base.enabled = false;
			}
		}
		else if (time >= lightDelay)
		{
			light.enabled = true;
			time = 0f;
		}
	}

	private void OnEnable()
	{
		for (int num = particles.Count - 1; num >= 0; num--)
		{
			particles[num].Play(withChildren: false);
		}
		if ((bool)light)
		{
			time = 0f;
		}
		else
		{
			base.enabled = false;
		}
	}

	private void OnDisable()
	{
		if ((bool)light)
		{
			light.enabled = false;
		}
	}
}
