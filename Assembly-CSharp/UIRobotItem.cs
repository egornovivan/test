using System.Collections;
using ItemAsset;
using UnityEngine;
using WhiteCat;

public class UIRobotItem : MonoBehaviour
{
	[SerializeField]
	private GameObject mAttackBtn;

	[SerializeField]
	private GameObject mDefenceBtn;

	[SerializeField]
	private GameObject mCureBtn;

	[SerializeField]
	private GameObject mRestBtn;

	[SerializeField]
	private GameObject mChoseList;

	[SerializeField]
	private UICheckbox mAttCk;

	[SerializeField]
	private UICheckbox mDefCk;

	[SerializeField]
	private UICheckbox mCureCk;

	[SerializeField]
	private UICheckbox mResCk;

	[SerializeField]
	private UICheckbox mBtnCk;

	[SerializeField]
	private BoxCollider mAttCol;

	[SerializeField]
	private BoxCollider mDefCol;

	[SerializeField]
	private BoxCollider mCureCol;

	[SerializeField]
	private BoxCollider mResCol;

	[SerializeField]
	private UISlider mHealth;

	[SerializeField]
	private UISlider mEnergy;

	[SerializeField]
	private UITexture mHeadTex;

	[SerializeField]
	private TweenAlpha mAnimation;

	[SerializeField]
	private UISlicedSprite mForeground;

	[HideInInspector]
	public ItemObject mItemObj;

	[HideInInspector]
	public GameObject mGameobj;

	private AIMode mBattlType = AIMode.Passive;

	private AIMode mOldBattlType = AIMode.Passive;

	private bool init;

	private LifeLimit mLifeCmpt;

	private Energy mEnergyCmpt;

	public bool IsShow => base.gameObject.activeSelf;

	private void Update()
	{
		RefreshInfo();
		if (RobotController.playerFollower != null)
		{
			mBattlType = RobotController.playerFollower.aiMode;
		}
		if (mOldBattlType != mBattlType)
		{
			ShowBattle();
		}
	}

	private void LateUpdate()
	{
		if (mChoseList.gameObject.activeSelf && (Input.GetMouseButton(0) || Input.GetMouseButton(1)) && !init)
		{
			init = true;
			Ray ray = UICamera.mainCamera.ScreenPointToRay(Input.mousePosition);
			if (mAttCol.Raycast(ray, out var hitInfo, 300f) || mDefCol.Raycast(ray, out hitInfo, 300f) || mCureCol.Raycast(ray, out hitInfo, 300f) || mResCol.Raycast(ray, out hitInfo, 300f))
			{
				StartCoroutine(BtnCkStateChange(0.2f));
			}
			else
			{
				StartCoroutine(BtnCkStateChange(0.1f));
			}
		}
	}

	private IEnumerator BtnCkStateChange(float _waittime)
	{
		yield return new WaitForSeconds(_waittime);
		mChoseList.SetActive(value: false);
		mBtnCk.isChecked = false;
		init = false;
	}

	private void ShowBattle()
	{
		mOldBattlType = mBattlType;
		switch (RobotController.playerFollower.aiMode)
		{
		case AIMode.Attack:
			mAttackBtn.SetActive(value: true);
			mDefenceBtn.SetActive(value: false);
			mCureBtn.SetActive(value: false);
			mRestBtn.SetActive(value: false);
			mAttCk.isChecked = true;
			mDefCk.isChecked = false;
			mCureCk.isChecked = false;
			mResCk.isChecked = false;
			break;
		case AIMode.Defence:
			mAttackBtn.SetActive(value: false);
			mDefenceBtn.SetActive(value: true);
			mCureBtn.SetActive(value: false);
			mRestBtn.SetActive(value: false);
			mAttCk.isChecked = false;
			mDefCk.isChecked = true;
			mCureCk.isChecked = false;
			mResCk.isChecked = false;
			break;
		case AIMode.Cure:
			mAttackBtn.SetActive(value: false);
			mDefenceBtn.SetActive(value: false);
			mCureBtn.SetActive(value: true);
			mRestBtn.SetActive(value: false);
			mAttCk.isChecked = false;
			mDefCk.isChecked = false;
			mCureCk.isChecked = true;
			mResCk.isChecked = false;
			break;
		case AIMode.Passive:
			mAttackBtn.SetActive(value: false);
			mDefenceBtn.SetActive(value: false);
			mCureBtn.SetActive(value: false);
			mRestBtn.SetActive(value: true);
			mAttCk.isChecked = false;
			mDefCk.isChecked = false;
			mCureCk.isChecked = false;
			mResCk.isChecked = true;
			break;
		}
	}

	private void RefreshInfo()
	{
		if (mItemObj != null)
		{
			mHeadTex.mainTexture = mItemObj.iconTex;
			mLifeCmpt = mItemObj.GetCmpt<LifeLimit>();
			mEnergyCmpt = mItemObj.GetCmpt<Energy>();
			mHealth.sliderValue = mLifeCmpt.floatValue.percent;
			mEnergy.sliderValue = mEnergyCmpt.floatValue.percent;
			if (mEnergyCmpt.floatValue.percent < 0.3f)
			{
				mAnimation.enabled = true;
				return;
			}
			mAnimation.enabled = false;
			mForeground.alpha = 1f;
		}
	}

	private void OnBtnClick()
	{
		if (!(mGameobj == null))
		{
			DragItemMousePickRobot component = mGameobj.GetComponent<DragItemMousePickRobot>();
			if (component != null)
			{
				component.DoGetItem();
			}
		}
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
	}

	private void ShowOptions(bool active)
	{
		if (!mChoseList.activeSelf && active)
		{
			mChoseList.SetActive(active);
		}
	}

	private void OnAttackChosebtn(bool active)
	{
		if (active)
		{
			ChangeBattle(AIMode.Attack);
		}
	}

	private void OnDefenceChoseBtn(bool active)
	{
		if (active)
		{
			ChangeBattle(AIMode.Defence);
		}
	}

	private void OnRestChoseBtn(bool active)
	{
		if (active)
		{
			ChangeBattle(AIMode.Passive);
		}
	}

	private void OnCureChoseBtn(bool active)
	{
		if (active)
		{
			ChangeBattle(AIMode.Cure);
		}
	}

	private void ChangeBattle(AIMode type)
	{
		if (!(RobotController.playerFollower == null))
		{
			RobotController.playerFollower.aiMode = type;
		}
	}
}
