using Pathea;
using SkillSystem;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class PeCameraImageEffect : MonoBehaviour
{
	private static PeCameraImageEffect inst;

	[Header("Resource References")]
	public Material DamageMat;

	public Material FoodPoisonMat;

	public Material InjuredPoisonMat;

	public Material GRVInfestMat;

	public Material DizzyMat;

	public Material ScreenDirtMat;

	public Material ScreenMaskMat;

	public Grayscale grayScale;

	public MotionBlur motionBlur;

	public BloomOptimized bloom;

	[Header("Damage")]
	public float Scale;

	public float hp = 1f;

	private float damageIntensity;

	private float t;

	private float v;

	private float damageIntensityTarget;

	public bool testHit;

	private bool registed;

	private SkAliveEntity mainPlayerAliveEntity;

	[Header("Poison")]
	[Range(0f, 1f)]
	[SerializeField]
	private float foodPoisonStrength;

	private float foodPoisonStrengthCurrent;

	[SerializeField]
	[Range(0f, 1f)]
	private float injuredPoisonStrength;

	private float injuredPoisonStrengthCurrent;

	[SerializeField]
	private float poisonMaxIntensityAtDaytime = 0.7f;

	[SerializeField]
	private float poisonMaxIntensityAtNight = 0.35f;

	[Range(0f, 1f)]
	[Header("Dizzy")]
	[SerializeField]
	private float dizzyStrength;

	[SerializeField]
	private float dizzyMaxDistortion = 0.01f;

	private float dizzyStrengthCurrent;

	[Range(0f, 1f)]
	[SerializeField]
	[Header("GRV")]
	private float grvInfestStrength;

	[SerializeField]
	private float grvInfestEffectDuration = 5f;

	[SerializeField]
	private AnimationCurve grvInfestEffectIntensity;

	private float grvInfestEffectTime;

	private float grvInfestEffectTimeSpeed;

	[SerializeField]
	private float grvMaxDistortion = 0.007f;

	[SerializeField]
	private float GRVEffectCDDuration = 10f;

	private float GRVCDTime;

	[Header("Flashlight")]
	[SerializeField]
	private bool triggerFlashlight;

	[SerializeField]
	private float FlashStrength = 1f;

	private float flashlightTime;

	private float flashlightTimeSpeed;

	private float flashBloomStrength;

	private float flashBlurStrength;

	[SerializeField]
	private float flashlightDuration = 10f;

	[SerializeField]
	private AnimationCurve bloomStrengthAnimation;

	[SerializeField]
	private AnimationCurve blurStrengthAnimation;

	[SerializeField]
	[Header("Screen Mask")]
	private Texture2D[] maskTextures = new Texture2D[0];

	[SerializeField]
	private AnimationCurve screenMaskAnimation;

	[SerializeField]
	private float screenMaskDuration = 10f;

	private bool maskenabled;

	private float screenMaskTime;

	public bool TestMask;

	public static void PlayHitEffect(float hpDrop)
	{
		if (hpDrop > 1f)
		{
			inst.Hit(hpDrop);
			PeCamera.PlayShakeEffect(0, 0.2f, 0f);
		}
	}

	public static void SetFoodPoisonStrength(float s)
	{
		inst.foodPoisonStrength = Mathf.Clamp01(s);
	}

	public static void SetInjuredPoisonStrength(float s)
	{
		inst.injuredPoisonStrength = Mathf.Clamp01(s);
	}

	public static void SetDizzyStrength(float s)
	{
		inst.dizzyStrength = Mathf.Clamp01(s);
	}

	public static void SetGRVInfestStrength(float s)
	{
		inst.grvInfestStrength = Mathf.Clamp01(s);
	}

	public static void FlashlightExplode(float flashStrength = 1f)
	{
		inst.FlashStrength = flashStrength;
		inst.flashlightTime = inst.flashlightDuration;
		inst.flashlightTimeSpeed = -1f;
	}

	public static void SprayDirtToScreen(int dirtIndex)
	{
	}

	public static void ScreenMask(int maskIndex, bool show = true, float duration = 10f)
	{
		if (inst.maskTextures.Length > maskIndex)
		{
			inst.ScreenMaskMat.mainTexture = inst.maskTextures[maskIndex];
			inst.screenMaskTime = 0f;
			inst.maskenabled = show;
			inst.screenMaskDuration = duration;
		}
	}

	private void Awake()
	{
		inst = this;
	}

	private void OnDestroy()
	{
		inst = null;
	}

	private void Start()
	{
		DamageMat = Object.Instantiate(DamageMat);
		FoodPoisonMat = Object.Instantiate(FoodPoisonMat);
		InjuredPoisonMat = Object.Instantiate(InjuredPoisonMat);
		GRVInfestMat = Object.Instantiate(GRVInfestMat);
		DizzyMat = Object.Instantiate(DizzyMat);
		ScreenDirtMat = Object.Instantiate(ScreenDirtMat);
		ScreenMaskMat = Object.Instantiate(ScreenMaskMat);
	}

	private void TryRegister()
	{
		if (mainPlayerAliveEntity != null)
		{
			hp = mainPlayerAliveEntity.HPPercent;
			grayScale.saturate = Mathf.Lerp(grayScale.saturate, (!(hp < 1E-06f)) ? 1f : 0f, 0.02f);
		}
		if (!registed && PeSingleton<PeCreature>.Instance != null && !(PeSingleton<PeCreature>.Instance.mainPlayer == null))
		{
			mainPlayerAliveEntity = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<SkAliveEntity>();
			if (!(mainPlayerAliveEntity == null))
			{
				mainPlayerAliveEntity.onHpChange += HandleAliveEntityHpChange;
				registed = true;
			}
		}
	}

	private void HandleAliveEntityHpChange(SkEntity caster, float hpChange)
	{
		if (hpChange < 0f)
		{
			Hit(0f - hpChange);
			PeCamera.PlayShakeEffect(0, 0.2f, 0f);
		}
	}

	private void Update()
	{
		float num = Mathf.Sin(t * 10f) * 0.2f + 0.8f;
		float num2 = (float)GameTime.Timer.CycleInDay * 0.5f + 0.5f;
		if (testHit)
		{
			testHit = false;
			Hit(50f);
		}
		TryRegister();
		damageIntensity = Mathf.SmoothDamp(damageIntensity, damageIntensityTarget, ref v, 0.16f);
		if (damageIntensity > damageIntensityTarget * 0.75f)
		{
			damageIntensityTarget = Mathf.Lerp(damageIntensityTarget, Mathf.Pow(Mathf.Clamp01(0.4f - hp), 2f) * 2.5f, 0.15f);
		}
		t += Time.deltaTime;
		float value = Scale * damageIntensity * num * (num2 * 0.5f + 0.5f);
		DamageMat.SetFloat("_Intensity", value);
		if (foodPoisonStrength > injuredPoisonStrength - 0.001f)
		{
			foodPoisonStrengthCurrent = Mathf.Lerp(foodPoisonStrengthCurrent, foodPoisonStrength, 0.1f);
			injuredPoisonStrengthCurrent = Mathf.Lerp(injuredPoisonStrengthCurrent, 0f, 0.1f);
		}
		else
		{
			foodPoisonStrengthCurrent = Mathf.Lerp(foodPoisonStrengthCurrent, 0f, 0.1f);
			injuredPoisonStrengthCurrent = Mathf.Lerp(injuredPoisonStrengthCurrent, injuredPoisonStrength, 0.1f);
		}
		float num3 = Mathf.Lerp(poisonMaxIntensityAtNight, poisonMaxIntensityAtDaytime, num2);
		FoodPoisonMat.SetFloat("_Intensity", foodPoisonStrengthCurrent * num * num3);
		InjuredPoisonMat.SetFloat("_Intensity", injuredPoisonStrengthCurrent * num * num3);
		dizzyStrengthCurrent = Mathf.Lerp(dizzyStrengthCurrent, dizzyStrength, 0.02f);
		DizzyMat.SetFloat("_DistortionStrength", dizzyStrengthCurrent * num * dizzyMaxDistortion);
		DizzyMat.SetFloat("_Speed", dizzyStrengthCurrent * 0.03f);
		float num4 = dizzyStrengthCurrent * 0.8f;
		float num5 = grvInfestStrength * 0.005f - 1E-05f;
		if (Random.value < num5 && GRVCDTime > GRVEffectCDDuration)
		{
			grvInfestEffectTime = 0f;
			grvInfestEffectTimeSpeed = 1f;
		}
		float num6 = Mathf.Clamp01(grvInfestEffectIntensity.Evaluate(grvInfestEffectTime / grvInfestEffectDuration));
		num6 *= grvInfestStrength;
		float num7 = num6 * 0.75f;
		num6 *= grvMaxDistortion;
		num6 *= num;
		grvInfestEffectTime += Time.deltaTime * grvInfestEffectTimeSpeed;
		GRVCDTime += Time.deltaTime;
		if (grvInfestEffectTime > grvInfestEffectDuration)
		{
			grvInfestEffectTime = 0f;
			grvInfestEffectTimeSpeed = 0f;
			GRVCDTime = 0f;
		}
		GRVInfestMat.SetFloat("_DistortionStrength", num6);
		if (triggerFlashlight)
		{
			FlashlightExplode(1f);
			triggerFlashlight = false;
		}
		float num8 = bloomStrengthAnimation.Evaluate(flashlightTime / flashlightDuration);
		float b = blurStrengthAnimation.Evaluate(flashlightTime / flashlightDuration);
		flashBloomStrength = Mathf.Lerp(flashBloomStrength, num8, 0.4f);
		flashBlurStrength = Mathf.Lerp(flashBlurStrength, b, 0.4f);
		flashlightTime += flashlightTimeSpeed * Time.deltaTime;
		if (flashlightTime < 0f)
		{
			flashlightTime = 0f;
			flashlightTimeSpeed = 0f;
		}
		float num9 = num8 * FlashStrength;
		float num10 = Mathf.Max(num4, num7, flashBlurStrength) * FlashStrength;
		bloom.enabled = num9 > 0.01f;
		motionBlur.enabled = num10 > 0.05f;
		bloom.intensity = num9;
		motionBlur.blurAmount = num10;
		if (maskenabled && screenMaskDuration > float.Epsilon)
		{
			screenMaskTime += Time.deltaTime;
			if (screenMaskTime > screenMaskDuration)
			{
				maskenabled = false;
				screenMaskTime = screenMaskDuration;
			}
			ScreenMaskMat.SetFloat("_Intensity", screenMaskAnimation.Evaluate(screenMaskTime / screenMaskDuration));
		}
		if (TestMask)
		{
			TestMask = false;
			ScreenMask(0);
		}
	}

	public void Hit(float amount)
	{
		amount /= 150f;
		amount = Mathf.Clamp01(amount);
		damageIntensityTarget = damageIntensity + amount;
		if (damageIntensity < 0.01f)
		{
			t = 0f;
		}
	}

	private void OnPostRender()
	{
		if (damageIntensity > 0.01f)
		{
			DrawGLQuad(DamageMat);
		}
		if (foodPoisonStrengthCurrent > 0.005f)
		{
			DrawGLQuad(FoodPoisonMat);
		}
		if (injuredPoisonStrengthCurrent > 0.005f)
		{
			DrawGLQuad(InjuredPoisonMat);
		}
		if (dizzyStrengthCurrent > 0.005f)
		{
			DrawGLQuad(DizzyMat);
		}
		if (grvInfestEffectTime > 0.005f)
		{
			DrawGLQuad(GRVInfestMat);
		}
		if (maskenabled)
		{
			DrawGLQuad(ScreenMaskMat);
		}
	}

	private void DrawGLQuad(Material mat)
	{
		for (int i = 0; i < mat.passCount; i++)
		{
			mat.SetPass(i);
			GL.PushMatrix();
			GL.LoadOrtho();
			GL.Color(Color.white);
			GL.Begin(7);
			GL.TexCoord2(0f, 0f);
			GL.Vertex3(0f, 0f, 0f);
			GL.TexCoord2(0f, 1f);
			GL.Vertex3(-0f, 1f, 0f);
			GL.TexCoord2(1f, 1f);
			GL.Vertex3(1f, 1f, 0f);
			GL.TexCoord2(1f, 0f);
			GL.Vertex3(1f, 0f, 0f);
			GL.End();
			GL.PopMatrix();
		}
	}
}
