using UnityEngine;

public class testTrail6x : MonoBehaviour
{
	private WeaponTrail wt;

	private void Start()
	{
		wt = GetComponent<WeaponTrail>();
		wt.ClearTrail();
		wt.StartTrail(0.2f, 0.2f);
	}

	private void LateUpdate()
	{
		if (null != wt)
		{
			float deltaTime = Mathf.Clamp(Time.deltaTime * 1f, 0f, 0.066f);
			wt.Itterate(0f);
			if (wt.time > 0f)
			{
				wt.UpdateTrail(Time.time, deltaTime);
			}
		}
	}

	public void OpenTrail()
	{
	}

	public void CloseTrail()
	{
	}
}
