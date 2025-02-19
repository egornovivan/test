using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[RequireComponent(typeof(LegController))]
[RequireComponent(typeof(AlignmentTracker))]
public class LegAnimator : MonoBehaviour
{
	public bool startAutomatically = true;

	public bool useIK = true;

	public float maxFootRotationAngle = 45f;

	public float maxIKAdjustmentDistance = 0.5f;

	public float minStepDistance = 0.2f;

	public float maxStepDuration = 1.5f;

	public float maxStepRotation = 160f;

	public float maxStepAcceleration = 5f;

	public float maxStepHeight = 1f;

	public float maxSlopeAngle = 60f;

	public bool enableLegParking = true;

	public float blendSmoothing = 0.2f;

	public LayerMask groundLayers = 1;

	public float groundHugX;

	public float groundHugZ;

	public float climbTiltAmount = 0.5f;

	public float climbTiltSensitivity;

	public float accelerateTiltAmount = 0.02f;

	public float accelerateTiltSensitivity;

	public bool renderFootMarkers;

	public bool renderBlendingGraph;

	public bool renderCycleGraph;

	public bool renderAnimationStates;

	private bool isActive;

	private float currentTime;

	private LegController legC;

	private AlignmentTracker tr;

	private LegInfo[] legs;

	private LegState[] legStates;

	private Vector3 position;

	private float speed;

	private float hSpeedSmoothed;

	private Vector3 objectVelocity;

	private Vector3 usedObjectVelocity;

	private Quaternion rotation;

	private Vector3 up;

	private Vector3 forward;

	private float scale;

	private Vector3 baseUpGround;

	private Vector3 bodyUp;

	private Vector3 legsUp;

	private float accelerationTiltX;

	private float accelerationTiltZ;

	private AnimationState controlMotionState;

	private MotionGroupState[] motionGroupStates;

	private AnimationState[] nonGroupMotionStates;

	private float[] nonGroupMotionWeights;

	private AnimationState[] motionStates;

	private AnimationState[] cycleMotionStates;

	private float[] motionWeights;

	private float[] cycleMotionWeights;

	private float summedMotionWeight;

	private float summedCycleMotionWeight;

	private float locomotionWeight;

	private float cycleDuration;

	private float cycleDistance;

	private float normalizedTime;

	private bool updateStates = true;

	[NonSerialized]
	public GameObject ghost;

	private Dictionary<string, TrajectoryVisualizer> trajectories = new Dictionary<string, TrajectoryVisualizer>();

	[NonSerialized]
	private Material lineMaterial;

	public bool IsActive => isActive;

	[Conditional("VISUALIZE")]
	private void AddTrajectoryPoint(string name, Vector3 point)
	{
		trajectories[name].AddPoint(Time.time, point);
	}

	[Conditional("DEBUG")]
	private void Assert(bool condition, string text)
	{
		if (!condition)
		{
			UnityEngine.Debug.LogError(text);
		}
	}

	[Conditional("DEBUG")]
	private void AssertSane(float f, string text)
	{
		if (!Util.IsSaneNumber(f))
		{
			UnityEngine.Debug.LogError(text + "=" + f);
		}
	}

	[Conditional("DEBUG")]
	private void AssertSane(Vector3 vect, string text)
	{
		if (!Util.IsSaneNumber(vect.x) || !Util.IsSaneNumber(vect.y) || !Util.IsSaneNumber(vect.z))
		{
			UnityEngine.Debug.LogError(text + "=" + vect);
		}
	}

	[Conditional("DEBUG")]
	private void AssertSane(Quaternion q, string text)
	{
		if (!Util.IsSaneNumber(q.x) || !Util.IsSaneNumber(q.y) || !Util.IsSaneNumber(q.z) || !Util.IsSaneNumber(q.w))
		{
			UnityEngine.Debug.LogError(text + "=" + q);
		}
	}

	private void Start()
	{
		tr = GetComponent(typeof(AlignmentTracker)) as AlignmentTracker;
		legC = GetComponent(typeof(LegController)) as LegController;
		legs = legC.legs;
		if (!legC.initialized)
		{
			UnityEngine.Debug.LogError(base.name + ": Locomotion System has not been initialized.", this);
			base.enabled = false;
		}
		legStates = new LegState[legs.Length];
		updateStates = true;
		ResetMotionStates();
		ResetSteps();
		isActive = false;
		for (int i = 0; i < legs.Length; i++)
		{
			trajectories.Add("leg" + i + "heel", new TrajectoryVisualizer(legs[i].debugColor, 3f));
			trajectories.Add("leg" + i + "toetip", new TrajectoryVisualizer(legs[i].debugColor, 3f));
			trajectories.Add("leg" + i + "footbase", new TrajectoryVisualizer(legs[i].debugColor, 3f));
		}
	}

	private void OnEnable()
	{
		updateStates = true;
		if (!(legC == null))
		{
			ResetMotionStates();
			ResetSteps();
			if (!legC.initialized)
			{
				UnityEngine.Debug.LogError(base.name + ": Locomotion System has not been initialized.", this);
				base.enabled = false;
			}
		}
	}

	private void ResetMotionStates()
	{
		motionStates = new AnimationState[legC.motions.Length];
		cycleMotionStates = new AnimationState[legC.cycleMotions.Length];
		motionWeights = new float[legC.motions.Length];
		cycleMotionWeights = new float[legC.cycleMotions.Length];
		nonGroupMotionWeights = new float[legC.nonGroupMotions.Length];
		controlMotionState = GetComponent<Animation>()["LocomotionSystem"];
		if (controlMotionState == null)
		{
			GetComponent<Animation>().AddClip(new AnimationClip(), "LocomotionSystem");
			controlMotionState = GetComponent<Animation>()["LocomotionSystem"];
		}
		controlMotionState.enabled = true;
		controlMotionState.wrapMode = WrapMode.Loop;
		controlMotionState.weight = 1f;
		controlMotionState.layer = 10000;
		motionGroupStates = new MotionGroupState[legC.motionGroups.Length];
		int num = 0;
		for (int i = 0; i < legC.motions.Length; i++)
		{
			motionStates[i] = GetComponent<Animation>()[legC.motions[i].name];
			if (motionStates[i] == null)
			{
				GetComponent<Animation>().AddClip(legC.motions[i].animation, legC.motions[i].name);
				motionStates[i] = GetComponent<Animation>()[legC.motions[i].name];
			}
			motionStates[i].wrapMode = WrapMode.Loop;
			if (legC.motions[i].motionType == MotionType.WalkCycle)
			{
				cycleMotionStates[num] = motionStates[i];
				cycleMotionStates[num].speed = 0f;
				num++;
			}
		}
		for (int j = 0; j < motionGroupStates.Length; j++)
		{
			AnimationState animationState = GetComponent<Animation>()[legC.motionGroups[j].name];
			if (animationState == null)
			{
				GetComponent<Animation>().AddClip(new AnimationClip(), legC.motionGroups[j].name);
				animationState = GetComponent<Animation>()[legC.motionGroups[j].name];
			}
			animationState.enabled = true;
			animationState.wrapMode = WrapMode.Loop;
			if (startAutomatically && j == 0)
			{
				animationState.weight = 1f;
			}
			motionGroupStates[j] = new MotionGroupState();
			motionGroupStates[j].controller = animationState;
			motionGroupStates[j].motionStates = new AnimationState[legC.motionGroups[j].motions.Length];
			motionGroupStates[j].relativeWeights = new float[legC.motionGroups[j].motions.Length];
			for (int k = 0; k < motionGroupStates[j].motionStates.Length; k++)
			{
				motionGroupStates[j].motionStates[k] = GetComponent<Animation>()[legC.motionGroups[j].motions[k].name];
			}
			motionGroupStates[j].primaryMotionIndex = 0;
		}
		nonGroupMotionStates = new AnimationState[legC.nonGroupMotions.Length];
		for (int l = 0; l < legC.nonGroupMotions.Length; l++)
		{
			nonGroupMotionStates[l] = GetComponent<Animation>()[legC.nonGroupMotions[l].name];
			if (nonGroupMotionStates[l] == null)
			{
				GetComponent<Animation>().AddClip(legC.nonGroupMotions[l].animation, legC.nonGroupMotions[l].name);
				nonGroupMotionStates[l] = GetComponent<Animation>()[legC.nonGroupMotions[l].name];
				nonGroupMotionWeights[l] = nonGroupMotionStates[l].weight;
			}
		}
		for (int m = 0; m < legs.Length; m++)
		{
			legStates[m] = new LegState();
		}
	}

