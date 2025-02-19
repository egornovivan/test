using System.Collections.Generic;
using UnityEngine;

public class UIBtnSoundMgr : MonoBehaviour
{
	[HideInInspector]
	public List<UIButtonSound> mBtnSndList;

	private void Awake()
	{
		mBtnSndList = new List<UIButtonSound>();
	}

	private void Start()
	{
		mBtnSndList.AddRange(base.gameObject.GetComponentsInChildren<UIButtonSound>(includeInactive: true));
	}

	private void Update()
	{
		UpdateBtnSound();
	}

	private void UpdateBtnSound()
	{
		if (SystemSettingData.Instance == null)
		{
			return;
		}
		foreach (UIButtonSound mBtnSnd in mBtnSndList)
		{
			mBtnSnd.volume = SystemSettingData.Instance.SoundVolume * SystemSettingData.Instance.EffectVolume;
		}
	}
}
