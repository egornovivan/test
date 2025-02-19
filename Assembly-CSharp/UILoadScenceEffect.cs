using Pathea;
using UnityEngine;

public class UILoadScenceEffect : MonoBehaviour
{
	public enum LogoState
	{
		Null,
		LogoIn,
		Logo,
		LogoOut
	}

	public delegate void CallbackEnvet();

	private const float mLogoFadeTime = 2f;

	private const float mLogoShowTime = 1f;

	private const float mWaitTime = 1.2f;

	private static UILoadScenceEffect mInstance;

	[SerializeField]
	private GameObject mProcessUI;

	[SerializeField]
	private UILabel mLbProcess;

	[SerializeField]
	private UISprite mLoadSpr;

	[SerializeField]
	private UITexture mBlackTexture;

	[SerializeField]
	private UITexture mLogoTexture;

	[SerializeField]
	private GameObject mMaskUI;

	private bool isBeginScence;

	private bool isEndScence;

	private bool isStarLoad;

	private LogoState mLogoState;

	private float tempTime;

	public static UILoadScenceEffect Instance => mInstance;

	public bool isInProgress => base.isActiveAndEnabled ? true : false;

	private event CallbackEnvet e_BeginScenceOK;

	private event CallbackEnvet e_EndScenceOK;

	private event CallbackEnvet e_LogoAllIn;

	private event CallbackEnvet e_LogoAllOut;

	public void EnableProgress(bool enable)
	{
		base.gameObject.SetActive(value: true);
		mBlackTexture.alpha = 1f;
		mProcessUI.SetActive(enable);
		isStarLoad = enable;
		tempTime = 0f;
	}

	public void SetProgress(int value)
	{
		mLbProcess.text = value + "%";
	}

	public void BeginScence(CallbackEnvet beginScenceOK, bool enableProgressUI = false)
	{
		this.e_BeginScenceOK = beginScenceOK;
		base.gameObject.SetActive(value: true);
		mBlackTexture.alpha = 1f;
		mBlackTexture.enabled = true;
		mProcessUI.SetActive(enableProgressUI);
		SetProgress(0);
		tempTime = 0f;
		mMaskUI.SetActive(value: true);
		isBeginScence = true;
		isEndScence = false;
	}

	public void EndScence(CallbackEnvet endScenceOK, bool enableProgressUI = false)
	{
		PeGameMgr.yirdName = string.Empty;
		this.e_EndScenceOK = endScenceOK;
		base.gameObject.SetActive(value: true);
		mBlackTexture.enabled = true;
		if (enableProgressUI)
		{
			mBlackTexture.alpha = 1f;
			mProcessUI.SetActive(value: true);
			SetProgress(0);
			tempTime = 1.2f;
		}
		else
		{
			mBlackTexture.alpha = 0f;
			mProcessUI.SetActive(value: false);
			SetProgress(0);
			tempTime = 1.2f;
		}
		mMaskUI.SetActive(value: true);
		isEndScence = true;
		isBeginScence = false;
		if (null != MainPlayerCmpt.gMainPlayer)
		{
			MainPlayerCmpt.gMainPlayer.StartInvincible();
		}
	}

	public void PalyLogoTexture(CallbackEnvet logoAllIn, CallbackEnvet logoAllOut)
	{
		base.gameObject.SetActive(value: true);
		mBlackTexture.enabled = true;
		mProcessUI.SetActive(value: false);
		this.e_LogoAllIn = logoAllIn;
		this.e_LogoAllOut = logoAllOut;
		mLogoTexture.mainTexture = Resources.Load("Texture2d/Tex/leibang_intro_logo") as Texture2D;
		tempTime = 0f;
		mMaskUI.gameObject.SetActive(value: true);
		mLogoState = LogoState.LogoIn;
	}

	private void Awake()
	{
		mInstance = this;
		mLogoState = LogoState.Null;
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Update()
	{
		if (isStarLoad)
		{
			tempTime += Time.deltaTime;
			mLoadSpr.transform.rotation = Quaternion.Euler(0f, 0f, (float)(int)(tempTime / 0.025f) * -14.4f);
			return;
		}
		if (isBeginScence)
		{
			mBlackTexture.alpha = Mathf.Clamp01(1f - tempTime / 1.2f);
			if (tempTime >= 1.2f)
			{
				isBeginScence = false;
				mMaskUI.SetActive(value: false);
				base.gameObject.SetActive(value: false);
				AudioListener.pause = false;
				if (this.e_BeginScenceOK != null)
				{
					this.e_BeginScenceOK();
					this.e_BeginScenceOK = null;
				}
			}
			tempTime += Time.deltaTime;
			return;
		}
		if (isEndScence)
		{
			tempTime += Time.deltaTime;
			mBlackTexture.alpha = Mathf.Clamp01(tempTime / 1.2f);
			if (tempTime >= 1.2f)
			{
				isEndScence = false;
				if (this.e_EndScenceOK != null)
				{
					this.e_EndScenceOK();
					this.e_EndScenceOK = null;
				}
			}
			return;
		}
		switch (mLogoState)
		{
		case LogoState.LogoIn:
			if (!mMaskUI.gameObject.activeSelf)
			{
				mMaskUI.gameObject.SetActive(value: true);
			}
			mBlackTexture.alpha = 1f;
			mLogoTexture.alpha = tempTime / 2f;
			tempTime += Time.deltaTime;
			if (tempTime > 2f)
			{
				if (this.e_LogoAllIn != null)
				{
					this.e_LogoAllIn();
					this.e_LogoAllIn = null;
				}
				mLogoState = LogoState.Logo;
				tempTime = Time.realtimeSinceStartup;
			}
			break;
		case LogoState.Logo:
			mBlackTexture.alpha = 1f;
			mLogoTexture.alpha = 1f;
			if (Time.realtimeSinceStartup - tempTime > 1f)
			{
				mLogoState = LogoState.LogoOut;
				tempTime = 0f;
			}
			break;
		case LogoState.LogoOut:
			mBlackTexture.alpha = 1f;
			mLogoTexture.alpha = 1f - tempTime / 2f;
			tempTime += Time.deltaTime;
			if (tempTime > 2f)
			{
				if (this.e_LogoAllOut != null)
				{
					this.e_LogoAllOut();
					this.e_LogoAllOut = null;
				}
				mMaskUI.gameObject.SetActive(value: false);
				tempTime = 0f;
				mLogoTexture.mainTexture = null;
				mLogoState = LogoState.Null;
			}
			break;
		}
	}
}
