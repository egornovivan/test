using UnityEngine;

public class BSFreeSizeBrush : BSBrush
{
	public enum EPhase
	{
		Free,
		DragPlane,
		AdjustHeight,
		Drawing
	}

	[SerializeField]
	protected BSGizmoTriggerEvent gizmoTrigger;

	public Vector3 maxDragSize;

	protected BSMath.DrawTarget m_Target;

	protected EPhase m_Phase;

	protected Vector3 m_Begin;

	protected Vector3 m_End;

	public BSGizmoCubeMesh gizmoCube;

	public ECoordPlane DragPlane = ECoordPlane.XZ;

	private Vector3 m_PointBeforeAdjustHeight;

	protected Vector3 m_Cursor = Vector3.zero;

	protected Vector3 m_GizmoCursor = Vector3.zero;

	private Vector3 _beginPos = Vector3.zero;

	private Vector3 _prevMousePos = Vector3.zero;

	private Vector3 m_EndOffset = Vector3.zero;

	public string ExtraTips = string.Empty;

	private Vector3 _glCenter;

	public float circleRadius = 3f;

	private bool _drawGL = true;

	public override Bounds brushBound => gizmoTrigger.boxCollider.bounds;

	protected Vector3 Min => new Vector3(Mathf.Min(m_Begin.x, m_End.x), Mathf.Min(m_Begin.y, m_End.y), Mathf.Min(m_Begin.z, m_End.z));

	protected Vector3 Max => new Vector3(Mathf.Max(m_Begin.x, m_End.x), Mathf.Max(m_Begin.y, m_End.y), Mathf.Max(m_Begin.z, m_End.z));

	public Vector3 Size => (Max - Min) * dataSource.ScaleInverted;

	protected void Start()
	{
		GlobalGLs.AddGL(this);
	}

