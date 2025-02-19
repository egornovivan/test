using UnityEngine;

public class MinimapCamCtrl : MonoBehaviour
{
	public Material mWater;

	public Material mRiver;

	public Light mDirectionalLight;

	private void OnPreRender()
	{
		if ((bool)mWater)
		{
			mWater.shader.maximumLOD = 101;
		}
		if ((bool)mRiver)
		{
			mRiver.shader.maximumLOD = 101;
		}
	}

	private void OnPreCull()
	{
		if ((bool)mDirectionalLight)
		{
			mDirectionalLight.enabled = true;
		}
	}

	private void OnPostRender()
	{
		if ((bool)mWater)
		{
			mWater.shader.maximumLOD = 501;
		}
		if ((bool)mRiver)
		{
			mRiver.shader.maximumLOD = 501;
		}
		if ((bool)mDirectionalLight)
		{
			mDirectionalLight.enabled = false;
		}
	}
}
