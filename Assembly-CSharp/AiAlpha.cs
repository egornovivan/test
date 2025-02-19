using System.Collections;
using UnityEngine;

public class AiAlpha : MonoBehaviour
{
	public delegate void OnAlphaChangeCompleted(AiAlpha alpha, float dstAlpha);

	private static float Accuracy = 0.05f;

	private SkinnedMeshRenderer render;

	private Shader[] originalShaders;

	private Shader transparent;

	private bool isDormant;

	public float CurrentAlphaValue
	{
		get
		{
			if (render == null)
			{
				return 0f;
			}
			return render.material.color.a;
		}
	}

	public void Fadein(float delayTime, float fadeTime)
	{
		if (!(render == null) && !(transparent == null))
		{
			for (int i = 0; i < render.materials.Length; i++)
			{
				Color color = render.materials[i].color;
				render.materials[i].shader = transparent;
				render.materials[i].color = new Color(color.r, color.g, color.b, 0f);
			}
			ChangeAlphaToValue(1f, delayTime, fadeTime);
		}
	}

	public void ChangeAlphaToValue(float dstAlpha, float delayTime = 0f, float time = 2f, OnAlphaChangeCompleted alphaChange = null)
	{
		if (!(render == null) && !(Mathf.Abs(render.material.color.a - dstAlpha) < 0.1f))
		{
			StopAllCoroutines();
			StartCoroutine(AlphaUpdate(dstAlpha, delayTime, time, alphaChange));
		}
	}

	private void ActivateTransparent(bool isTransparent)
	{
		if (isTransparent)
		{
			if (isDormant)
			{
				isDormant = false;
				for (int i = 0; i < render.materials.Length; i++)
				{
					render.materials[i].shader = transparent;
				}
			}
		}
		else if (!isDormant)
		{
			isDormant = true;
			for (int j = 0; j < render.materials.Length; j++)
			{
				render.materials[j].shader = originalShaders[j];
			}
		}
	}

	private IEnumerator AlphaUpdate(float dstAlpha, float delayTime = 0f, float time = 2f, OnAlphaChangeCompleted alphaChange = null)
	{
		yield return new WaitForSeconds(delayTime);
		ActivateTransparent(isTransparent: true);
		float startTime = Time.time;
		float startAlpha = render.material.color.a;
		float alphaValue = -1000f;
		while (render != null && Mathf.Abs(alphaValue - dstAlpha) > Accuracy)
		{
			alphaValue = Mathf.Lerp(startAlpha, dstAlpha, (Time.time - startTime) / time);
			for (int i = 0; i < render.materials.Length; i++)
			{
				Color oldColor = render.materials[i].color;
				render.materials[i].color = new Color(oldColor.r, oldColor.g, oldColor.b, alphaValue);
			}
			yield return new WaitForSeconds(0.1f);
		}
		alphaChange?.Invoke(this, dstAlpha);
		if (Mathf.Abs(dstAlpha - 1f) <= Accuracy && Mathf.Abs(render.material.color.a - 1f) <= Accuracy)
		{
			ActivateTransparent(isTransparent: false);
		}
	}

	private void Awake()
	{
		if (base.transform.parent != null)
		{
			render = base.transform.parent.GetComponentInChildren<SkinnedMeshRenderer>();
		}
		else
		{
			render = base.transform.GetComponentInChildren<SkinnedMeshRenderer>();
		}
		if (!(render == null))
		{
			isDormant = true;
			originalShaders = new Shader[render.materials.Length];
			for (int i = 0; i < render.materials.Length; i++)
			{
				originalShaders[i] = render.materials[i].shader;
			}
			transparent = Shader.Find("Transparent/Bumped Diffuse");
		}
	}
}
