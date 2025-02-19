using System;
using UnityEngine;
using UnityScript.Lang;

[Serializable]
public class Scale : MonoBehaviour
{
	public ParticleEmitter[] particleEmitters;

	public float scale;

	[HideInInspector]
	[SerializeField]
	private float[] minsize;

	[HideInInspector]
	[SerializeField]
	private float[] maxsize;

	[SerializeField]
	[HideInInspector]
	private Vector3[] worldvelocity;

	[SerializeField]
	[HideInInspector]
	private Vector3[] localvelocity;

	[HideInInspector]
	[SerializeField]
	private Vector3[] rndvelocity;

	[SerializeField]
	[HideInInspector]
	private Vector3[] scaleBackUp;

	[HideInInspector]
	[SerializeField]
	private bool firstUpdate;

	public Scale()
	{
		scale = 1f;
		firstUpdate = true;
	}

	public virtual void UpdateScale()
	{
		int num = Extensions.get_length((System.Array)particleEmitters);
		if (firstUpdate)
		{
			minsize = new float[num];
			maxsize = new float[num];
			worldvelocity = new Vector3[num];
			localvelocity = new Vector3[num];
			rndvelocity = new Vector3[num];
			scaleBackUp = new Vector3[num];
		}
		for (int i = 0; i < Extensions.get_length((System.Array)particleEmitters); i++)
		{
			if (firstUpdate)
			{
				minsize[i] = particleEmitters[i].minSize;
				maxsize[i] = particleEmitters[i].maxSize;
				ref Vector3 reference = ref worldvelocity[i];
				reference = particleEmitters[i].worldVelocity;
				ref Vector3 reference2 = ref localvelocity[i];
				reference2 = particleEmitters[i].localVelocity;
				ref Vector3 reference3 = ref rndvelocity[i];
				reference3 = particleEmitters[i].rndVelocity;
				ref Vector3 reference4 = ref scaleBackUp[i];
				reference4 = particleEmitters[i].transform.localScale;
			}
			particleEmitters[i].minSize = minsize[i] * scale;
			particleEmitters[i].maxSize = maxsize[i] * scale;
			particleEmitters[i].worldVelocity = worldvelocity[i] * scale;
			particleEmitters[i].localVelocity = localvelocity[i] * scale;
			particleEmitters[i].rndVelocity = rndvelocity[i] * scale;
			particleEmitters[i].transform.localScale = scaleBackUp[i] * scale;
		}
		firstUpdate = false;
	}

	public virtual void Main()
	{
	}
}
