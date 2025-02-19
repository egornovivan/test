using System;
using UnityEngine;

[Serializable]
public class EffectDesc
{
	private bool IsPlayed;

	[HideInInspector]
	public Transform m_effroot;

	public GameObject EffectObject;

	public float DelayTime;

	public float DestroyTime;

	public string Parent;

	public Vector3 Position;

	public Vector3 Rotation;

	public void Play()
	{
		if (!IsPlayed)
		{
			IsPlayed = true;
			GameObject gameObject = UnityEngine.Object.Instantiate(EffectObject);
			if (m_effroot == null)
			{
				Debug.LogError("Can't find the parent to create effect");
			}
			else
			{
				gameObject.transform.parent = m_effroot;
			}
			gameObject.transform.localPosition = Position;
			gameObject.transform.localRotation = Quaternion.Euler(Rotation);
			gameObject.AddComponent<DestroyTimer>().m_LifeTime = DestroyTime;
		}
	}
}
