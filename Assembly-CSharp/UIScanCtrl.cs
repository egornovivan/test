using System;
using System.Collections.Generic;
using UnityEngine;

public class UIScanCtrl : UIBaseWidget
{
	private const int m_ScanSoundID = 915;

	public UITexture mMatelScanTex;

	[SerializeField]
	private UISprite mMetalSpr;

	[SerializeField]
	private UILabel mMetalDes;

	[SerializeField]
	private UIGrid mMetalScanGrid;

	[SerializeField]
	private MetalScanItem_N mMetalScanItemPerfab;

	[SerializeField]
	private UILabel mScanTextLabel;

	[SerializeField]
	private Camera mMetalScanCam;

	[Header("金")]
	public Color AuCol;

	[Header("铜")]
	public Color CuCol;

	[Header("铁")]
	public Color FeCol;

	[Header("银")]
	public Color AgCol;

	[Header("铝")]
	public Color AlCol;

	[Header("石油")]
	public Color OilCol;

	[Header("煤")]
	public Color CoalCol;

	[Header("锌")]
	public Color ZnCol;

	[SerializeField]
	private bool m_UseDebugMode;

	private List<MetalScanItem_N> mMetalScanItemList = new List<MetalScanItem_N>();

	[SerializeField]
	private float ViewDisMax = 400f;

	[SerializeField]
	private float ViewDisMin = 10f;

	private float mCamViewDis = 250f;

	private float mCamDegX = -90f;

	private float mCamDegY = 45f;

	private AudioController mScanSoundEffect;

	public override void Show()
	{
		base.Show();
	}

	public override void OnCreate()
	{
		MetalScanData.e_OnAddMetal += OnAddMetal;
		base.OnCreate();
		GetCurColor();
		ResetMetal();
	}

	public override void OnDelete()
	{
		MetalScanData.e_OnAddMetal -= OnAddMetal;
		base.OnDelete();
	}

	protected override void OnHide()
	{
		base.OnHide();
		if (MSScan.Instance.bInScanning)
		{
			StopScanSoundEffect();
			MSScan.Instance.bInScanning = false;
		}
	}

	private void Start()
	{
		UITexture uITexture = mMatelScanTex;
		RenderTexture renderTexture = new RenderTexture(662, 360, 16);
		mMetalScanCam.targetTexture = renderTexture;
		uITexture.mainTexture = renderTexture;
	}

	private void OnDisable()
	{
		if (MSScan.Instance.bInScanning)
		{
			StopScanSoundEffect();
			MSScan.Instance.bInScanning = false;
		}
	}

	private void Update()
	{
		if (GameUI.Instance == null)
		{
			return;
		}
		if (UICamera.hoveredObject == mMatelScanTex.gameObject)
		{
			if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
			{
				mCamDegX += Input.GetAxis("Mouse X") * 15f * (float)(SystemSettingData.Instance.CameraHorizontalInverse ? 1 : (-1));
				if (mCamDegX < 0f)
				{
					mCamDegX += 360f;
				}
				else if (mCamDegX > 360f)
				{
					mCamDegX -= 360f;
				}
				mCamDegY = Mathf.Clamp(mCamDegY + Input.GetAxis("Mouse Y") * 5f * (float)(SystemSettingData.Instance.CameraVerticalInverse ? 1 : (-1)), -80f, 80f);
			}
			mCamViewDis = Mathf.Clamp(mCamViewDis - Input.GetAxis("Mouse ScrollWheel") * 30f, ViewDisMin, ViewDisMax);
		}
		if (null != GameUI.Instance.mMainPlayer)
		{
			float f = mCamDegX / 180f * (float)Math.PI;
			float f2 = mCamDegY / 180f * (float)Math.PI;
			mMetalScanCam.transform.position = GameUI.Instance.mMainPlayer.position + mCamViewDis * new Vector3(Mathf.Cos(f) * Mathf.Cos(f2), Mathf.Sin(f2), Mathf.Sin(f) * Mathf.Cos(f2));
			mMetalScanCam.transform.LookAt(GameUI.Instance.mMainPlayer.position, Vector3.up);
		}
		if (mScanTextLabel.gameObject.activeSelf != MSScan.Instance.bInScanning)
		{
			mScanTextLabel.gameObject.SetActive(MSScan.Instance.bInScanning);
		}
		if (MSScan.Instance.bInScanning)
		{
			PlayScanSoundEffect();
		}
		else
		{
			StopScanSoundEffect();
		}
	}

	protected override void InitWindow()
	{
		ResetMetal();
		base.InitWindow();
		base.SelfWndType = UIEnum.WndType.Scan;
	}

	private void OnAddMetal()
	{
		ResetMetal();
	}

