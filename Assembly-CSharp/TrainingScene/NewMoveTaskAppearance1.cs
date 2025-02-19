using UnityEngine;

namespace TrainingScene;

public class NewMoveTaskAppearance1 : MonoBehaviour
{
	public Transform orgterrain;

	public float fadeTime;

	public bool produce;

	public bool destroy;

	public float part1time;

	public float minwidth;

	public float reduceSpeed;

	private static Vector3 fadeCenter = new Vector3(14.8f, 1.53f, 11f);

	private Material mat;

	private float ctime;

	private float progress;

	private void Start()
	{
		mat = base.transform.GetComponent<MeshRenderer>().material;
		mat.SetTexture(0, HoloCameraControl.Instance.textureBase);
		base.transform.position = Vector3.zero;
		base.transform.rotation = Quaternion.identity;
	}

	private void FixedUpdate()
	{
		if (produce)
		{
			ctime += Time.deltaTime;
			if (ctime >= fadeTime)
			{
				ctime = fadeTime;
				mat.SetFloat("_Scale", 1f);
				produce = false;
			}
		}
		else if (destroy)
		{
			ctime -= Time.deltaTime;
			if (ctime <= 0f)
			{
				ctime = 0f;
				destroy = false;
			}
		}
	}

	private void LateUpdate()
	{
		int i = 0;
		int num = 0;
		for (; i < Block45Man.self.transform.childCount; i++)
		{
			if (num >= 2)
			{
				break;
			}
			GameObject gameObject = Block45Man.self.transform.GetChild(i).gameObject;
			if (gameObject.activeSelf && gameObject.name == "b45Chnk_8_0_8_0")
			{
				num++;
				if (gameObject.name == "b45Chnk_8_0_8_0")
				{
					orgterrain = gameObject.transform;
				}
				GetComponent<MeshFilter>().mesh = orgterrain.GetComponent<MeshFilter>().mesh;
				NewMoveTask.Instance.ChangeRenderTarget(orgterrain.GetComponent<MeshRenderer>(), org: false);
			}
		}
	}

	private void FadeHoloTerrain()
	{
		progress = Mathf.Clamp(ctime / fadeTime, 0f, 1f);
		if (progress == 0f)
		{
			orgterrain.localScale = Vector3.zero;
		}
		else if (progress < part1time)
		{
			orgterrain.localScale = new Vector3(minwidth, Mathf.Min(1f, progress / part1time), minwidth);
		}
		else
		{
			orgterrain.localScale = new Vector3(Mathf.Clamp((progress - part1time) / (1f - part1time), minwidth, 1f), 1f, Mathf.Clamp((progress - part1time) / (1f - part1time), minwidth, 1f));
		}
		orgterrain.position = new Vector3(fadeCenter.x * (1f - orgterrain.localScale.x), fadeCenter.y * (1f - orgterrain.localScale.y), fadeCenter.z * (1f - orgterrain.localScale.z));
		mat.SetFloat("_Scale", 1f / progress);
	}
}