	private void ResetSteps()
	{
		up = base.transform.up;
		forward = base.transform.forward;
		baseUpGround = up;
		legsUp = up;
		accelerationTiltX = 0f;
		accelerationTiltZ = 0f;
		bodyUp = up;
		tr.Reset();
		for (int i = 0; i < legs.Length; i++)
		{
			legStates[i].stepFromTime = Time.time - 0.01f;
			legStates[i].stepToTime = Time.time;
			legStates[i].stepFromMatrix = FindGroundedBase(base.transform.TransformPoint(legStates[i].stancePosition / scale), base.transform.rotation, legStates[i].heelToetipVector, avoidLedges: false);
			legStates[i].stepFromPosition = legStates[i].stepFromMatrix.GetColumn(3);
			legStates[i].stepToPosition = legStates[i].stepFromPosition;
			legStates[i].stepToMatrix = legStates[i].stepFromMatrix;
		}
		normalizedTime = 0f;
		cycleDuration = maxStepDuration;
		cycleDistance = 0f;
	}

	private void Update()
	{
		if (Time.deltaTime == 0f || Time.timeScale == 0f)
		{
			return;
		}
		scale = base.transform.lossyScale.z;
		Vector3 vector = Util.ProjectOntoPlane(tr.velocity, up);
		speed = vector.magnitude;
		speed = (vector + up * Mathf.Clamp(Vector3.Dot(tr.velocity, up), 0f - speed, speed)).magnitude;
		hSpeedSmoothed = Util.ProjectOntoPlane(tr.velocitySmoothed, up).magnitude;
		objectVelocity = base.transform.InverseTransformPoint(tr.velocitySmoothed) - base.transform.InverseTransformPoint(Vector3.zero);
		bool flag = false;
		if ((objectVelocity - usedObjectVelocity).magnitude > 0.002f * Mathf.Min(objectVelocity.magnitude, usedObjectVelocity.magnitude) || updateStates)
		{
			flag = true;
			usedObjectVelocity = objectVelocity;
		}
		bool flag2 = false;
		float num = 0.001f;
		for (int i = 0; i < legC.motionGroups.Length; i++)
		{
			MotionGroupState motionGroupState = motionGroupStates[i];
			bool flag3 = false;
			bool flag4 = false;
			float num2 = motionGroupState.controller.weight;
			if (!motionGroupState.controller.enabled || num2 < num)
			{
				num2 = 0f;
			}
			else if (num2 > 1f - num)
			{
				num2 = 1f;
			}
			if (Mathf.Abs(num2 - motionGroupState.weight) > num)
			{
				flag3 = true;
				flag2 = true;
				if (motionGroupState.weight == 0f && num2 > 0f)
				{
					flag4 = true;
				}
				motionGroupState.weight = num2;
			}
			else if (Mathf.Abs(motionGroupState.motionStates[motionGroupState.primaryMotionIndex].weight - motionGroupState.relativeWeights[motionGroupState.primaryMotionIndex] * motionGroupState.weight) > num || motionGroupState.motionStates[motionGroupState.primaryMotionIndex].layer != motionGroupState.controller.layer)
			{
				flag3 = true;
			}
			if ((flag || flag3) && (flag || flag4) && motionGroupState.weight > 0f)
			{
				flag2 = true;
				MotionGroupInfo motionGroupInfo = legC.motionGroups[i];
				motionGroupState.relativeWeights = motionGroupInfo.GetMotionWeights(new Vector3(objectVelocity.x, 0f, objectVelocity.z));
			}
			if (motionGroupState.weight > 0f)
			{
				if (motionGroupState.relativeWeightsBlended == null)
				{
					motionGroupState.relativeWeightsBlended = new float[motionGroupState.relativeWeights.Length];
					for (int j = 0; j < motionGroupState.motionStates.Length; j++)
					{
						motionGroupState.relativeWeightsBlended[j] = motionGroupState.relativeWeights[j];
					}
				}
				float num3 = 0f;
				int layer = motionGroupState.controller.layer;
				for (int k = 0; k < motionGroupState.motionStates.Length; k++)
				{
					if (blendSmoothing > 0f)
					{
						motionGroupState.relativeWeightsBlended[k] = Mathf.Lerp(motionGroupState.relativeWeightsBlended[k], motionGroupState.relativeWeights[k], Time.deltaTime / blendSmoothing);
					}
					else
					{
						motionGroupState.relativeWeightsBlended[k] = motionGroupState.relativeWeights[k];
					}
					float num4 = motionGroupState.relativeWeightsBlended[k] * motionGroupState.weight;
					motionGroupState.motionStates[k].weight = num4;
					if (num4 > 0f)
					{
						motionGroupState.motionStates[k].enabled = true;
					}
					else
					{
						motionGroupState.motionStates[k].enabled = false;
					}
					motionGroupState.motionStates[k].layer = layer;
					if (num4 > num3)
					{
						motionGroupState.primaryMotionIndex = k;
						num3 = num4;
					}
				}
			}
			else
			{
				for (int l = 0; l < motionGroupState.motionStates.Length; l++)
				{
					motionGroupState.motionStates[l].weight = 0f;
					motionGroupState.motionStates[l].enabled = false;
				}
				motionGroupState.relativeWeightsBlended = null;
			}
		}
		for (int m = 0; m < nonGroupMotionStates.Length; m++)
		{
			float num5 = nonGroupMotionStates[m].weight;
			if (!nonGroupMotionStates[m].enabled)
			{
				num5 = 0f;
			}
			if (Mathf.Abs(num5 - nonGroupMotionWeights[m]) > num || (num5 == 0f && nonGroupMotionWeights[m] != 0f))
			{
				flag2 = true;
				nonGroupMotionWeights[m] = num5;
			}
		}
		bool flag5 = updateStates;
		if (flag2 || updateStates)
		{
			summedMotionWeight = 0f;
			summedCycleMotionWeight = 0f;
			int num6 = 0;
			for (int n = 0; n < legC.motions.Length; n++)
			{
				motionWeights[n] = motionStates[n].weight;
				summedMotionWeight += motionWeights[n];
				if (legC.motions[n].motionType == MotionType.WalkCycle)
				{
					cycleMotionWeights[num6] = motionWeights[n];
					summedCycleMotionWeight += motionWeights[n];
					num6++;
				}
			}
			if (summedMotionWeight == 0f)
			{
				isActive = false;
				if (ghost != null)
				{
					GhostOriginal ghostOriginal = ghost.GetComponent(typeof(GhostOriginal)) as GhostOriginal;
					ghostOriginal.Synch();
				}
				return;
			}
			if (!isActive)
			{
				flag5 = true;
			}
			isActive = true;
			for (int num7 = 0; num7 < legC.motions.Length; num7++)
			{
				motionWeights[num7] /= summedMotionWeight;
			}
			if (summedCycleMotionWeight > 0f)
			{
				for (int num8 = 0; num8 < legC.cycleMotions.Length; num8++)
				{
					cycleMotionWeights[num8] /= summedCycleMotionWeight;
				}
			}
			for (int num9 = 0; num9 < legs.Length; num9++)
			{
				legStates[num9].stancePosition = Vector3.zero;
				legStates[num9].heelToetipVector = Vector3.zero;
			}
			for (int num10 = 0; num10 < legC.motions.Length; num10++)
			{
				IMotionAnalyzer motionAnalyzer = legC.motions[num10];
				float num11 = motionWeights[num10];
				if (num11 > 0f)
				{
					for (int num12 = 0; num12 < legs.Length; num12++)
					{
						legStates[num12].stancePosition += motionAnalyzer.cycles[num12].stancePosition * scale * num11;
						legStates[num12].heelToetipVector += motionAnalyzer.cycles[num12].heelToetipVector * scale * num11;
					}
				}
			}
			if (summedCycleMotionWeight > 0f)
			{
				for (int num13 = 0; num13 < legs.Length; num13++)
				{
					legStates[num13].liftTime = 0f;
					legStates[num13].liftoffTime = 0f;
					legStates[num13].postliftTime = 0f;
					legStates[num13].prelandTime = 0f;
					legStates[num13].strikeTime = 0f;
					legStates[num13].landTime = 0f;
				}
				for (int num14 = 0; num14 < legC.cycleMotions.Length; num14++)
				{
					IMotionAnalyzer motionAnalyzer2 = legC.cycleMotions[num14];
					float num15 = cycleMotionWeights[num14];
					if (num15 > 0f)
					{
						for (int num16 = 0; num16 < legs.Length; num16++)
						{
							legStates[num16].liftTime += motionAnalyzer2.cycles[num16].liftTime * num15;
							legStates[num16].liftoffTime += motionAnalyzer2.cycles[num16].liftoffTime * num15;
							legStates[num16].postliftTime += motionAnalyzer2.cycles[num16].postliftTime * num15;
							legStates[num16].prelandTime += motionAnalyzer2.cycles[num16].prelandTime * num15;
							legStates[num16].strikeTime += motionAnalyzer2.cycles[num16].strikeTime * num15;
							legStates[num16].landTime += motionAnalyzer2.cycles[num16].landTime * num15;
						}
					}
				}
			}
			if (summedCycleMotionWeight > 0f)
			{
				for (int num17 = 0; num17 < legs.Length; num17++)
				{
					Vector2 zero = Vector2.zero;
					for (int num18 = 0; num18 < legC.cycleMotions.Length; num18++)
					{
						IMotionAnalyzer motionAnalyzer3 = legC.cycleMotions[num18];
						float num19 = cycleMotionWeights[num18];
						if (num19 > 0f)
						{
							zero += new Vector2(Mathf.Cos(motionAnalyzer3.cycles[num17].stanceTime * 2f * (float)Math.PI), Mathf.Sin(motionAnalyzer3.cycles[num17].stanceTime * 2f * (float)Math.PI)) * num19;
						}
					}
					legStates[num17].stanceTime = Util.Mod(Mathf.Atan2(zero.y, zero.x) / 2f / (float)Math.PI);
				}
			}
		}
		float num20 = controlMotionState.weight;
		if (!controlMotionState.enabled)
		{
			num20 = 0f;
		}
		locomotionWeight = Mathf.Clamp01(summedMotionWeight * num20);
		if (updateStates || flag5)
		{
			ResetSteps();
		}
		float num21 = 0f;
		float num22 = 0f;
		for (int num23 = 0; num23 < legC.motions.Length; num23++)
		{
			IMotionAnalyzer motionAnalyzer4 = legC.motions[num23];
			float num24 = motionWeights[num23];
			if (num24 > 0f)
			{
				if (motionAnalyzer4.motionType == MotionType.WalkCycle)
				{
					num21 += 1f / motionAnalyzer4.cycleDuration * num24;
				}
				num22 += motionAnalyzer4.cycleSpeed * num24;
			}
		}
		float num25 = maxStepDuration;
		if (num21 > 0f)
		{
			num25 = 1f / num21;
		}
		float num26 = 1f;
		if (speed != 0f)
		{
			num26 = num22 * scale / speed;
		}
		if (num26 > 0f)
		{
			num25 *= num26;
		}
		float magnitude = Vector3.Project(tr.rotation * tr.angularVelocitySmoothed, up).magnitude;
		if (magnitude > 0f)
		{
			num25 = Mathf.Min(maxStepRotation / magnitude, num25);
		}
		float magnitude2 = Util.ProjectOntoPlane(tr.accelerationSmoothed, up).magnitude;
		if (magnitude2 > 0f)
		{
			num25 = Mathf.Clamp(maxStepAcceleration / magnitude2, num25 / 2f, num25);
		}
		num25 = Mathf.Min(num25, maxStepDuration);
		cycleDuration = num25;
		cycleDistance = cycleDuration * speed;
		bool flag6 = false;
		if (enableLegParking)
		{
			flag6 = true;
			for (int num27 = 0; num27 < legs.Length; num27++)
			{
				if (!legStates[num27].parked)
				{
					flag6 = false;
				}
			}
		}
		if (!flag6)
		{
			normalizedTime = Util.Mod(normalizedTime + 1f / cycleDuration * Time.deltaTime);
			for (int num28 = 0; num28 < legC.cycleMotions.Length; num28++)
			{
				if (legC.cycleMotions[num28].GetType() == typeof(MotionAnalyzerBackwards))
				{
					cycleMotionStates[num28].normalizedTime = 1f - (normalizedTime - legC.cycleMotions[num28].cycleOffset);
				}
				else
				{
					cycleMotionStates[num28].normalizedTime = normalizedTime - legC.cycleMotions[num28].cycleOffset;
				}
			}
		}
		updateStates = false;
		currentTime = Time.time;
		if (ghost != null)
		{
			GhostOriginal ghostOriginal2 = ghost.GetComponent(typeof(GhostOriginal)) as GhostOriginal;
			ghostOriginal2.Synch();
		}
	}

