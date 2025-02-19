using System;
using UnityEngine;

[Serializable]
public class MotionAnalyzer : IMotionAnalyzer
{
	private delegate Vector3 GetVector3Member(LegCycleSample s);

	private delegate float GetFloatMember(LegCycleSample s);

	public bool alsoUseBackwards;

	public bool fixFootSkating;

	[HideInInspector]
	public LegCycleData[] m_cycles;

	[HideInInspector]
	public int m_samples;

	[HideInInspector]
	public Vector3 m_cycleDirection;

	[HideInInspector]
	public float m_cycleDistance;

	[HideInInspector]
	public float m_cycleDuration;

	[HideInInspector]
	public float m_cycleSpeed;

	public float nativeSpeed;

	[HideInInspector]
	public float m_cycleOffset;

	[HideInInspector]
	public Vector3 graphMin;

	[HideInInspector]
	public Vector3 graphMax;

	[HideInInspector]
	public GameObject gameObject;

	[HideInInspector]
	public int legs;

	[HideInInspector]
	public LegController legC;

	[HideInInspector]
	public Material lineMaterial;

	public override LegCycleData[] cycles => m_cycles;

	public override int samples => m_samples;

	public override Vector3 cycleDirection => m_cycleDirection;

	public override float cycleDistance => m_cycleDistance;

	public override Vector3 cycleVector => m_cycleDirection * m_cycleDistance;

	public override float cycleDuration => m_cycleDuration;

	public override float cycleSpeed => m_cycleSpeed;

	public override Vector3 cycleVelocity => m_cycleDirection * m_cycleSpeed;

	public override float cycleOffset
	{
		get
		{
			return m_cycleOffset;
		}
		set
		{
			m_cycleOffset = value;
		}
	}

	private float FindContactTime(LegCycleData data, bool useToe, int searchDirection, float yRange, float threshold)
	{
		int num = 5;
		float num2 = 0f;
		int num3 = data.stanceIndex;
		for (int i = 0; i < samples && i > -samples; i += searchDirection)
		{
			int[] array = new int[3];
			float[] array2 = new float[3];
			for (int j = 0; j < 3; j++)
			{
				array[j] = Util.Mod(i + data.stanceIndex - num + num * j, samples);
				if (useToe)
				{
					array2[j] = data.samples[array[j]].toetip.y;
				}
				else
				{
					array2[j] = data.samples[array[j]].heel.y;
				}
			}
			float num4 = Mathf.Atan((array2[2] - array2[1]) * 10f / yRange) - Mathf.Atan((array2[1] - array2[0]) * 10f / yRange);
			if (array2[1] > legC.groundPlaneHeight && num4 > num2 && Mathf.Sign(array2[2] - array2[0]) == Mathf.Sign(searchDirection))
			{
				num2 = num4;
				num3 = array[1];
			}
			if (array2[1] > legC.groundPlaneHeight + yRange * threshold)
			{
				break;
			}
		}
		return GetTimeFromIndex(Util.Mod(num3 - data.stanceIndex, samples));
	}

	private float FindSwingChangeTime(LegCycleData data, int searchDirection, float threshold)
	{
		int num = samples / 5;
		float num2 = 0f;
		for (int i = 0; i < samples && i > -samples; i += searchDirection)
		{
			int[] array = new int[3];
			float[] array2 = new float[3];
			for (int j = 0; j < 3; j++)
			{
				array[j] = Util.Mod(i + data.stanceIndex - num + num * j, samples);
				array2[j] = Vector3.Dot(data.samples[array[j]].footBase, data.cycleDirection);
			}
			float num3 = array2[2] - array2[0];
			if (i == 0)
			{
				num2 = num3;
			}
			if (Mathf.Abs((num3 - num2) / num2) > threshold)
			{
				return GetTimeFromIndex(Util.Mod(array[1] - data.stanceIndex, samples));
			}
		}
		return Util.Mod((float)searchDirection * -0.01f);
	}

	private float GetFootGrounding(int leg, float time)
	{
		if (time <= cycles[leg].liftTime || time >= cycles[leg].landTime)
		{
			return 0f;
		}
		if (time >= cycles[leg].postliftTime && time <= cycles[leg].prelandTime)
		{
			return 1f;
		}
		if (time < cycles[leg].postliftTime)
		{
			return (time - cycles[leg].liftTime) / (cycles[leg].postliftTime - cycles[leg].liftTime);
		}
		return 1f - (time - cycles[leg].prelandTime) / (cycles[leg].landTime - cycles[leg].prelandTime);
	}

	private float GetFootGroundingOrig(int leg, float time)
	{
		if (time <= cycles[leg].liftTime || time >= cycles[leg].landTime)
		{
			return 0f;
		}
		if (time >= cycles[leg].liftoffTime && time <= cycles[leg].strikeTime)
		{
			return 1f;
		}
		if (time < cycles[leg].liftoffTime)
		{
			return (time - cycles[leg].liftTime) / (cycles[leg].liftoffTime - cycles[leg].liftTime);
		}
		return 1f - (time - cycles[leg].strikeTime) / (cycles[leg].landTime - cycles[leg].strikeTime);
	}