	protected void Update()
	{
		if (dataSource == null || pattern == null || !ExtraAdjust() || GameConfig.IsInVCE)
		{
			return;
		}
		gizmoCube.m_VoxelSize = dataSource.Scale;
		if (m_Phase == EPhase.Free)
		{
			if (BSInput.s_MouseOnUI)
			{
				gizmoCube.gameObject.SetActive(value: false);
				return;
			}
			if (BSInput.s_Cancel)
			{
				ResetDrawing();
			}
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			bool ignoreDiagonal = mode == EBSBrushMode.Add;
			if (BSMath.RayCastDrawTarget(ray, dataSource, out m_Target, minvol, ignoreDiagonal, BuildingMan.Datas))
			{
				Vector3 vector = Vector3.zero;
				if (mode == EBSBrushMode.Add)
				{
					vector = BSBrush.CalcCursor(m_Target, dataSource, pattern.size);
					m_Cursor = m_Target.rch.point;
				}
				else if (mode == EBSBrushMode.Subtract)
				{
					vector = BSBrush.CalcSnapto(m_Target, dataSource, pattern);
					m_Cursor = m_Target.rch.point;
				}
				m_GizmoCursor = vector;
				_drawGL = true;
				gizmoCube.CubeSize = new IntVector3(pattern.size, pattern.size, pattern.size);
				gizmoCube.gameObject.SetActive(value: true);
				gizmoCube.transform.position = vector + dataSource.Offset;
				UpdateGizmoTrigger();
				float num = (float)pattern.size * dataSource.Scale;
				Vector3 vector2 = vector;
				Vector3 end = vector2 + new Vector3(num, num, num);
				_glCenter = vector + new Vector3(num, num, num) * 0.5f + dataSource.Offset;
				_glCenter.y = vector2.y;
				if (Input.GetMouseButtonDown(0))
				{
					m_Begin = vector2;
					_beginPos = vector2;
					m_End = end;
					m_Phase = EPhase.DragPlane;
					_prevMousePos = Input.mousePosition;
				}
			}
			else
			{
				ResetDrawing();
			}
			if (Input.GetKeyDown(KeyCode.G))
			{
				if (DragPlane == ECoordPlane.XZ)
				{
					DragPlane = ECoordPlane.ZY;
				}
				else if (DragPlane == ECoordPlane.ZY)
				{
					DragPlane = ECoordPlane.XY;
				}
				else if (DragPlane == ECoordPlane.XY)
				{
					DragPlane = ECoordPlane.XZ;
				}
				Debug.Log("Switch the drag reference plane : " + DragPlane);
			}
		}
		else if (m_Phase == EPhase.DragPlane)
		{
			if (BSInput.s_Cancel)
			{
				ResetDrawing();
				return;
			}
			Ray ray2 = Camera.main.ScreenPointToRay(Input.mousePosition);
			float position = 0f;
			if (DragPlane == ECoordPlane.XY)
			{
				position = m_Cursor.z;
			}
			else if (DragPlane == ECoordPlane.XZ)
			{
				position = m_Cursor.y;
			}
			else if (DragPlane == ECoordPlane.ZY)
			{
				position = m_Cursor.x;
			}
			if (BSMath.RayCastCoordPlane(ray2, DragPlane, position, out var rch))
			{
				m_PointBeforeAdjustHeight = rch.point;
				if (!object.Equals(_prevMousePos, Input.mousePosition))
				{
					Vector3 vector3 = rch.point - dataSource.Offset;
					if (DragPlane == ECoordPlane.XZ)
					{
						float outPut = 0f;
						m_Begin.x = CalcValue(vector3.x, _beginPos.x, out outPut);
						float outPut2 = 0f;
						m_Begin.z = CalcValue(vector3.z, _beginPos.z, out outPut2);
						m_End = new Vector3(outPut, _beginPos.y + (float)pattern.size * dataSource.Scale, outPut2);
						m_End = Clamp(m_Begin, m_End);
					}
					else if (DragPlane == ECoordPlane.XY)
					{
						float outPut3 = 0f;
						m_Begin.x = CalcValue(vector3.x, _beginPos.x, out outPut3);
						float outPut4 = 0f;
						m_Begin.y = CalcValue(vector3.y, _beginPos.y, out outPut4);
						m_End = new Vector3(outPut3, outPut4, _beginPos.z + (float)pattern.size * dataSource.Scale);
						m_End = Clamp(m_Begin, m_End);
					}
					else if (DragPlane == ECoordPlane.ZY)
					{
						float outPut5 = 0f;
						m_Begin.y = CalcValue(vector3.y, _beginPos.y, out outPut5);
						float outPut6 = 0f;
						m_Begin.z = CalcValue(vector3.z, _beginPos.z, out outPut6);
						m_End = new Vector3(_beginPos.x + (float)pattern.size * dataSource.Scale, outPut5, outPut6);
						m_End = Clamp(m_Begin, m_End);
					}
					DragPlaneExtraDo(DragPlane);
					gizmoCube.transform.position = Min + dataSource.Offset;
					gizmoCube.CubeSize = new IntVector3((int)Size.x, (int)Size.y, (int)Size.z);
					UpdateGizmoTrigger();
				}
				if (Input.GetMouseButtonUp(0))
				{
					m_Phase = EPhase.AdjustHeight;
					_prevMousePos = new Vector3(-100f, -100f, -100f);
				}
			}
		}
		else if (m_Phase == EPhase.AdjustHeight)
		{
			if (BSInput.s_Cancel)
			{
				ResetDrawing();
				return;
			}
			Ray ray3 = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (DragPlane == ECoordPlane.XZ)
			{
				float height = m_PointBeforeAdjustHeight.y;
				BSMath.RayAdjustHeight(ray3, ECoordAxis.Y, m_PointBeforeAdjustHeight, out height);
				float outPut7 = 0f;
				m_Begin.y = CalcValue(height, _beginPos.y, out outPut7);
				m_End.y = outPut7 + m_EndOffset.y;
			}
			else if (DragPlane == ECoordPlane.XY)
			{
				float height2 = m_PointBeforeAdjustHeight.z;
				BSMath.RayAdjustHeight(ray3, ECoordAxis.Z, m_PointBeforeAdjustHeight, out height2);
				float outPut8 = 0f;
				m_Begin.z = CalcValue(height2, _beginPos.z, out outPut8);
				m_End.z = outPut8 + m_EndOffset.z;
			}
			else if (DragPlane == ECoordPlane.ZY)
			{
				float height3 = m_PointBeforeAdjustHeight.x;
				BSMath.RayAdjustHeight(ray3, ECoordAxis.X, m_PointBeforeAdjustHeight, out height3);
				float outPut9 = 0f;
				m_Begin.x = CalcValue(height3, _beginPos.x, out outPut9);
				m_End.x = outPut9 + m_EndOffset.x;
			}
			m_End = Clamp(m_Begin, m_End);
			AdjustHeightExtraDo(DragPlane);
			gizmoCube.transform.position = Min + dataSource.Offset;
			gizmoCube.CubeSize = new IntVector3((int)Size.x, (int)Size.y, (int)Size.z);
			UpdateGizmoTrigger();
			if (PeInput.Get(PeInput.LogicFunction.Build))
			{
				Do();
				m_Phase = EPhase.Free;
			}
		}
		AfterDo();
	}