	private void FixedUpdate()
	{
		if (Time.deltaTime != 0f && Time.timeScale != 0f)
		{
			tr.ControlledFixedUpdate();
		}
	}

	private void LateUpdate()
	{
		if (Time.deltaTime == 0f || Time.timeScale == 0f)
		{
			return;
		}
		MonitorFootsteps();
		tr.ControlledLateUpdate();
		position = tr.position;
		rotation = tr.rotation;
		up = rotation * Vector3.up;
		forward = rotation * Vector3.forward;
		Vector3 vector = rotation * Vector3.right;
		if (!isActive || currentTime != Time.time || !useIK)
		{
			return;
		}
		int layer = base.gameObject.layer;
		base.gameObject.layer = 2;
		for (int i = 0; i < legs.Length; i++)
		{
			float num = Util.CyclicDiff(normalizedTime, legStates[i].stanceTime);
			bool flag = false;
			if (num < legStates[i].designatedCycleTimePrev - 0.5f)
			{
				flag = true;
				legStates[i].stepNr++;
				if (!legStates[i].parked)
				{
					legStates[i].stepFromTime = legStates[i].stepToTime;
					legStates[i].stepFromPosition = legStates[i].stepToPosition;
					legStates[i].stepFromMatrix = legStates[i].stepToMatrix;
					legStates[i].debugHistory.Clear();
					legStates[i].cycleTime = num;
				}
				legStates[i].parked = false;
			}
			legStates[i].designatedCycleTimePrev = num;
			legStates[i].stepToTime = Time.time + (1f - num) * cycleDuration;
			float num2 = (legStates[i].strikeTime - num) * cycleDuration;
			if (num >= legStates[i].strikeTime)
			{
				legStates[i].cycleTime = num;
			}
			else
			{
				legStates[i].cycleTime += (legStates[i].strikeTime - legStates[i].cycleTime) * Time.deltaTime / num2;
			}
			if (legStates[i].cycleTime >= num)
			{
				legStates[i].cycleTime = num;
			}
			if (legStates[i].cycleTime < legStates[i].strikeTime)
			{
				float num3 = Mathf.InverseLerp(legStates[i].liftoffTime, legStates[i].strikeTime, legStates[i].cycleTime);
				Quaternion quaternion = Quaternion.AngleAxis(tr.angularVelocitySmoothed.magnitude * (legStates[i].stepToTime - Time.time), tr.angularVelocitySmoothed) * tr.rotation;
				Quaternion quaternion2;
				if (legStates[i].cycleTime <= legStates[i].liftoffTime)
				{
					quaternion2 = quaternion;
				}
				else
				{
					Quaternion quaternion3 = Util.QuaternionFromMatrix(legStates[i].stepToMatrix);
					quaternion3 = Quaternion.FromToRotation(quaternion3 * Vector3.up, up) * quaternion3;
					float num4 = Mathf.Max(tr.angularVelocitySmoothed.magnitude * 3f, maxStepRotation / maxStepDuration);
					float angle = num4 / num3 * Time.deltaTime;
					quaternion2 = Util.ConstantSlerp(quaternion3, quaternion, angle);
				}
				float num5 = Vector3.Dot(tr.angularVelocitySmoothed, up);
				Vector3 originalVector;
				if (num5 * cycleDuration < 5f)
				{
					originalVector = tr.position + quaternion2 * legStates[i].stancePosition + tr.velocity * (legStates[i].stepToTime - Time.time);
				}
				else
				{
					Vector3 vector2 = Vector3.Cross(up, tr.velocity) / (num5 * (float)Math.PI / 180f);
					Vector3 vector3 = vector2 + Quaternion.AngleAxis(num5 * (legStates[i].stepToTime - Time.time), up) * -vector2;
					originalVector = tr.position + quaternion2 * legStates[i].stancePosition + vector3;
				}
				originalVector = Util.SetHeight(originalVector, position + legC.groundPlaneHeight * up * scale, up);
				Matrix4x4 stepToMatrix = FindGroundedBase(originalVector, quaternion2, legStates[i].heelToetipVector, avoidLedges: true);
				originalVector = stepToMatrix.GetColumn(3);
				if (flag)
				{
					legStates[i].stepToPosition = originalVector;
					legStates[i].stepToPositionGoal = originalVector;
				}
				else
				{
					float num6 = Mathf.Max(speed * 3f + tr.accelerationSmoothed.magnitude / 10f, legs[i].footLength * scale * 3f);
					float num7 = legStates[i].cycleTime / legStates[i].strikeTime;
					if ((originalVector - legStates[i].stepToPosition).sqrMagnitude < Mathf.Pow(num6 * (1f / num7 - 1f), 2f))
					{
						legStates[i].stepToPositionGoal = originalVector;
					}
					Vector3 vector4 = legStates[i].stepToPositionGoal - legStates[i].stepToPosition;
					if (vector4 != Vector3.zero && num2 > 0f)
					{
						float magnitude = vector4.magnitude;
						float num8 = Mathf.Min(magnitude, Mathf.Max(num6 / Mathf.Max(0.1f, num3) * Time.deltaTime, (1f + 2f * Mathf.Pow(num7 - 1f, 2f)) * (Time.deltaTime / num2) * magnitude));
						legStates[i].stepToPosition += (legStates[i].stepToPositionGoal - legStates[i].stepToPosition) / magnitude * num8;
					}
				}
				stepToMatrix.SetColumn(3, legStates[i].stepToPosition);
				stepToMatrix[3, 3] = 1f;
				legStates[i].stepToMatrix = stepToMatrix;
			}
			if (enableLegParking)
			{
				float magnitude2 = Util.ProjectOntoPlane(legStates[i].stepToPosition - legStates[i].stepFromPosition, up).magnitude;
				bool flag2 = magnitude2 > minStepDistance || Vector3.Angle(legStates[i].stepToMatrix.GetColumn(2), legStates[i].stepFromMatrix.GetColumn(2)) > maxStepRotation / 2f;
				if (flag && !flag2)
				{
					legStates[i].parked = true;
				}
				if (legStates[i].parked && num < 0.67f && flag2)
				{
					legStates[i].parked = false;
				}
				if (legStates[i].parked)
				{
					legStates[i].cycleTime = 0f;
				}
			}
		}
		Vector3 vector5 = Quaternion.Inverse(tr.rotation) * tr.velocity;
		vector5.y = 0f;
		if (vector5.sqrMagnitude > 0f)
		{
			vector5 = vector5.normalized;
		}
		Vector3[] array = new Vector3[legs.Length];
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		Vector3 zero3 = Vector3.zero;
		float num9 = 0f;
		for (int j = 0; j < legs.Length; j++)
		{
			float num10 = Mathf.Cos(legStates[j].cycleTime * 2f * (float)Math.PI) / 2f + 0.5f;
			num9 += num10 + 0.001f;
			float num11 = Mathf.InverseLerp(legStates[j].liftTime, legStates[j].landTime, legStates[j].cycleTime);
			float num12 = (0f - Mathf.Cos(num11 * (float)Math.PI)) / 2f + 0.5f;
			Vector3 vector6 = base.transform.TransformDirection(-legStates[j].stancePosition) * scale;
			ref Vector3 reference = ref array[j];
			reference = (legStates[j].stepFromPosition + legStates[j].stepFromMatrix.MultiplyVector(vector5) * cycleDistance * legStates[j].cycleTime) * (1f - num12) + (legStates[j].stepToPosition + legStates[j].stepToMatrix.MultiplyVector(vector5) * cycleDistance * (legStates[j].cycleTime - 1f)) * num12;
			if (float.IsNaN(array[j].x) || float.IsNaN(array[j].y) || float.IsNaN(array[j].z))
			{
				UnityEngine.Debug.LogError(string.Concat("legStates[leg].cycleTime=", legStates[j].cycleTime, ", strideSCurve=", num12, ", tangentDir=", vector5, ", cycleDistance=", cycleDistance, ", legStates[leg].stepFromPosition=", legStates[j].stepFromPosition, ", legStates[leg].stepToPosition=", legStates[j].stepToPosition, ", legStates[leg].stepToMatrix.MultiplyVector(tangentDir)=", legStates[j].stepToMatrix.MultiplyVector(vector5), ", legStates[leg].stepFromMatrix.MultiplyVector(tangentDir)=", legStates[j].stepFromMatrix.MultiplyVector(vector5)));
			}
			zero += (array[j] + vector6) * (num10 + 0.001f);
			zero3 += array[j];
			zero2 += (legStates[j].stepToPosition - legStates[j].stepFromPosition) * (1f - num10 + 0.001f);
		}
		zero3 /= (float)legs.Length;
		zero /= num9;
		if (float.IsNaN(zero.x) || float.IsNaN(zero.y) || float.IsNaN(zero.z))
		{
			zero = position;
		}
		Vector3 referenceHeightVector = zero + up * legC.groundPlaneHeight;
		Vector3 vector7 = up;
		if (groundHugX >= 0f || groundHugZ >= 0f)
		{
			Vector3 lhs = up * 0.1f;
			for (int k = 0; k < legs.Length; k++)
			{
				Vector3 vector8 = array[k] - zero3;
				lhs += Vector3.Cross(Vector3.Cross(vector8, baseUpGround), vector8);
				UnityEngine.Debug.DrawLine(array[k], zero3);
			}
			float num13 = Vector3.Dot(lhs, up);
			if (num13 > 0f)
			{
				lhs /= num13;
				baseUpGround = lhs;
			}
			vector7 = ((!(groundHugX >= 1f) || !(groundHugZ >= 1f)) ? (up + groundHugX * Vector3.Project(baseUpGround, vector) + groundHugZ * Vector3.Project(baseUpGround, forward)).normalized : baseUpGround.normalized);
		}
		Vector3 vector9 = up;
		if (zero2 != Vector3.zero)
		{
			vector9 = Vector3.Cross(zero2, Vector3.Cross(up, zero2));
		}
		vector9 /= Vector3.Dot(vector9, up);
		Vector3 vector10 = Vector3.zero;
		if (accelerateTiltAmount * accelerateTiltSensitivity != 0f)
		{
			float b = Vector3.Dot(tr.accelerationSmoothed * accelerateTiltSensitivity * accelerateTiltAmount, vector) * (1f - groundHugX);
			float b2 = Vector3.Dot(tr.accelerationSmoothed * accelerateTiltSensitivity * accelerateTiltAmount, forward) * (1f - groundHugZ);
			accelerationTiltX = Mathf.Lerp(accelerationTiltX, b, Time.deltaTime * 10f);
			accelerationTiltZ = Mathf.Lerp(accelerationTiltZ, b2, Time.deltaTime * 10f);
			vector10 = (accelerationTiltX * vector + accelerationTiltZ * forward) * (1f - 1f / (hSpeedSmoothed * accelerateTiltSensitivity + 1f));
		}
		Vector3 vector11 = Vector3.zero;
		if (climbTiltAmount * climbTiltAmount != 0f)
		{
			vector11 = (Vector3.Project(vector9, vector) * (1f - groundHugX) + Vector3.Project(vector9, forward) * (1f - groundHugZ)) * (0f - climbTiltAmount) * (1f - 1f / (hSpeedSmoothed * climbTiltSensitivity + 1f));
		}
		bodyUp = (vector7 + vector10 + vector11).normalized;
		Quaternion quaternion4 = Quaternion.AngleAxis(Vector3.Angle(up, bodyUp), Vector3.Cross(up, bodyUp));
		legsUp = (up + vector10).normalized;
		Quaternion quaternion5 = Quaternion.AngleAxis(Vector3.Angle(up, legsUp), Vector3.Cross(up, legsUp));
		for (int l = 0; l < legs.Length; l++)
		{
			float num14 = Mathf.InverseLerp(legStates[l].liftoffTime, legStates[l].strikeTime, legStates[l].cycleTime);
			float num15 = Mathf.InverseLerp(legStates[l].liftTime, legStates[l].landTime, legStates[l].cycleTime);
			float num16 = 0f;
			int phase;
			if (legStates[l].cycleTime < legStates[l].liftoffTime)
			{
				phase = 0;
				num16 = Mathf.InverseLerp(0f, legStates[l].liftoffTime, legStates[l].cycleTime);
			}
			else if (legStates[l].cycleTime > legStates[l].strikeTime)
			{
				phase = 2;
				num16 = Mathf.InverseLerp(legStates[l].strikeTime, 1f, legStates[l].cycleTime);
			}
			else
			{
				phase = 1;
				num16 = num14;
			}
			Vector3 zero4 = Vector3.zero;
			for (int m = 0; m < legC.motions.Length; m++)
			{
				IMotionAnalyzer motionAnalyzer = legC.motions[m];
				float num17 = motionWeights[m];
				if (num17 > 0f)
				{
					zero4 += motionAnalyzer.GetFlightFootPosition(l, num16, phase) * num17;
				}
			}
			Vector3 stepFromPosition = legStates[l].stepFromPosition;
			Vector3 stepToPosition = legStates[l].stepToPosition;
			Vector3 lhs2 = legStates[l].stepFromMatrix.MultiplyVector(Vector3.up);
			Vector3 lhs3 = legStates[l].stepToMatrix.MultiplyVector(Vector3.up);
			float num18 = Mathf.Sin(zero4.z * (float)Math.PI);
			float t = Mathf.Sin(num14 * (float)Math.PI);
			legStates[l].footBase = stepFromPosition * (1f - zero4.z) + stepToPosition * zero4.z;
			Vector3 vector12 = tr.position + tr.rotation * legStates[l].stancePosition - Vector3.Lerp(stepFromPosition, stepToPosition, legStates[l].cycleTime);
			legStates[l].footBase += Util.ProjectOntoPlane(vector12 * num18, legsUp);
			Vector3 vector13 = (stepFromPosition + stepToPosition) / 2f;
			float a = Vector3.Dot(lhs2, stepFromPosition - vector13) / Vector3.Dot(lhs2, legsUp);
			float b3 = Vector3.Dot(lhs3, stepToPosition - vector13) / Vector3.Dot(lhs3, legsUp);
			float num19 = Mathf.Max(a, b3) * 2f / (float)Math.PI;
			legStates[l].footBase += Mathf.Max(0f, num19 * num18 - zero4.y * scale) * legsUp;
			Quaternion quaternion6 = Quaternion.Slerp(Util.QuaternionFromMatrix(legStates[l].stepFromMatrix), Util.QuaternionFromMatrix(legStates[l].stepToMatrix), num14);
			if ((double)num15 < 0.5)
			{
				legStates[l].footBaseRotation = Quaternion.Slerp(Util.QuaternionFromMatrix(legStates[l].stepFromMatrix), rotation, num15 * 2f);
			}
			else
			{
				legStates[l].footBaseRotation = Quaternion.Slerp(rotation, Util.QuaternionFromMatrix(legStates[l].stepToMatrix), num15 * 2f - 1f);
			}
			float num20 = Quaternion.Angle(rotation, legStates[l].footBaseRotation);
			if (num20 > maxFootRotationAngle)
			{
				legStates[l].footBaseRotation = Quaternion.Slerp(rotation, legStates[l].footBaseRotation, maxFootRotationAngle / num20);
			}
			legStates[l].footBaseRotation = Quaternion.FromToRotation(legStates[l].footBaseRotation * Vector3.up, quaternion6 * Vector3.up) * legStates[l].footBaseRotation;
			legStates[l].footBase += zero4.y * legsUp * scale;
			Vector3 normalized = Vector3.Cross(legsUp, stepToPosition - stepFromPosition).normalized;
			legStates[l].footBase += zero4.x * normalized * scale;
			Vector3 vector14 = Vector3.Lerp(legStates[l].footBase, Util.SetHeight(legStates[l].footBase, referenceHeightVector, legsUp), t);
			if (Vector3.Dot(vector14, legsUp) > Vector3.Dot(legStates[l].footBase, legsUp))
			{
				legStates[l].footBase = vector14;
			}
			UnityEngine.Debug.DrawLine(legStates[l].footBase, legStates[l].footBase + legStates[l].footBaseRotation * legStates[l].heelToetipVector, legs[l].debugColor);
		}
		for (int n = 0; n < legs.Length; n++)
		{
			Vector3 vector15 = -MotionAnalyzer.GetHeelOffset(legs[n].ankle, legs[n].ankleHeelVector, legs[n].toe, legs[n].toeToetipVector, legStates[n].heelToetipVector, legStates[n].footBaseRotation) + legs[n].ankle.TransformPoint(legs[n].ankleHeelVector);
			if (locomotionWeight < 1f)
			{
				legStates[n].footBase = Vector3.Lerp(vector15, legStates[n].footBase, locomotionWeight);
				legStates[n].footBaseRotation = Quaternion.Slerp(rotation, legStates[n].footBaseRotation, locomotionWeight);
			}
			legStates[n].footBase = Vector3.MoveTowards(vector15, legStates[n].footBase, maxIKAdjustmentDistance);
		}
		legC.root.transform.rotation = tr.rotation * Quaternion.Inverse(base.transform.rotation) * quaternion4 * legC.root.transform.rotation;
		for (int num21 = 0; num21 < legs.Length; num21++)
		{
			legs[num21].hip.rotation = quaternion5 * Quaternion.Inverse(quaternion4) * legs[num21].hip.rotation;
		}
		Vector3 vector16 = legC.root.transform.position;
		Vector3 vector17 = base.transform.TransformPoint(legC.hipAverage);
		Vector3 vector18 = base.transform.TransformPoint(legC.hipAverageGround);
		Vector3 vector19 = vector16;
		vector19 += quaternion4 * (vector16 - vector17) - (vector16 - vector17);
		vector19 += quaternion5 * (vector17 - vector18) - (vector17 - vector18);
		legC.root.transform.position = vector19 + position - base.transform.position;
		for (int num22 = 0; num22 < legs.Length; num22++)
		{
			legStates[num22].hipReference = legs[num22].hip.position;
			legStates[num22].ankleReference = legs[num22].ankle.position;
		}
		for (int num23 = 1; num23 <= 2; num23++)
		{
			for (int num24 = 0; num24 < legs.Length; num24++)
			{
				legStates[num24].ankle = MotionAnalyzer.GetAnklePosition(legs[num24].ankle, legs[num24].ankleHeelVector, legs[num24].toe, legs[num24].toeToetipVector, legStates[num24].heelToetipVector, legStates[num24].footBase, legStates[num24].footBaseRotation);
			}
			FindHipOffset();
			for (int num25 = 0; num25 < legs.Length; num25++)
			{
				AdjustLeg(num25, legStates[num25].ankle, num23 == 2);
			}
		}
		for (int num26 = 0; num26 < legs.Length; num26++)
		{
			for (int num27 = 0; num27 < legs[num26].legChain.Length - 1; num27++)
			{
				UnityEngine.Debug.DrawLine(legs[num26].legChain[num27].position, legs[num26].legChain[num27 + 1].position, legs[num26].debugColor);
			}
		}
		Vector3 vector20 = position;
		UnityEngine.Debug.DrawRay(vector20, up / 10f, Color.white);
		UnityEngine.Debug.DrawRay(vector20 - forward / 20f, forward / 10f, Color.white);
		UnityEngine.Debug.DrawLine(vector17, vector18, Color.white);
		UnityEngine.Debug.DrawRay(vector20, vector7 * 2f, Color.blue);
		UnityEngine.Debug.DrawRay(vector17, bodyUp * 2f, Color.yellow);
		base.gameObject.layer = layer;
	}

