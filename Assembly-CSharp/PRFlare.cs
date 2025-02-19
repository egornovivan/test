using System.Collections;
using Pathea.Effect;
using Pathea.Projectile;
using UnityEngine;

public class PRFlare : SkProjectile
{
	public float timeSign1;

	public float timeSign2;

	public float maxRange;

	public float finalRange;

	public float maxIntensity;

	public float finalIntensity;

	public float existTime;

	private Light light;

	private float startTime;

	private float process;

	public void Start()
	{
		light = GetComponentInChildren<Light>();
		startTime = Time.time;
		StartCoroutine(FlareIntensity());
	}

	private IEnumerator FlareIntensity()
	{
		while (true)
		{
			process = Time.time - startTime;
			if (process <= timeSign1)
			{
				light.range = Mathf.Lerp(0f, maxRange, process / timeSign1);
				light.intensity = Mathf.Lerp(0f, maxIntensity, process / timeSign1);
			}
			else if (process <= timeSign2)
			{
				light.range = maxRange;
				light.intensity = maxIntensity;
			}
			else
			{
				light.range = Mathf.Lerp(maxRange, finalRange, (process - timeSign2) / (existTime - timeSign2));
				light.intensity = Mathf.Lerp(maxIntensity, finalIntensity, (process - timeSign2) / (existTime - timeSign2));
			}
			yield return new WaitForSeconds(0.05f);
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		Singleton<EffectBuilder>.Instance.Register(67, null, base.transform.position, Quaternion.identity);
	}
}
