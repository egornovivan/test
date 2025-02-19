using UnityEngine;

namespace AnimFollow;

public static class HashIDs
{
	public static int Idle = Animator.StringToHash("Base Layer.GroundLocomotion.Move");

	public static int IdleWater = Animator.StringToHash("Base Layer.WaterLocomotion.Move");

	public static int MonsterIdle = Animator.StringToHash("Base Layer.Locomotion");

	public static int FrontTrigger = Animator.StringToHash("GetUpFront");

	public static int BackTrigger = Animator.StringToHash("GetUpBack");

	public static int FrontMirrorTrigger = Animator.StringToHash("GetUpFrontMirror");

	public static int BackMirrorTrigger = Animator.StringToHash("GetUpBackMirror");

	public static int GetupFront = Animator.StringToHash("Base Layer.Ragdoll.GetupFront");

	public static int GetupBack = Animator.StringToHash("Base Layer.Ragdoll.GetupBack");

	public static int GetupFrontMirror = Animator.StringToHash("Base Layer.Ragdoll.GetupFrontMirror");

	public static int GetupBackMirror = Animator.StringToHash("Base Layer.Ragdoll.GetupBackMirror");

	public static int GetupFrontWater = Animator.StringToHash("Base Layer.Ragdoll.GetupFront_Water");

	public static int GetupBackWater = Animator.StringToHash("Base Layer.Ragdoll.GetupBack_Water");

	public static int GetupFrontMirrorWater = Animator.StringToHash("Base Layer.Ragdoll.GetupFrontMirror_Water");

	public static int GetupBackMirrorWater = Animator.StringToHash("Base Layer.Ragdoll.GetupBackMirror_Water");

	public static int AnyStateToGetupFront = Animator.StringToHash("AnyState -> GetupFront");

	public static int AnyStateToGetupBack = Animator.StringToHash("AnyState -> GetupBack");

	public static int AnyStateToGetupFrontMirror = Animator.StringToHash("AnyState -> GetupFrontMirror");

	public static int AnyStateToGetupBackMirror = Animator.StringToHash("AnyState -> GetupBackMirror");

	public static int AnyStateToGetupFrontWater = Animator.StringToHash("AnyState -> GetupFront_Water");

	public static int AnyStateToGetupBackWater = Animator.StringToHash("AnyState -> GetupBack_Water");

	public static int AnyStateToGetupFrontMirrorWater = Animator.StringToHash("AnyState -> GetupFrontMirror_Water");

	public static int AnyStateToGetupBackMirrorWater = Animator.StringToHash("AnyState -> GetupBackMirror_Water");
}
