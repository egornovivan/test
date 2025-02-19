using UnityEngine;

[ExecuteInEditMode]
public class UISpecularHandler : UIEffectAlpha
{
	private UITexture uitex;

	public Shader SpecShader;

	public Texture2D MainTex;

	public Texture2D SpecularMask;

	public Texture2D RandomTex;

	public RectOffset Border = new RectOffset();

	public float Intensity = 1.8f;

	public Color Color0;

	public Color Color1;

	public Color Color2;

	public Color Color3;

	public float Randomness = 0.05f;

	public float WaveThreshold = 0.28f;

	public float WaveLength = 20f;

	public float WaveSpeed = 4f;

	private void Start()
	{
		uitex = GetComponent<UITexture>();
		uitex.material = new Material(SpecShader);
	}

	private void Update()
	{
		if (!(uitex == null))
		{
			if (uitex.material == null)
			{
				uitex.material = new Material(SpecShader);
			}
			uitex.material.SetTexture("_MainTex", MainTex);
			uitex.material.SetTexture("_SpecularMask", SpecularMask);
			uitex.material.SetTexture("_RandomTex", RandomTex);
			if (SpecularMask != null)
			{
				uitex.material.SetVector("_SizeSettings", new Vector4(base.transform.localScale.x, base.transform.localScale.y, SpecularMask.width, SpecularMask.height));
				uitex.material.SetVector("_Border", new Vector4(Border.left, Border.top, Border.right, Border.bottom));
			}
			else
			{
				uitex.material.SetVector("_SizeSettings", new Vector4(base.transform.localScale.x, base.transform.localScale.y, 64f, 64f));
				uitex.material.SetVector("_Border", new Vector4(0f, 0f, 0f, 0f));
			}
			uitex.material.SetFloat("_FadeTime", Time.time);
			Color color = Color.Lerp(Color.black, Color0, alpha);
			uitex.material.SetColor("_Color0", color);
			Color color2 = Color.Lerp(Color.black, Color1, alpha);
			uitex.material.SetColor("_Color1", color2);
			Color color3 = Color.Lerp(Color.black, Color2, alpha);
			uitex.material.SetColor("_Color2", color3);
			Color color4 = Color.Lerp(Color.black, Color3, alpha);
			uitex.material.SetColor("_Color3", color4);
			uitex.material.SetFloat("_Intensity", Intensity);
			uitex.material.SetVector("_OtherSettings", new Vector4(Randomness, WaveThreshold, WaveLength, WaveSpeed));
		}
	}
}
