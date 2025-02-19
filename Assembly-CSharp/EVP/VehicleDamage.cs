using System;
using UnityEngine;

namespace EVP;

[RequireComponent(typeof(VehicleController))]
public class VehicleDamage : MonoBehaviour
{
	public MeshFilter[] meshes;

	public MeshCollider[] colliders;

	public Transform[] nodes;

	[Space(5f)]
	public float minVelocity = 1f;

	public float multiplier = 1f;

	[Space(5f)]
	public float damageRadius = 1f;

	public float maxDisplacement = 0.5f;

	public float maxVertexFracture = 0.1f;

	[Space(5f)]
	public float nodeDamageRadius = 0.5f;

	public float maxNodeRotation = 14f;

	public float nodeRotationRate = 10f;

	[Space(5f)]
	public float vertexRepairRate = 0.1f;

	public bool enableRepairKey = true;

	public KeyCode repairKey = KeyCode.R;

	private VehicleController m_vehicle;

	private Vector3[][] m_originalMeshes;

	private Vector3[][] m_originalColliders;

	private Vector3[] m_originalNodePositions;

	private Quaternion[] m_originalNodeRotations;

	private bool m_repairing;

	public bool isRepairing => m_repairing;

	private void OnEnable()
	{
		m_vehicle = GetComponent<VehicleController>();
		m_vehicle.processContacts = true;
		VehicleController vehicle = m_vehicle;
		vehicle.onImpact = (VehicleController.OnImpact)Delegate.Combine(vehicle.onImpact, new VehicleController.OnImpact(ProcessImpact));
		m_originalMeshes = new Vector3[meshes.Length][];
		for (int i = 0; i < meshes.Length; i++)
		{
			m_originalMeshes[i] = meshes[i].mesh.vertices;
		}
		m_originalColliders = new Vector3[colliders.Length][];
		for (int j = 0; j < colliders.Length; j++)
		{
			m_originalColliders[j] = colliders[j].sharedMesh.vertices;
		}
		m_originalNodePositions = new Vector3[nodes.Length];
		m_originalNodeRotations = new Quaternion[nodes.Length];
		for (int k = 0; k < nodes.Length; k++)
		{
			ref Vector3 reference = ref m_originalNodePositions[k];
			reference = nodes[k].transform.localPosition;
			ref Quaternion reference2 = ref m_originalNodeRotations[k];
			reference2 = nodes[k].transform.localRotation;
		}
	}

	private void Update()
	{
		if (enableRepairKey && Input.GetKeyDown(repairKey))
		{
			m_repairing = true;
		}
		ProcessRepair();
	}

	public void Repair()
	{
		m_repairing = true;
	}

	private void ProcessImpact()
	{
		Vector3 vector = Vector3.zero;
		if (m_vehicle.localImpactVelocity.sqrMagnitude > minVelocity * minVelocity)
		{
			vector = m_vehicle.cachedTransform.TransformDirection(m_vehicle.localImpactVelocity) * multiplier * 0.02f;
		}
		if (vector.sqrMagnitude > 0f)
		{
			Vector3 contactPoint = base.transform.TransformPoint(m_vehicle.localImpactPosition);
			int i = 0;
			for (int num = meshes.Length; i < num; i++)
			{
				DeformMesh(meshes[i].mesh, m_originalMeshes[i], meshes[i].transform, contactPoint, vector);
			}
			DeformColliders(contactPoint, vector);
			int j = 0;
			for (int num2 = nodes.Length; j < num2; j++)
			{
				DeformNode(nodes[j], m_originalNodePositions[j], m_originalNodeRotations[j], contactPoint, vector * 0.5f);
			}
		}
	}