	public Matrix4x4 FindGroundedBase(Vector3 pos, Quaternion rot, Vector3 heelToToetipVector, bool avoidLedges)
	{
		Vector3 vector = default(Vector3);
		Vector3 vector2 = default(Vector3);
		Vector3 vector3 = default(Vector3);
		Vector3 vector4 = default(Vector3);
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if (isActive && Physics.Raycast(pos + up * maxStepHeight, -up, out var hitInfo, maxStepHeight * 2f, groundLayers))
		{
			flag3 = true;
			vector = hitInfo.point;
			if (Vector3.Angle(hitInfo.normal, up) < maxSlopeAngle)
			{
				vector3 = hitInfo.normal;
				flag = true;
			}
		}
		Vector3 vector5 = rot * heelToToetipVector;
		float magnitude = vector5.magnitude;
		if (isActive && Physics.Raycast(pos + up * maxStepHeight + vector5, -up, out hitInfo, maxStepHeight * 2f, groundLayers))
		{
			flag3 = true;
			vector2 = hitInfo.point;
			if (Vector3.Angle(hitInfo.normal, up) < maxSlopeAngle)
			{
				vector4 = hitInfo.normal;
				flag2 = true;
			}
		}
		if (!flag3)
		{
			Matrix4x4 identity = Matrix4x4.identity;
			identity.SetTRS(pos, rot, Vector3.one);
			return identity;
		}
		bool flag4 = false;
		if (avoidLedges)
		{
			if (!flag && !flag2)
			{
				flag = true;
			}
			else if (flag && flag2)
			{
				Vector3 normalized = (vector3 + vector4).normalized;
				float num = Vector3.Dot(vector, normalized);
				float num2 = Vector3.Dot(vector2, normalized);
				if (num >= num2)
				{
					flag2 = false;
				}
				else
				{
					flag = false;
				}
				if (Mathf.Abs(num - num2) > magnitude / 4f)
				{
					flag4 = true;
				}
			}
			else
			{
				flag4 = true;
			}
		}
		Vector3 fromDirection = rot * Vector3.up;
		Vector3 p;
		if (flag)
		{
			if (vector3 != Vector3.zero)
			{
				rot = Quaternion.FromToRotation(fromDirection, vector3) * rot;
			}
			p = vector;
			if (flag4)
			{
				vector5 = rot * heelToToetipVector;
				p -= vector5 * 0.5f;
			}
		}
		else
		{
			if (vector4 != Vector3.zero)
			{
				rot = Quaternion.FromToRotation(fromDirection, vector4) * rot;
			}
			vector5 = rot * heelToToetipVector;
			p = vector2 - vector5;
			if (flag4)
			{
				p += vector5 * 0.5f;
			}
		}
		return Util.MatrixFromQuaternionPosition(rot, p);
	}

