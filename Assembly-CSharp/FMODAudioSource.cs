using System;
using System.Collections.Generic;
using FMOD;
using FMOD.Studio;
using UnityEngine;

public class FMODAudioSource : MonoBehaviour
{
	[Serializable]
	public class Parameter
	{
		public string name;

		public float value;
	}

	[SerializeField]
	private FMODAsset _asset;

	[SerializeField]
	private string _path = string.Empty;

	public bool playOnReset = true;

	private float _volume = 1f;

	private float _pitch = 1f;

	private DSP _panner3d;

	public bool minmaxDistanceFromDesc = true;

	private float _minDistance = 1f;

	private float _maxDistance = 20f;

	private EventInstance evt;

	private Rigidbody cachedRigidBody;

	[SerializeField]
	private List<Parameter> initParams = new List<Parameter>();

	public static bool rteActive;

	private FMODAudioSourceRTE rte;

	public FMODAsset asset
	{
		get
		{
			return _asset;
		}
		set
		{
			if (_asset != value)
			{
				_asset = value;
				if (Application.isPlaying)
				{
					Reset();
				}
			}
		}
	}

	public string path
	{
		get
		{
			return _path;
		}
		set
		{
			if (_path != value)
			{
				_path = value;
				if (Application.isPlaying)
				{
					Reset();
				}
			}
		}
	}

	public float volume
	{
		get
		{
			return _volume;
		}
		set
		{
			_volume = Mathf.Clamp01(value);
			if (evt != null && evt.isValid())
			{
				evt.setVolume(_volume);
			}
		}
	}

	public float pitch
	{
		get
		{
			return _pitch;
		}
		set
		{
			_pitch = Mathf.Clamp(value, 0f, 4f);
			if (evt != null && evt.isValid())
			{
				evt.setPitch(_pitch);
			}
		}
	}

	private DSP panner3d
	{
		get
		{
			if (evt == null)
			{
				_panner3d = null;
				return null;
			}
			if (_panner3d != null)
			{
				return _panner3d;
			}
			ChannelGroup group = null;
			evt.getChannelGroup(out group);
			if (group != null && group.isValid())
			{
				int numdsps = 0;
				group.getNumDSPs(out numdsps);
				for (int i = 0; i < numdsps; i++)
				{
					group.getDSP(i, out var dsp);
					if (dsp != null && dsp.isValid())
					{
						dsp.getType(out var type);
						if (type == DSP_TYPE.PAN)
						{
							_panner3d = dsp;
							break;
						}
					}
				}
			}
			return _panner3d;
		}
	}

	public bool is3D
	{
		get
		{
			if (evt != null && evt.isValid())
			{
				EventDescription description = null;
				evt.getDescription(out description);
				if (description != null && description.isValid())
				{
					bool result = false;
					description.is3D(out result);
					return result;
				}
			}
			return false;
		}
	}

	public float minDistance
	{
		get
		{
			return _minDistance;
		}
		set
		{
			_minDistance = Mathf.Clamp(value, 0f, _maxDistance);
			if (panner3d != null)
			{
				panner3d.setParameterFloat(12, _minDistance);
			}
		}
	}

	public float maxDistance
	{
		get
		{
			return _maxDistance;
		}
		set
		{
			_maxDistance = Mathf.Clamp(value, _minDistance, 10000f);
			if (panner3d != null)
			{
				panner3d.setParameterFloat(13, _maxDistance);
			}
		}
	}

	public EventInstance audioInst => evt;

	public bool isStopped => playbackState == PLAYBACK_STATE.STOPPED;

	public PLAYBACK_STATE playbackState => GetPlaybackState();

	public static FMODAudioListener listener => FMODAudioListener.listener;