	private int GetIndexFromTime(float time)
	{
		return Util.Mod((int)((double)(time * (float)samples) + 0.5), samples);
	}

	private float GetTimeFromIndex(int index)
	{
		return (float)index * 1f / (float)samples;
	}

	private Vector3 FootPositionNormalized(LegCycleSample s)
	{
		return s.footBaseNormalized;
	}

	private Vector3 FootPosition(LegCycleSample s)
	{
		return s.footBase;
	}

	private float Balance(LegCycleSample s)
	{
		return s.balance;
	}

	private Vector3 GetVector3AtTime(int leg, float flightTime, GetVector3Member get)
	{
		flightTime = Mathf.Clamp01(flightTime);
		int num = (int)(flightTime * (float)samples);
		float num2 = flightTime * (float)samples - (float)num;
		if (num >= samples - 1)
		{
			num = samples - 1;
			num2 = 0f;
		}
		num = Util.Mod(num + cycles[leg].stanceIndex, samples);
		return get(cycles[leg].samples[num]) * (1f - num2) + get(cycles[leg].samples[Util.Mod(num + 1, samples)]) * num2;
	}

	private float GetFloatAtTime(int leg, float flightTime, GetFloatMember get)
	{
		flightTime = Mathf.Clamp01(flightTime);
		int num = (int)(flightTime * (float)samples);
		float num2 = flightTime * (float)samples - (float)num;
		if (num >= samples - 1)
		{
			num = samples - 1;
			num2 = 0f;
		}
		num = Util.Mod(num + cycles[leg].stanceIndex, samples);
		return get(cycles[leg].samples[num]) * (1f - num2) + get(cycles[leg].samples[Util.Mod(num + 1, samples)]) * num2;
	}

	public override Vector3 GetFlightFootPosition(int leg, float flightTime, int phase)
	{
		if (motionType != 0)
		{
			switch (phase)
			{
			case 0:
				return Vector3.zero;
			case 1:
				return ((0f - Mathf.Cos(flightTime * (float)Math.PI)) / 2f + 0.5f) * Vector3.forward;
			case 2:
				return Vector3.forward;
			}
		}
		float num = 0f;
		num = phase switch
		{
			0 => Mathf.Lerp(0f, cycles[leg].liftoffTime, flightTime), 
			1 => Mathf.Lerp(cycles[leg].liftoffTime, cycles[leg].strikeTime, flightTime), 
			_ => Mathf.Lerp(cycles[leg].strikeTime, 1f, flightTime), 
		};
		int num2 = (int)(num * (float)samples);
		float num3 = num * (float)samples - (float)num2;
		if (num2 >= samples - 1)
		{
			num2 = samples - 1;
			num3 = 0f;
		}
		num2 = Util.Mod(num2 + cycles[leg].stanceIndex, samples);
		return cycles[leg].samples[num2].footBaseNormalized * (1f - num3) + cycles[leg].samples[Util.Mod(num2 + 1, samples)].footBaseNormalized * num3;
	}

	public static Vector3 GetHeelOffset(Transform ankleT, Vector3 ankleHeelVector, Transform toeT, Vector3 toeToetipVector, Vector3 stanceFootVector, Quaternion footBaseRotation)
	{
		Vector3 vector = ankleT.localToWorldMatrix.MultiplyPoint(ankleHeelVector);
		Vector3 vector2 = toeT.localToWorldMatrix.MultiplyPoint(toeToetipVector);
		float footBalance = GetFootBalance((Quaternion.Inverse(footBaseRotation) * vector).y, (Quaternion.Inverse(footBaseRotation) * vector2).y, stanceFootVector.magnitude);
		return footBalance * (footBaseRotation * stanceFootVector + (vector - vector2));
	}

	public static Vector3 GetAnklePosition(Transform ankleT, Vector3 ankleHeelVector, Transform toeT, Vector3 toeToetipVector, Vector3 stanceFootVector, Vector3 footBasePosition, Quaternion footBaseRotation)
	{
		Vector3 heelOffset = GetHeelOffset(ankleT, ankleHeelVector, toeT, toeToetipVector, stanceFootVector, footBaseRotation);
		return footBasePosition + heelOffset + ankleT.localToWorldMatrix.MultiplyVector(ankleHeelVector * -1f);
	}

	public static float GetFootBalance(float heelElevation, float toeElevation, float footLength)
	{
		return Mathf.Atan((heelElevation - toeElevation) / footLength * 20f) / (float)Math.PI + 0.5f;
	}

