using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class FBIKBoxing : MonoBehaviour
{
	public Transform target;

	public Transform pin;

	public FullBodyBipedIK ik;

	public AimIK aim;

	public float weight;

	public FullBodyBipedEffector effector;

	public AnimationCurve unityFreeHitWeight;

	public AnimationCurve aimWeight;

	private Animator animator;

	private AnimatorStateInfo info;

	private float GetHitWeight()
	{
		info = animator.GetCurrentAnimatorStateInfo(0);
		if (info.IsName("Boxing"))
		{
			return unityFreeHitWeight.Evaluate(info.normalizedTime);
		}
		return 0f;
	}

	private void Start()
	{
		animator = GetComponent<Animator>();
		ik.Disable();
		if (aim != null)
		{
			aim.Disable();
		}
	}

	private void LateUpdate()
	{
		float hitWeight = GetHitWeight();
		ik.solver.GetEffector(effector).position = target.position;
		ik.solver.GetEffector(effector).positionWeight = hitWeight * weight;
		if (aim != null)
		{
			aim.solver.transform.LookAt(pin.position);
			aim.solver.IKPosition = target.position;
			aim.solver.IKPositionWeight = aimWeight.Evaluate(hitWeight) * weight;
			aim.solver.Update();
		}
		ik.solver.Update();
	}
}
