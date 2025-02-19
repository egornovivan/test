using System;
using System.Collections.Generic;
using UnityEngine;
using WhiteCat;

public abstract class TransformGizmo : MonoBehaviour
{
	public Camera MainCamera;

	public bool useCustomMouse;

	public Vector3 customMousePosition;

	public Material material;

	public bool hideCenter;

	public List<Transform> Targets;

	public Space Orientation;

	[SerializeField]
	protected Transform GizmoGroup;

	[SerializeField]
	protected Color XColor;

	[SerializeField]
	protected Color YColor;

	[SerializeField]
	protected Color ZColor;

	[SerializeField]
	protected Color HColor;

	protected Material linemat;

	protected Camera cam;

	public Transform Focus
	{
		get
		{
			if (Targets == null || Targets.Count == 0)
			{
				return null;
			}
			return Targets[Targets.Count - 1];
		}
	}

	public virtual bool MouseOver => false;

	public virtual bool Working => false;

	public Vector3 mousePosition => (!useCustomMouse) ? Input.mousePosition : customMousePosition;

	protected virtual void Awake()
	{
		CreateLineMaterials();
		Targets = new List<Transform>(4);
	}

	protected virtual void Start()
	{
		cam = base.gameObject.GetComponent<Camera>();
		if (cam == null)
		{
			cam = base.gameObject.AddComponent<Camera>();
			cam.backgroundColor = Color.clear;
			cam.cullingMask = 1 << base.gameObject.layer;
			cam.clearFlags = CameraClearFlags.Depth;
			cam.orthographic = false;
			cam.depth = 10000f;
			cam.farClipPlane = 10f;
			cam.nearClipPlane = 3f;
			cam.renderingPath = RenderingPath.Forward;
			cam.useOcclusionCulling = false;
			cam.hdr = false;
		}
		cam.fieldOfView = MainCamera.fieldOfView;
		cam.targetTexture = MainCamera.targetTexture;
		cam.enabled = true;
	}

	protected virtual void OnDestroy()
	{
		UnityEngine.Object.Destroy(linemat);
	}

	private void Update()
	{
		for (int num = Targets.Count - 1; num >= 0; num--)
		{
			if (Targets[num] == null)
			{
				Targets.RemoveAt(num);
			}
		}
		bool active = Focus != null && MainCamera != null;
		GizmoGroup.gameObject.SetActive(active);
		cam.enabled = active;
		if (!(Focus != null))
		{
			return;
		}
		if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
		{
			if (Input.GetMouseButtonUp(0))
			{
				EndModify();
			}
			Idle();
			return;
		}
		if (Input.GetMouseButtonDown(0))
		{
			BeginModify();
		}
		if (Input.GetMouseButton(0))
		{
			Modifying();
		}
	}

	private void LateUpdate()
	{
		if (Focus != null)
		{
			cam.fieldOfView = MainCamera.fieldOfView;
			Vector3 normalized = (MainCamera.transform.position - Focus.position).normalized;
			float f = Mathf.Clamp(Vector3.Angle(MainCamera.transform.forward, -normalized), -80f, 80f) * ((float)Math.PI / 180f);
			float num = 7.5f / Mathf.Cos(f);
			base.transform.position = Focus.position + normalized * num;
			base.transform.rotation = MainCamera.transform.rotation;
			GizmoGroup.position = Focus.position;
			if (Orientation == Space.Self)
			{
				GizmoGroup.rotation = Focus.rotation;
			}
			else
			{
				GizmoGroup.rotation = Quaternion.identity;
			}
			CustomLateUpdate();
		}
	}

	private void OnPostRender()
	{
		if (Focus != null)
		{
			GL.PushMatrix();
			if (linemat.SetPass(0))
			{
				OnGL();
			}
			GL.PopMatrix();
		}
	}

	protected abstract void OnGL();

	protected virtual void CustomLateUpdate()
	{
	}

	protected abstract void Idle();

	protected abstract void BeginModify();

	protected abstract void Modifying();

	protected abstract void EndModify();

	private void CreateLineMaterials()
	{
		if (!linemat)
		{
			if ((bool)material)
			{
				linemat = UnityEngine.Object.Instantiate(material);
			}
			else
			{
				linemat = UnityEngine.Object.Instantiate(PEVCConfig.instance.handleMaterial);
			}
			linemat.hideFlags = HideFlags.HideAndDontSave;
			linemat.shader.hideFlags = HideFlags.HideAndDontSave;
		}
	}
}
