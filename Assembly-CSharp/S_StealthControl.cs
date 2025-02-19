using UnityEngine;

public class S_StealthControl : MonoBehaviour
{
	private SkinnedMeshRenderer smr;

	public bool anti_stealth;

	private float fadeTime1;

	private float fadeTime2;

	private float dt1;

	private float dt2;

	private void Start()
	{
		smr = base.transform.GetComponent<SkinnedMeshRenderer>();
		Material material = Resources.Load("Materials/Stealth") as Material;
		Texture texture = smr.GetComponent<Renderer>().material.GetTexture(0);
		material.SetTexture(0, texture);
		smr.GetComponent<Renderer>().material = material;
		fadeTime1 = smr.GetComponent<Renderer>().material.GetFloat("_HidTime") * (1f + smr.GetComponent<Renderer>().material.GetFloat("_Tail")) + 1f;
		fadeTime2 = smr.GetComponent<Renderer>().material.GetFloat("_AprTime") + smr.GetComponent<Renderer>().material.GetFloat("_FlsTime") + 1f;
	}

	private void Update()
	{
		if (anti_stealth)
		{
			if (dt2 < fadeTime2)
			{
				dt2 += Time.deltaTime;
				smr.GetComponent<Renderer>().material.SetFloat("_Dt2", dt2);
			}
			else
			{
				Object.Destroy(this);
			}
		}
		else if (dt1 < fadeTime1)
		{
			dt1 += Time.deltaTime;
			smr.GetComponent<Renderer>().material.SetFloat("_Dt1", dt1);
		}
	}
}
