using UnityEngine;

namespace TrainingScene;

public class EmitlineAppearance : MonoBehaviour
{
	private static EmitlineAppearance s_instance;

	private float lineMaxAlpha = 0.4f;

	private float signMaxColor = 0.5f;

	private float fadeTime;

	private float ctime;

	private float progress;

	private float colorNum;

	private bool produce;

	private bool destroy;

	public Material matLine;

	public Material eff_true;

	public Material eff_false;

	public static EmitlineAppearance Instance => s_instance;

	private void Awake()
	{
		s_instance = this;
	}

	private void Update()
	{
		if (produce)
		{
			ctime += Time.deltaTime;
			SetNewColor();
			if (fadeTime < ctime)
			{
				produce = false;
			}
		}
		else if (destroy)
		{
			ctime -= Time.deltaTime;
			SetNewColor();
			if (ctime < 0f)
			{
				destroy = false;
			}
		}
	}

	private void SetNewColor()
	{
		progress = ctime / fadeTime;
		colorNum = progress * 0.5f;
		matLine.SetColor("_TintColor", new Color(0.33f, 0.33f, 0.33f, progress * lineMaxAlpha));
		eff_true.SetColor("_TintColor", new Color(colorNum, colorNum, colorNum, 0.5f));
		eff_false.SetColor("_TintColor", new Color(colorNum, colorNum, colorNum, 0.5f));
	}

	public void StartFadeLine(float fade, bool prod)
	{
		fadeTime = fade;
		if (prod)
		{
			produce = true;
			ctime = 0f;
		}
		else
		{
			destroy = true;
			ctime = fadeTime;
		}
	}
}
