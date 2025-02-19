using UnityEngine;

public class GroundEffect_RK : MonoBehaviour
{
	public Transform dirtyLeft;

	public Transform dirtyRight;

	public int particleId;

	private void Start()
	{
		Animation componentInChildren = GetComponentInChildren<Animation>();
		if (componentInChildren != null)
		{
			AnimationEvent animationEvent = new AnimationEvent();
			animationEvent.time = 0.5f;
			animationEvent.functionName = "GoundDirtyEffect";
			AnimationClip clip = componentInChildren["skywalk"].clip;
			if (clip != null)
			{
				clip.AddEvent(animationEvent);
			}
			AnimationClip clip2 = componentInChildren["skyaTKIdle"].clip;
			if (clip2 != null)
			{
				clip2.AddEvent(animationEvent);
			}
			AnimationClip clip3 = componentInChildren["skyrun"].clip;
			if (clip3 != null)
			{
				clip3.AddEvent(animationEvent);
			}
			AnimationClip clip4 = componentInChildren["skyidle"].clip;
			if (clip4 != null)
			{
				clip4.AddEvent(animationEvent);
			}
		}
	}

	public void GoundDirtyEffect()
	{
		if (!(dirtyLeft == null) && !(dirtyRight == null) && particleId > 0)
		{
			if (Physics.Raycast(dirtyLeft.position, -Vector3.up, out var hitInfo, 8f, AiUtil.groundedLayer))
			{
				EffectManager.Instance.Instantiate(particleId, hitInfo.point, Quaternion.identity);
			}
			if (Physics.Raycast(dirtyRight.position, -Vector3.up, out hitInfo, 8f, AiUtil.groundedLayer))
			{
				EffectManager.Instance.Instantiate(particleId, hitInfo.point, Quaternion.identity);
			}
		}
	}
}
