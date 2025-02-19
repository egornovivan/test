using UnityEngine;

namespace TrainingScene;

public class HolotreeAppearance : MonoBehaviour
{
	public Transform maincmr;

	public Transform orgtree;

	public float fadeTime;

	[HideInInspector]
	public bool produce;

	[HideInInspector]
	public bool destroy;

	public float part1time;

	public float minwidth;

	public float reduceSpeed;

	private Material mat;

	private float ctime;

	private float progress;

	[HideInInspector]
	public float hppct = 1f;

	private void Start()
	{
		maincmr = Camera.main.transform;
		mat = base.transform.GetComponent<MeshRenderer>().material;
		mat.SetTexture(0, HoloCameraControl.Instance.textureBase);
	}

	private void FixedUpdate()
	{
		if (produce)
		{
			ctime += Time.deltaTime;
			FadeHoloTree();
			if (ctime >= fadeTime)
			{
				ctime = fadeTime;
				produce = false;
			}
		}
		else if (destroy)
		{
			ctime -= Time.deltaTime;
			FadeHoloTree();
			if (ctime <= 0f)
			{
				ctime = 0f;
				destroy = false;
			}
		}
	}

	private void LateUpdate()
	{
		base.transform.forward = -maincmr.forward;
	}

	private void FadeHoloTree()
	{
		progress = Mathf.Clamp(ctime / fadeTime, 0f, 1f);
		if (progress == 0f)
		{
			orgtree.localScale = Vector3.zero;
		}
		else if (progress < part1time)
		{
			orgtree.localScale = new Vector3(minwidth, Mathf.Min(1f, progress / part1time), minwidth);
		}
		else
		{
			orgtree.localScale = new Vector3(Mathf.Clamp((progress - part1time) / (1f - part1time), minwidth, 1f), 1f, Mathf.Clamp((progress - part1time) / (1f - part1time), minwidth, 1f));
		}
		mat.SetFloat("_Scale", 1f / progress);
	}
}
