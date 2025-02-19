using System;
using UnityEngine;
using WhiteCat;

public class UIDrivingCtrl : MonoBehaviour
{
	private const string SAVESHOWTIPKEY = "UIDriveTips";

	[SerializeField]
	private UILabel durabilityLabel;

	[SerializeField]
	private UISlider durabilitySlider;

	[SerializeField]
	private UILabel energyLabel;

	[SerializeField]
	private UISlider energySlider;

	[SerializeField]
	private UILabel speedLabel;

	[SerializeField]
	private UISlider speedSlider;

	[SerializeField]
	private UILabel jetExhaustLabel;

	[SerializeField]
	private UISlider jetExhaustSlider;

	[SerializeField]
	private GameObject[] weaponTogglesOn;

	[SerializeField]
	private GameObject[] weaponTogglesOff;

	[SerializeField]
	private TweenInterpolator interpolator;

	[SerializeField]
	private TweenInterpolator attackModeAnim;

	[SerializeField]
	private float updateInterval = 0.1f;

	[SerializeField]
	private TweenInterpolator driveHelpTween;

	[SerializeField]
	private Transform driveHelpContentTrans;

	[SerializeField]
	private UILabel driveHelpLbl;

	[SerializeField]
	private UIButton driveHelpBtn;

	private static UIDrivingCtrl mInstance;

	private Func<float> maxDurability;

	private Func<float> durability;

	private Func<float> maxEnergy;

	private Func<float> energy;

	private Func<float> speed;

	private Func<float> jetExhaust;

	private int lastDurability;

	private int lastDurabilityPermillage;

	private int lastEnergy;

	private int lastEnergyPermillage;

	private int lastSpeedx10;

	private int lastJetExhaustPermillage;

	private float nextUpdateTime = -1f;

	private bool _helpIsShow = true;

	public static UIDrivingCtrl Instance => mInstance;

	public bool IsShow => base.gameObject.activeInHierarchy;

	private bool driveHelpIsShow
	{
		get
		{
			return _helpIsShow;
		}
		set
		{
			if (_helpIsShow != value)
			{
				_helpIsShow = value;
				if (null != UIRecentDataMgr.Instance)
				{
					UIRecentDataMgr.Instance.SetIntValue("UIDriveTips", _helpIsShow ? 1 : 0);
				}
			}
			driveHelpBtn.transform.localRotation = ((!_helpIsShow) ? Quaternion.Euler(new Vector3(0f, 0f, 180f)) : Quaternion.Euler(Vector3.one));
			driveHelpContentTrans.gameObject.SetActive(_helpIsShow);
		}
	}

