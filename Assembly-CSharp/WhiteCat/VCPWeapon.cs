using System.Collections.Generic;
using SkillSystem;
using UnityEngine;

namespace WhiteCat;

public class VCPWeapon : VCPart, IProjectileData
{
	private class DiscreteSounds
	{
		public AudioClip sound;

		public float volume;

		public float lastTime;

		public DiscreteSounds(AudioClip sound, float volume)
		{
			this.sound = sound;
			this.volume = volume;
			lastTime = 0f;
		}

		public void Play(AudioSource source)
		{
			if (Time.timeSinceLevelLoad - lastTime > PEVCConfig.instance.minWeaponSoundInterval)
			{
				lastTime = Time.timeSinceLevelLoad;
				source.volume = SystemSettingData.Instance.EffectVolume;
				source.PlayOneShot(sound, volume);
			}
		}
	}

	[SerializeField]
	private WeaponType _weaponType;

	[SerializeField]
	private int _skillId;

	[SerializeField]
	private float _attackPerBullet;

	[SerializeField]
	private int _bulletCapacity;

	[SerializeField]
	private int _bulletProtoID;

	[SerializeField]
	private int _groupBulletsCount = 4;

	[SerializeField]
	private float _groupIntervalTime = 1f;

	[SerializeField]
	private float _intervalTime = 0.2f;

	[SerializeField]
	private float _energyPerShot = 10f;

	[SerializeField]
	private Transform[] _muzzles;

	[SerializeField]
	private int _groupIndex;

	[Header("Rotation Params")]
	[SerializeField]
	private bool _rotatable;

	[Range(0f, 90f)]
	[SerializeField]
	private float _maxUpPitchAngle = 60f;

	[Range(0f, 45f)]
	[SerializeField]
	private float _maxDownPitchAngle = 15f;

	[SerializeField]
	private bool _clampHorizontalAngle;

	[Range(0f, 75f)]
	[SerializeField]
	private float _maxHorizontalAngle = 30f;

	[Range(0f, 360f)]
	[SerializeField]
	private float _rotateSpeed = 120f;

	[SerializeField]
	private Transform _horizontalRotate;

	[SerializeField]
	private Transform _verticalRotate;

	[SerializeField]
	[Header("Effects")]
	private AudioClip _soundClip;

	[Range(0f, 1f)]
	[SerializeField]
	private float _soundVolume = 1f;

	[SerializeField]
	private ParticlePlayer _particlePlayer;

	[Range(0f, 1f)]
	[SerializeField]
	private float _cameraShakeRange;

	private int _currentMuzzleIndex;

	private float _remainTime;

	private int _bulletsCount;

	private float _targetAngleY;

	private float _targetAngleX;

	private Vector3 _angles;

	private Vector3 _muzzleLocalCenter;

	private BehaviourController _controller;

	private SkCarrierCanonPara _weaponParam;

	private Transform[] _realMuzzles;

	private static List<DiscreteSounds> sounds = new List<DiscreteSounds>(4);

	private int soundIndex = -1;

	public Transform emissionTransform
	{
		get
		{
			Transform transform = _realMuzzles[_currentMuzzleIndex];
			Transform transform2 = _muzzles[_currentMuzzleIndex];
			transform.forward = transform2.TransformPoint(0f, 0f, 1f) - transform2.position;
			return transform;
		}
	}

	public Vector3 targetPosition => _controller.attackTargetPoint;

	public int groupIndex
	{
		set
		{
			_groupIndex = value;
		}
	}

	public float attackPerSecond
	{
		get
		{
			float num = (float)(_groupBulletsCount - 1) * _intervalTime + _groupIntervalTime;
			return _attackPerBullet * (float)_groupBulletsCount / num;
		}
	}

	public int bulletCapacity => _bulletCapacity;

	public int bulletProtoID => _bulletProtoID;

