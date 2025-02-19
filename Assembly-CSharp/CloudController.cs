using UnityEngine;

public class CloudController : MonoBehaviour
{
	public const int WeatherCloud = 0;

	public const int TerrainCloud = 1;

	public Color mBasColor = Color.white;

	public int CloudType;

	public Light mSun;

	public void InitCloud(Light sun, Cloud3D cloud)
	{
		mSun = sun;
		CloudType = cloud.mCloudType;
		mBasColor = cloud.mBaseColor;
		base.transform.localPosition = cloud.mPosition;
	}

	private void Update()
	{
		if (!(mSun == null))
		{
			switch (CloudType)
			{
			case 0:
				GetComponent<Renderer>().material.SetColor("_TintColor", mSun.color * 0.8f);
				break;
			case 1:
				GetComponent<Renderer>().material.SetColor("_TintColor", mSun.color * 0.4f + mBasColor * 0.6f);
				break;
			}
		}
	}
}