	private void Awake()
	{
		mInstance = this;
		UIEventListener uIEventListener = UIEventListener.Get(driveHelpBtn.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnTipsBtnClick));
		InitDriveHelpState();
	}

	private void OnDestroy()
	{
		if (null != driveHelpBtn)
		{
			UIEventListener uIEventListener = UIEventListener.Get(driveHelpBtn.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Remove(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnTipsBtnClick));
		}
	}

	public void Show(Func<float> maxDurability, Func<float> durability, Func<float> maxEnergy, Func<float> energy, Func<float> speed, Func<float> jetExhaust)
	{
		this.maxDurability = maxDurability;
		this.durability = durability;
		this.maxEnergy = maxEnergy;
		this.energy = energy;
		this.speed = speed;
		this.jetExhaust = jetExhaust;
		Update();
		base.gameObject.SetActive(value: true);
		interpolator.speed = 1f;
		interpolator.isPlaying = true;
		UpdateDriveHelpContent();
		driveHelpTween.speed = 1f;
		driveHelpTween.isPlaying = true;
	}

	public void Hide()
	{
		nextUpdateTime = -1f;
		interpolator.speed = -1f;
		interpolator.isPlaying = true;
		driveHelpTween.speed = -1f;
		driveHelpTween.isPlaying = true;
	}

	public void SetWweaponGroupTogglesVisible(bool visible, BehaviourController controller)
	{
		attackModeAnim.speed = ((!visible) ? (-1f) : 1f);
		for (int i = 0; i < weaponTogglesOn.Length; i++)
		{
			SetWweaponGroupToggles(i, controller.IsWeaponGroupEnabled(i));
		}
	}

	public void SetWweaponGroupToggles(int index, bool on)
	{
		weaponTogglesOn[index].SetActive(on);
		weaponTogglesOff[index].SetActive(!on);
	}

	private void Update()
	{
		try
		{
			if (Time.timeSinceLevelLoad >= nextUpdateTime)
			{
				nextUpdateTime = Time.timeSinceLevelLoad + updateInterval;
				float num = durability();
				int num2 = Mathf.CeilToInt(num);
				if (num2 != lastDurability)
				{
					lastDurability = num2;
					durabilityLabel.text = num2.ToString();
				}
				num /= maxDurability();
				num2 = Mathf.CeilToInt(num * 1000f);
				if (num2 != lastDurabilityPermillage)
				{
					lastDurabilityPermillage = num2;
					durabilitySlider.sliderValue = (float)num2 * 0.001f;
				}
				num = energy();
				num2 = Mathf.CeilToInt(num);
				if (num2 != lastEnergy)
				{
					lastEnergy = num2;
					energyLabel.text = num2.ToString();
				}
				num /= maxEnergy();
				num2 = Mathf.CeilToInt(num * 1000f);
				if (num2 != lastEnergyPermillage)
				{
					lastEnergyPermillage = num2;
					energySlider.sliderValue = (float)num2 * 0.001f;
				}
				num = speed();
				num2 = Mathf.RoundToInt(num * 10f);
				if (num2 != lastSpeedx10)
				{
					lastSpeedx10 = num2;
					speedLabel.text = ((float)num2 * 0.1f).ToString("0.0");
					speedSlider.sliderValue = (float)num2 / PEVCConfig.instance.maxRigidbodySpeed / 36f;
				}
				num = ((jetExhaust != null) ? jetExhaust() : 0f);
				num2 = Mathf.CeilToInt(num * 1000f);
				if (num2 != lastJetExhaustPermillage)
				{
					lastJetExhaustPermillage = num2;
					jetExhaustSlider.sliderValue = (float)num2 * 0.001f;
					jetExhaustLabel.text = Mathf.CeilToInt(num * 100f).ToString();
				}
			}
		}
		catch
		{
		}
	}

	private void InitDriveHelpState()
	{
		if (null != UIRecentDataMgr.Instance)
		{
			driveHelpIsShow = UIRecentDataMgr.Instance.GetIntValue("UIDriveTips", _helpIsShow ? 1 : 0) > 0;
		}
	}

	private void OnTipsBtnClick(GameObject go)
	{
		driveHelpIsShow = !driveHelpIsShow;
	}

	private void UpdateDriveHelpContent()
	{
		string @string = PELocalization.GetString(82201084);
		if (!string.IsNullOrEmpty(@string))
		{
			@string = @string.Replace("$W$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.MoveForward).ToString());
			@string = @string.Replace("$S$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.MoveBackward).ToString());
			@string = @string.Replace("$A$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.MoveLeft).ToString());
			@string = @string.Replace("$D$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.MoveRight).ToString());
			@string = @string.Replace("$E$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.InteractWithItem).ToString());
			@string = @string.Replace("$L$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.SwitchLight).ToString());
			@string = @string.Replace("$F$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.Vehicle_AttackModeOnOff).ToString());
			@string = @string.Replace("$Space$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.Vehicle_LiftUp).ToString());
			@string = @string.Replace("$Alt$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.Vehicle_LiftDown).ToString());
			@string = @string.Replace("$F1$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.VehicleWeaponGrp1).ToString());
			@string = @string.Replace("$F2$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.VehicleWeaponGrp2).ToString());
			@string = @string.Replace("$F3$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.VehicleWeaponGrp3).ToString());
			@string = @string.Replace("$F4$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.VehicleWeaponGrp4).ToString());
			@string = @string.Replace("$Shift$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.Vehicle_Sprint).ToString());
			@string = @string.Replace("$Z$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.MissleTarget).ToString());
			@string = @string.Replace("$X$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.MissleLaunch).ToString());
			driveHelpLbl.text = @string;
			driveHelpLbl.MakePixelPerfect();
		}
	}
}
