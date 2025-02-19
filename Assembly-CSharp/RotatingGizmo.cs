using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class RotatingGizmo : TransformGizmo
{
	public delegate void DNotify(Quaternion quat);

	private const float radius = 0.9f;

	private const float interval = 6f;

	[SerializeField]
	private Collider XCollider;

	[SerializeField]
	private Collider YCollider;

	[SerializeField]
	private Collider ZCollider;

	[SerializeField]
	private SphereCollider SCollider;

	[SerializeField]
	private Collider ACollider;

	private Color xlc;

	private Color ylc;

	private Color zlc;

	private Color slc;

	private Color alc;

	private Color xpc;

	private Color ypc;

	private Color zpc;

	private bool focus_x;

	private bool focus_y;

	private bool focus_z;

	private bool focus_s;

	private bool focus_a;

	private bool dragging;

	private Vector3 dragorigin3d = Vector3.zero;

	private Vector3 dragdir;

	private Vector3 vdragdir;

	private Quaternion begin_tar_rot = Quaternion.identity;

	private Quaternion new_tar_rot = Quaternion.identity;

	public override bool MouseOver => base.enabled && base.Focus != null && (focus_x || focus_y || focus_z || focus_s || focus_a || dragging);

	public override bool Working => base.enabled && base.Focus != null && dragging;

	public event DNotify OnBeginTargetRotate;

	public event DNotify OnTargetRotate;

	public event DNotify OnEndTargetRotate;

	protected override void Start()
	{
		base.Start();
		XCollider.transform.localScale = 0.9f * Vector3.one;
		YCollider.transform.localScale = 0.9f * Vector3.one;
		ZCollider.transform.localScale = 0.9f * Vector3.one;
		SCollider.radius = 0.80999994f;
		ACollider.transform.localScale = 1.0619999f * Vector3.one;
		SCollider.enabled = !hideCenter;
		ACollider.enabled = !hideCenter;
	}

	protected override void Idle()
	{
		bool flag = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
		Vector3 normalized = (base.transform.position - GizmoGroup.position).normalized;
		Ray ray = cam.ScreenPointToRay(base.mousePosition);
		RaycastHit hitInfo = default(RaycastHit);
		if (!flag)
		{
			Physics.Raycast(ray, out hitInfo, 11f, 1 << base.gameObject.layer);
		}
		float num = Vector3.Dot((hitInfo.point - GizmoGroup.position).normalized, normalized);
		focus_x = hitInfo.collider == XCollider && num > -0.1f;
		focus_y = hitInfo.collider == YCollider && num > -0.1f;
		focus_z = hitInfo.collider == ZCollider && num > -0.1f;
		focus_s = hitInfo.collider == SCollider;
		focus_a = hitInfo.collider == ACollider;
		xlc = ((!focus_x) ? XColor : HColor);
		ylc = ((!focus_y) ? YColor : HColor);
		zlc = ((!focus_z) ? ZColor : HColor);
		slc = ((!focus_s) ? (Color.white * 0.8f) : HColor);
		alc = ((!focus_a) ? (Color.white * 0.8f) : HColor);
		xpc = XColor;
		ypc = YColor;
		zpc = ZColor;
		xpc.a = 0.3f;
		ypc.a = 0.3f;
		zpc.a = 0.3f;
		dragging = false;
		dragorigin3d = hitInfo.point;
	}

	protected override void BeginModify()
	{
		dragging = focus_a || focus_s || focus_x || focus_y || focus_z;
		if (dragging)
		{
			Vector3 normalized = (dragorigin3d - GizmoGroup.position).normalized;
			Vector3 vector = Vector3.zero;
			Vector3 normalized2 = (base.transform.position - GizmoGroup.position).normalized;
			if (focus_x)
			{
				vector = -Vector3.Cross(normalized, GizmoGroup.right).normalized;
			}
			else if (focus_y)
			{
				vector = -Vector3.Cross(normalized, GizmoGroup.up).normalized;
			}
			else if (focus_z)
			{
				vector = -Vector3.Cross(normalized, GizmoGroup.forward).normalized;
			}
			else if (focus_a)
			{
				vector = -Vector3.Cross(normalized, normalized2).normalized;
			}
			else if (focus_s)
			{
				vector = -ACollider.transform.right;
			}
			dragdir = cam.WorldToScreenPoint(dragorigin3d + vector) - base.mousePosition;
			dragdir.z = 0f;
			dragdir.Normalize();
			vdragdir = cam.WorldToScreenPoint(dragorigin3d + ACollider.transform.up) - base.mousePosition;
			vdragdir.z = 0f;
			vdragdir.Normalize();
			begin_tar_rot = base.Focus.rotation;
			new_tar_rot = base.Focus.rotation;
			if (this.OnBeginTargetRotate != null)
			{
				this.OnBeginTargetRotate(begin_tar_rot);
			}
		}
	}

	protected override void CustomLateUpdate()
	{
		Quaternion identity = Quaternion.identity;
		identity.SetLookRotation(GizmoGroup.position - MainCamera.transform.position);
		ACollider.transform.rotation = identity;
	}

	protected override void Modifying()
	{
		if (!dragging)
		{
			return;
		}
		Vector3 lhs = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0f);
		float angle = Vector3.Dot(lhs, dragdir) * 5f;
		float angle2 = Vector3.Dot(lhs, vdragdir) * 5f;
		Quaternion quaternion = new_tar_rot;
		Vector3 normalized = (base.transform.position - GizmoGroup.position).normalized;
		Quaternion quaternion2 = Quaternion.identity;
		if (focus_x)
		{
			quaternion2 = Quaternion.AngleAxis(angle, GizmoGroup.right) * quaternion2;
		}
		if (focus_y)
		{
			quaternion2 = Quaternion.AngleAxis(angle, GizmoGroup.up) * quaternion2;
		}
		if (focus_z)
		{
			quaternion2 = Quaternion.AngleAxis(angle, GizmoGroup.forward) * quaternion2;
		}
		if (focus_s)
		{
			quaternion2 = Quaternion.AngleAxis(angle, ACollider.transform.up) * Quaternion.AngleAxis(angle2, ACollider.transform.right) * quaternion2;
		}
		if (focus_a)
		{
			quaternion2 = Quaternion.AngleAxis(angle, normalized) * quaternion2;
		}
		new_tar_rot = quaternion2 * new_tar_rot;
		if (!(quaternion != new_tar_rot))
		{
			return;
		}
		foreach (Transform target in Targets)
		{
			if (target != null)
			{
				target.rotation = quaternion2 * target.rotation;
			}
		}
		if (this.OnTargetRotate != null)
		{
			this.OnTargetRotate(new_tar_rot);
		}
	}

	protected override void EndModify()
	{
		if (dragging && begin_tar_rot != new_tar_rot && this.OnEndTargetRotate != null)
		{
			this.OnEndTargetRotate(new_tar_rot);
		}
	}

	protected override void OnGL()
	{
		Vector3 position = GizmoGroup.position;
		Vector3 right = GizmoGroup.right;
		Vector3 up = GizmoGroup.up;
		Vector3 forward = GizmoGroup.forward;
		Vector3 forward2 = ACollider.transform.forward;
		Vector3 right2 = ACollider.transform.right;
		Vector3 up2 = ACollider.transform.up;
		GL.Begin(1);
		if (!hideCenter)
		{
			GL.Color(slc);
			for (float num = 0f; num <= 354.01f; num += 6f)
			{
				Vector3 v = position + right2 * Mathf.Cos(num * ((float)Math.PI / 180f)) * 0.9f * 0.94f + up2 * Mathf.Sin(num * ((float)Math.PI / 180f)) * 0.9f * 0.94f;
				float num2 = num + 6f;
				Vector3 v2 = position + right2 * Mathf.Cos(num2 * ((float)Math.PI / 180f)) * 0.9f * 0.94f + up2 * Mathf.Sin(num2 * ((float)Math.PI / 180f)) * 0.9f * 0.94f;
				GL.Vertex(v);
				GL.Vertex(v2);
			}
			GL.Color(alc);
			for (float num3 = 0f; num3 <= 354.01f; num3 += 6f)
			{
				Vector3 v3 = position + right2 * Mathf.Cos(num3 * ((float)Math.PI / 180f)) * 0.9f * 1.18f + up2 * Mathf.Sin(num3 * ((float)Math.PI / 180f)) * 0.9f * 1.18f;
				float num4 = num3 + 6f;
				Vector3 v4 = position + right2 * Mathf.Cos(num4 * ((float)Math.PI / 180f)) * 0.9f * 1.18f + up2 * Mathf.Sin(num4 * ((float)Math.PI / 180f)) * 0.9f * 1.18f;
				GL.Vertex(v3);
				GL.Vertex(v4);
			}
		}
		GL.Color(xlc);
		for (float num5 = 0f; num5 <= 354.01f; num5 += 6f)
		{
			Vector3 vector = position + up * Mathf.Cos(num5 * ((float)Math.PI / 180f)) * 0.9f + forward * Mathf.Sin(num5 * ((float)Math.PI / 180f)) * 0.9f;
			if (!(Vector3.Dot(forward2.normalized, (vector - position).normalized) > 0.17f))
			{
				float num6 = num5 + 6f;
				Vector3 v5 = position + up * Mathf.Cos(num6 * ((float)Math.PI / 180f)) * 0.9f + forward * Mathf.Sin(num6 * ((float)Math.PI / 180f)) * 0.9f;
				GL.Vertex(vector);
				GL.Vertex(v5);
			}
		}
		GL.Color(ylc);
		for (float num7 = 0f; num7 <= 354.01f; num7 += 6f)
		{
			Vector3 vector2 = position + forward * Mathf.Cos(num7 * ((float)Math.PI / 180f)) * 0.9f + right * Mathf.Sin(num7 * ((float)Math.PI / 180f)) * 0.9f;
			if (!(Vector3.Dot(forward2.normalized, (vector2 - position).normalized) > 0.17f))
			{
				float num8 = num7 + 6f;
				Vector3 v6 = position + forward * Mathf.Cos(num8 * ((float)Math.PI / 180f)) * 0.9f + right * Mathf.Sin(num8 * ((float)Math.PI / 180f)) * 0.9f;
				GL.Vertex(vector2);
				GL.Vertex(v6);
			}
		}
		GL.Color(zlc);
		for (float num9 = 0f; num9 <= 354.01f; num9 += 6f)
		{
			Vector3 vector3 = position + right * Mathf.Cos(num9 * ((float)Math.PI / 180f)) * 0.9f + up * Mathf.Sin(num9 * ((float)Math.PI / 180f)) * 0.9f;
			if (!(Vector3.Dot(forward2.normalized, (vector3 - position).normalized) > 0.17f))
			{
				float num10 = num9 + 6f;
				Vector3 v7 = position + right * Mathf.Cos(num10 * ((float)Math.PI / 180f)) * 0.9f + up * Mathf.Sin(num10 * ((float)Math.PI / 180f)) * 0.9f;
				GL.Vertex(vector3);
				GL.Vertex(v7);
			}
		}
		GL.End();
	}

	private void OnDisable()
	{
		dragging = false;
		focus_a = false;
		focus_x = false;
		focus_y = false;
		focus_z = false;
		focus_s = false;
	}
}
