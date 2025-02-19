using UnityEngine;

namespace AnimFollow;

public class HashIDs_AF : MonoBehaviour
{
	public readonly int version = 4;

	public int dyingState;

	public int locomotionState;

	public int deadBool;

	public int speedFloat;

	public int sneakingBool;

	public int frontTrigger;

	public int backTrigger;

	public int frontMirrorTrigger;

	public int backMirrorTrigger;

	public int idle;

	public int getupFront;

	public int getupBack;

	public int getupFrontMirror;

	public int getupBackMirror;

	public int anyStateToGetupFront;

	public int anyStateToGetupBack;

	public int anyStateToGetupFrontMirror;

	public int anyStateToGetupBackMirror;

	private void Awake()
	{
		dyingState = Animator.StringToHash("Base Layer.Dying");
		locomotionState = Animator.StringToHash("Base Layer.Locomotion");
		deadBool = Animator.StringToHash("Dead");
		sneakingBool = Animator.StringToHash("Sneaking");
		idle = Animator.StringToHash("Base Layer.MoveWithOutWeapon");
		speedFloat = Animator.StringToHash("Speed");
		frontTrigger = Animator.StringToHash("GetUpFront");
		backTrigger = Animator.StringToHash("GetUpBack");
		frontMirrorTrigger = Animator.StringToHash("GetUpFrontMirror");
		backMirrorTrigger = Animator.StringToHash("GetUpBackMirror");
		getupFront = Animator.StringToHash("Base Layer.GetupFront");
		getupBack = Animator.StringToHash("Base Layer.GetupBack");
		getupFrontMirror = Animator.StringToHash("Base Layer.GetupFrontMirror");
		getupBackMirror = Animator.StringToHash("Base Layer.GetupBackMirror");
		anyStateToGetupFront = Animator.StringToHash("AnyState -> Base Layer.GetupFront");
		anyStateToGetupBack = Animator.StringToHash("AnyState -> Base Layer.GetupBack");
		anyStateToGetupFrontMirror = Animator.StringToHash("AnyState -> Base Layer.GetupFrontMirror");
		anyStateToGetupBackMirror = Animator.StringToHash("AnyState -> Base Layer.GetupBackMirror");
	}
}
