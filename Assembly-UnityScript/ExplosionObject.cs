using System;
using UnityEngine;
using UnityScript.Lang;

[Serializable]
public class ExplosionObject : MonoBehaviour
{
	public Vector3 Force;

	public GameObject Objcet;

	public int Num;

	public int Scale;

	public int ScaleMin;

	public AudioClip[] Sounds;

	public float LifeTimeObject;

	public float postitionoffset;

	public bool random;

	public ExplosionObject()
	{
		Scale = 20;
		ScaleMin = 10;
		LifeTimeObject = 2f;
		postitionoffset = 2f;
	}

	public virtual void Start()
	{
		Physics.IgnoreLayerCollision(2, 2);
		if (Extensions.get_length((System.Array)Sounds) > 0)
		{
			AudioSource.PlayClipAtPoint(Sounds[UnityEngine.Random.Range(0, Extensions.get_length((System.Array)Sounds))], transform.position);
		}
		if (!Objcet)
		{
			return;
		}
		if (random)
		{
			Num = UnityEngine.Random.Range(1, Num);
		}
		for (int i = 0; i < Num; i++)
		{
			Vector3 vector = new Vector3(UnityEngine.Random.Range(0f - postitionoffset, postitionoffset), UnityEngine.Random.Range(0f - postitionoffset, postitionoffset), UnityEngine.Random.Range(0f - postitionoffset, postitionoffset)) / 10f;
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Objcet, transform.position + vector, UnityEngine.Random.rotation);
			int num = UnityEngine.Random.Range(ScaleMin, Scale);
			UnityEngine.Object.Destroy(gameObject, LifeTimeObject);
			if (Scale > 0)
			{
				gameObject.transform.localScale = new Vector3(num, num, num);
			}
			if ((bool)gameObject.GetComponent<Rigidbody>())
			{
				gameObject.GetComponent<Rigidbody>().AddForce(new Vector3(UnityEngine.Random.Range(0f - Force.x, Force.x), UnityEngine.Random.Range(0f - Force.y, Force.y), UnityEngine.Random.Range(0f - Force.z, Force.z)));
			}
		}
	}

	public virtual void Main()
	{
	}
}
