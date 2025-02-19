using System.Collections.Generic;
using UnityEngine;

public class FadeInOut : MonoBehaviour
{
	private Dictionary<Renderer, Material[]> rendererDic = new Dictionary<Renderer, Material[]>(1);

	private float currentAlpha = 1f;

	private float targetAlpha;

	private bool isDormant = true;

	public bool ChangeAlphaToValue(float dstAlpha)
	{
		if (Mathf.Abs(currentAlpha - dstAlpha) < 0.1f)
		{
			isDormant = true;
			return true;
		}
		isDormant = false;
		targetAlpha = dstAlpha;
		return false;
	}

	private void Update()
	{
		if (isDormant)
		{
			return;
		}
		currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * 5f);
		foreach (KeyValuePair<Renderer, Material[]> item in rendererDic)
		{
			if (null == item.Key)
			{
				continue;
			}
			for (int i = 0; i < item.Key.materials.Length; i++)
			{
				if (item.Value.Length <= i)
				{
					Debug.LogError(base.gameObject.name + "'s fadeinout error.");
					break;
				}
				if (!(null == item.Value[i]))
				{
					Color color = Color.white;
					if (item.Value[i].HasProperty("_Color"))
					{
						color = item.Value[i].color;
					}
					color.a *= currentAlpha;
					item.Key.materials[i].color = color;
				}
			}
		}
	}

	private bool InitRenderer()
	{
		Renderer[] componentsInChildren = base.transform.GetComponentsInChildren<Renderer>();
		if (componentsInChildren.Length <= 0)
		{
			Debug.LogError("no render to fade in or out");
			return false;
		}
		Shader shader = Shader.Find("Transparent/Bumped Diffuse");
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
			{
				rendererDic.Add(renderer, renderer.materials);
				int num = renderer.materials.Length;
				Material[] array2 = new Material[num];
				for (int j = 0; j < num; j++)
				{
					array2[j] = new Material(renderer.materials[j]);
					array2[j].shader = shader;
				}
				renderer.materials = array2;
			}
		}
		return true;
	}

	private void RestoreRenderer()
	{
		foreach (KeyValuePair<Renderer, Material[]> item in rendererDic)
		{
			item.Key.materials = item.Value;
		}
	}

	private void Awake()
	{
		InitRenderer();
	}

	private void OnDestroy()
	{
		RestoreRenderer();
	}
}