	public void Init(int weaponIndex)
	{
		_controller = GetComponentInParent<BehaviourController>();
		_weaponParam = new SkCarrierCanonPara(weaponIndex);
		base.enabled = true;
		_muzzleLocalCenter = Vector3.zero;
		for (int i = 0; i < _muzzles.Length; i++)
		{
			_muzzleLocalCenter += _muzzles[i].localPosition;
		}
		_muzzleLocalCenter /= (float)_muzzles.Length;
		_realMuzzles = new Transform[_muzzles.Length];
		for (int j = 0; j < _muzzles.Length; j++)
		{
			_realMuzzles[j] = new GameObject("RealMuzzle").transform;
			_realMuzzles[j].SetParent(_muzzles[j], worldPositionStays: false);
		}
	}

	private void PlaySound()
	{
		if (soundIndex < 0)
		{
			soundIndex = sounds.FindIndex((DiscreteSounds sound) => sound.sound == _soundClip);
			if (soundIndex < 0)
			{
				sounds.Add(new DiscreteSounds(_soundClip, _soundVolume));
				soundIndex = sounds.Count - 1;
			}
		}
		sounds[soundIndex].Play(_controller.audioSource);
	}

	public void PlayEffects()
	{
		_particlePlayer.enabled = true;
		PlaySound();
	}

	private void UpdateRotation()
	{
		if (_controller.isAttackMode && (_weaponType == WeaponType.AI || _controller.IsWeaponGroupEnabled(_groupIndex)))
		{
			_angles = _horizontalRotate.parent.InverseTransformVector(_controller.attackTargetPoint - _horizontalRotate.position);
			_targetAngleY = Mathf.Atan2(_angles.x, _angles.z) * 57.29578f;
			if (_clampHorizontalAngle)
			{
				_targetAngleY = Mathf.Clamp(_targetAngleY, 0f - _maxHorizontalAngle, _maxHorizontalAngle);
			}
			_angles = _verticalRotate.parent.InverseTransformVector(_controller.attackTargetPoint - _muzzles[0].parent.TransformPoint(_muzzleLocalCenter));
			_targetAngleX = Mathf.Clamp(Mathf.Asin((0f - _angles.y) / _angles.magnitude) * 57.29578f, 0f - _maxUpPitchAngle, _maxDownPitchAngle);
		}
		else
		{
			_targetAngleY = 0f;
			_targetAngleX = 0f;
		}
		_targetAngleY = Mathf.MoveTowardsAngle(_horizontalRotate.localEulerAngles.y, _targetAngleY, _rotateSpeed * Time.deltaTime);
		_targetAngleX = Mathf.MoveTowardsAngle(_verticalRotate.localEulerAngles.x, _targetAngleX, _rotateSpeed * Time.deltaTime);
		if (_horizontalRotate == _verticalRotate)
		{
			_angles.Set(_targetAngleX, _targetAngleY, 0f);
			_horizontalRotate.localEulerAngles = _angles;
			return;
		}
		_angles.Set(0f, _targetAngleY, 0f);
		_horizontalRotate.localEulerAngles = _angles;
		_angles.Set(_targetAngleX, 0f, 0f);
		_verticalRotate.localEulerAngles = _angles;
	}

	private void FixedUpdate()
	{
		if (_rotatable)
		{
			UpdateRotation();
		}
		_remainTime -= Time.deltaTime;
		if (_controller.isPlayerHost && _controller.isAttackMode && _remainTime <= 0f && (_weaponType == WeaponType.AI || _controller.IsWeaponGroupEnabled(_groupIndex)) && _controller.IsWeaponControlEnabled(_weaponType) && _controller.isEnergyEnough(_energyPerShot))
		{
			_controller.StartSkill(_skillId, _weaponParam);
			_controller.ExpendEnergy(_energyPerShot);
			_currentMuzzleIndex = (_currentMuzzleIndex + 1) % _muzzles.Length;
			_bulletsCount++;
			if (_bulletsCount == _groupBulletsCount)
			{
				_bulletsCount = 0;
				_remainTime = _groupIntervalTime;
			}
			else
			{
				_remainTime = _intervalTime;
			}
			if (_cameraShakeRange > 0f)
			{
				PeCamera.PlayAttackShake();
			}
		}
		else if (_remainTime <= 0f - _groupIntervalTime)
		{
			_bulletsCount = 0;
			_remainTime = 0f;
		}
	}
}
