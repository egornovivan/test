using UnityEngine;

public class PlanetAperture : MonoBehaviour
{
	public Transform planetEdge;

	public Transform sun;

	public float distToPlanet;

	public AnimationCurve blueThickness;

	private Camera cmr;

	private Vector3 planetCenter;

	private float side1;

	private float side2;

	private float t2;

	private void Start()
	{
		cmr = Camera.main;
	}

	private void Update()
	{
		planetCenter = planetEdge.parent.position;
		base.transform.forward = (cmr.transform.position - planetCenter).normalized;
		base.transform.position = planetCenter + base.transform.forward * distToPlanet;
		side1 = Vector3.SqrMagnitude(planetEdge.position - planetCenter);
		side2 = Vector3.SqrMagnitude(planetCenter - cmr.transform.position) - side1;
		t2 = side2 / Vector3.SqrMagnitude(base.transform.position - cmr.transform.position);
		GetComponent<Renderer>().material.SetFloat("_Radius", Mathf.Sqrt(side1 / t2) / base.transform.localScale.x);
		GetComponent<Renderer>().material.SetFloat("_SunAngle", Vector3.Angle(base.transform.forward, sun.forward));
		Vector3 vector = Quaternion.Inverse(base.transform.rotation) * sun.forward;
		GetComponent<Renderer>().material.SetVector("_SunDirect", vector);
	}
}
