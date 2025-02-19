using UnityEngine;

[ExecuteInEditMode]
public class UIHoloLinesHandler : MonoBehaviour
{
	private UITexture uitex;

	public Shader HoloShader;

	public Color MainColor;

	public Texture2D MainTex;

	public Texture2D TexH;

	public Texture2D TexV;

	public float Intensity = 1.8f;

	public Vector4 TileAndSpeed = new Vector4(48f, 7f, 1f, 1f);

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
				uitex.material = new Material(HoloShader);
			}
			uitex.material.SetColor("_Color", MainColor);
			uitex.material.SetTexture("_MainTex", MainTex);
			uitex.material.SetTexture("_TexH", TexH);
			uitex.material.SetTexture("_TexV", TexV);
			uitex.material.SetFloat("_Intensity", Intensity);
			uitex.material.SetVector("_TileSpeed", TileAndSpeed);
		}
	}
}
