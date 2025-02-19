using UnityEngine;

public abstract class VCEFreeSizeBrush : VCEBrush
{
	public enum EPhase
	{
		Free,
		DragPlane,
		AdjustHeight,
		Drawing
	}

	protected VCEMath.DrawTarget m_Target;

	protected EPhase m_Phase;

	protected Vector3 m_PointBeforeAdjustHeight;

	protected IntVector3 m_Begin;

	protected IntVector3 m_End;

	public VCEGizmoCubeMesh m_GizmoCube;

	protected IntVector3 Min
	{
		get
		{
			if (m_Begin == null || m_End == null)
			{
				return null;
			}
			return new IntVector3(Mathf.Min(m_Begin.x, m_End.x), Mathf.Min(m_Begin.y, m_End.y), Mathf.Min(m_Begin.z, m_End.z));
		}
	}

	protected IntVector3 Max
	{
		get
		{
			if (m_Begin == null || m_End == null)
			{
				return null;
			}
			return new IntVector3(Mathf.Max(m_Begin.x, m_End.x), Mathf.Max(m_Begin.y, m_End.y), Mathf.Max(m_Begin.z, m_End.z));
		}
	}

	protected IntVector3 Size
	{
		get
		{
			if (m_Begin == null || m_End == null)
			{
				return null;
			}
			return Max - Min + IntVector3.One;
		}
	}

	protected void Start()
	{
		m_Target = default(VCEMath.DrawTarget);
		m_Begin = null;
		m_End = null;
		m_GizmoCube.m_Shrink = -0.04f;
		VCEStatusBar.ShowText(BrushDesc().ToLocalizationString(), 7f, typeeffect: true);
	}

	protected void Update()
	{
		if (!VCEditor.DocumentOpen() || VCEditor.SelectedVoxelType < 0)
		{
			m_Phase = EPhase.Free;
			m_GizmoCube.gameObject.SetActive(value: false);
			return;
		}
		float voxelSize = VCEditor.s_Scene.m_Setting.m_VoxelSize;
		m_GizmoCube.m_VoxelSize = voxelSize;
		ExtraAdjust();
		if (m_Phase == EPhase.Free)
		{
			if (!VCEInput.s_MouseOnUI)
			{
				if (VCEInput.s_Cancel)
				{
					ResetDrawing();
					Cancel();
				}
				VCEMath.RayCastDrawTarget(VCEInput.s_PickRay, out m_Target, 128);
				if (VCEditor.s_Scene.m_IsoData.IsPointIn(m_Target.cursor))
				{
					m_GizmoCube.CubeSize = IntVector3.One;
					m_GizmoCube.transform.position = m_Target.cursor.ToVector3() * voxelSize;
					m_GizmoCube.gameObject.SetActive(value: true);
					if (Input.GetMouseButtonDown(0))
					{
						m_Begin = new IntVector3(m_Target.cursor);
						m_End = new IntVector3(m_Target.cursor);
						m_Phase = EPhase.DragPlane;
						VCEditor.Instance.m_UI.DisableFunctions();
						VCEditor.s_ProtectLock0 = true;
						VCEStatusBar.ShowText("Drag an area".ToLocalizationString(), 2f);
					}
				}
				else
				{
					m_GizmoCube.gameObject.SetActive(value: false);
				}
			}
			else
			{
				m_GizmoCube.gameObject.SetActive(value: false);
			}
			VCEditor.Instance.m_NearVoxelIndicator.enabled = true;
		}
		else if (m_Phase == EPhase.DragPlane)
		{
			if (VCEInput.s_Cancel)
			{
				ResetDrawing();
			}
			if (VCEMath.RayCastCoordPlane(VCEInput.s_PickRay, ECoordPlane.XZ, m_Begin.y, out var rch))
			{
				m_End = new IntVector3(Mathf.FloorToInt(rch.point.x), m_Begin.y, Mathf.FloorToInt(rch.point.z));
				VCEditor.s_Scene.m_IsoData.ClampPointI(m_End, (float)m_Begin.x < 0.5f * (float)VCEditor.s_Scene.m_IsoData.m_HeadInfo.xSize);
				m_PointBeforeAdjustHeight = rch.point;
				VCEditor.s_Scene.m_IsoData.ClampPointI(m_End, (float)m_Begin.x < 0.5f * (float)VCEditor.s_Scene.m_IsoData.m_HeadInfo.xSize);
				m_GizmoCube.CubeSize = Size;
				m_GizmoCube.transform.position = Min.ToVector3() * voxelSize;
			}
			else
			{
				ResetDrawing();
			}
			if (Input.GetMouseButtonUp(0))
			{
				m_Phase = EPhase.AdjustHeight;
				VCEStatusBar.ShowText("Adjust height".ToLocalizationString(), 2f);
			}
			VCEditor.Instance.m_NearVoxelIndicator.enabled = false;
		}
		else if (m_Phase == EPhase.AdjustHeight)
		{
			if (VCEInput.s_Cancel)
			{
				ResetDrawing();
			}
			float height = m_PointBeforeAdjustHeight.y;
			VCEMath.RayAdjustHeight(VCEInput.s_PickRay, m_PointBeforeAdjustHeight, out height);
			m_End.y = Mathf.FloorToInt(height + 0.3f);
			VCEditor.s_Scene.m_IsoData.ClampPointI(m_End);
			m_GizmoCube.CubeSize = Size;
			m_GizmoCube.transform.position = Min.ToVector3() * voxelSize;
			if (Input.GetMouseButtonDown(0))
			{
				m_Phase = EPhase.Drawing;
				Do();
				VCEStatusBar.ShowText("Done".ToLocalizationString(), 2f);
			}
			VCEditor.Instance.m_NearVoxelIndicator.enabled = false;
		}
	}

	protected void ResetDrawing()
	{
		m_Phase = EPhase.Free;
		m_GizmoCube.gameObject.SetActive(value: false);
		m_GizmoCube.CubeSize = IntVector3.One;
		VCEditor.Instance.m_UI.EnableFunctions();
		VCEditor.s_ProtectLock0 = false;
	}

	public override void Cancel()
	{
		if (VCEditor.DocumentOpen())
		{
			VCEditor.Instance.m_UI.m_DefaultBrush_Material.isChecked = true;
			VCEditor.Instance.m_NearVoxelIndicator.enabled = true;
		}
	}

	protected abstract string BrushDesc();

	protected virtual void ExtraAdjust()
	{
	}

	protected virtual void ExtraGUI()
	{
	}

	private void OnGUI()
	{
		GUI.skin = VCEditor.Instance.m_GUISkin;
		if (m_Phase != 0)
		{
			IntVector3 size = Size;
			string text = size.x + " x " + size.z + " x " + size.y;
			GUI.color = new Color(1f, 1f, 1f, 0.8f);
			GUI.Label(new Rect(Input.mousePosition.x + 24f, (float)Screen.height - Input.mousePosition.y + 26f, 100f, 100f), text, "CursorText2");
		}
		ExtraGUI();
	}
}
