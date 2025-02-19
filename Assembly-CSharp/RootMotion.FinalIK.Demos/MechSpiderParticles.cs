using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class MechSpiderParticles : MonoBehaviour
{
	public MechSpiderController mechSpiderController;

	private ParticleSystem particles;

	private void Start()
	{
		particles = (ParticleSystem)GetComponent(typeof(ParticleSystem));
	}

	private void Update()
	{
		float magnitude = mechSpiderController.inputVector.magnitude;
		particles.emissionRate = Mathf.Clamp(magnitude * 50f, 30f, 50f);
		particles.startColor = new Color(particles.startColor.r, particles.startColor.g, particles.startColor.b, Mathf.Clamp(magnitude, 0.4f, 1f));
	}
}
