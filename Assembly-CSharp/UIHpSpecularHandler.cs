using UnityEngine;

[ExecuteInEditMode]
public class UIHpSpecularHandler : MonoBehaviour
{
	private UITexture uitex;

	public Shader HpSpecShader;

	public Texture2D MainTex;

	public Texture2D SpecularMask;

	public Texture2D RandomTex;

	public float Intensity = 1.8f;

	public Color Color0;

	public Color Color1;

	public Color Color2;

	public Color Color3;

	public float Randomness = 0.05f;

	public float WaveThreshold = 0.28f;

	public float WaveLength = 20f;

	public float WaveSpeed = 4f;

	public float FadeTime = 1f;

	public float Value = 1f;

	private float scale_x = 164f;

	private void Start()
	{
		uitex = GetComponent<UITexture>();
		uitex.material = new Material(HpSpecShader);
	}

	private void Update()
	{
		if (!(uitex == null))
		{
			Vector3 localScale = uitex.transform.localScale;
			localScale.x = Value * scale_x;
			uitex.transform.localScale = localScale;
			if (uitex.material == null)
			{
				uitex.material = new Material(HpSpecShader);
			}
			uitex.material.SetTexture("_MainTex", MainTex);
			uitex.material.SetTexture("_SpecularMask", SpecularMask);
			uitex.material.SetTexture("_RandomTex", RandomTex);
			if (SpecularMask != null)
			{
				uitex.material.SetVector("_SizeSettings", new Vector4(base.transform.localScale.x, base.transform.localScale.y, SpecularMask.width, SpecularMask.height));
			}
			else
			{
				uitex.material.SetVector("_SizeSettings", new Vector4(base.transform.localScale.x, base.transform.localScale.y, 64f, 64f));
				uitex.material.SetVector("_Border", new Vector4(0f, 0f, 0f, 0f));
			}
			uitex.material.SetFloat("_FadeTime", Time.time * FadeTime);
			uitex.material.SetColor("_Color0", Color0);
			uitex.material.SetColor("_Color1", Color1);
			uitex.material.SetColor("_Color2", Color2);
			uitex.material.SetColor("_Color3", Color3);
			uitex.material.SetFloat("_Intensity", Intensity);
			uitex.material.SetVector("_OtherSettings", new Vector4(Randomness, WaveThreshold, WaveLength, WaveSpeed));
		}
	}
}