	private void UpdateGizmoTrigger()
	{
		if ((bool)gizmoTrigger)
		{
			Vector3 vector = new Vector3((float)gizmoCube.CubeSize.x * dataSource.Scale, (float)gizmoCube.CubeSize.y * dataSource.Scale, (float)gizmoCube.CubeSize.z * dataSource.Scale);
			gizmoTrigger.boxCollider.size = vector;
			gizmoTrigger.boxCollider.center = vector * 0.5f;
		}
	}

	private void FineTuningHeight(ECoordAxis axis)
	{
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			switch (axis)
			{
			case ECoordAxis.X:
				m_EndOffset.x += dataSource.Scale * (float)pattern.size;
				break;
			case ECoordAxis.Y:
				m_EndOffset.y += dataSource.Scale * (float)pattern.size;
				break;
			case ECoordAxis.Z:
				m_EndOffset.z += dataSource.Scale * (float)pattern.size;
				break;
			}
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			switch (axis)
			{
			case ECoordAxis.X:
				m_EndOffset.x -= dataSource.Scale * (float)pattern.size;
				break;
			case ECoordAxis.Y:
				m_EndOffset.y -= dataSource.Scale * (float)pattern.size;
				break;
			case ECoordAxis.Z:
				m_EndOffset.z -= dataSource.Scale * (float)pattern.size;
				break;
			}
		}
	}

	private float CalcValue(float end, float beign, out float outPut)
	{
		float num = Mathf.Sign(end - beign);
		float num2 = ((!(num >= 0f)) ? pattern.size : 0);
		int num3 = Mathf.FloorToInt((end - beign) * (float)dataSource.ScaleInverted);
		int num4 = Mathf.Abs(num3 % pattern.size);
		num4 = ((!(num > 0f)) ? ((num4 != 0) ? (pattern.size - num4) : 0) : (pattern.size - num4));
		outPut = Mathf.Floor(end * (float)dataSource.ScaleInverted) * dataSource.Scale + (float)num4 * dataSource.Scale * num;
		return beign + num2 * dataSource.Scale;
	}

	protected void ResetDrawing()
	{
		m_Phase = EPhase.Free;
		gizmoCube.CubeSize = new IntVector3(pattern.size, pattern.size, pattern.size);
		gizmoCube.gameObject.SetActive(value: false);
		m_EndOffset = Vector3.zero;
		_drawGL = false;
	}

	private Vector3 Clamp(Vector3 begin, Vector3 end)
	{
		Vector3 zero = Vector3.zero;
		Vector3 vector = maxDragSize * dataSource.Scale;
		zero.x = begin.x + Mathf.Clamp(end.x - begin.x, 0f - vector.x, vector.x);
		zero.y = begin.y + Mathf.Clamp(end.y - begin.y, 0f - vector.y, vector.y);
		zero.z = begin.z + Mathf.Clamp(end.z - begin.z, 0f - vector.z, vector.z);
		return zero;
	}

	protected override void Do()
	{
	}

	protected virtual bool ExtraAdjust()
	{
		return true;
	}

	protected virtual void AfterDo()
	{
	}

	protected virtual void AdjustHeightExtraDo(ECoordPlane drag_plane)
	{
	}

	protected virtual void DragPlaneExtraDo(ECoordPlane drag_plane)
	{
	}

	private void OnGUI()
	{
		if (BuildingMan.Self != null)
		{
			GUI.skin = BuildingMan.Self.guiSkin;
		}
		if (m_Phase != 0)
		{
			IntVector3 intVector = Size;
			string text = intVector.x + " x " + intVector.z + " x " + intVector.y;
			GUI.color = new Color(1f, 1f, 1f, 0.8f);
			GUI.Label(new Rect(Input.mousePosition.x + 24f, (float)Screen.height - Input.mousePosition.y + 26f, 100f, 100f), text, "CursorText2");
			if (!string.IsNullOrEmpty(ExtraTips))
			{
				GUI.color = Color.yellow;
				GUI.Label(new Rect(Input.mousePosition.x + 24f, (float)Screen.height - Input.mousePosition.y + 66f, 100f, 100f), "(" + ExtraTips + ")", "CursorText2");
				GUI.color = Color.white;
			}
		}
	}

	public virtual bool CanDrawGL()
	{
		return true;
	}

	public override void OnGL()
	{
		if (dataSource == null || pattern == null || !base.gameObject.activeInHierarchy || !base.enabled || !_drawGL || !CanDrawGL() || BSInput.s_MouseOnUI)
		{
			return;
		}
		if (m_Material == null)
		{
			m_Material = new Material(Shader.Find("Unlit/Transparent Colored"));
			m_Material.hideFlags = HideFlags.HideAndDontSave;
			m_Material.shader.hideFlags = HideFlags.HideAndDontSave;
		}
		GL.PushMatrix();
		Color color = Color.white;
		if (DragPlane == ECoordPlane.XZ)
		{
			color = new Color(0f, 1f, 0.07f, 0.05f);
		}
		else if (DragPlane == ECoordPlane.XY)
		{
			color = new Color(0.29f, 0.5f, 0.9f, 0.05f);
		}
		else if (DragPlane == ECoordPlane.ZY)
		{
			color = new Color(1f, 0.32f, 0.42f, 0.05f);
		}
		Vector3 vector = ((!(Camera.main == null)) ? Camera.main.transform.forward : Vector3.up);
		if (DragPlane == ECoordPlane.XZ)
		{
			for (int i = 0; i < m_Material.passCount; i++)
			{
				m_Material.SetPass(i);
				GL.Begin(1);
				Color c = color * 1.2f;
				c.a = 0.7f;
				GL.Color(c);
				Vector3 glCenter = _glCenter;
				GL.Vertex3(glCenter.x, glCenter.y - 0.2f, glCenter.z);
				GL.Vertex3(glCenter.x, glCenter.y + 2.5f, glCenter.z);
				GL.Vertex3(glCenter.x, glCenter.y + 2.5f, glCenter.z);
				GL.Vertex3(glCenter.x + 0.2f, glCenter.y + 2f, glCenter.z);
				GL.Vertex3(glCenter.x, glCenter.y + 2.5f, glCenter.z);
				GL.Vertex3(glCenter.x - 0.2f, glCenter.y + 2f, glCenter.z);
				GL.Vertex3(glCenter.x, glCenter.y + 2.5f, glCenter.z);
				GL.Vertex3(glCenter.x, glCenter.y + 2f, glCenter.z + 0.2f);
				GL.Vertex3(glCenter.x, glCenter.y + 2.5f, glCenter.z);
				GL.Vertex3(glCenter.x, glCenter.y + 2f, glCenter.z - 0.2f);
				GL.End();
			}
		}
		else if (DragPlane == ECoordPlane.XY)
		{
			for (int j = 0; j < m_Material.passCount; j++)
			{
				m_Material.SetPass(j);
				GL.Begin(1);
				Color c2 = color * 1.2f;
				c2.a = 0.7f;
				GL.Color(c2);
				int num = ((!(vector.z >= 0f)) ? 1 : (-1));
				Vector3 glCenter2 = _glCenter;
				GL.Vertex3(glCenter2.x, glCenter2.y, glCenter2.z - 0.2f * (float)num);
				GL.Vertex3(glCenter2.x, glCenter2.y, glCenter2.z + 2.5f * (float)num);
				GL.Vertex3(glCenter2.x, glCenter2.y, glCenter2.z + 2.5f * (float)num);
				GL.Vertex3(glCenter2.x + 0.2f, glCenter2.y, glCenter2.z + 2f * (float)num);
				GL.Vertex3(glCenter2.x, glCenter2.y, glCenter2.z + 2.5f * (float)num);
				GL.Vertex3(glCenter2.x - 0.2f, glCenter2.y, glCenter2.z + 2f * (float)num);
				GL.Vertex3(glCenter2.x, glCenter2.y, glCenter2.z + 2.5f * (float)num);
				GL.Vertex3(glCenter2.x, glCenter2.y + 0.2f, glCenter2.z + 2f * (float)num);
				GL.Vertex3(glCenter2.x, glCenter2.y, glCenter2.z + 2.5f * (float)num);
				GL.Vertex3(glCenter2.x, glCenter2.y - 0.2f, glCenter2.z + 2f * (float)num);
				GL.End();
			}
		}
		else if (DragPlane == ECoordPlane.ZY)
		{
			for (int k = 0; k < m_Material.passCount; k++)
			{
				m_Material.SetPass(k);
				GL.Begin(1);
				Color c3 = color * 1.2f;
				c3.a = 0.7f;
				GL.Color(c3);
				int num2 = ((!(vector.x >= 0f)) ? 1 : (-1));
				Vector3 glCenter3 = _glCenter;
				GL.Vertex3(glCenter3.x - 0.2f * (float)num2, glCenter3.y, glCenter3.z);
				GL.Vertex3(glCenter3.x + 2.5f * (float)num2, glCenter3.y, glCenter3.z);
				GL.Vertex3(glCenter3.x + 2.5f * (float)num2, glCenter3.y, glCenter3.z);
				GL.Vertex3(glCenter3.x + 2f * (float)num2, glCenter3.y, glCenter3.z + 0.2f);
				GL.Vertex3(glCenter3.x + 2.5f * (float)num2, glCenter3.y, glCenter3.z);
				GL.Vertex3(glCenter3.x + 2f * (float)num2, glCenter3.y, glCenter3.z - 0.2f);
				GL.Vertex3(glCenter3.x + 2.5f * (float)num2, glCenter3.y, glCenter3.z);
				GL.Vertex3(glCenter3.x + 2f * (float)num2, glCenter3.y + 0.2f, glCenter3.z);
				GL.Vertex3(glCenter3.x + 2.5f * (float)num2, glCenter3.y, glCenter3.z);
				GL.Vertex3(glCenter3.x + 2f * (float)num2, glCenter3.y - 0.2f, glCenter3.z);
				GL.End();
			}
		}
		GL.PopMatrix();
	}
}
