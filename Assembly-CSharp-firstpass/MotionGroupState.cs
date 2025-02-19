using UnityEngine;

public class MotionGroupState
{
	public AnimationState controller;

	public float weight;

	public AnimationState[] motionStates;

	public float[] relativeWeights;

	public float[] relativeWeightsBlended;

	public int primaryMotionIndex;
}
