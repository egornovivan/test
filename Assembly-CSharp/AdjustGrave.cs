using UnityEngine;

public class AdjustGrave : MonoBehaviour
{
	private float checkTime;

	private Renderer grave;

	private void Start()
	{
		checkTime = Time.time;
		grave = GetComponent<Renderer>();
		if (grave == null)
		{
			Object.Destroy(this);
		}
	}

	private void FixedUpdate()
	{
		if (Time.time - checkTime > 5f)
		{
			checkTime = Time.time;
			if (KillNPC.IsBurried(base.transform.position, out var upPos))
			{
				base.transform.position = upPos;
				grave.enabled = true;
			}
			else
			{
				grave.enabled = false;
			}
		}
	}
}
