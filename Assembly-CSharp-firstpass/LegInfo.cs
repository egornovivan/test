using System;
using UnityEngine;

[Serializable]
public class LegInfo
{
	public Transform hip;

	public Transform ankle;

	public Transform toe;

	public float footWidth;

	public float footLength;

	public Vector2 footOffset;

	[HideInInspector]
	public Transform[] legChain;

	[HideInInspector]
	public Transform[] footChain;

	[HideInInspector]
	public float legLength;

	[HideInInspector]
	public Vector3 ankleHeelVector;

	[HideInInspector]
	public Vector3 toeToetipVector;

	[HideInInspector]
	public Color debugColor;
}
