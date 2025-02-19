using UnityEngine;
using UnityEngine.EventSystems;

public class ScalingGizmo : TransformGizmo
{
	public delegate void DNotify(Vector3 scale);

	private const float end = 0.81f;

	private const float half = 0.24f;

	[SerializeField]
	private Renderer XPointRenderer;

	[SerializeField]
	private Renderer YPointRenderer;

	[SerializeField]
	private Renderer ZPointRenderer;

	[SerializeField]
	private Renderer OPointRenderer;

	[SerializeField]
	private BoxCollider OCollider;

	[SerializeField]
	private BoxCollider XCollider;

	[SerializeField]
	private BoxCollider YCollider;

	[SerializeField]
	private BoxCollider ZCollider;

	[SerializeField]
	private BoxCollider XYCollider;

	[SerializeField]
	private BoxCollider YZCollider;

	[SerializeField]
	private BoxCollider ZXCollider;

	private Material omat;

	private Material xmat;

	private Material ymat;

	private Material zmat;

	private float xsign = 1f;

	private float ysign = 1f;

	private float zsign = 1f;

	private Color xlc;

	private Color ylc;

	private Color zlc;

	private Color olc;

	private Color xpc;

	private Color ypc;

	private Color zpc;

	private bool focus_xl;

	private bool focus_yl;

	private bool focus_zl;

	private bool focus_ol;

	private bool focus_xp;

	private bool focus_yp;

	private bool focus_zp;

	private bool xdragging;

	private bool ydragging;

	private bool zdragging;

	private bool dragplane;

	private Vector3 dragorigin = Vector3.zero;

	private Vector3 xdragdir;

	private Vector3 ydragdir;

	private Vector3 zdragdir;

	private Vector3 begin_pos_3d = Vector3.zero;

	private Vector3 begin_tar_scale = Vector3.zero;

	private Vector3 new_tar_scale = Vector3.zero;

	public override bool MouseOver => base.enabled && base.Focus != null && (focus_xl || focus_xp || focus_yl || focus_yp || focus_zl || focus_zp || focus_ol || xdragging || ydragging || zdragging);

	public override bool Working => base.enabled && base.Focus != null && (xdragging || ydragging || zdragging);

	public event DNotify OnBeginTargetScale;

	public event DNotify OnTargetScale;

	public event DNotify OnEndTargetScale;

	protected override void Start()
	{
		base.Start();
		xmat = Object.Instantiate(XPointRenderer.material);
		xmat.color = XColor;
		xmat.SetColor("_RimColor", XColor * 0.25f);
		XPointRenderer.material = xmat;
		ymat = Object.Instantiate(YPointRenderer.material);
		ymat.color = YColor;
		ymat.SetColor("_RimColor", YColor * 0.25f);
		YPointRenderer.material = ymat;
		zmat = Object.Instantiate(ZPointRenderer.material);
		zmat.color = ZColor;
		zmat.SetColor("_RimColor", ZColor * 0.25f);
		ZPointRenderer.material = zmat;
		omat = Object.Instantiate(OPointRenderer.material);
		omat.color = Color.white;
		omat.SetColor("_RimColor", Color.white * 0.25f);
		OPointRenderer.material = omat;
		XYCollider.enabled = !hideCenter;
		YZCollider.enabled = !hideCenter;
		ZXCollider.enabled = !hideCenter;
	}

	protected override void OnDestroy()
	{
		Object.Destroy(xmat);
		Object.Destroy(ymat);
		Object.Destroy(zmat);
		Object.Destroy(omat);
		base.OnDestroy();
	}