	public void FindHipOffset()
	{
		float num = float.PositiveInfinity;
		float num2 = float.PositiveInfinity;
		float num3 = 0f;
		for (int i = 0; i < legs.Length; i++)
		{
			Vector3 v = legStates[i].ankleReference - legStates[i].hipReference;
			float magnitude = v.magnitude;
			float magnitude2 = Util.ProjectOntoPlane(v, legsUp).magnitude;
			Vector3 vector = Util.ProjectOntoPlane(legStates[i].ankle - legs[i].hip.position, legsUp);
			float num4 = vector.magnitude - magnitude2;
			if (num4 > 0f)
			{
				float num5 = legs[i].legLength * scale * 0.999f - magnitude2;
				legStates[i].ankle = legStates[i].ankle - vector + vector.normalized * (magnitude2 + (1f - 1f / (num4 / num5 + 1f)) * num5);
			}
			float[] lineSphereIntersections = Util.GetLineSphereIntersections(Vector3.zero, legsUp, legStates[i].ankle - legs[i].hip.position, magnitude);
			float num6 = ((lineSphereIntersections == null) ? Vector3.Dot(legStates[i].footBase - legs[i].hip.position, legsUp) : lineSphereIntersections[1]);
			lineSphereIntersections = Util.GetLineSphereIntersections(Vector3.zero, legsUp, legStates[i].ankle - legs[i].hip.position, legs[i].legLength * scale * 0.999f);
			float num7 = ((lineSphereIntersections == null) ? Vector3.Dot(legStates[i].ankle - legs[i].hip.position, legsUp) : lineSphereIntersections[1]);
			if (num6 < num)
			{
				num = num6;
			}
			if (num7 < num2)
			{
				num2 = num7;
			}
			num3 += num6 / (float)legs.Length;
		}
		if (num > num2)
		{
			num = num2;
		}
		float num8 = num3 - num;
		float num9 = num2 - num;
		float num10 = num;
		if (num8 + num9 > 0f)
		{
			num10 += num8 * num9 / (num8 + num9);
		}
		legC.root.position += num10 * legsUp;
	}