	private void ResetMetal()
	{
		if (!(GameUI.Instance != null) || !(GameUI.Instance.mMainPlayer != null))
		{
			return;
		}
		if (mMetalScanItemList.Count < MetalScanData.m_ActiveIDList.Count)
		{
			for (int num = MetalScanData.m_ActiveIDList.Count - mMetalScanItemList.Count; num >= 0; num--)
			{
				MetalScanItem_N metalScanItem_N = UnityEngine.Object.Instantiate(mMetalScanItemPerfab);
				metalScanItem_N.transform.parent = mMetalScanGrid.transform;
				metalScanItem_N.transform.localPosition = Vector3.back;
				metalScanItem_N.transform.localScale = Vector3.one;
				metalScanItem_N.e_OnClick += OnMetalSelected;
				metalScanItem_N.mCheckBox.isChecked = true;
				mMetalScanItemList.Add(metalScanItem_N);
			}
		}
		for (int i = 0; i < mMetalScanItemList.Count; i++)
		{
			if (i < MetalScanData.m_ActiveIDList.Count)
			{
				MetalScanItem itemByID = MetalScanData.GetItemByID(MetalScanData.m_ActiveIDList[i]);
				if (itemByID != null)
				{
					mMetalScanItemList[i].gameObject.SetActive(value: true);
					mMetalScanItemList[i].SetItem(itemByID.mMatName, itemByID.mColor, itemByID.mType, itemByID.mDesID);
					mMetalScanItemList[i].mCheckBox.isChecked = MetalScanData.m_ScanState[i];
				}
			}
			else
			{
				mMetalScanItemList[i].gameObject.SetActive(value: false);
			}
		}
		mMetalScanGrid.repositionNow = true;
	}

	private void OnMetalSelected(object sender)
	{
		MetalScanItem_N metalScanItem_N = sender as MetalScanItem_N;
		if (metalScanItem_N == null)
		{
			return;
		}
		byte mType = metalScanItem_N.mType;
		MetalScanItem itemByVoxelType = MetalScanData.GetItemByVoxelType(mType);
		if (itemByVoxelType != null)
		{
			mMetalSpr.spriteName = itemByVoxelType.mTexName;
			mMetalSpr.MakePixelPerfect();
			mMetalDes.text = PELocalization.GetString(itemByVoxelType.mDesID);
		}
		for (int i = 0; i < mMetalScanItemList.Count; i++)
		{
			if (mMetalScanItemList[i] == metalScanItem_N)
			{
				MetalScanData.m_ScanState[i] = metalScanItem_N.mCheckBox.isChecked;
				break;
			}
		}
	}

	private void BtnOnScan()
	{
		DebugMetalColor();
		if (!(null != GameUI.Instance.mMainPlayer))
		{
			return;
		}
		List<byte> list = new List<byte>();
		for (int i = 0; i < mMetalScanItemList.Count; i++)
		{
			if (mMetalScanItemList[i].mCheckBox.isChecked && mMetalScanItemList[i].gameObject.activeSelf && mMetalScanItemList[i].mType != 0)
			{
				list.Add(mMetalScanItemList[i].mType);
			}
		}
		if (list.Count > 0)
		{
			MSScan.Instance.MakeAScan(GameUI.Instance.mMainPlayer.position, list);
			StopScanSoundEffect();
			PlayScanSoundEffect();
		}
	}

	private void PlayScanSoundEffect()
	{
		if (null == mScanSoundEffect)
		{
			mScanSoundEffect = AudioManager.instance.Create(Vector3.zero, 915, null, isPlay: false, isDelete: false);
		}
		if (null != mScanSoundEffect && !mScanSoundEffect.isPlaying)
		{
			mScanSoundEffect.PlayAudio();
		}
	}

	private void StopScanSoundEffect()
	{
		if (null != mScanSoundEffect && mScanSoundEffect.isPlaying)
		{
			mScanSoundEffect.StopAudio();
		}
	}

	private void PauseScanSoundEffect()
	{
		if (null != mScanSoundEffect && mScanSoundEffect.isPlaying)
		{
			mScanSoundEffect.PauseAudio();
		}
	}

	private void BtnOnSeclectAll()
	{
		foreach (MetalScanItem_N mMetalScanItem in mMetalScanItemList)
		{
			mMetalScanItem.mCheckBox.isChecked = true;
		}
		for (int i = 0; i < MetalScanData.m_ScanState.Count; i++)
		{
			MetalScanData.m_ScanState[i] = true;
		}
	}

	private void BtnOnDeseclectAll()
	{
		foreach (MetalScanItem_N mMetalScanItem in mMetalScanItemList)
		{
			mMetalScanItem.mCheckBox.isChecked = false;
		}
		for (int i = 0; i < MetalScanData.m_ScanState.Count; i++)
		{
			MetalScanData.m_ScanState[i] = false;
		}
	}

	private void DebugMetalColor()
	{
		if (Application.isEditor && m_UseDebugMode)
		{
			MetalScanData.mMetalDic[1].mColor = AuCol;
			MetalScanData.mMetalDic[2].mColor = CuCol;
			MetalScanData.mMetalDic[3].mColor = FeCol;
			MetalScanData.mMetalDic[4].mColor = AgCol;
			MetalScanData.mMetalDic[5].mColor = AlCol;
			MetalScanData.mMetalDic[6].mColor = OilCol;
			MetalScanData.mMetalDic[7].mColor = CoalCol;
			MetalScanData.mMetalDic[8].mColor = ZnCol;
		}
	}

	private void GetCurColor()
	{
		if (Application.isEditor && m_UseDebugMode)
		{
			AuCol = MetalScanData.mMetalDic[1].mColor;
			CuCol = MetalScanData.mMetalDic[2].mColor;
			FeCol = MetalScanData.mMetalDic[3].mColor;
			AgCol = MetalScanData.mMetalDic[4].mColor;
			AlCol = MetalScanData.mMetalDic[5].mColor;
			OilCol = MetalScanData.mMetalDic[6].mColor;
			CoalCol = MetalScanData.mMetalDic[7].mColor;
			ZnCol = MetalScanData.mMetalDic[8].mColor;
		}
	}
}
