using System;
using System.Collections.Generic;
using Pathea;
using UnityEngine;
using UnityEngine.Events;
using WhiteCat;

[ExecuteInEditMode]
public class CutsceneClip : MonoBehaviour
{
	private Path _mainPath;

	private TweenInterpolator _interpolator;

	private PathDriver targetPathDriver;

	private TweenPathDriver targetTweenPathDriver;

	public CutsceneRotationNode[] Rotations = new CutsceneRotationNode[0];

	private List<CutsceneRotationNode> rots;

	public float fadeInSpeed = 0.1f;

	public float fadeOutSpeed = 0.1f;

	public UnityEvent onArriveAtBeginning = new UnityEvent();

	public UnityEvent onArriveAtEnding = new UnityEvent();

	public bool testClip;

	private bool _lastTrackClip;

	public bool trackClip;

	public float trackTime;

	[NonSerialized]
	[HideInInspector]
	public bool isEditMode = true;

	private Path mainPath
	{
		get
		{
			if (_mainPath != null)
			{
				return _mainPath;
			}
			_mainPath = GetComponent<Path>();
			return _mainPath;
		}
	}

	private TweenInterpolator interpolator
	{
		get
		{
			if (_interpolator != null)
			{
				return _interpolator;
			}
			_interpolator = GetComponent<TweenInterpolator>();
			return _interpolator;
		}
	}

	public float normalizedTime => interpolator.normalizedTime;

	public float time => interpolator.normalizedTime * interpolator.duration;

	private void Start()
	{
		if (!isEditMode)
		{
			BeginClip();
		}
	}

	private void Update()
	{
		if (isEditMode)
		{
			if (testClip)
			{
				if (!trackClip)
				{
					BeginClip();
				}
				testClip = false;
			}
			if (trackClip && !_lastTrackClip)
			{
				CreateDriver();
				interpolator.enabled = false;
				FadeIn(1f);
			}
			if (!trackClip && _lastTrackClip)
			{
				FadeOut(1f);
				DestroyDriver();
			}
			if (trackClip)
			{
				TrackClip(trackTime);
			}
			_lastTrackClip = trackClip;
			PrepareRotation();
			foreach (CutsceneRotationNode rot in rots)
			{
				float pathLength = interpolator.Interpolate(rot.time) * mainPath.pathTotalLength;
				int splineIndex = 0;
				float splineTime = 0f;
				mainPath.GetPathPositionAtPathLength(pathLength, ref splineIndex, ref splineTime);
				Vector3 splinePoint = mainPath.GetSplinePoint(splineIndex, splineTime);
				rot.rotation.position = splinePoint;
				Debug.DrawLine(rot.rotation.position, rot.rotation.position + rot.rotation.forward * 0.65f, Color.yellow);
				Debug.DrawLine(rot.rotation.position, rot.rotation.position + rot.rotation.up * 0.2f, Color.white);
				Debug.DrawLine(rot.rotation.position, rot.rotation.position - rot.rotation.up * 0.15f, Color.white);
				Debug.DrawLine(rot.rotation.position, rot.rotation.position + rot.rotation.right * 0.3f, Color.white);
				Debug.DrawLine(rot.rotation.position, rot.rotation.position - rot.rotation.right * 0.25f, Color.white);
				Debug.DrawLine(rot.rotation.position - rot.rotation.right * 0.25f + rot.rotation.up * 0.15f, rot.rotation.position + rot.rotation.right * 0.25f + rot.rotation.up * 0.15f, Color.white * 0.7f);
				Debug.DrawLine(rot.rotation.position - rot.rotation.right * 0.25f - rot.rotation.up * 0.15f, rot.rotation.position + rot.rotation.right * 0.25f - rot.rotation.up * 0.15f, Color.white * 0.7f);
				Debug.DrawLine(rot.rotation.position + rot.rotation.right * 0.25f + rot.rotation.up * 0.15f, rot.rotation.position + rot.rotation.right * 0.25f - rot.rotation.up * 0.15f, Color.white * 0.7f);
				Debug.DrawLine(rot.rotation.position - rot.rotation.right * 0.25f + rot.rotation.up * 0.15f, rot.rotation.position - rot.rotation.right * 0.25f - rot.rotation.up * 0.15f, Color.white * 0.7f);
			}
		}
		if (interpolator.isPlaying)
		{
			UpdateRotation();
		}
	}

