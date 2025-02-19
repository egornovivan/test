using UnityEngine;

namespace RootMotion.FinalIK;

public class SolverManager : MonoBehaviour
{
	public float timeStep;

	public bool fixTransforms = true;

	private float lastTime;

	private Animator animator;

	private Animation animation;

	private bool updateFrame;

	private bool componentInitiated;

	private bool animatePhysics
	{
		get
		{
			if (animator != null)
			{
				return animator.updateMode == AnimatorUpdateMode.AnimatePhysics;
			}
			if (animation != null)
			{
				return animation.animatePhysics;
			}
			return false;
		}
	}

	private bool isAnimated => animator != null || animation != null;

	public void Disable()
	{
		Initiate();
		base.enabled = false;
	}

	protected virtual void InitiateSolver()
	{
	}

	protected virtual void UpdateSolver()
	{
	}

	protected virtual void FixTransforms()
	{
	}

	private void Start()
	{
		Initiate();
	}

	private void Update()
	{
		if (!animatePhysics && fixTransforms)
		{
			FixTransforms();
		}
	}

	private void Initiate()
	{
		if (!componentInitiated)
		{
			FindAnimatorRecursive(base.transform, findInChildren: true);
			InitiateSolver();
			componentInitiated = true;
		}
	}

	private void FindAnimatorRecursive(Transform t, bool findInChildren)
	{
		if (isAnimated)
		{
			return;
		}
		animator = t.GetComponent<Animator>();
		animation = t.GetComponent<Animation>();
		if (!isAnimated)
		{
			if (animator == null && findInChildren)
			{
				animator = t.GetComponentInChildren<Animator>();
			}
			if (animation == null && findInChildren)
			{
				animation = t.GetComponentInChildren<Animation>();
			}
			if (!isAnimated && t.parent != null)
			{
				FindAnimatorRecursive(t.parent, findInChildren: false);
			}
		}
	}

	private void FixedUpdate()
	{
		updateFrame = true;
		if (animatePhysics && fixTransforms)
		{
			FixTransforms();
		}
	}

	private void LateUpdate()
	{
		if (!animatePhysics)
		{
			updateFrame = true;
		}
		if (updateFrame)
		{
			updateFrame = false;
			if (timeStep == 0f)
			{
				UpdateSolver();
			}
			else if (Time.time >= lastTime + timeStep)
			{
				UpdateSolver();
				lastTime = Time.time;
			}
		}
	}

	public void ExecStart()
	{
		Start();
	}
}