	public void AdjustLeg(int leg, Vector3 desiredAnklePosition, bool secondPass)
	{
		LegInfo legInfo = legs[leg];
		LegState legState = legStates[leg];
		Quaternion b;
		if (!secondPass)
		{
			Quaternion quaternion = legStates[leg].footBaseRotation * Quaternion.Inverse(rotation);
			b = quaternion * legInfo.ankle.rotation;
		}
		else
		{
			b = legInfo.ankle.rotation;
		}
		IKSolver iKSolver = ((legInfo.legChain.Length != 3) ? ((IKSolver)new IKSimple()) : ((IKSolver)new IK1JointAnalytic()));
		iKSolver.Solve(legInfo.legChain, desiredAnklePosition);
		Vector3 vector = legInfo.hip.position;
		Vector3 vector2 = legInfo.ankle.position;
		if (!secondPass)
		{
			Quaternion a = Quaternion.FromToRotation(forward, Util.ProjectOntoPlane(legStates[leg].footBaseRotation * Vector3.forward, up)) * legInfo.ankle.rotation;
			legInfo.ankle.rotation = Quaternion.Slerp(a, b, 1f - legState.GetFootGrounding(legState.cycleTime));
			return;
		}
		Vector3 normal = vector2 - vector;
		Quaternion quaternion2 = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(Util.ProjectOntoPlane(forward, normal), Util.ProjectOntoPlane(legStates[leg].footBaseRotation * Vector3.forward, normal)), 0.5f);
		legInfo.hip.rotation = quaternion2 * legInfo.hip.rotation;
		legInfo.ankle.rotation = b;
	}

	private void OnRenderObject()
	{
		CreateLineMaterial();
		lineMaterial.SetPass(0);
		if (isActive)
		{
			if (renderFootMarkers)
			{
				RenderFootMarkers();
			}
			if (renderBlendingGraph)
			{
				RenderBlendingGraph();
			}
			if (renderCycleGraph)
			{
				RenderCycleGraph();
			}
		}
	}

	private void CreateLineMaterial()
	{
		if (!lineMaterial)
		{
			lineMaterial = new Material(Shader.Find("Lines/Colored Blended"));
			lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	private void DrawLine(Vector3 a, Vector3 b, Color color)
	{
		GL.Color(color);
		GL.Vertex(a);
		GL.Vertex(b);
	}

	public void RenderBlendingGraph()
	{
		Matrix4x4 matrix = Util.CreateMatrix(base.transform.right, base.transform.forward, base.transform.up, base.transform.TransformPoint(legC.hipAverage));
		float num = (Camera.main.transform.position - base.transform.TransformPoint(legC.hipAverage)).magnitude / 2f;
		DrawArea drawArea = new DrawArea3D(new Vector3(0f - num, 0f - num, 0f), new Vector3(num, num, 0f), matrix);
		GL.Begin(7);
		drawArea.DrawRect(new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 0f), new Color(0f, 0f, 0f, 0.2f));
		GL.End();
		Color c = new Color(0.7f, 0.7f, 0.7f, 1f);
		float num2 = 0f;
		for (int i = 0; i < legC.motions.Length; i++)
		{
			IMotionAnalyzer motionAnalyzer = legC.motions[i];
			num2 = Mathf.Max(num2, Mathf.Abs(motionAnalyzer.cycleVelocity.x));
			num2 = Mathf.Max(num2, Mathf.Abs(motionAnalyzer.cycleVelocity.z));
		}
		num2 = ((num2 != 0f) ? (num2 * 1.2f) : 1f);
		GL.Begin(1);
		drawArea.DrawLine(new Vector3(0.5f, 0f, 0f), new Vector3(0.5f, 1f, 0f), c);
		drawArea.DrawLine(new Vector3(0f, 0.5f, 0f), new Vector3(1f, 0.5f, 0f), c);
		drawArea.DrawLine(new Vector3(0f, 0f, 0f), new Vector3(1f, 0f, 0f), c);
		drawArea.DrawLine(new Vector3(1f, 0f, 0f), new Vector3(1f, 1f, 0f), c);
		drawArea.DrawLine(new Vector3(1f, 1f, 0f), new Vector3(0f, 1f, 0f), c);
		drawArea.DrawLine(new Vector3(0f, 1f, 0f), new Vector3(0f, 0f, 0f), c);
		GL.End();
		float num3;
		float num4;
		for (int j = 0; j < motionGroupStates.Length; j++)
		{
			Vector3 vector = Quaternion.AngleAxis(((float)j + 0.5f) * 360f / (float)motionGroupStates.Length, Vector3.one) * Vector3.right;
			Color color = new Color(vector.x, vector.y, vector.z);
			IMotionAnalyzer[] motions = legC.motionGroups[j].motions;
			GL.Begin(7);
			Color c2 = color * 0.4f;
			c2.a = 0.8f;
			foreach (IMotionAnalyzer motionAnalyzer2 in motions)
			{
				num3 = motionAnalyzer2.cycleVelocity.x / num2 / 2f + 0.5f;
				num4 = motionAnalyzer2.cycleVelocity.z / num2 / 2f + 0.5f;
				float num5 = 0.02f;
				drawArea.DrawDiamond(new Vector3(num3 - num5, num4 - num5, 0f), new Vector3(num3 + num5, num4 + num5, 0f), c2);
			}
			GL.End();
			if (motionGroupStates[j].weight != 0f)
			{
				float[] relativeWeights = motionGroupStates[j].relativeWeights;
				GL.Begin(7);
				color.a = motionGroupStates[j].weight;
				for (int l = 0; l < motions.Length; l++)
				{
					IMotionAnalyzer motionAnalyzer3 = motions[l];
					num3 = motionAnalyzer3.cycleVelocity.x / num2 / 2f + 0.5f;
					num4 = motionAnalyzer3.cycleVelocity.z / num2 / 2f + 0.5f;
					float num6 = Mathf.Pow(relativeWeights[l], 0.5f) * 0.05f;
					drawArea.DrawRect(new Vector3(num3 - num6, num4 - num6, 0f), new Vector3(num3 + num6, num4 + num6, 0f), color);
				}
				GL.End();
			}
		}
		GL.Begin(7);
		num3 = objectVelocity.x / num2 / 2f + 0.5f;
		num4 = objectVelocity.z / num2 / 2f + 0.5f;
		float num7 = 0.02f;
		drawArea.DrawRect(new Vector3(num3 - num7, num4 - num7, 0f), new Vector3(num3 + num7, num4 + num7, 0f), new Color(0f, 0f, 0f, 1f));
		num7 /= 2f;
		drawArea.DrawRect(new Vector3(num3 - num7, num4 - num7, 0f), new Vector3(num3 + num7, num4 + num7, 0f), new Color(1f, 1f, 1f, 1f));
		GL.End();
	}

	public void RenderCycleGraph()
	{
		float num = Camera.main.pixelWidth;
		float num2 = Camera.main.pixelHeight;
		Color c = new Color(0.7f, 0.7f, 0.7f, 1f);
		DrawArea drawArea = new DrawArea(new Vector3(num - 0.49f * num2, 0.01f * num2, 0f), new Vector3(num - 0.01f * num2, 0.49f * num2, 0f));
		drawArea.canvasMin = new Vector3(-1.1f, -1.1f, 0f);
		drawArea.canvasMax = new Vector3(1.1f, 1.1f, 0f);
		GL.Begin(7);
		drawArea.DrawRect(new Vector3(-1f, -1f, 0f), new Vector3(1f, 1f, 0f), new Color(0f, 0f, 0f, 0.2f));
		GL.End();
		GL.Begin(1);
		drawArea.DrawLine(-0.9f * Vector3.up, -1.1f * Vector3.up, c);
		for (int i = 0; i < 90; i++)
		{
			drawArea.DrawLine(Quaternion.AngleAxis((float)i / 90f * 360f, Vector3.forward) * Vector3.up, Quaternion.AngleAxis((float)(i + 1) / 90f * 360f, Vector3.forward) * Vector3.up, c);
		}
		for (int j = 0; j < legs.Length; j++)
		{
			Color debugColor = legs[j].debugColor;
			Vector3 vector = Quaternion.AngleAxis(360f * legStates[j].liftoffTime, -Vector3.forward) * -Vector3.up;
			drawArea.DrawLine(vector * 0.9f, vector * 1.1f, debugColor);
			vector = Quaternion.AngleAxis(360f * legStates[j].strikeTime, -Vector3.forward) * -Vector3.up;
			drawArea.DrawLine(vector * 0.9f, vector * 1.1f, debugColor);
		}
		GL.End();
		GL.Begin(7);
		for (int k = 0; k < legs.Length; k++)
		{
			Color debugColor2 = legs[k].debugColor;
			Vector3 vector2 = Quaternion.AngleAxis(360f * legStates[k].cycleTime, -Vector3.forward) * -Vector3.up;
			drawArea.DrawRect(vector2 - Vector3.one * 0.1f, vector2 + Vector3.one * 0.1f, debugColor2);
			float num3 = Util.CyclicDiff(normalizedTime, legStates[k].stanceTime);
			vector2 = Quaternion.AngleAxis(360f * num3, -Vector3.forward) * -Vector3.up * 0.8f;
			drawArea.DrawRect(vector2 - Vector3.one * 0.05f, vector2 + Vector3.one * 0.05f, debugColor2);
		}
		GL.End();
	}

	public void RenderMotionCycles()
	{
		float num = Camera.main.pixelHeight;
		Color c = new Color(0.7f, 0.7f, 0.7f, 1f);
		for (int i = 0; i < legC.cycleMotions.Length; i++)
		{
			DrawArea drawArea = new DrawArea(new Vector3(0.01f * num + 0.2f * num * (float)i, 0.31f * num, 0f), new Vector3(0.19f * num + 0.2f * num * (float)i, 0.49f * num, 0f));
			drawArea.canvasMin = new Vector3(-1.1f, -1.1f, 0f);
			drawArea.canvasMax = new Vector3(1.1f, 1.1f, 0f);
			GL.Begin(1);
			drawArea.DrawLine(-0.9f * Vector3.up, -1.1f * Vector3.up, c);
			for (int j = 0; j < 90; j++)
			{
				drawArea.DrawLine(Quaternion.AngleAxis((float)j / 90f * 360f, Vector3.forward) * Vector3.up, Quaternion.AngleAxis((float)(j + 1) / 90f * 360f, Vector3.forward) * Vector3.up, c);
			}
			GL.End();
			GL.Begin(7);
			for (int k = 0; k < legs.Length; k++)
			{
				Color debugColor = legs[k].debugColor;
				float num2 = legC.cycleMotions[i].cycles[k].stanceTime - legC.cycleMotions[i].cycleOffset * (0.5f + 0.5f * Mathf.Sin(Time.time * 2f));
				Vector3 vector = Quaternion.AngleAxis(360f * num2, -Vector3.forward) * -Vector3.up;
				drawArea.DrawRect(vector - Vector3.one * 0.1f, vector + Vector3.one * 0.1f, debugColor);
			}
			GL.End();
		}
	}

	public void RenderFootMarkers()
	{
		GL.Begin(1);
		GL.End();
		GL.Begin(7);
		for (int i = 0; i < legs.Length; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (legStates[i] != null)
				{
					Matrix4x4 matrix4x;
					switch (j)
					{
					case 0:
						matrix4x = legStates[i].stepFromMatrix;
						GL.Color(legs[i].debugColor * 0.8f);
						break;
					case 1:
						matrix4x = legStates[i].stepToMatrix;
						GL.Color(legs[i].debugColor);
						break;
					default:
						matrix4x = legStates[i].stepToMatrix;
						GL.Color(legs[i].debugColor * 0.4f);
						break;
					}
					Vector3 vector = matrix4x.MultiplyPoint3x4(Vector3.zero);
					Vector3 vector2 = matrix4x.MultiplyVector(legStates[i].heelToetipVector);
					Vector3 axis = matrix4x.MultiplyVector(Vector3.up);
					Vector3 vector3 = (Quaternion.AngleAxis(90f, axis) * vector2).normalized * legs[i].footWidth * scale;
					vector += axis.normalized * vector3.magnitude / 20f;
					if (j == 2)
					{
						vector += legStates[i].stepToPositionGoal - legStates[i].stepToPosition;
					}
					GL.Vertex(vector + vector3 / 2f);
					GL.Vertex(vector - vector3 / 2f);
					GL.Vertex(vector - vector3 / 4f + vector2);
					GL.Vertex(vector + vector3 / 4f + vector2);
				}
			}
		}
		GL.End();
	}

	private void OnGUI()
	{
		if (renderAnimationStates)
		{
			RenderAnimationStates();
		}
	}

	public void RenderAnimationStates()
	{
		int num = 0;
		foreach (AnimationState item in GetComponent<Animation>())
		{
			string text = item.name;
			float num2 = 0.5f + 0.5f * item.weight;
			GUI.color = new Color(0f, 0f, num2, 1f);
			if (item.enabled)
			{
				GUI.color = new Color(num2, num2, num2, 1f);
			}
			text = text + " " + item.weight.ToString("0.000");
			GUI.Label(new Rect(Screen.width - 200, 10 + 20 * num, 200f, 30f), text);
			num++;
		}
	}

	private void MonitorFootsteps()
	{
		if (legStates == null)
		{
			return;
		}
		for (int i = 0; i < legStates.Length; i++)
		{
			LegState legState = legStates[i];
			switch (legState.phase)
			{
			case LegCyclePhase.Stance:
				if (legState.cycleTime >= legState.liftTime && legState.cycleTime < legState.landTime)
				{
					legState.phase = LegCyclePhase.Lift;
					SendMessage("OnFootLift", SendMessageOptions.DontRequireReceiver);
				}
				break;
			case LegCyclePhase.Lift:
				if (legState.cycleTime >= legState.liftoffTime || legState.cycleTime < legState.liftTime)
				{
					legState.phase = LegCyclePhase.Flight;
					SendMessage("OnFootLiftoff", SendMessageOptions.DontRequireReceiver);
				}
				break;
			case LegCyclePhase.Flight:
				if (legState.cycleTime >= legState.strikeTime || legState.cycleTime < legState.liftoffTime)
				{
					legState.phase = LegCyclePhase.Land;
					SendMessage("OnFootStrike", SendMessageOptions.DontRequireReceiver);
				}
				break;
			case LegCyclePhase.Land:
				if (legState.cycleTime >= legState.landTime || legState.cycleTime < legState.strikeTime)
				{
					legState.phase = LegCyclePhase.Stance;
					SendMessage("OnFootLand", SendMessageOptions.DontRequireReceiver);
				}
				break;
			}
		}
	}
}
