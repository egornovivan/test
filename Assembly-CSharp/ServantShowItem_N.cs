using Pathea;
using UnityEngine;

public class ServantShowItem_N : MonoBehaviour
{
	public UITexture mHead;

	public UISprite mDeadSpr;

	public UISlider mLife;

	public UISlider mComfort;

	public UISlider mOxygen;

	public UISprite mState;

	private PeEntity mNpc;

	private EntityInfoCmpt entityInfo;

	private bool mIsDead;

	public PeEntity NPC => mNpc;

	private void Update()
	{
	}

	public void SetNpc(PeEntity npc)
	{
		mNpc = npc;
		if ((bool)mNpc)
		{
			mIsDead = false;
			mHead.mainTexture = npc.GetCmpt<EntityInfoCmpt>().faceTex;
			mHead.enabled = true;
			mDeadSpr.enabled = false;
			mState.enabled = true;
		}
		else
		{
			mHead.mainTexture = null;
			mHead.enabled = false;
			mDeadSpr.enabled = false;
			mLife.sliderValue = 0f;
			mComfort.sliderValue = 0f;
			mOxygen.sliderValue = 0f;
			mState.enabled = false;
		}
	}

	private void OnServantStateBtn()
	{
	}

	private void OnServanteHead()
	{
	}
}
