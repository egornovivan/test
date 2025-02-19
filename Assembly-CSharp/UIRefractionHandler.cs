using UnityEngine;

[ExecuteInEditMode]
public class UIRefractionHandler : MonoBehaviour
{
	private UITexture uitex;

	public Shader _shader;

	public Texture2D MainTex;

	public Texture2D DistortionTex;

	public Texture2D RandomTex;

	public RectOffset Border = new RectOffset();

	public float Intensity = 1.8f;

	public float Randomness = 0.05f;

	private void Start()
	{
		uitex = GetComponent<UITexture>();
	}

	private void Update()
	{
		if (!(uitex == null))
		{
			if (uitex.material == null)
			{
				uitex.material = new Material(_shader);
			}
			uitex.material.SetTexture("_MainTex", MainTex);
			uitex.material.SetTexture("_DistortionTex", DistortionTex);
			uitex.material.SetTexture("_RandomTex", RandomTex);
			if (DistortionTex != null)
			{
				uitex.material.SetVector("_SizeSettings", new Vector4(base.transform.localScale.x, base.transform.localScale.y, DistortionTex.width, DistortionTex.height));
				uitex.material.SetVector("_Border", new Vector4(Border.left, Border.top, Border.right, Border.bottom));
			}
			else
			{
				uitex.material.SetVector("_SizeSettings", new Vector4(base.transform.localScale.x, base.transform.localScale.y, 64f, 64f));
				uitex.material.SetVector("_Border", new Vector4(0f, 0f, 0f, 0f));
			}
			uitex.material.SetFloat("_FadeTime", Time.time);
			uitex.material.SetFloat("_Intensity", Intensity);
			uitex.material.SetFloat("_Randomness", Randomness);
		}
	}
}
