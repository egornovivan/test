using System.Collections.Generic;
using UnityEngine;

namespace EpsilonIndi;

public class CloudEmitter : MonoBehaviour
{
	[SerializeField]
	private GameObject cloudFrag;

	[SerializeField]
	private Material[] cloudSample;

	[SerializeField]
	private float maxSpeed = 2f;

	[SerializeField]
	private float minSpeed = -2f;

	[SerializeField]
	private float maxAngle = 180f;

	[SerializeField]
	private float minAngle;

	[SerializeField]
	private float cdTime = 0.75f;

	[HideInInspector]
	public List<CloudMotion> cms = new List<CloudMotion>();

	[HideInInspector]
	public float relativeRotY;

	private float ctime;

	private static float cloud1prob = 0.005f;

	private static float cloud2prob = 0.005f;

	private void Start()
	{
		int num = (int)(360f / GetComponent<SelfRotation>().selfSpeed / cdTime);
		for (int i = 0; i < num; i++)
		{
			CreateACloud(360f * (float)i / (float)num, -2f, 2f);
		}
	}

	private void Update()
	{
		ctime += Time.deltaTime;
		if (ctime > cdTime)
		{
			ctime -= cdTime;
			CreateACloud(relativeRotY, -2f, 2f);
		}
	}

	private float GetRandom1(float mi, float ma)
	{
		float value = Random.value;
		value *= value * value - 1.5f * value + 1.5f;
		return Mathf.Lerp(mi, ma, value);
	}

	private int GetRandomID(int cloudNumMax)
	{
		float value = Random.value;
		if (value < cloud1prob)
		{
			return 0;
		}
		if (value < cloud2prob)
		{
			return 1;
		}
		return Random.Range(2, cloudNumMax);
	}

	private void CreateACloud(float correctRotY, float minAngleOffset, float maxAngleOffset)
	{
		Transform transform = Object.Instantiate(cloudFrag).transform;
		transform.parent = base.transform;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one * Random.Range(0.0502f, 0.0505f);
		int randomID = GetRandomID(cloudSample.Length - 1);
		transform.GetComponent<MeshRenderer>().material = cloudSample[randomID];
		CloudMotion component = transform.GetComponent<CloudMotion>();
		cms.Add(component);
		if (randomID <= 1)
		{
			component.InitProp(GetRandom1(minSpeed, maxSpeed), GetRandom1(minAngle, maxAngle), correctRotY + Random.Range(minAngleOffset, maxAngleOffset), (float)randomID - 4f);
		}
		else
		{
			component.InitProp(GetRandom1(minSpeed, maxSpeed), GetRandom1(minAngle, maxAngle), correctRotY + Random.Range(minAngleOffset, maxAngleOffset), 0f);
		}
	}
}