	private void DeformMesh(Mesh mesh, Vector3[] originalMesh, Transform localTransform, Vector3 contactPoint, Vector3 contactVelocity)
	{
		Vector3[] vertices = mesh.vertices;
		float num = damageRadius * damageRadius;
		float num2 = maxDisplacement * maxDisplacement;
		Vector3 vector = localTransform.InverseTransformPoint(contactPoint);
		Vector3 vector2 = localTransform.InverseTransformDirection(contactVelocity);
		for (int i = 0; i < vertices.Length; i++)
		{
			float sqrMagnitude = (vector - vertices[i]).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				vertices[i] += vector2 * (damageRadius - Mathf.Sqrt(sqrMagnitude)) / damageRadius + UnityEngine.Random.onUnitSphere * maxVertexFracture;
				Vector3 vector3 = vertices[i] - originalMesh[i];
				if (vector3.sqrMagnitude > num2)
				{
					ref Vector3 reference = ref vertices[i];
					reference = originalMesh[i] + vector3.normalized * maxDisplacement;
				}
			}
		}
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}

	private void DeformNode(Transform T, Vector3 originalLocalPos, Quaternion originalLocalRot, Vector3 contactPoint, Vector3 contactVelocity)
	{
		float sqrMagnitude = (contactPoint - T.position).sqrMagnitude;
		float num = (damageRadius - Mathf.Sqrt(sqrMagnitude)) / damageRadius;
		if (sqrMagnitude < damageRadius * damageRadius)
		{
			T.position += contactVelocity * num + UnityEngine.Random.onUnitSphere * maxVertexFracture;
			Vector3 vector = T.localPosition - originalLocalPos;
			if (vector.sqrMagnitude > maxDisplacement * maxDisplacement)
			{
				T.localPosition = originalLocalPos + vector.normalized * maxDisplacement;
			}
		}
		if (sqrMagnitude < nodeDamageRadius * nodeDamageRadius)
		{
			Vector3 vector2 = AnglesToVector(T.localEulerAngles);
			Vector3 vector3 = new Vector3(maxNodeRotation, maxNodeRotation, maxNodeRotation);
			Vector3 vector4 = vector2 + vector3;
			Vector3 vector5 = vector2 - vector3;
			vector2 += num * nodeRotationRate * UnityEngine.Random.onUnitSphere;
			T.localEulerAngles = new Vector3(Mathf.Clamp(vector2.x, vector5.x, vector4.x), Mathf.Clamp(vector2.y, vector5.y, vector4.y), Mathf.Clamp(vector2.z, vector5.z, vector4.z));
		}
	}

	private Vector3 AnglesToVector(Vector3 Angles)
	{
		if (Angles.x > 180f)
		{
			Angles.x = -360f + Angles.x;
		}
		if (Angles.y > 180f)
		{
			Angles.y = -360f + Angles.y;
		}
		if (Angles.z > 180f)
		{
			Angles.z = -360f + Angles.z;
		}
		return Angles;
	}

	private void DeformColliders(Vector3 contactPoint, Vector3 impactVelocity)
	{
	}

	private void ProcessRepair()
	{
		if (!m_repairing)
		{
			return;
		}
		float repairedThreshold = 0.002f;
		bool flag = true;
		int i = 0;
		for (int num = meshes.Length; i < num; i++)
		{
			flag = RepairMesh(meshes[i].mesh, m_originalMeshes[i], vertexRepairRate, repairedThreshold) && flag;
		}
		int j = 0;
		for (int num2 = nodes.Length; j < num2; j++)
		{
			flag = RepairNode(nodes[j], m_originalNodePositions[j], m_originalNodeRotations[j], vertexRepairRate, repairedThreshold) && flag;
		}
		if (flag)
		{
			m_repairing = false;
			int k = 0;
			for (int num3 = nodes.Length; k < num3; k++)
			{
				nodes[k].localPosition = m_originalNodePositions[k];
				nodes[k].localRotation = m_originalNodeRotations[k];
			}
			RestoreColliders();
		}
	}

	private bool RepairMesh(Mesh mesh, Vector3[] originalMesh, float repairRate, float repairedThreshold)
	{
		bool result = true;
		Vector3[] vertices = mesh.vertices;
		repairRate *= Time.deltaTime;
		repairedThreshold *= repairedThreshold;
		int i = 0;
		for (int num = vertices.Length; i < num; i++)
		{
			ref Vector3 reference = ref vertices[i];
			reference = Vector3.MoveTowards(vertices[i], originalMesh[i], repairRate);
			if ((originalMesh[i] - vertices[i]).sqrMagnitude >= repairedThreshold)
			{
				result = false;
			}
		}
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return result;
	}

	private bool RepairNode(Transform T, Vector3 originalLocalPosition, Quaternion originalLocalRotation, float repairRate, float repairedThreshold)
	{
		repairRate *= Time.deltaTime;
		T.localPosition = Vector3.MoveTowards(T.localPosition, originalLocalPosition, repairRate);
		T.localRotation = Quaternion.RotateTowards(T.localRotation, originalLocalRotation, repairRate * 50f);
		return (originalLocalPosition - T.localPosition).sqrMagnitude < repairedThreshold * repairedThreshold && Quaternion.Angle(originalLocalRotation, T.localRotation) < repairedThreshold;
	}

	private void RestoreColliders()
	{
	}
}
