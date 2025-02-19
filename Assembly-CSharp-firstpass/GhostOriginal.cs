using UnityEngine;

public class GhostOriginal : MonoBehaviour
{
	public GameObject character;

	public Vector3 offset;

	public void Synch()
	{
		foreach (AnimationState item in character.GetComponent<Animation>())
		{
			AnimationState animationState2 = GetComponent<Animation>()[item.name];
			if (animationState2 == null)
			{
				GetComponent<Animation>().AddClip(item.clip, item.name);
				animationState2 = GetComponent<Animation>()[item.name];
			}
			if (animationState2.enabled != item.enabled)
			{
				animationState2.wrapMode = item.wrapMode;
				animationState2.enabled = item.enabled;
				animationState2.speed = item.speed;
			}
			animationState2.weight = item.weight;
			animationState2.time = item.time;
		}
	}

	private void LateUpdate()
	{
		base.transform.position = character.transform.position + offset;
		base.transform.rotation = character.transform.rotation;
	}
}
