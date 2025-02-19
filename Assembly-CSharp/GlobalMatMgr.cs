using UnityEngine;

public class GlobalMatMgr : MonoBehaviour
{
	public Material EnergySheildMat;

	private void Start()
	{
	}

	private void Update()
	{
		if (EnergySheildMat != null)
		{
			EnergySheildMat.SetFloat("_TimeFactor", Time.time);
		}
	}
}
