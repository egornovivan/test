using Pathea;
using UnityEngine;

namespace TrainingScene;

public class HoloherbGather : MonoBehaviour
{
	public HoloherbAppearance appearance;

	private PeEntity mplayer;

	private Transform playerTrans;

	private AnimatorCmpt anmt;

	private MotionMgrCmpt mmc;

	private bool isGathering;

	[HideInInspector]
	public bool isDead;

	private float ctime;

	private void Start()
	{
		mplayer = PeSingleton<PeCreature>.Instance.mainPlayer;
		playerTrans = mplayer.peTrans.trans;
		anmt = mplayer.GetCmpt<AnimatorCmpt>();
		mmc = mplayer.GetCmpt<MotionMgrCmpt>();
	}

	private void Update()
	{
		if (!isDead && PeInput.Get(PeInput.LogicFunction.GatherHerb) && !isGathering && Vector3.SqrMagnitude(playerTrans.position - base.transform.position) < 7f)
		{
			mmc.EndImmediately(PEActionType.Move);
			mmc.EndImmediately(PEActionType.Sprint);
			mmc.EndImmediately(PEActionType.Rotate);
			ctime = 0f - Time.deltaTime;
			isGathering = true;
			anmt.SetBool("Gather", value: true);
			mmc.SetMaskState(PEActionMask.Gather, state: true);
		}
		if (!isGathering)
		{
			return;
		}
		ctime += Time.deltaTime;
		if (ctime < 1.966f)
		{
			if (!isDead && ctime >= 1.1f)
			{
				ApplyCut();
			}
		}
		else
		{
			isGathering = false;
		}
	}

	private void ApplyCut()
	{
		isDead = true;
		HoloherbTask.Instance.SubHerb(appearance);
		anmt.SetBool("Gather", value: false);
		mmc.SetMaskState(PEActionMask.Gather, state: false);
	}
}