	public string xml
	{
		get
		{
			if (audioInst == null)
			{
				return string.Empty;
			}
			float num = 1f;
			audioInst.getPitch(out num);
			string text = "<AUDIO posx=\"" + base.transform.position.x.ToString("0.000") + "\" posy=\"" + base.transform.position.y.ToString("0.000") + "\" posz=\"" + base.transform.position.z.ToString("0.000") + "\" rotx=\"" + base.transform.eulerAngles.x.ToString("0.000") + "\" roty=\"" + base.transform.eulerAngles.y.ToString("0.000") + "\" rotz=\"" + base.transform.eulerAngles.z.ToString("0.000") + "\" path=\"" + path + "\" volume=\"" + volume.ToString("0.00") + "\" pitch=\"" + num.ToString("0.00") + "\" mindist=\"" + minDistance.ToString("0.000") + "\" maxdist=\"" + maxDistance.ToString("0.000") + "\"";
			if (audioInst != null && audioInst.isValid())
			{
				audioInst.getDescription(out var description);
				int count = 0;
				description.getParameterCount(out count);
				if (count > 0)
				{
					text += ">\r\n";
					for (int i = 0; i < count; i++)
					{
						ParameterInstance instance = null;
						description.getParameterByIndex(i, out var parameter);
						audioInst.getParameterByIndex(i, out instance);
						float value = 0f;
						instance.getValue(out value);
						string text2 = text;
						text = text2 + "\t<PARAM name=\"" + parameter.name + "\" value=\"" + value + "\" />\r\n";
					}
					return text + "</AUDIO>\r\n";
				}
			}
			return text + " />\r\n";
		}
	}

	private void FetchMinMaxDistance()
	{
		if (!(evt != null) || !evt.isValid())
		{
			return;
		}
		EventDescription description = null;
		evt.getDescription(out description);
		if (description != null && description.isValid())
		{
			bool flag = false;
			description.is3D(out flag);
			if (flag)
			{
				description.getMinimumDistance(out _minDistance);
				description.getMaximumDistance(out _maxDistance);
			}
			else
			{
				_minDistance = 0f;
				_maxDistance = 0f;
			}
		}
	}

	private void UpdateMinMaxDistance()
	{
		if (panner3d != null && panner3d.isValid())
		{
			panner3d.setParameterFloat(12, _minDistance);
			panner3d.setParameterFloat(13, _maxDistance);
		}
	}

	public Parameter FetchInitialParam(string _name)
	{
		if (initParams != null)
		{
			foreach (Parameter initParam in initParams)
			{
				if (initParam.name == _name)
				{
					return initParam;
				}
			}
		}
		return null;
	}

	public void ResetInitialParams(EventDescription desc)
	{
		if (desc == null || !desc.isValid())
		{
			return;
		}
		int count = 0;
		desc.getParameterCount(out count);
		for (int i = 0; i < count; i++)
		{
			PARAMETER_DESCRIPTION parameter = default(PARAMETER_DESCRIPTION);
			desc.getParameterByIndex(i, out parameter);
			Parameter parameter2 = FetchInitialParam(parameter.name);
			if (parameter2 == null)
			{
				parameter2 = new Parameter();
				parameter2.name = parameter.name;
				parameter2.value = parameter.minimum;
				initParams.Add(parameter2);
			}
		}
	}

	private void Init()
	{
		cachedRigidBody = GetComponent<Rigidbody>();
		if (evt == null || !evt.isValid())
		{
			CreateEventInstance();
			if (playOnReset)
			{
				Play();
			}
		}
	}

	public void Free()
	{
		if (evt != null && evt.isValid())
		{
			if (playbackState != PLAYBACK_STATE.STOPPED)
			{
				UnityUtil.Log("Release evt: " + path);
				ERRCHECK(evt.stop(STOP_MODE.IMMEDIATE));
			}
			ERRCHECK(evt.release());
			evt = null;
		}
	}

	public void Reset()
	{
		Free();
		Init();
	}

	public void Play()
	{
		if (evt != null && evt.isValid())
		{
			Update3DAttributes();
			ERRCHECK(evt.start());
		}
	}

	public void Stop()
	{
		if (evt != null && evt.isValid())
		{
			ERRCHECK(evt.stop(STOP_MODE.IMMEDIATE));
		}
	}

