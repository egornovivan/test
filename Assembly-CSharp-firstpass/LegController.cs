using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LegController : MonoBehaviour
{
	public float groundPlaneHeight;

	public AnimationClip groundedPose;

	public Transform rootBone;

	public LegInfo[] legs;

	public MotionAnalyzer[] sourceAnimations;

	[HideInInspector]
	public bool initialized;

	[HideInInspector]
	public MotionAnalyzerBackwards[] sourceAnimationsBackwards;

	[HideInInspector]
	public Vector3 m_HipAverage;

	[HideInInspector]
	public Vector3 m_HipAverageGround;

	[HideInInspector]
	public IMotionAnalyzer[] m_Motions;

	[HideInInspector]
	public IMotionAnalyzer[] m_CycleMotions;

	[HideInInspector]
	public MotionGroupInfo[] m_MotionGroups;

	[HideInInspector]
	public IMotionAnalyzer[] m_NonGroupMotions;

	public Transform root => rootBone;

	public Vector3 hipAverage => m_HipAverage;

	public Vector3 hipAverageGround => m_HipAverageGround;

	public IMotionAnalyzer[] motions => m_Motions;

	public IMotionAnalyzer[] cycleMotions => m_CycleMotions;

	public MotionGroupInfo[] motionGroups => m_MotionGroups;

	public IMotionAnalyzer[] nonGroupMotions => m_NonGroupMotions;

	public void InitFootData(int leg)
	{
		groundedPose.SampleAnimation(base.gameObject, 0f);
		Vector3 vector = Quaternion.AngleAxis((float)leg * 360f / (float)legs.Length, Vector3.one) * Vector3.right;
		legs[leg].debugColor = new Color(vector.x, vector.y, vector.z);
		Matrix4x4 matrix4x = Util.RelativeMatrix(legs[leg].ankle, base.gameObject.transform);
		Vector3 vector2 = matrix4x.MultiplyPoint(Vector3.zero);
		Vector3 vector3 = vector2;
		vector3.y = groundPlaneHeight;
		Matrix4x4 matrix4x2 = Util.RelativeMatrix(legs[leg].toe, base.gameObject.transform);
		Vector3 vector4 = matrix4x2.MultiplyPoint(Vector3.zero);
		Vector3 vector5 = vector4;
		vector5.y = groundPlaneHeight;
		Vector3 vector6 = (vector3 + vector5) / 2f;
		Vector3 vector7;
		if (vector4 == vector2)
		{
			vector7 = matrix4x.MultiplyVector(legs[leg].ankle.localPosition);
			vector7.y = 0f;
			vector7 = vector7.normalized;
		}
		else
		{
			vector7 = (vector5 - vector3).normalized;
		}
		Vector3 vector8 = Vector3.Cross(Vector3.up, vector7);
		legs[leg].ankleHeelVector = vector6 + ((0f - legs[leg].footLength) / 2f + legs[leg].footOffset.y) * vector7 + legs[leg].footOffset.x * vector8;
		legs[leg].ankleHeelVector = matrix4x.inverse.MultiplyVector(legs[leg].ankleHeelVector - vector2);
		legs[leg].toeToetipVector = vector6 + (legs[leg].footLength / 2f + legs[leg].footOffset.y) * vector7 + legs[leg].footOffset.x * vector8;
		legs[leg].toeToetipVector = matrix4x2.inverse.MultiplyVector(legs[leg].toeToetipVector - vector4);
	}

	public void Init()
	{
		initialized = false;
		Debug.Log("Initializing " + base.name + " Locomotion System...");
		if (rootBone == null)
		{
			if (legs[0].hip == null)
			{
				Debug.LogError(base.name + ": Leg Transforms are null.", this);
				return;
			}
			rootBone = legs[0].hip;
			while (root.parent != base.transform)
			{
				rootBone = root.parent;
			}
		}
		m_HipAverage = Vector3.zero;
		for (int i = 0; i < legs.Length; i++)
		{
			if (legs[i].toe == null)
			{
				legs[i].toe = legs[i].ankle;
			}
			legs[i].legChain = GetTransformChain(legs[i].hip, legs[i].ankle);
			legs[i].footChain = GetTransformChain(legs[i].ankle, legs[i].toe);
			legs[i].legLength = 0f;
			for (int j = 0; j < legs[i].legChain.Length - 1; j++)
			{
				legs[i].legLength += (base.transform.InverseTransformPoint(legs[i].legChain[j + 1].position) - base.transform.InverseTransformPoint(legs[i].legChain[j].position)).magnitude;
			}
			m_HipAverage += base.transform.InverseTransformPoint(legs[i].legChain[0].position);
			InitFootData(i);
		}
		m_HipAverage /= (float)legs.Length;
		m_HipAverageGround = m_HipAverage;
		m_HipAverageGround.y = groundPlaneHeight;
	}

	public void Init2()
	{
		List<MotionAnalyzerBackwards> list = new List<MotionAnalyzerBackwards>();
		for (int i = 0; i < sourceAnimations.Length; i++)
		{
			Debug.Log("Analysing sourceAnimations[" + i + "]: " + sourceAnimations[i].name);
			sourceAnimations[i].Analyze(base.gameObject);
			if (sourceAnimations[i].alsoUseBackwards)
			{
				MotionAnalyzerBackwards motionAnalyzerBackwards = new MotionAnalyzerBackwards();
				motionAnalyzerBackwards.orig = sourceAnimations[i];
				motionAnalyzerBackwards.Analyze(base.gameObject);
				list.Add(motionAnalyzerBackwards);
			}
		}
		sourceAnimationsBackwards = list.ToArray();
		groundedPose.SampleAnimation(base.gameObject, 0f);
		initialized = true;
		Debug.Log("Initializing " + base.name + " Locomotion System... Done!");
	}

	private void Awake()
	{
		if (!initialized)
		{
			Debug.LogError(base.name + ": Locomotion System has not been initialized.", this);
			return;
		}
		m_Motions = new IMotionAnalyzer[sourceAnimations.Length + sourceAnimationsBackwards.Length];
		for (int i = 0; i < sourceAnimations.Length; i++)
		{
			motions[i] = sourceAnimations[i];
		}
		for (int j = 0; j < sourceAnimationsBackwards.Length; j++)
		{
			motions[sourceAnimations.Length + j] = sourceAnimationsBackwards[j];
		}
		int num = 0;
		for (int k = 0; k < motions.Length; k++)
		{
			if (motions[k].motionType == MotionType.WalkCycle)
			{
				num++;
			}
		}
		m_CycleMotions = new IMotionAnalyzer[num];
		int num2 = 0;
		for (int l = 0; l < motions.Length; l++)
		{
			if (motions[l].motionType == MotionType.WalkCycle)
			{
				cycleMotions[num2] = motions[l];
				num2++;
			}
		}
		List<string> list = new List<string>();
		List<MotionGroupInfo> list2 = new List<MotionGroupInfo>();
		List<List<IMotionAnalyzer>> list3 = new List<List<IMotionAnalyzer>>();
		List<IMotionAnalyzer> list4 = new List<IMotionAnalyzer>();
		for (int m = 0; m < motions.Length; m++)
		{
			if (motions[m].motionGroup == string.Empty)
			{
				list4.Add(motions[m]);
				continue;
			}
			string motionGroup = motions[m].motionGroup;
			if (!list.Contains(motionGroup))
			{
				MotionGroupInfo motionGroupInfo = new MotionGroupInfo();
				motionGroupInfo.name = motionGroup;
				list2.Add(motionGroupInfo);
				list.Add(motionGroup);
				list3.Add(new List<IMotionAnalyzer>());
			}
			list3[list.IndexOf(motionGroup)].Add(motions[m]);
		}
		m_NonGroupMotions = list4.ToArray();
		m_MotionGroups = list2.ToArray();
		for (int n = 0; n < motionGroups.Length; n++)
		{
			motionGroups[n].motions = list3[n].ToArray();
		}
		for (int num3 = 0; num3 < motionGroups.Length; num3++)
		{
			MotionGroupInfo motionGroupInfo2 = motionGroups[num3];
			Vector3[] array = new Vector3[motionGroupInfo2.motions.Length];
			float[][] array2 = new float[motionGroupInfo2.motions.Length][];
			for (int num4 = 0; num4 < motionGroupInfo2.motions.Length; num4++)
			{
				ref Vector3 reference = ref array[num4];
				reference = motionGroupInfo2.motions[num4].cycleVelocity;
				array2[num4] = new float[3]
				{
					array[num4].x,
					array[num4].y,
					array[num4].z
				};
			}
			motionGroupInfo2.interpolator = new PolarGradientBandInterpolator(array2);
		}
		CalculateTimeOffsets();
	}

	public Transform[] GetTransformChain(Transform upper, Transform lower)
	{
		Transform transform = lower;
		int num = 1;
		while (transform != upper)
		{
			transform = transform.parent;
			num++;
		}
		Transform[] array = new Transform[num];
		transform = lower;
		for (int i = 0; i < num; i++)
		{
			array[num - 1 - i] = transform;
			transform = transform.parent;
		}
		return array;
	}

	public void CalculateTimeOffsets()
	{
		float[] array = new float[cycleMotions.Length];
		float[] array2 = new float[cycleMotions.Length];
		for (int i = 0; i < cycleMotions.Length; i++)
		{
			array[i] = 0f;
		}
		int num = (cycleMotions.Length * cycleMotions.Length - cycleMotions.Length) / 2;
		int num2 = 0;
		bool flag = false;
		while (num2 < 100 && !flag)
		{
			for (int j = 0; j < cycleMotions.Length; j++)
			{
				array2[j] = 0f;
			}
			for (int k = 1; k < cycleMotions.Length; k++)
			{
				for (int l = 0; l < k; l++)
				{
					for (int m = 0; m < legs.Length; m++)
					{
						float num3 = cycleMotions[k].cycles[m].stanceTime + array[k];
						float num4 = cycleMotions[l].cycles[m].stanceTime + array[l];
						Vector2 vector = new Vector2(Mathf.Cos(num3 * 2f * (float)Math.PI), Mathf.Sin(num3 * 2f * (float)Math.PI));
						Vector2 vector2 = new Vector2(Mathf.Cos(num4 * 2f * (float)Math.PI), Mathf.Sin(num4 * 2f * (float)Math.PI));
						Vector2 vector3 = vector2 - vector;
						Vector2 vector4 = vector + vector3 * 0.1f;
						Vector2 vector5 = vector2 - vector3 * 0.1f;
						float num5 = Util.Mod(Mathf.Atan2(vector4.y, vector4.x) / 2f / (float)Math.PI);
						float num6 = Util.Mod(Mathf.Atan2(vector5.y, vector5.x) / 2f / (float)Math.PI);
						float num7 = Util.Mod(num5 - num3);
						float num8 = Util.Mod(num6 - num4);
						if (num7 > 0.5f)
						{
							num7 -= 1f;
						}
						if (num8 > 0.5f)
						{
							num8 -= 1f;
						}
						array2[k] += num7 * 5f / (float)num;
						array2[l] += num8 * 5f / (float)num;
					}
				}
			}
			float num9 = 0f;
			for (int n = 0; n < cycleMotions.Length; n++)
			{
				array[n] += array2[n];
				num9 = Mathf.Max(num9, Mathf.Abs(array2[n]));
			}
			num2++;
			if ((double)num9 < 0.0001)
			{
				flag = true;
			}
		}
		for (int num10 = 0; num10 < cycleMotions.Length; num10++)
		{
			cycleMotions[num10].cycleOffset = array[num10];
			for (int num11 = 0; num11 < legs.Length; num11++)
			{
				cycleMotions[num10].cycles[num11].stanceTime = Util.Mod(cycleMotions[num10].cycles[num11].stanceTime + array[num10]);
			}
		}
	}
}
