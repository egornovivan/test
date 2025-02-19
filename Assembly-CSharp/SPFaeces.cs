using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPFaeces : MonoBehaviour
{
	public float probability;

	public float interval;

	public GameObject[] faeces;

	private Shader transparent;

	private List<IntVector2> mExists = new List<IntVector2>();

	private List<GameObject> mFaeces = new List<GameObject>();

	private bool MatchFaece(IntVector4 node, GameObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		float num = obj.transform.position.x - (float)node.x;
		float num2 = obj.transform.position.z - (float)node.z;
		return num >= float.Epsilon && num <= (float)(32 << node.w) && num2 >= float.Epsilon && num2 <= (float)(32 << node.w);
	}

	private bool Match(AiObject aiObj)
	{
		return AiUtil.GetChild(aiObj.transform, "faece") != null;
	}

	private IEnumerator DestroyFaece(GameObject faece, float delayTime)
	{
		yield return new WaitForSeconds(delayTime);
		if (faece == null)
		{
			yield break;
		}
		MeshRenderer renderer = faece.GetComponentInChildren<MeshRenderer>();
		if (faece != null && transparent != null && renderer != null)
		{
			float alphaValue = 1f;
			while (Mathf.Abs(alphaValue) > float.Epsilon && faece != null)
			{
				alphaValue = Mathf.Clamp01(alphaValue - 0.05f);
				for (int i = 0; i < renderer.materials.Length; i++)
				{
					Color color = renderer.materials[i].color;
					color.a = alphaValue;
					renderer.materials[i].shader = transparent;
					renderer.materials[i].color = color;
				}
				yield return new WaitForSeconds(0.1f);
			}
		}
		if (faece != null)
		{
			mFaeces.Remove(faece);
			Object.Destroy(faece);
		}
	}

	private GameObject GetRandomFaeces()
	{
		return faeces[Random.Range(0, faeces.Length)];
	}

	private void RegisterEvent()
	{
		LODOctreeMan.self.AttachEvents(null, OnTerrainColliderDestroy, OnTerrainColliderCreated, null);
	}

	private void RemoveEvent()
	{
		LODOctreeMan.self.DetachEvents(null, OnTerrainColliderDestroy, OnTerrainColliderCreated, null);
	}

	private void OnTerrainColliderCreated(IntVector4 node)
	{
		if (node.w != 0)
		{
			return;
		}
		IntVector2 item = new IntVector2(node.x, node.z);
		if (!mExists.Contains(item))
		{
			if (Random.value < probability)
			{
				Vector3 position = AiUtil.GetRandomPosition(node);
				if (AiUtil.CheckPositionOnGround(ref position, 0f, 32 << node.w, AiUtil.groundedLayer) && !AiUtil.CheckPositionUnderWater(position))
				{
					GameObject gameObject = Object.Instantiate(GetRandomFaeces(), position, Quaternion.identity) as GameObject;
					gameObject.transform.parent = base.transform;
					mFaeces.Add(gameObject);
					StartCoroutine(DestroyFaece(gameObject, 400f));
				}
			}
			mExists.Add(item);
		}
		List<GameObject> list = mFaeces.FindAll((GameObject ret) => MatchFaece(node, ret));
		foreach (GameObject item2 in list)
		{
			Vector3 position2 = item2.transform.position;
			float num = position2.y - (float)node.y;
			if (num > float.Epsilon && Physics.Raycast(position2 + Vector3.up * 0.1f, Vector3.down, out var _, num, AiUtil.groundedLayer) && item2.GetComponent<Rigidbody>() != null)
			{
				item2.GetComponent<Rigidbody>().useGravity = true;
			}
		}
	}

	private void OnTerrainColliderDestroy(IntVector4 node)
	{
		if (node.w != 0)
		{
			return;
		}
		IntVector2 item = new IntVector2(node.x, node.z);
		if (mExists.Contains(item))
		{
			mExists.Remove(item);
		}
		List<GameObject> list = mFaeces.FindAll((GameObject ret) => MatchFaece(node, ret));
		foreach (GameObject item2 in list)
		{
			if (item2.GetComponent<Rigidbody>() != null)
			{
				item2.GetComponent<Rigidbody>().useGravity = false;
			}
		}
	}

	private void Start()
	{
		transparent = Shader.Find("Transparent/Bumped Diffuse");
		RegisterEvent();
	}

	private void Destroy()
	{
		RemoveEvent();
	}
}
