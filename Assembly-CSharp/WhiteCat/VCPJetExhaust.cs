using UnityEngine;

namespace WhiteCat;

public class VCPJetExhaust : VCPart
{
	[SerializeField]
	[Min(0f)]
	private float _energyExpendSpeed;

	[Min(0f)]
	[SerializeField]
	private float _pushForce;

	[SerializeField]
	private GameObject _effect;

	[SerializeField]
	private AudioSource _sound;

	[SerializeField]
	private float _volume = 0.5f;

	private CarrierController _controller;

	protected override void Awake()
	{
		base.Awake();
		_effect.SetActive(value: false);
	}

	public void Init(CarrierController controller, float maxForce, int count)
	{
		_controller = controller;
		_pushForce = Mathf.Min(maxForce, _pushForce);
		_volume *= Mathf.Pow(0.75f, count - 1);
	}

	private void FixedUpdate()
	{
		float expend = _energyExpendSpeed * Time.deltaTime;
		if (_effect.activeSelf)
		{
			if (!_controller.isJetting || !_controller.isEnergyEnough(expend))
			{
				_effect.SetActive(value: false);
			}
			else
			{
				_controller.rigidbody.AddForce(base.transform.forward * _pushForce * _controller.speedScale);
				if (_controller.isPlayerHost)
				{
					_controller.ExpendEnergy(expend);
				}
			}
			_sound.volume = _volume * SystemSettingData.Instance.AbsEffectVolume;
		}
		else
		{
			if (_controller.isJetting && _controller.isEnergyEnough(expend))
			{
				_effect.SetActive(value: true);
				_sound.volume = _volume * SystemSettingData.Instance.AbsEffectVolume;
				_sound.time = 0f;
				_sound.Play();
			}
			_sound.volume -= _volume * Time.deltaTime * SystemSettingData.Instance.AbsEffectVolume;
		}
	}
}
