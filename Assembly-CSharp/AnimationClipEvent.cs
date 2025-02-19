using UnityEngine;

public class AnimationClipEvent : MonoBehaviour
{
	private void AnimationSound(int id)
	{
		AudioManager.instance.Create(base.transform.position, id);
	}
}