	private void FindCycleAxis(int leg)
	{
		cycles[leg].cycleCenter = Vector3.zero;
		for (int i = 0; i < samples; i++)
		{
			LegCycleSample legCycleSample = cycles[leg].samples[i];
			cycles[leg].cycleCenter += Util.ProjectOntoPlane(legCycleSample.middle, Vector3.up);
		}
		cycles[leg].cycleCenter /= (float)samples;
		Vector3 vector = cycles[leg].cycleCenter;
		float num = 0f;
		for (int j = 0; j < samples; j++)
		{
			LegCycleSample legCycleSample2 = cycles[leg].samples[j];
			Vector3 vector2 = Util.ProjectOntoPlane(legCycleSample2.middle, Vector3.up);
			float magnitude = (vector2 - cycles[leg].cycleCenter).magnitude;
			if (magnitude > num)
			{
				vector = vector2;
				num = magnitude;
			}
		}
		Vector3 vector3 = vector;
		num = 0f;
		for (int k = 0; k < samples; k++)
		{
			LegCycleSample legCycleSample3 = cycles[leg].samples[k];
			Vector3 vector4 = Util.ProjectOntoPlane(legCycleSample3.middle, Vector3.up);
			float magnitude2 = (vector4 - vector).magnitude;
			if (magnitude2 > num)
			{
				vector3 = vector4;
				num = magnitude2;
			}
		}
		cycles[leg].cycleDirection = (vector3 - vector).normalized;
		cycles[leg].cycleScaling = (vector3 - vector).magnitude;
	}