	protected override void Idle()
	{
		bool flag = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
		Vector3 normalized = (base.transform.position - GizmoGroup.position).normalized;
		xsign = Mathf.Sign(Vector3.Dot(normalized, GizmoGroup.right));
		ysign = Mathf.Sign(Vector3.Dot(normalized, GizmoGroup.up));
		zsign = Mathf.Sign(Vector3.Dot(normalized, GizmoGroup.forward));
		XYCollider.transform.localPosition = new Vector3(xsign * 0.24f * 0.5f, ysign * 0.24f * 0.5f, 0f);
		YZCollider.transform.localPosition = new Vector3(0f, ysign * 0.24f * 0.5f, zsign * 0.24f * 0.5f);
		ZXCollider.transform.localPosition = new Vector3(xsign * 0.24f * 0.5f, 0f, zsign * 0.24f * 0.5f);
		Ray ray = cam.ScreenPointToRay(base.mousePosition);
		RaycastHit hitInfo = default(RaycastHit);
		if (!flag)
		{
			Physics.Raycast(ray, out hitInfo, 11f, 1 << base.gameObject.layer);
		}
		float num = Vector3.Angle(ray.direction, GizmoGroup.right);
		float num2 = Vector3.Angle(ray.direction, GizmoGroup.up);
		float num3 = Vector3.Angle(ray.direction, GizmoGroup.forward);
		focus_xl = hitInfo.collider == XCollider && num > 5f && num < 175f;
		focus_yl = hitInfo.collider == YCollider && num2 > 5f && num2 < 175f;
		focus_zl = hitInfo.collider == ZCollider && num3 > 5f && num3 < 175f;
		focus_ol = hitInfo.collider == OCollider;
		focus_xp = hitInfo.collider == YZCollider && Mathf.Abs(num - 90f) > 4f;
		focus_yp = hitInfo.collider == ZXCollider && Mathf.Abs(num2 - 90f) > 4f;
		focus_zp = hitInfo.collider == XYCollider && Mathf.Abs(num3 - 90f) > 4f;
		xlc = ((!focus_xl) ? XColor : HColor);
		ylc = ((!focus_yl) ? YColor : HColor);
		zlc = ((!focus_zl) ? ZColor : HColor);
		olc = ((!focus_ol) ? Color.white : HColor);
		xpc = ((!focus_xp && !focus_ol) ? XColor : HColor);
		ypc = ((!focus_yp && !focus_ol) ? YColor : HColor);
		zpc = ((!focus_zp && !focus_ol) ? ZColor : HColor);
		xpc.a = 0.3f;
		ypc.a = 0.3f;
		zpc.a = 0.3f;
		xmat.color = xlc;
		ymat.color = ylc;
		zmat.color = zlc;
		omat.color = olc;
		xdragging = false;
		ydragging = false;
		zdragging = false;
	}

	protected override void BeginModify()
	{
		xdragging = focus_xl || focus_yp || focus_zp || focus_ol;
		ydragging = focus_yl || focus_zp || focus_xp || focus_ol;
		zdragging = focus_zl || focus_xp || focus_yp || focus_ol;
		if (xdragging || ydragging || zdragging)
		{
			xdragdir = cam.WorldToScreenPoint(GizmoGroup.position + GizmoGroup.right) - base.mousePosition;
			ydragdir = cam.WorldToScreenPoint(GizmoGroup.position + GizmoGroup.up) - base.mousePosition;
			zdragdir = cam.WorldToScreenPoint(GizmoGroup.position + GizmoGroup.forward) - base.mousePosition;
			xdragdir.z = 0f;
			ydragdir.z = 0f;
			zdragdir.z = 0f;
			xdragdir.Normalize();
			ydragdir.Normalize();
			zdragdir.Normalize();
			begin_pos_3d = base.Focus.position;
			begin_tar_scale = base.Focus.localScale;
			new_tar_scale = base.Focus.localScale;
			dragplane = GetDragPos_Plane(out dragorigin);
			if (this.OnBeginTargetScale != null)
			{
				this.OnBeginTargetScale(begin_tar_scale);
			}
		}
	}

