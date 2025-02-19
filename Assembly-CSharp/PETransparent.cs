using System.Collections;
using UnityEngine;

public class PETransparent : MonoBehaviour
{
	public delegate void OnAlphaChangeCompleted(AiAlpha alpha, float dstAlpha);

	private static float Accuracy = 0.05f;

	private Renderer render;

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

	public void Fadein(float fadeTime)
	{
		if (!(render == null) && !(transparent == null))
		{
			for (int i = 0; i < render.materials.Length; i++)
			{
				Color color = render.materials[i].color;
				render.materials[i].shader = transparent;
				render.materials[i].color = new Color(color.r, color.g, color.b, 0f);
			}
			ChangeAlphaToValue(1f, fadeTime);
		}
	}

	public void Fadeout(float fadeTime)
	{
		if (!(render == null) && !(transparent == null) && render != null)
		{
			ChangeAlphaToValue(0f, fadeTime);
		}
	}

	public void ChangeAlphaToValue(float dstAlpha, float time = 2f)
	{
		if (!(render == null) && !(Mathf.Abs(render.material.color.a - dstAlpha) < 0.1f))
		{
			StopAllCoroutines();
			StartCoroutine(AlphaUpdate(dstAlpha, time));
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

	private IEnumerator AlphaUpdate(float dstAlpha, float time = 2f)
	{
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
		if (Mathf.Abs(dstAlpha - 1f) <= Accuracy && Mathf.Abs(render.material.color.a - 1f) <= Accuracy)
		{
			ActivateTransparent(isTransparent: false);
		}
	}

	private void Awake()
	{
		render = base.transform.GetComponentInChildren<Renderer>();
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
