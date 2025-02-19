using UnityEngine;

public class particleFactory : MonoBehaviour
{
	public bool loop;

	public float cycleTime;

	public int explodeNum;

	public float interval;

	public GameObject unit;

	public Color color;

	private float pastTime;

	private float unitTime;

	private int totalNum = 1;

	private GameObject obj;

	private void Start()
	{
		obj = Object.Instantiate(unit, base.transform.position, base.transform.rotation) as GameObject;
		obj.GetComponent<ParticleSystem>().startColor = color;
		obj.GetComponent<ParticleSystem>().startRotation = Vector3.Angle(base.transform.forward, Vector3.forward) / 57.29578f * Mathf.Sign(base.transform.forward.x);
		obj.transform.parent = base.transform;
	}

	private void FixedUpdate()
	{
		if (!loop)
		{
			pastTime += Time.deltaTime;
			if (pastTime > cycleTime)
			{
				pastTime = 0f;
				obj = Object.Instantiate(unit, base.transform.position, base.transform.rotation) as GameObject;
				obj.GetComponent<ParticleSystem>().startColor = color;
				obj.GetComponent<ParticleSystem>().startRotation = Vector3.Angle(base.transform.forward, Vector3.forward) / 57.29578f * Mathf.Sign(base.transform.forward.x);
				obj.transform.parent = base.transform;
				totalNum = 1;
				unitTime = 0f - Time.deltaTime;
			}
			if (totalNum >= explodeNum)
			{
				return;
			}
		}
		if (unitTime >= interval)
		{
			obj = Object.Instantiate(unit, base.transform.position, base.transform.rotation) as GameObject;
			obj.GetComponent<ParticleSystem>().startColor = color;
			obj.GetComponent<ParticleSystem>().startRotation = Vector3.Angle(base.transform.forward, Vector3.forward) / 57.29578f * Mathf.Sign(base.transform.forward.x);
			obj.transform.parent = base.transform;
			unitTime -= interval;
			totalNum++;
		}
		else
		{
			unitTime += Time.deltaTime;
		}
	}
}
