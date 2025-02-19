using Pathea;
using UnityEngine;

namespace TrainingScene;

public class TerrainDigAppearance : MonoBehaviour
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
		PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>().Add(12, 1);
	}

	private void FixedUpdate()
	{
		if (produce)
		{
			ctime += Time.deltaTime;
			FadeHoloTerrain();
			if (ctime >= fadeTime)
			{
				ctime = fadeTime;
				produce = false;
			}
		}
		else if (destroy)
		{
			ctime -= Time.deltaTime;
			FadeHoloTerrain();
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
		for (; i < VFVoxelTerrain.self.transform.childCount; i++)
		{
			if (num >= 2)
			{
				break;
			}
			GameObject gameObject = VFVoxelTerrain.self.transform.GetChild(i).gameObject;
			if (gameObject.activeSelf && gameObject.name == "Chnk_0_0_0_0")
			{
				num++;
				if (gameObject.GetComponent<VFVoxelChunkGo>().OriginalChunkGo == null)
				{
					orgterrain = gameObject.transform;
					Debug.Log(gameObject.transform.position);
					GetComponent<MeshFilter>().mesh = orgterrain.GetComponent<MeshFilter>().mesh;
					TerrainDigTask.Instance.ChangeRenderTarget(orgterrain.GetComponent<MeshRenderer>());
				}
				else
				{
					gameObject.GetComponent<MeshRenderer>().enabled = false;
				}
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