	private void CreateEventInstance()
	{
		if (asset != null)
		{
			evt = FMOD_StudioSystem.instance.GetEvent(asset.id);
		}
		else if (!string.IsNullOrEmpty(path))
		{
			evt = FMOD_StudioSystem.instance.GetEvent(path);
		}
		if (evt != null && evt.isValid())
		{
			if (minmaxDistanceFromDesc)
			{
				FetchMinMaxDistance();
			}
			foreach (Parameter initParam in initParams)
			{
				SetParam(initParam.name, initParam.value);
			}
			evt.setVolume(volume);
			evt.setPitch(pitch);
			UpdateMinMaxDistance();
		}
		else
		{
			evt = null;
		}
	}

	private PLAYBACK_STATE GetPlaybackState()
	{
		if (evt == null || !evt.isValid())
		{
			return PLAYBACK_STATE.STOPPED;
		}
		PLAYBACK_STATE state = PLAYBACK_STATE.STOPPED;
		if (ERRCHECK(evt.getPlaybackState(out state)) == RESULT.OK)
		{
			return state;
		}
		return PLAYBACK_STATE.STOPPED;
	}

	private void Update3DAttributes()
	{
		if (evt != null && evt.isValid())
		{
			FMOD.Studio.ATTRIBUTES_3D attributes = UnityUtil.to3DAttributes(base.gameObject, cachedRigidBody);
			ERRCHECK(evt.set3DAttributes(attributes));
		}
	}

	public float GetParam(string _name)
	{
		if (evt != null && evt.isValid())
		{
			ParameterInstance instance = null;
			if (evt.getParameter(_name, out instance) != 0)
			{
				return 0f;
			}
			float value = 0f;
			instance.getValue(out value);
			return value;
		}
		return FetchInitialParam(_name)?.value ?? 0f;
	}

	public float SetParam(string _name, float _value)
	{
		if (evt != null && evt.isValid())
		{
			ParameterInstance instance = null;
			if (evt.getParameter(_name, out instance) != 0)
			{
				return 0f;
			}
			PARAMETER_DESCRIPTION description = default(PARAMETER_DESCRIPTION);
			instance.getDescription(out description);
			_value = Mathf.Clamp(_value, description.minimum, description.maximum);
			instance.setValue(_value);
			return _value;
		}
		Parameter parameter = FetchInitialParam(_name);
		if (parameter != null)
		{
			parameter.value = _value;
			return parameter.value;
		}
		parameter = new Parameter();
		parameter.name = _name;
		parameter.value = _value;
		initParams.Add(parameter);
		return 0f;
	}

	private void OnEnable()
	{
		Reset();
	}

	private void OnDisable()
	{
		if (!FMODAudioSystem.isShutDown)
		{
			Free();
		}
	}

	private void OnDestroy()
	{
		if (!FMODAudioSystem.isShutDown)
		{
			UnityUtil.Log("Destroy called");
			Free();
		}
	}

	private void Update()
	{
		if (evt != null && evt.isValid())
		{
			UpdateMinMaxDistance();
			Update3DAttributes();
		}
		else
		{
			evt = null;
		}
		if (rteActive && rte == null)
		{
			rte = base.gameObject.AddComponent<FMODAudioSourceRTE>();
		}
		else if (!rteActive && rte != null)
		{
			UnityEngine.Object.Destroy(rte);
			rte = null;
		}
	}

	private RESULT ERRCHECK(RESULT result)
	{
		UnityUtil.ERRCHECK(result);
		return result;
	}

	public static void PlayOneShot(string path, Vector3 position)
	{
		FMOD_StudioSystem.instance.PlayOneShot(path, position);
	}

	public static void PlayOneShot(string path, Vector3 position, float volume)
	{
		FMOD_StudioSystem.instance.PlayOneShot(path, position, volume);
	}

	public static void PlayOneShot(FMODAsset asset, Vector3 position)
	{
		FMOD_StudioSystem.instance.PlayOneShot(asset.id, position);
	}

	public static void PlayOneShot(FMODAsset asset, Vector3 position, float volume)
	{
		FMOD_StudioSystem.instance.PlayOneShot(asset.id, position, volume);
	}
}
