using UnityEngine;

public class Bloodcmpt : MonoBehaviour
{
	[SerializeField]
	private Transform ForeGround;

	[SerializeField]
	private Transform BackGround;

	public void setForeScale(Vector3 scale)
	{
		ForeGround.localScale = scale;
	}

	public void setBackScale(Vector3 scale)
	{
		BackGround.localScale = scale;
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