	public override void Analyze(GameObject o)
	{
		Debug.Log("Starting analysis");
		gameObject = o;
		name = animation.name;
		m_samples = 50;
		legC = gameObject.GetComponent(typeof(LegController)) as LegController;
		legs = legC.legs.Length;
		m_cycles = new LegCycleData[legs];
		for (int i = 0; i < legs; i++)
		{
			cycles[i] = new LegCycleData();
			cycles[i].samples = new LegCycleSample[samples + 1];
			for (int j = 0; j < samples + 1; j++)
			{
				cycles[i].samples[j] = new LegCycleSample();
			}
			cycles[i].debugInfo = new CycleDebugInfo();
		}
		graphMin = new Vector3(0f, 1000f, 1000f);
		graphMax = new Vector3(0f, -1000f, -1000f);
		for (int k = 0; k < legs; k++)
		{
			Transform ankle = legC.legs[k].ankle;
			Transform toe = legC.legs[k].toe;
			float num = 0f;
			float num2 = 1000f;
			float num3 = -1000f;
			float num4 = 1000f;
			float num5 = -1000f;
			for (int l = 0; l < samples + 1; l++)
			{
				LegCycleSample legCycleSample = cycles[k].samples[l];
				animation.SampleAnimation(gameObject, (float)l * 1f / (float)samples * animation.length);
				legCycleSample.ankleMatrix = Util.RelativeMatrix(ankle, gameObject.transform);
				legCycleSample.toeMatrix = Util.RelativeMatrix(toe, gameObject.transform);
				legCycleSample.heel = legCycleSample.ankleMatrix.MultiplyPoint(legC.legs[k].ankleHeelVector);
				legCycleSample.toetip = legCycleSample.toeMatrix.MultiplyPoint(legC.legs[k].toeToetipVector);
				legCycleSample.middle = (legCycleSample.heel + legCycleSample.toetip) / 2f;
				legCycleSample.balance = GetFootBalance(legCycleSample.heel.y, legCycleSample.toetip.y, legC.legs[k].footLength);
				num2 = Mathf.Min(num2, legCycleSample.heel.y);
				num4 = Mathf.Min(num4, legCycleSample.toetip.y);
				num3 = Mathf.Max(num3, legCycleSample.heel.y);
				num5 = Mathf.Max(num5, legCycleSample.toetip.y);
			}
			num = Mathf.Max(num3 - num2, num5 - num4);
			if (motionType == MotionType.WalkCycle)
			{
				FindCycleAxis(k);
				float num6 = float.PositiveInfinity;
				for (int m = 0; m < samples + 1; m++)
				{
					LegCycleSample legCycleSample2 = cycles[k].samples[m];
					float num7 = Mathf.Max(legCycleSample2.heel.y, legCycleSample2.toetip.y) / num + Mathf.Abs(Util.ProjectOntoPlane(legCycleSample2.middle - cycles[k].cycleCenter, Vector3.up).magnitude) / cycles[k].cycleScaling;
					if (num7 < num6)
					{
						cycles[k].stanceIndex = m;
						num6 = num7;
					}
				}
			}
			else
			{
				cycles[k].cycleDirection = Vector3.forward;
				cycles[k].cycleScaling = 0f;
				cycles[k].stanceIndex = 0;
			}
			cycles[k].stanceTime = GetTimeFromIndex(cycles[k].stanceIndex);
			LegCycleSample legCycleSample3 = cycles[k].samples[cycles[k].stanceIndex];
			animation.SampleAnimation(gameObject, cycles[k].stanceTime * animation.length);
			cycles[k].heelToetipVector = legCycleSample3.toeMatrix.MultiplyPoint(legC.legs[k].toeToetipVector) - legCycleSample3.ankleMatrix.MultiplyPoint(legC.legs[k].ankleHeelVector);
			cycles[k].heelToetipVector = Util.ProjectOntoPlane(cycles[k].heelToetipVector, Vector3.up);
			cycles[k].heelToetipVector = cycles[k].heelToetipVector.normalized * legC.legs[k].footLength;
			for (int n = 0; n < samples + 1; n++)
			{
				LegCycleSample legCycleSample4 = cycles[k].samples[n];
				legCycleSample4.footBase = legCycleSample4.heel * (1f - legCycleSample4.balance) + (legCycleSample4.toetip - cycles[k].heelToetipVector) * legCycleSample4.balance;
			}
			cycles[k].stancePosition = legCycleSample3.footBase;
			cycles[k].stancePosition.y = legC.groundPlaneHeight;
			if (motionType == MotionType.WalkCycle)
			{
				float num8 = FindContactTime(cycles[k], useToe: false, 1, num, 0.1f);
				cycles[k].debugInfo.ankleLiftTime = num8;
				float num9 = FindContactTime(cycles[k], useToe: true, 1, num, 0.1f);
				cycles[k].debugInfo.toeLiftTime = num9;
				if (num8 < num9)
				{
					cycles[k].liftTime = num8;
					cycles[k].liftoffTime = num9;
				}
				else
				{
					cycles[k].liftTime = num9;
					cycles[k].liftoffTime = num8;
				}
				num8 = FindSwingChangeTime(cycles[k], 1, 0.5f);
				cycles[k].debugInfo.footLiftTime = num8;
				if (cycles[k].liftoffTime > num8)
				{
					cycles[k].liftoffTime = num8;
					if (cycles[k].liftTime > cycles[k].liftoffTime)
					{
						cycles[k].liftTime = cycles[k].liftoffTime;
					}
				}
				num8 = FindContactTime(cycles[k], useToe: false, -1, num, 0.1f);
				num9 = FindContactTime(cycles[k], useToe: true, -1, num, 0.1f);
				if (num8 < num9)
				{
					cycles[k].strikeTime = num8;
					cycles[k].landTime = num9;
				}
				else
				{
					cycles[k].strikeTime = num9;
					cycles[k].landTime = num8;
				}
				num8 = FindSwingChangeTime(cycles[k], -1, 0.5f);
				cycles[k].debugInfo.footLandTime = num8;
				if (cycles[k].strikeTime < num8)
				{
					cycles[k].strikeTime = num8;
					if (cycles[k].landTime < cycles[k].strikeTime)
					{
						cycles[k].landTime = cycles[k].strikeTime;
					}
				}
				float num10 = 0.2f;
				cycles[k].postliftTime = cycles[k].liftoffTime;
				if (cycles[k].postliftTime < cycles[k].liftTime + num10)
				{
					cycles[k].postliftTime = cycles[k].liftTime + num10;
				}
				cycles[k].prelandTime = cycles[k].strikeTime;
				if (cycles[k].prelandTime > cycles[k].landTime - num10)
				{
					cycles[k].prelandTime = cycles[k].landTime - num10;
				}
				Vector3 vector = cycles[k].samples[GetIndexFromTime(Util.Mod(cycles[k].liftoffTime + cycles[k].stanceTime))].footBase - cycles[k].samples[GetIndexFromTime(Util.Mod(cycles[k].strikeTime + cycles[k].stanceTime))].footBase;
				vector.y = 0f;
				cycles[k].cycleDistance = vector.magnitude / (cycles[k].liftoffTime - cycles[k].strikeTime + 1f);
				cycles[k].cycleDirection = -vector.normalized;
			}
			else
			{
				cycles[k].cycleDirection = Vector3.zero;
				cycles[k].cycleDistance = 0f;
			}
			graphMax.y = Mathf.Max(graphMax.y, Mathf.Max(num3, num5));
		}
		m_cycleDistance = 0f;
		m_cycleDirection = Vector3.zero;
		for (int num11 = 0; num11 < legs; num11++)
		{
			m_cycleDistance += cycles[num11].cycleDistance;
			m_cycleDirection += cycles[num11].cycleDirection;
			Debug.Log(string.Concat("Cycle direction of leg ", num11, " is ", cycles[num11].cycleDirection, " with step distance ", cycles[num11].cycleDistance));
		}
		m_cycleDistance /= legs;
		m_cycleDirection /= (float)legs;
		m_cycleDuration = animation.length;
		m_cycleSpeed = cycleDistance / cycleDuration;
		Debug.Log(string.Concat("Overall cycle direction is ", m_cycleDirection, " with step distance ", m_cycleDistance, " and speed ", m_cycleSpeed));
		nativeSpeed = m_cycleSpeed * gameObject.transform.localScale.x;
		for (int num12 = 0; num12 < legs; num12++)
		{
			if (motionType == MotionType.WalkCycle)
			{
				for (int num13 = 0; num13 < samples; num13++)
				{
					int num14 = Util.Mod(num13 + cycles[num12].stanceIndex, samples);
					LegCycleSample legCycleSample5 = cycles[num12].samples[num14];
					float timeFromIndex = GetTimeFromIndex(num13);
					legCycleSample5.footBaseNormalized = legCycleSample5.footBase;
					if (fixFootSkating)
					{
						Vector3 vector2 = (0f - cycles[num12].cycleDistance) * cycles[num12].cycleDirection * (timeFromIndex - cycles[num12].liftoffTime) + cycles[num12].samples[GetIndexFromTime(cycles[num12].liftoffTime + cycles[num12].stanceTime)].footBase;
						legCycleSample5.footBaseNormalized -= vector2;
						if (cycles[num12].cycleDirection != Vector3.zero)
						{
							legCycleSample5.footBaseNormalized = Quaternion.Inverse(Quaternion.LookRotation(cycles[num12].cycleDirection)) * legCycleSample5.footBaseNormalized;
						}
						legCycleSample5.footBaseNormalized.z /= cycles[num12].cycleDistance;
						if (timeFromIndex <= cycles[num12].liftoffTime)
						{
							legCycleSample5.footBaseNormalized.z = 0f;
						}
						if (timeFromIndex >= cycles[num12].strikeTime)
						{
							legCycleSample5.footBaseNormalized.z = 1f;
						}
						legCycleSample5.footBaseNormalized.y = legCycleSample5.footBase.y - legC.groundPlaneHeight;
					}
					else
					{
						Vector3 vector3 = (0f - m_cycleDistance) * m_cycleDirection * (timeFromIndex - cycles[num12].liftoffTime * 0f) + cycles[num12].samples[GetIndexFromTime(cycles[num12].liftoffTime * 0f + cycles[num12].stanceTime)].footBase;
						legCycleSample5.footBaseNormalized -= vector3;
						if (cycles[num12].cycleDirection != Vector3.zero)
						{
							legCycleSample5.footBaseNormalized = Quaternion.Inverse(Quaternion.LookRotation(m_cycleDirection)) * legCycleSample5.footBaseNormalized;
						}
						legCycleSample5.footBaseNormalized.z /= m_cycleDistance;
						legCycleSample5.footBaseNormalized.y = legCycleSample5.footBase.y - legC.groundPlaneHeight;
					}
				}
				cycles[num12].samples[samples] = cycles[num12].samples[0];
			}
			else
			{
				for (int num15 = 0; num15 < samples; num15++)
				{
					int num16 = Util.Mod(num15 + cycles[num12].stanceIndex, samples);
					LegCycleSample legCycleSample6 = cycles[num12].samples[num16];
					legCycleSample6.footBaseNormalized = legCycleSample6.footBase - cycles[num12].stancePosition;
				}
			}
		}
		for (int num17 = 0; num17 < legs; num17++)
		{
			float num18 = Vector3.Dot(cycles[num17].heelToetipVector, cycleDirection);
			for (int num19 = 0; num19 < samples; num19++)
			{
				LegCycleSample legCycleSample7 = cycles[num17].samples[num19];
				float num20 = Vector3.Dot(legCycleSample7.footBase, cycleDirection);
				if (num20 < graphMin.z)
				{
					graphMin.z = num20;
				}
				if (num20 > graphMax.z)
				{
					graphMax.z = num20;
				}
				if (num20 + num18 < graphMin.z)
				{
					graphMin.z = num20 + num18;
				}
				if (num20 + num18 > graphMax.z)
				{
					graphMax.z = num20 + num18;
				}
			}
		}
		graphMin.y = legC.groundPlaneHeight;
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

	private void DrawRay(Vector3 a, Vector3 b, Color color)
	{
		DrawLine(a, a + b, color);
	}

	private void DrawDiamond(Vector3 a, float size, Color color)
	{
		Vector3 up = Camera.main.transform.up;
		Vector3 right = Camera.main.transform.right;
		GL.Color(color);
		GL.Vertex(a + up * size);
		GL.Vertex(a + right * size);
		GL.Vertex(a - up * size);
		GL.Vertex(a - right * size);
	}

	public void RenderGraph(MotionAnalyzerDrawOptions opt)
	{
		CreateLineMaterial();
		lineMaterial.SetPass(0);
		if (opt.drawAllFeet)
		{
			for (int i = 0; i < legs; i++)
			{
				RenderGraphAll(opt, i);
			}
		}
		else
		{
			RenderGraphAll(opt, opt.currentLeg);
		}
	}

	public void RenderGraphAll(MotionAnalyzerDrawOptions opt, int currentLeg)
	{
		bool drawAllFeet = opt.drawAllFeet;
		bool flag = !drawAllFeet || currentLeg == 0;
		Transform ankle = legC.legs[currentLeg].ankle;
		Transform toe = legC.legs[currentLeg].toe;
		Vector3 vector = graphMax - graphMin;
		Matrix4x4 localToWorldMatrix = gameObject.transform.localToWorldMatrix;
		float num = Util.Mod(gameObject.GetComponent<Animation>()[animation.name].normalizedTime);
		int indexFromTime = GetIndexFromTime(num);
		float timeFromIndex = GetTimeFromIndex(indexFromTime);
		float z = gameObject.transform.localScale.z;
		float num2 = cycleDistance * z;
		float size = vector.z * z * 0.03f;
		Color c = new Color(0.7f, 0.7f, 0.7f, 1f);
		Color color = new Color(0.8f, 0f, 0f, 1f);
		Color color2 = new Color(0f, 0.7f, 0f, 1f);
		Color color3 = new Color(0f, 0f, 0f, 1f);
		Color color4 = new Color(0.7f, 0.7f, 0.7f, 1f);
		if (drawAllFeet)
		{
			color = legC.legs[currentLeg].debugColor;
			color2 = legC.legs[currentLeg].debugColor;
			color3 = legC.legs[currentLeg].debugColor * 0.5f + Color.black * 0.5f;
			color4 = legC.legs[currentLeg].debugColor * 0.5f + Color.white * 0.5f;
		}
		Color color5 = color3;
		color5.a = 0.5f;
		GL.Begin(7);
		Vector3 a = ankle.position + Util.TransformVector(ankle, legC.legs[currentLeg].ankleHeelVector);
		Vector3 a2 = toe.position + Util.TransformVector(toe, legC.legs[currentLeg].toeToetipVector);
		Vector3 a3 = localToWorldMatrix.MultiplyPoint3x4(cycles[currentLeg].samples[indexFromTime].footBase);
		Vector3 vector2 = localToWorldMatrix.MultiplyPoint3x4(cycles[currentLeg].samples[indexFromTime].footBase + cycles[currentLeg].heelToetipVector);
		if (opt.drawHeelToe)
		{
			DrawDiamond(a, size, color);
			DrawDiamond(a2, size, color2);
		}
		if (opt.drawFootBase)
		{
			DrawDiamond(a3, size, color5);
			DrawDiamond(vector2, size, color5);
			GL.End();
			GL.Begin(1);
			DrawLine(a3, vector2, color3);
		}
		GL.End();
		if (opt.drawFootPrints)
		{
			float num3 = Util.Mod(num - cycles[currentLeg].stanceTime + cycleOffset);
			Color c2 = color4 * GetFootGrounding(currentLeg, num3) + color3 * (1f - GetFootGrounding(currentLeg, num3));
			if (currentLeg == currentLeg)
			{
				GL.Begin(7);
			}
			else
			{
				GL.Begin(1);
			}
			GL.Color(c2);
			Vector3 vector3 = cycles[currentLeg].stancePosition + cycleDirection * (0f - cycleDistance) * (Util.Mod(num3 + 0.5f, 1f) - 0.5f);
			Vector3 vector4 = Vector3.up * -0.5f * cycles[currentLeg].heelToetipVector.magnitude;
			GL.Vertex(localToWorldMatrix.MultiplyPoint3x4(vector3 + vector4));
			GL.Vertex(localToWorldMatrix.MultiplyPoint3x4(vector3));
			GL.Vertex(localToWorldMatrix.MultiplyPoint3x4(vector3 + cycles[currentLeg].heelToetipVector));
			GL.Vertex(localToWorldMatrix.MultiplyPoint3x4(vector3 + cycles[currentLeg].heelToetipVector + vector4));
			if (currentLeg != currentLeg)
			{
				GL.Vertex(localToWorldMatrix.MultiplyPoint3x4(vector3));
				GL.Vertex(localToWorldMatrix.MultiplyPoint3x4(vector3 + cycles[currentLeg].heelToetipVector));
				GL.Vertex(localToWorldMatrix.MultiplyPoint3x4(vector3 + cycles[currentLeg].heelToetipVector + vector4));
				GL.Vertex(localToWorldMatrix.MultiplyPoint3x4(vector3 + vector4));
			}
			GL.End();
		}
		if (motionType != 0)
		{
			return;
		}
		if (opt.drawTrajectories)
		{
			GL.Begin(1);
			for (int i = 0; i < samples * 2; i++)
			{
				int num4 = Util.Mod(indexFromTime - i, samples);
				LegCycleSample legCycleSample = cycles[currentLeg].samples[num4];
				LegCycleSample legCycleSample2 = cycles[currentLeg].samples[Util.Mod(num4 - 1, samples)];
				float timeFromIndex2 = GetTimeFromIndex(i);
				float timeFromIndex3 = GetTimeFromIndex(i + 1);
				float num5 = 0f;
				float num6 = 0f;
				if (opt.normalizeGraph)
				{
					num5 = (0f - timeFromIndex2) * cycleDistance;
					num6 = (0f - timeFromIndex3) * cycleDistance;
				}
				Vector3 a4 = localToWorldMatrix.MultiplyPoint3x4(legCycleSample.heel + num5 * cycleDirection);
				Vector3 b = localToWorldMatrix.MultiplyPoint3x4(legCycleSample2.heel + num6 * cycleDirection);
				Vector3 a5 = localToWorldMatrix.MultiplyPoint3x4(legCycleSample.toetip + num5 * cycleDirection);
				Vector3 b2 = localToWorldMatrix.MultiplyPoint3x4(legCycleSample2.toetip + num6 * cycleDirection);
				Vector3 a6 = localToWorldMatrix.MultiplyPoint3x4(legCycleSample.footBase + num5 * cycleDirection);
				Vector3 b3 = localToWorldMatrix.MultiplyPoint3x4(legCycleSample2.footBase + num6 * cycleDirection);
				if (opt.drawHeelToe)
				{
					DrawLine(a4, b, color);
					DrawLine(a5, b2, color2);
				}
				if (opt.drawFootBase)
				{
					DrawLine(a6, b3, (num4 % 2 != 0) ? color4 : color3);
				}
				if (opt.drawTrajectoriesProjected)
				{
					DrawLine(localToWorldMatrix.MultiplyPoint3x4(Util.ProjectOntoPlane(legCycleSample.heel + legCycleSample.toetip, Vector3.up) / 2f + (legC.groundPlaneHeight - vector.y) * Vector3.up), localToWorldMatrix.MultiplyPoint3x4(Util.ProjectOntoPlane(legCycleSample2.heel + legCycleSample2.toetip, Vector3.up) / 2f + (legC.groundPlaneHeight - vector.y) * Vector3.up), color3);
				}
			}
			GL.End();
			if (opt.drawTrajectoriesProjected)
			{
				GL.Begin(7);
				DrawDiamond(localToWorldMatrix.MultiplyPoint3x4(Util.ProjectOntoPlane(cycles[currentLeg].samples[indexFromTime].heel + cycles[currentLeg].samples[indexFromTime].toetip, Vector3.up) / 2f + (legC.groundPlaneHeight - vector.y) * Vector3.up), size, color5);
				GL.End();
				if (opt.drawThreePoints)
				{
					for (int j = -1; j <= 1; j++)
					{
						GL.Begin(7);
						DrawDiamond(localToWorldMatrix.MultiplyPoint3x4(cycles[currentLeg].cycleCenter + (legC.groundPlaneHeight - vector.y) * Vector3.up + j * cycles[currentLeg].cycleDirection * cycles[currentLeg].cycleScaling * 0.5f), size, color5);
						GL.End();
					}
					GL.Begin(1);
					DrawLine(localToWorldMatrix.MultiplyPoint3x4(cycles[currentLeg].cycleCenter + (legC.groundPlaneHeight - vector.y) * Vector3.up - cycles[currentLeg].cycleDirection * cycles[currentLeg].cycleScaling), localToWorldMatrix.MultiplyPoint3x4(cycles[currentLeg].cycleCenter + (legC.groundPlaneHeight - vector.y) * Vector3.up + cycles[currentLeg].cycleDirection * cycles[currentLeg].cycleScaling), color5);
					GL.End();
				}
			}
		}
		if (!opt.drawGraph)
		{
			return;
		}
		GL.Begin(1);
		Vector3 vector5 = legC.groundPlaneHeight * gameObject.transform.up * z;
		float num7 = 2f * cycleDistance;
		if (!opt.normalizeGraph)
		{
			num7 = 0f;
		}
		Quaternion rotation = gameObject.transform.rotation;
		Vector3 right = rotation * Quaternion.Euler(0f, 90f, 0f) * cycleDirection;
		Vector3 vector6 = rotation * gameObject.transform.up;
		Vector3 vector7 = rotation * cycleDirection;
		Vector3 vector8 = gameObject.transform.position + vector5;
		DrawArea drawArea = new DrawArea3D(new Vector3((0f - num2) * 0.5f, 0f, 0f), new Vector3((0f - num2) * 1.5f, z, z), Util.CreateMatrix(right, vector6 * opt.graphScaleV, vector7 * opt.graphScaleH, vector8));
		DrawArea drawArea2 = new DrawArea3D(new Vector3((0f - num2) * 0.5f, vector.y * z * 0.7f, 0f), new Vector3((0f - num2) * 1.5f, vector.y * z * 1f, 1f), Util.CreateMatrix(right, vector6 + vector7, vector7 - vector6, vector8 + vector7 * graphMax.z * z + vector6 * vector.y * z));
		DrawArea drawArea3 = new DrawArea3D(new Vector3((0f - num2) * 0.5f, vector.y * z * 0.2f, 0f), new Vector3((0f - num2) * 1.5f, vector.y * z * 0.5f, 1f), Util.CreateMatrix(right, vector6 + vector7, vector7 - vector6, vector8 + vector7 * graphMax.z * z + vector6 * vector.y * z));
		if (flag)
		{
			drawArea.DrawCube(new Vector3(0f, 0f, graphMax.z), Vector3.right * 2f, Vector3.up * vector.y, -Vector3.forward * (vector.z + num7), c);
		}
		for (int k = 0; k < 2; k++)
		{
			if (opt.drawStanceMarkers)
			{
				drawArea.DrawRect(new Vector3(Util.Mod(timeFromIndex - cycles[currentLeg].stanceTime + cycleOffset, 1f) + (float)k, 0f, graphMax.z), Vector3.up * vector.y, -Vector3.forward * (vector.z + num7), color3);
			}
		}
		if (opt.drawHeelToe)
		{
			DrawLine(ankle.position + Util.TransformVector(ankle, legC.legs[currentLeg].ankleHeelVector), drawArea.Point(new Vector3(0f, cycles[currentLeg].samples[indexFromTime].heel.y - legC.groundPlaneHeight, Vector3.Dot(cycles[currentLeg].samples[indexFromTime].heel, cycleDirection))), color);
			DrawLine(toe.position + Util.TransformVector(toe, legC.legs[currentLeg].toeToetipVector), drawArea.Point(new Vector3(0f, cycles[currentLeg].samples[indexFromTime].toetip.y - legC.groundPlaneHeight, Vector3.Dot(cycles[currentLeg].samples[indexFromTime].toetip, cycleDirection))), color2);
		}
		if (opt.drawFootBase)
		{
			DrawLine(a3, drawArea.Point(new Vector3(0f, cycles[currentLeg].samples[indexFromTime].footBase.y - legC.groundPlaneHeight, Vector3.Dot(cycles[currentLeg].samples[indexFromTime].footBase, cycleDirection))), Color.black);
		}
		for (int l = 0; l < samples * 2; l++)
		{
			int num8 = Util.Mod(indexFromTime - l, samples);
			LegCycleSample legCycleSample3 = cycles[currentLeg].samples[num8];
			LegCycleSample legCycleSample4 = cycles[currentLeg].samples[Util.Mod(num8 - 1, samples)];
			float timeFromIndex4 = GetTimeFromIndex(l);
			float timeFromIndex5 = GetTimeFromIndex(l + 1);
			float timeFromIndex6 = GetTimeFromIndex(Util.Mod(num8 - cycles[currentLeg].stanceIndex, samples));
			float timeFromIndex7 = GetTimeFromIndex(Util.Mod(num8 - 1 - cycles[currentLeg].stanceIndex, samples));
			float num9 = 0f;
			float num10 = 0f;
			if (opt.normalizeGraph)
			{
				num9 = (0f - timeFromIndex4) * cycleDistance;
				num10 = (0f - timeFromIndex5) * cycleDistance;
			}
			if (opt.drawHeelToe)
			{
				drawArea.DrawLine(new Vector3(timeFromIndex4, legCycleSample3.heel.y - legC.groundPlaneHeight, Vector3.Dot(legCycleSample3.heel, cycleDirection) + num9), new Vector3(timeFromIndex5, legCycleSample4.heel.y - legC.groundPlaneHeight, Vector3.Dot(legCycleSample4.heel, cycleDirection) + num10), color);
				drawArea.DrawLine(new Vector3(timeFromIndex4, legCycleSample3.toetip.y - legC.groundPlaneHeight, Vector3.Dot(legCycleSample3.toetip, cycleDirection) + num9), new Vector3(timeFromIndex5, legCycleSample4.toetip.y - legC.groundPlaneHeight, Vector3.Dot(legCycleSample4.toetip, cycleDirection) + num10), color2);
			}
			if (opt.drawFootBase && num8 % 2 == 0)
			{
				drawArea.DrawLine(new Vector3(timeFromIndex4, legCycleSample3.footBase.y - legC.groundPlaneHeight, Vector3.Dot(legCycleSample3.footBase, cycleDirection) + num9), new Vector3(timeFromIndex5, legCycleSample4.footBase.y - legC.groundPlaneHeight, Vector3.Dot(legCycleSample4.footBase, cycleDirection) + num10), color3);
			}
			if (opt.drawBalanceCurve)
			{
				float num11 = (legCycleSample3.balance + legCycleSample4.balance) / 2f;
				drawArea2.DrawLine(new Vector3(timeFromIndex4, legCycleSample3.balance, 0f), new Vector3(timeFromIndex5, legCycleSample4.balance, 0f), color2 * num11 + color * (1f - num11));
			}
			if (opt.drawLiftedCurve)
			{
				float num12 = (GetFootGroundingOrig(currentLeg, timeFromIndex6) + GetFootGroundingOrig(currentLeg, timeFromIndex7)) / 2f;
				drawArea3.DrawLine(new Vector3(timeFromIndex4, GetFootGroundingOrig(currentLeg, timeFromIndex6), 0f), new Vector3(timeFromIndex5, GetFootGroundingOrig(currentLeg, timeFromIndex7), 0f), color3 * (1f - num12) + color4 * num12);
			}
		}
		GL.End();
	}
}