	private void OnDestroy()
	{
		EndClip();
	}

	private void CreateDriver()
	{
		if (targetTweenPathDriver == null && targetPathDriver == null)
		{
			targetPathDriver = PeCamera.cutsceneTransform.gameObject.AddComponent<PathDriver>();
			targetPathDriver.path = mainPath;
			targetPathDriver.location = 0f;
			targetTweenPathDriver = PeCamera.cutsceneTransform.gameObject.AddComponent<TweenPathDriver>();
			targetTweenPathDriver.interpolator = interpolator;
			targetTweenPathDriver.from = 0f;
			targetTweenPathDriver.to = mainPath.pathTotalLength;
		}
	}

	private void DestroyDriver()
	{
		if (targetTweenPathDriver != null && targetPathDriver != null)
		{
			UnityEngine.Object.Destroy(targetTweenPathDriver);
			UnityEngine.Object.Destroy(targetPathDriver);
			targetTweenPathDriver = null;
			targetPathDriver = null;
		}
	}

	private void BeginClip()
	{
		if (PeCamera.cutsceneTransform != null && targetTweenPathDriver == null && targetPathDriver == null)
		{
			CreateDriver();
			interpolator.enabled = true;
			interpolator.Replay();
			if (isEditMode)
			{
				interpolator.onArriveAtEnding.AddListener(EndClip);
			}
			else
			{
				interpolator.onArriveAtEnding.AddListener(SelfDestroy);
			}
			FadeIn(fadeInSpeed);
			onArriveAtBeginning.Invoke();
		}
	}

	private void EndClip()
	{
		interpolator.enabled = false;
		if (targetTweenPathDriver != null && targetPathDriver != null)
		{
			DestroyDriver();
			FadeOut(fadeOutSpeed);
			onArriveAtEnding.Invoke();
		}
	}

	private void TrackClip(float t)
	{
		interpolator.normalizedTime = t / interpolator.duration;
		UpdateRotation();
	}

	private void PrepareRotation()
	{
		if (rots == null)
		{
			rots = new List<CutsceneRotationNode>();
		}
		rots.Clear();
		CutsceneRotationNode[] rotations = Rotations;
		foreach (CutsceneRotationNode cutsceneRotationNode in rotations)
		{
			if (cutsceneRotationNode.rotation != null)
			{
				rots.Add(cutsceneRotationNode);
			}
		}
		rots.Sort(CutsceneRotationNode.Compare);
	}

	private void UpdateRotation()
	{
		if (!isEditMode && rots == null)
		{
			PrepareRotation();
		}
		if (!(PeCamera.cutsceneTransform != null))
		{
			return;
		}
		if (rots.Count == 0)
		{
			PeCamera.cutsceneTransform.rotation = Quaternion.identity;
			return;
		}
		if (normalizedTime <= rots[0].time)
		{
			PeCamera.cutsceneTransform.rotation = rots[0].rotation.rotation;
			return;
		}
		if (normalizedTime >= rots[rots.Count - 1].time)
		{
			PeCamera.cutsceneTransform.rotation = rots[rots.Count - 1].rotation.rotation;
			return;
		}
		Quaternion p = Quaternion.identity;
		Quaternion a = Quaternion.identity;
		Quaternion b = Quaternion.identity;
		Quaternion q = Quaternion.identity;
		float t = 0f;
		for (int i = 0; i < rots.Count - 1; i++)
		{
			if (normalizedTime >= rots[i].time && normalizedTime < rots[i + 1].time)
			{
				a = rots[(i != 0) ? (i - 1) : i].rotation.rotation;
				p = rots[i].rotation.rotation;
				q = rots[i + 1].rotation.rotation;
				b = rots[(i != rots.Count - 2) ? (i + 2) : (i + 1)].rotation.rotation;
				t = Mathf.InverseLerp(rots[i].time, rots[i + 1].time, normalizedTime);
				break;
			}
		}
		PeCamera.cutsceneTransform.rotation = SplineUtils.CalculateCubic(p, a, b, q, t);
	}

	private void SelfDestroy()
	{
		EndClip();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void FadeIn(float speed)
	{
		PeCamera.CrossFade("Cutscene Blend", 1, speed);
	}

	private void FadeOut(float speed)
	{
		PeCamera.CrossFade("Cutscene Blend", 0, speed);
	}
}
