using UnityEngine;

public class VCParticleMgr : MonoBehaviour
{
	private static VCParticleMgr s_Instance;

	public GameObject Smoke_3cm;

	public GameObject Smoke_10cm;

	public GameObject Fire_3cm;

	public GameObject Fire_10cm;

	public GameObject Explode_5x3m;

	public GameObject Explode_16x8m;

	public GameObject WreckageSpurt_3cm;

	public GameObject WreckageSpurt_10cm;

	public static VCParticleMgr Instance => s_Instance;

	private void Awake()
	{
		s_Instance = this;
	}

	public static GameObject GetEffect(string effect, VCESceneSetting scene_setting)
	{
		if (Instance == null)
		{
			return null;
		}
		if (scene_setting == null)
		{
			return null;
		}
		int num = Mathf.RoundToInt(scene_setting.m_VoxelSize * 100f);
		int num2 = Mathf.RoundToInt(scene_setting.EditorWorldSize.x);
		int num3 = Mathf.RoundToInt(scene_setting.EditorWorldSize.z);
		switch (num)
		{
		case 3:
			if (effect.ToLower() == "fire")
			{
				return Instance.Fire_3cm;
			}
			if (effect.ToLower() == "smoke")
			{
				return Instance.Smoke_3cm;
			}
			if (effect.ToLower() == "wreckage spurt")
			{
				return Instance.WreckageSpurt_3cm;
			}
			break;
		case 10:
			if (effect.ToLower() == "fire")
			{
				return Instance.Fire_10cm;
			}
			if (effect.ToLower() == "smoke")
			{
				return Instance.Smoke_10cm;
			}
			if (effect.ToLower() == "wreckage spurt")
			{
				return Instance.WreckageSpurt_10cm;
			}
			break;
		default:
			if (effect.ToLower() == "fire")
			{
				return Instance.Fire_3cm;
			}
			if (effect.ToLower() == "smoke")
			{
				return Instance.Smoke_3cm;
			}
			if (effect.ToLower() == "wreckage spurt")
			{
				return Instance.WreckageSpurt_3cm;
			}
			break;
		}
		if (num2 == 3 && num3 == 5)
		{
			if (effect.ToLower() == "explode")
			{
				return Instance.Explode_5x3m;
			}
		}
		else if (num2 == 8 && num3 == 16)
		{
			if (effect.ToLower() == "explode")
			{
				return Instance.Explode_16x8m;
			}
		}
		else if (effect.ToLower() == "explode")
		{
			return Instance.Explode_5x3m;
		}
		return null;
	}
}
