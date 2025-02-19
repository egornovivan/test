using System.Collections;
using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class MechSpiderLeg : MonoBehaviour
{
	public MechSpider mechSpider;

	public MechSpiderLeg unSync;

	public Vector3 offset;

	public float minDelay = 0.2f;

	public float maxOffset = 1f;

	public float stepSpeed = 5f;

	public float footHeight = 0.15f;

	public float velocityPrediction = 0.2f;

	public float raycastFocus = 0.1f;

	public AnimationCurve yOffset;

	public ParticleSystem sand;

	private IK ik;

	private float stepProgress = 1f;

	private float lastStepTime;

	private Vector3 defaultDirection;

	private RaycastHit hit = default(RaycastHit);

	public bool isStepping => stepProgress < 1f;

	public Vector3 position
	{
		get
		{
			return ik.GetIKSolver().GetIKPosition();
		}
		set
		{
			ik.GetIKSolver().SetIKPosition(value);
		}
	}

	private void Start()
	{
		ik = GetComponent<IK>();
		stepProgress = 1f;
		hit = default(RaycastHit);
		hit.point = position;
		defaultDirection = mechSpider.transform.TransformDirection(position + offset - mechSpider.transform.position);
	}

	private Vector3 GetStepTarget(out bool stepFound, float focus, float distance)
	{
		stepFound = false;
		Vector3 vector = mechSpider.transform.position + mechSpider.transform.TransformDirection(defaultDirection);
		vector += (hit.point - position) * velocityPrediction;
		Vector3 up = mechSpider.transform.up;
		Vector3 rhs = mechSpider.body.position - position;
		Vector3 axis = Vector3.Cross(up, rhs);
		up = Quaternion.AngleAxis(focus, axis) * up;
		if (Physics.Raycast(vector + up * mechSpider.raycastHeight, -up, out hit, mechSpider.raycastHeight + distance, mechSpider.raycastLayers))
		{
			stepFound = true;
		}
		return hit.point + mechSpider.transform.up * footHeight;
	}

	private void Update()
	{
		if (!isStepping && !(Time.time < lastStepTime + minDelay) && (!(unSync != null) || !unSync.isStepping))
		{
			bool stepFound = false;
			Vector3 stepTarget = GetStepTarget(out stepFound, raycastFocus, mechSpider.raycastDistance);
			if (!stepFound)
			{
				stepTarget = GetStepTarget(out stepFound, 0f - raycastFocus, mechSpider.raycastDistance * 3f);
			}
			if (stepFound && !(Vector3.Distance(position, stepTarget) < maxOffset * Random.Range(0.9f, 1.2f)))
			{
				StopAllCoroutines();
				StartCoroutine(Step(position, stepTarget));
			}
		}
	}

	private IEnumerator Step(Vector3 stepStartPosition, Vector3 targetPosition)
	{
		stepProgress = 0f;
		while (stepProgress < 1f)
		{
			stepProgress += Time.deltaTime * stepSpeed;
			position = Vector3.Lerp(stepStartPosition, targetPosition, stepProgress);
			position += mechSpider.transform.up * yOffset.Evaluate(stepProgress);
			yield return null;
		}
		if (sand != null)
		{
			sand.transform.position = position - mechSpider.transform.up * footHeight;
			sand.Emit(20);
		}
		lastStepTime = Time.time;
	}
}