	protected override void Modifying()
	{
		if (!xdragging && !ydragging && !zdragging)
		{
			return;
		}
		Vector3 lhs = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0f);
		float num = Mathf.Min(Vector3.Distance(MainCamera.transform.position, begin_pos_3d), 1024f);
		float num2 = Mathf.Min(Vector3.Distance(MainCamera.transform.position, new_tar_scale), 1024f);
		float num3 = Vector3.Dot(lhs, xdragdir) * num2 / 5000f;
		float num4 = Vector3.Dot(lhs, ydragdir) * num2 / 5000f;
		float num5 = Vector3.Dot(lhs, zdragdir) * num2 / 5000f;
		Vector3 vector = new_tar_scale;
		Vector3 vector2 = Vector3.zero;
		if (xdragging && !ydragging && !zdragging)
		{
			vector2 += num3 * Vector3.right;
		}
		if (ydragging && !zdragging && !xdragging)
		{
			vector2 += num4 * Vector3.up;
		}
		if (zdragging && !xdragging && !ydragging)
		{
			vector2 += num5 * Vector3.forward;
		}
		if (dragplane && GetDragPos_Plane(out var pos) && Vector3.Distance(pos, MainCamera.transform.position) < num * 50f)
		{
			vector2 = Vector3.zero;
			if (focus_ol)
			{
				vector2 = (0f - lhs.y * num2 / 5000f) * Vector3.one;
			}
			else
			{
				Vector3 lhs2 = pos - dragorigin;
				vector2.x = Vector3.Dot(lhs2, GizmoGroup.right) * xsign;
				vector2.y = Vector3.Dot(lhs2, GizmoGroup.up) * ysign;
				vector2.z = Vector3.Dot(lhs2, GizmoGroup.forward) * zsign;
			}
			dragorigin = pos;
		}
		new_tar_scale += vector2;
		if (!(vector != new_tar_scale))
		{
			return;
		}
		foreach (Transform target in Targets)
		{
			if (target != null)
			{
				target.localScale += vector2;
			}
		}
		if (this.OnTargetScale != null)
		{
			this.OnTargetScale(new_tar_scale);
		}
	}

	protected override void EndModify()
	{
		if ((xdragging || ydragging || zdragging) && begin_tar_scale != new_tar_scale && this.OnEndTargetScale != null)
		{
			this.OnEndTargetScale(new_tar_scale);
		}
	}

	private bool GetDragPos_Plane(out Vector3 pos)
	{
		pos = Vector3.zero;
		if (xdragging && ydragging && !zdragging)
		{
			Ray ray = MainCamera.ScreenPointToRay(base.mousePosition);
			Plane plane = new Plane(GizmoGroup.forward, begin_pos_3d);
			float enter = 0f;
			if (plane.Raycast(ray, out enter))
			{
				pos = ray.GetPoint(enter);
				return true;
			}
		}
		else if (ydragging && zdragging && !xdragging)
		{
			Ray ray2 = MainCamera.ScreenPointToRay(base.mousePosition);
			Plane plane2 = new Plane(GizmoGroup.right, begin_pos_3d);
			float enter2 = 0f;
			if (plane2.Raycast(ray2, out enter2))
			{
				pos = ray2.GetPoint(enter2);
				return true;
			}
		}
		else if (zdragging && xdragging && !ydragging)
		{
			Ray ray3 = MainCamera.ScreenPointToRay(base.mousePosition);
			Plane plane3 = new Plane(GizmoGroup.up, begin_pos_3d);
			float enter3 = 0f;
			if (plane3.Raycast(ray3, out enter3))
			{
				pos = ray3.GetPoint(enter3);
				return true;
			}
		}
		else if (xdragging && ydragging && zdragging)
		{
			Ray ray4 = MainCamera.ScreenPointToRay(base.mousePosition);
			Plane plane4 = new Plane((base.transform.position - GizmoGroup.position).normalized, begin_pos_3d);
			float enter4 = 0f;
			if (plane4.Raycast(ray4, out enter4))
			{
				pos = ray4.GetPoint(enter4);
				return true;
			}
		}
		return false;
	}

	protected override void OnGL()
	{
		Vector3 position = GizmoGroup.position;
		Vector3 right = GizmoGroup.right;
		Vector3 up = GizmoGroup.up;
		Vector3 forward = GizmoGroup.forward;
		Vector3 v = position + right * 0.81f;
		Vector3 v2 = position + up * 0.81f;
		Vector3 v3 = position + forward * 0.81f;
		Vector3 v4 = position + right * 0.24f * xsign;
		Vector3 v5 = position + up * 0.24f * ysign;
		Vector3 v6 = position + forward * 0.24f * zsign;
		Vector3 v7 = position + right * 0.24f * xsign + up * 0.24f * ysign;
		Vector3 v8 = position + up * 0.24f * ysign + forward * 0.24f * zsign;
		Vector3 v9 = position + forward * 0.24f * zsign + right * 0.24f * xsign;
		GL.Begin(1);
		GL.Color(xlc);
		GL.Vertex(position);
		GL.Vertex(v);
		GL.Color(ylc);
		GL.Vertex(position);
		GL.Vertex(v2);
		GL.Color(zlc);
		GL.Vertex(position);
		GL.Vertex(v3);
		if (!hideCenter)
		{
			GL.Color(new Color(xpc.r, xpc.g, xpc.b, 1f));
			GL.Vertex(position);
			GL.Vertex(v5);
			GL.Vertex(v5);
			GL.Vertex(v8);
			GL.Vertex(v8);
			GL.Vertex(v6);
			GL.Vertex(v6);
			GL.Vertex(position);
			GL.Color(new Color(ypc.r, ypc.g, ypc.b, 1f));
			GL.Vertex(position);
			GL.Vertex(v6);
			GL.Vertex(v6);
			GL.Vertex(v9);
			GL.Vertex(v9);
			GL.Vertex(v4);
			GL.Vertex(v4);
			GL.Vertex(position);
			GL.Color(new Color(zpc.r, zpc.g, zpc.b, 1f));
			GL.Vertex(position);
			GL.Vertex(v4);
			GL.Vertex(v4);
			GL.Vertex(v7);
			GL.Vertex(v7);
			GL.Vertex(v5);
			GL.Vertex(v5);
			GL.Vertex(position);
		}
		GL.End();
		if (!hideCenter)
		{
			GL.Begin(7);
			GL.Color(xpc);
			GL.Vertex(position);
			GL.Vertex(v5);
			GL.Vertex(v8);
			GL.Vertex(v6);
			GL.Color(ypc);
			GL.Vertex(position);
			GL.Vertex(v6);
			GL.Vertex(v9);
			GL.Vertex(v4);
			GL.Color(zpc);
			GL.Vertex(position);
			GL.Vertex(v4);
			GL.Vertex(v7);
			GL.Vertex(v5);
			GL.End();
		}
	}

	private void OnDisable()
	{
		xdragging = false;
		ydragging = false;
		zdragging = false;
		dragplane = false;
		focus_xl = false;
		focus_yl = false;
		focus_zl = false;
		focus_xp = false;
		focus_yp = false;
		focus_zp = false;
		focus_ol = false;
	}
}
