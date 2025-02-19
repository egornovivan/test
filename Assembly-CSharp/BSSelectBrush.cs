using System.Collections.Generic;
using BSTools;
using UnityEngine;

public class BSSelectBrush : BSBrush
{
	public const int MaxVoxelCount = 10000;

	protected Dictionary<IntVector3, byte> m_Selections = new Dictionary<IntVector3, byte>();

	protected List<BSTools.SelBox> m_SelectionBoxes = new List<BSTools.SelBox>();

	private bool m_RecalcBoxes;

	public float Depth = 1f;

	public Gradient voxelsCountColor;

	public int maxDragSize = 32;

	public Vector3 maxSelectBoxSize = new Vector3(128f, 128f, 128f);

	protected Vector3 m_Begin;

	protected Vector3 m_End;

	private ECoordPlane m_Coord = ECoordPlane.XZ;

	private float m_PlanePos;

	protected bool m_Selecting;

	private BSMath.DrawTarget m_Target;

	private bool m_Drawing;

	public BSGLSelectionBoxes seletionBoxeRenderer;

	private bool m_IsValidBox = true;

	protected IBSDataSource m_PrevDS;

	protected BSAction m_Action = new BSAction();

	public bool canDo = true;

	private Vector3 _beginPos = Vector3.zero;

	private IBSDataSource _datasource;

	private float m_GUIAlpha;

	public Dictionary<IntVector3, byte> Selections => m_Selections;

	public override Bounds brushBound
	{
		get
		{
			BSTools.IntBox intBox = BSTools.SelBox.CalculateBound(m_SelectionBoxes);
			Bounds result = default(Bounds);
			result.min = new Vector3(intBox.xMin, intBox.yMin, intBox.zMin);
			result.max = new Vector3(intBox.xMax, intBox.yMax, intBox.zMax);
			if (dataSource == BuildingMan.Blocks)
			{
				result.min *= 0.5f;
				result.max *= 0.5f;
			}
			return result;
		}
	}

	public void AddSelection(IntVector3 ipos, byte val)
	{
		m_Selections.Add(ipos, val);
		m_RecalcBoxes = true;
	}

	public void RemoveSelection(IntVector3 ipos)
	{
		m_Selections.Remove(ipos);
		m_RecalcBoxes = true;
	}

	public void ClearSelection(BSAction action)
	{
		if (action == null)
		{
			m_Selections.Clear();
			m_RecalcBoxes = true;
			return;
		}
		Dictionary<IntVector3, byte> old_value = new Dictionary<IntVector3, byte>(m_Selections);
		m_Selections.Clear();
		Dictionary<IntVector3, byte> new_value = new Dictionary<IntVector3, byte>(m_Selections);
		BSSelectedBoxModify modify = new BSSelectedBoxModify(old_value, new_value, this);
		action.AddModify(modify);
		m_RecalcBoxes = true;
	}

	public void ResetSelection(Dictionary<IntVector3, byte> selection)
	{
		m_Selections = selection;
		CalcBoxes();
	}

	public bool IsEmpty()
	{
		return m_Selections.Count == 0;
	}

	protected void CalcBoxes()
	{
		BSTools.LeastBox.Calculate(m_Selections, ref m_SelectionBoxes);
	}

	protected void Start()
	{
		GlobalGLs.AddGL(this);
	}

	protected void OnDestroy()
	{
		ClearSelection(null);
	}

	protected void Update()
	{
		if (dataSource == null || BSInput.s_MouseOnUI)
		{
			return;
		}
		if (_datasource == dataSource)
		{
			ClearSelection(null);
			_datasource = dataSource;
		}
		if (seletionBoxeRenderer != null)
		{
			seletionBoxeRenderer.m_Boxes = m_SelectionBoxes;
			seletionBoxeRenderer.scale = dataSource.Scale;
			seletionBoxeRenderer.offset = dataSource.Offset;
		}
		if (m_RecalcBoxes)
		{
			CalcBoxes();
			m_RecalcBoxes = false;
		}
		if (GameConfig.IsInVCE)
		{
			return;
		}
		if (!BSInput.s_Shift && Input.GetKeyDown(KeyCode.UpArrow))
		{
			Depth = ((!((Depth += 1f) >= (float)maxDragSize)) ? Depth : ((float)maxDragSize));
			m_GUIAlpha = 5f;
		}
		else if (!BSInput.s_Shift && Input.GetKeyDown(KeyCode.DownArrow))
		{
			Depth = ((!((Depth -= 1f) >= 1f)) ? 1f : Depth);
			m_GUIAlpha = 5f;
		}
		m_GUIAlpha = Mathf.Lerp(m_GUIAlpha, 0f, 0.05f);
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (!m_Selecting)
		{
			if (BSInput.s_Cancel)
			{
				if (canDo)
				{
					Dictionary<IntVector3, byte> dictionary = new Dictionary<IntVector3, byte>(m_Selections);
					Cancel();
					Dictionary<IntVector3, byte> new_value = new Dictionary<IntVector3, byte>(m_Selections);
					if (dictionary.Count != 0)
					{
						BSSelectedBoxModify modify = new BSSelectedBoxModify(dictionary, new_value, this);
						m_Action.AddModify(modify);
						DoHistory();
					}
				}
				return;
			}
			if (BSMath.RayCastDrawTarget(ray, dataSource, out m_Target, minvol, ignoreDiagonal: true, BuildingMan.Datas))
			{
				m_Drawing = true;
				m_Begin = m_Target.snapto;
				_beginPos = m_Begin;
				m_End = m_Begin;
				float num = Mathf.Abs(m_Target.rch.normal.x);
				float num2 = Mathf.Abs(m_Target.rch.normal.y);
				float num3 = Mathf.Abs(m_Target.rch.normal.z);
				if (num > 0.9f * m_Target.ds.Scale)
				{
					m_Coord = ECoordPlane.ZY;
					m_PlanePos = m_Target.rch.point.x;
				}
				else if (num2 > 0.9f * m_Target.ds.Scale)
				{
					m_Coord = ECoordPlane.XZ;
					m_PlanePos = m_Target.rch.point.y;
				}
				else if (num3 > 0.9f * m_Target.ds.Scale)
				{
					m_Coord = ECoordPlane.XY;
					m_PlanePos = m_Target.rch.point.z;
				}
				if (Input.GetMouseButtonDown(0) && canDo)
				{
					m_Selecting = true;
				}
			}
			else
			{
				m_Drawing = false;
			}
		}
		else
		{
			m_Drawing = true;
			if (BSInput.s_Cancel)
			{
				if (canDo)
				{
					m_Selecting = false;
					m_Begin = new Vector3(-10000f, -10000f, -10000f);
					m_End = new Vector3(-10000f, -10000f, -10000f);
				}
				return;
			}
			if (BSMath.RayCastCoordPlane(ray, m_Coord, m_PlanePos, out var rch))
			{
				Vector3 vector = rch.point - dataSource.Offset;
				if (m_Coord == ECoordPlane.XY)
				{
					float outPut = 0f;
					m_Begin.x = CalcValue(vector.x, _beginPos.x, out outPut);
					float outPut2 = 0f;
					m_Begin.y = CalcValue(vector.y, _beginPos.y, out outPut2);
					m_End.x = outPut;
					m_End.y = outPut2;
					m_End.z = (float)Mathf.FloorToInt(vector.z * (float)dataSource.ScaleInverted) * dataSource.Scale;
					if (m_Target.rch.normal.z > 0f)
					{
						m_Begin.z = m_PlanePos - Depth * dataSource.Scale;
					}
					else
					{
						m_Begin.z = m_PlanePos + Depth * dataSource.Scale;
					}
					m_End = Clamp(m_Begin, m_End);
				}
				else if (m_Coord == ECoordPlane.XZ)
				{
					float outPut3 = 0f;
					m_Begin.x = CalcValue(vector.x, _beginPos.x, out outPut3);
					float outPut4 = 0f;
					m_Begin.z = CalcValue(vector.z, _beginPos.z, out outPut4);
					m_End.x = outPut3;
					m_End.y = (float)Mathf.FloorToInt(vector.y * (float)dataSource.ScaleInverted) * dataSource.Scale;
					m_End.z = outPut4;
					if (m_Target.rch.normal.y > 0f)
					{
						m_Begin.y = m_PlanePos - Depth * dataSource.Scale;
					}
					else
					{
						m_Begin.y = m_PlanePos + Depth * dataSource.Scale;
					}
					m_End = Clamp(m_Begin, m_End);
				}
				else if (m_Coord == ECoordPlane.ZY)
				{
					float outPut5 = 0f;
					m_Begin.y = CalcValue(vector.y, _beginPos.y, out outPut5);
					float outPut6 = 0f;
					m_Begin.z = CalcValue(vector.z, _beginPos.z, out outPut6);
					m_End.x = (float)Mathf.FloorToInt(vector.x * (float)dataSource.ScaleInverted) * dataSource.Scale;
					m_End.y = outPut5;
					m_End.z = outPut6;
					if (m_Target.rch.normal.x > 0f)
					{
						m_Begin.x = m_PlanePos - Depth * dataSource.Scale;
					}
					else
					{
						m_Begin.x = m_PlanePos + Depth * dataSource.Scale;
					}
					m_End = Clamp(m_Begin, m_End);
				}
			}
			if (m_PrevDS != null && m_PrevDS == dataSource)
			{
				if (m_Selections.Count != 0)
				{
					BSTools.SelBox selBox = new BSTools.SelBox();
					selBox.m_Box.xMin = (short)Mathf.FloorToInt(Mathf.Min(m_Begin.x, m_End.x) * (float)dataSource.ScaleInverted);
					selBox.m_Box.yMin = (short)Mathf.FloorToInt(Mathf.Min(m_Begin.y, m_End.y) * (float)dataSource.ScaleInverted);
					selBox.m_Box.zMin = (short)Mathf.FloorToInt(Mathf.Min(m_Begin.z, m_End.z) * (float)dataSource.ScaleInverted);
					selBox.m_Box.xMax = (short)Mathf.FloorToInt(Mathf.Max(m_Begin.x, m_End.x) * (float)dataSource.ScaleInverted);
					selBox.m_Box.yMax = (short)Mathf.FloorToInt(Mathf.Max(m_Begin.y, m_End.y) * (float)dataSource.ScaleInverted);
					selBox.m_Box.zMax = (short)Mathf.FloorToInt(Mathf.Max(m_Begin.z, m_End.z) * (float)dataSource.ScaleInverted);
					m_SelectionBoxes.Add(selBox);
					BSTools.IntBox intBox = BSTools.SelBox.CalculateBound(m_SelectionBoxes);
					Vector3 vector2 = new Vector3(intBox.xMax - intBox.xMin, intBox.yMax - intBox.yMin, intBox.zMax - intBox.zMin);
					if (vector2.x > maxSelectBoxSize.x || vector2.y > maxSelectBoxSize.y || vector2.z > maxSelectBoxSize.z)
					{
						m_IsValidBox = false;
					}
					else
					{
						m_IsValidBox = true;
					}
					m_SelectionBoxes.RemoveAt(m_SelectionBoxes.Count - 1);
				}
				else
				{
					m_IsValidBox = true;
				}
			}
			else
			{
				m_IsValidBox = true;
			}
			if (Input.GetMouseButtonUp(0))
			{
				if (!canDo)
				{
					return;
				}
				if (m_IsValidBox)
				{
					if (m_PrevDS == dataSource)
					{
						if (BSInput.s_Shift)
						{
							if (m_PrevDS == dataSource)
							{
								CalcSelection(0);
							}
						}
						else if (BSInput.s_Alt)
						{
							CalcSelection(1);
						}
						else if (BSInput.s_Control)
						{
							CalcSelection(2);
						}
						else
						{
							ClearSelection(m_Action);
							CalcSelection(0);
						}
						m_PrevDS = dataSource;
					}
					else
					{
						ClearSelection(m_Action);
						CalcSelection(0);
						m_PrevDS = dataSource;
					}
				}
				m_Selecting = false;
				ResetValue();
			}
		}
		if (AfterSelectionUpdate())
		{
			DoHistory();
		}
	}

	private void DoHistory()
	{
		if (!m_Action.IsEmpty())
		{
			BSHistory.AddAction(m_Action);
			m_Action = new BSAction();
		}
	}

	private float CalcValue(float end, float beign, out float outPut)
	{
		float num = Mathf.Sign(end - beign);
		float num2 = ((!(num >= 0f)) ? 1 : 0);
		int num3 = Mathf.FloorToInt((end - beign) * (float)dataSource.ScaleInverted);
		int num4 = Mathf.Abs(num3 % pattern.size);
		num4 = ((!(num > 0f)) ? ((num4 != 0) ? (1 - num4) : 0) : (1 - num4));
		outPut = Mathf.Floor(end * (float)dataSource.ScaleInverted) * dataSource.Scale + (float)num4 * dataSource.Scale * num;
		return beign + num2 * dataSource.Scale;
	}

	private Vector3 Clamp(Vector3 begin, Vector3 end)
	{
		Vector3 zero = Vector3.zero;
		float num = (float)maxDragSize * dataSource.Scale;
		zero.x = begin.x + Mathf.Clamp(end.x - begin.x, 0f - num, num);
		zero.y = begin.y + Mathf.Clamp(end.y - begin.y, 0f - num, num);
		zero.z = begin.z + Mathf.Clamp(end.z - begin.z, 0f - num, num);
		return zero;
	}

	protected override void Do()
	{
	}

	public override void Cancel()
	{
		ClearSelection(m_Action);
		m_Selecting = false;
		ResetValue();
	}

	private void ResetValue()
	{
		m_Begin = new Vector3(-10000f, -10000f, -10000f);
		m_End = new Vector3(-10000f, -10000f, -10000f);
		m_Target.rch.point = new Vector3(-10000f, -10000f, -10000f);
		m_Target.cursor = new Vector3(-10000f, -10000f, -10000f);
		m_Target.snapto = new Vector3(-10000f, -10000f, -10000f);
	}

	protected virtual bool AfterSelectionUpdate()
	{
		return true;
	}

	protected bool CalcSelection(int mode)
	{
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		zero.x = Mathf.Min(m_Begin.x, m_End.x);
		zero.y = Mathf.Min(m_Begin.y, m_End.y);
		zero.z = Mathf.Min(m_Begin.z, m_End.z);
		zero2.x = Mathf.Max(m_Begin.x, m_End.x);
		zero2.y = Mathf.Max(m_Begin.y, m_End.y);
		zero2.z = Mathf.Max(m_Begin.z, m_End.z);
		Dictionary<IntVector3, byte> dictionary = new Dictionary<IntVector3, byte>();
		Dictionary<IntVector3, byte> dictionary2 = new Dictionary<IntVector3, byte>();
		switch (mode)
		{
		case 0:
		{
			for (float num4 = zero.x; num4 < zero2.x; num4 += dataSource.Scale)
			{
				for (float num5 = zero.y; num5 < zero2.y; num5 += dataSource.Scale)
				{
					for (float num6 = zero.z; num6 < zero2.z; num6 += dataSource.Scale)
					{
						IntVector3 intVector2 = new IntVector3(Mathf.FloorToInt(num4 * (float)dataSource.ScaleInverted), Mathf.FloorToInt(num5 * (float)dataSource.ScaleInverted), Mathf.FloorToInt(num6 * (float)dataSource.ScaleInverted));
						List<IntVector4> posList2 = null;
						List<BSVoxel> voxels2 = null;
						if (dataSource.ReadExtendableBlock(new IntVector4(intVector2, 0), out posList2, out voxels2))
						{
							for (int l = 0; l < voxels2.Count; l++)
							{
								IntVector3 key2 = new IntVector3(posList2[l].x, posList2[l].y, posList2[l].z);
								if (!m_Selections.ContainsKey(key2))
								{
									dictionary[key2] = 254;
								}
							}
						}
						else if (!m_Selections.ContainsKey(intVector2))
						{
							BSVoxel voxel2 = dataSource.Read(intVector2.x, intVector2.y, intVector2.z);
							if (!dataSource.VoxelIsZero(voxel2, 1f))
							{
								dictionary[intVector2] = 254;
							}
						}
					}
				}
			}
			break;
		}
		case 1:
		{
			for (float num7 = zero.x; num7 < zero2.x; num7 += dataSource.Scale)
			{
				for (float num8 = zero.y; num8 < zero2.y; num8 += dataSource.Scale)
				{
					for (float num9 = zero.z; num9 < zero2.z; num9 += dataSource.Scale)
					{
						IntVector3 intVector3 = new IntVector3(Mathf.FloorToInt(num7 * (float)dataSource.ScaleInverted), Mathf.FloorToInt(num8 * (float)dataSource.ScaleInverted), Mathf.FloorToInt(num9 * (float)dataSource.ScaleInverted));
						List<IntVector4> posList3 = null;
						List<BSVoxel> voxels3 = null;
						if (dataSource.ReadExtendableBlock(new IntVector4(intVector3, 0), out posList3, out voxels3))
						{
							for (int m = 0; m < voxels3.Count; m++)
							{
								IntVector3 key3 = new IntVector3(posList3[m].x, posList3[m].y, posList3[m].z);
								if (m_Selections.ContainsKey(key3))
								{
									dictionary2[key3] = 0;
								}
							}
						}
						else if (m_Selections.ContainsKey(intVector3))
						{
							dictionary2[intVector3] = 0;
						}
					}
				}
			}
			break;
		}
		case 2:
		{
			for (float num = zero.x; num < zero2.x; num += dataSource.Scale)
			{
				for (float num2 = zero.y; num2 < zero2.y; num2 += dataSource.Scale)
				{
					for (float num3 = zero.z; num3 < zero2.z; num3 += dataSource.Scale)
					{
						IntVector3 intVector = new IntVector3(Mathf.FloorToInt(num * (float)dataSource.ScaleInverted), Mathf.FloorToInt(num2 * (float)dataSource.ScaleInverted), Mathf.FloorToInt(num3 * (float)dataSource.ScaleInverted));
						List<IntVector4> posList = null;
						List<BSVoxel> voxels = null;
						if (dataSource.ReadExtendableBlock(new IntVector4(intVector, 0), out posList, out voxels))
						{
							bool flag = false;
							for (int i = 0; i < voxels.Count; i++)
							{
								if (m_Selections.ContainsKey(new IntVector3(posList[i].x, posList[i].y, posList[i].z)))
								{
									for (int j = 0; j < voxels.Count; j++)
									{
										IntVector3 key = new IntVector3(posList[j].x, posList[j].y, posList[j].z);
										dictionary2[key] = 0;
									}
									flag = true;
									break;
								}
							}
							if (!flag)
							{
								for (int k = 0; k < voxels.Count; k++)
								{
									dictionary[new IntVector3(posList[k].x, posList[k].y, posList[k].z)] = 254;
								}
							}
						}
						else if (m_Selections.ContainsKey(intVector))
						{
							dictionary2[intVector] = 254;
						}
						else
						{
							BSVoxel voxel = dataSource.Read(intVector.x, intVector.y, intVector.z);
							if (!dataSource.VoxelIsZero(voxel, 1f))
							{
								dictionary[intVector] = 254;
							}
						}
					}
				}
			}
			break;
		}
		}
		if (dictionary.Count - dictionary2.Count + m_Selections.Count > 10000)
		{
			dictionary.Clear();
			dictionary2.Clear();
			return false;
		}
		Dictionary<IntVector3, byte> old_value = new Dictionary<IntVector3, byte>(m_Selections);
		bool flag2 = false;
		foreach (IntVector3 key4 in dictionary2.Keys)
		{
			flag2 = true;
			RemoveSelection(key4);
		}
		foreach (KeyValuePair<IntVector3, byte> item in dictionary)
		{
			flag2 = true;
			AddSelection(item.Key, item.Value);
		}
		dictionary.Clear();
		dictionary2.Clear();
		if (flag2)
		{
			Dictionary<IntVector3, byte> new_value = new Dictionary<IntVector3, byte>(m_Selections);
			BSSelectedBoxModify modify = new BSSelectedBoxModify(old_value, new_value, this);
			m_Action.AddModify(modify);
		}
		return true;
	}

	private void OnGUI()
	{
		if (!BSInput.s_MouseOnUI)
		{
			GUI.skin = VCEditor.Instance.m_GUISkin;
			GUI.color = Color.white;
			if (m_Selecting)
			{
				GUI.color = Color.white;
			}
			else
			{
				GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(m_GUIAlpha));
			}
			if (Depth > 1f)
			{
				GUI.Label(new Rect(Input.mousePosition.x + 26f, (float)Screen.height - Input.mousePosition.y + 5f, 100f, 100f), "Depth x " + Depth, "CursorText2");
			}
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			if (VCEInput.s_Shift)
			{
				GUI.Label(new Rect(Input.mousePosition.x - 105f, (float)Screen.height - Input.mousePosition.y - 75f, 100f, 100f), "ADD", "CursorText1");
			}
			else if (VCEInput.s_Alt)
			{
				GUI.Label(new Rect(Input.mousePosition.x - 105f, (float)Screen.height - Input.mousePosition.y - 75f, 100f, 100f), "SUBTRACT", "CursorText1");
			}
			else if (VCEInput.s_Control)
			{
				GUI.Label(new Rect(Input.mousePosition.x - 105f, (float)Screen.height - Input.mousePosition.y - 75f, 100f, 100f), "CROSS", "CursorText1");
			}
			if (m_Selecting)
			{
				int count = m_Selections.Count;
				Color color = voxelsCountColor.Evaluate((float)count / 10000f);
				GUI.color = color;
				GUI.Label(new Rect(Input.mousePosition.x + 26f, (float)Screen.height - Input.mousePosition.y + 58f, 100f, 100f), "Selected Voxels: " + count, "CursorText2");
				Vector3 zero = Vector3.zero;
				Vector3 zero2 = Vector3.zero;
				zero.x = Mathf.Min(m_Begin.x, m_End.x);
				zero.y = Mathf.Min(m_Begin.y, m_End.y);
				zero.z = Mathf.Min(m_Begin.z, m_End.z);
				zero2.x = Mathf.Max(m_Begin.x, m_End.x);
				zero2.y = Mathf.Max(m_Begin.y, m_End.y);
				zero2.z = Mathf.Max(m_Begin.z, m_End.z);
				IntVector3 intVector = new IntVector3((zero2.x - zero.x) * (float)dataSource.ScaleInverted, (zero2.y - zero.y) * (float)dataSource.ScaleInverted, (zero2.z - zero.z) * (float)dataSource.ScaleInverted);
				string text = "Pre-Selection: " + intVector.x + " x " + intVector.z + " x " + intVector.y;
				GUI.color = new Color(1f, 1f, 1f, 0.8f);
				GUI.Label(new Rect(Input.mousePosition.x + 26f, (float)Screen.height - Input.mousePosition.y + 31f, 100f, 100f), text, "CursorText2");
			}
		}
	}

	public override void OnGL()
	{
		if (dataSource == null || !base.gameObject.activeInHierarchy || !base.enabled || m_Target.ds == null || !m_Drawing)
		{
			return;
		}
		if (m_Material == null)
		{
			m_Material = new Material(Shader.Find("Lines/Colored Blended"));
			m_Material.hideFlags = HideFlags.HideAndDontSave;
			m_Material.shader.hideFlags = HideFlags.HideAndDontSave;
		}
		if (m_Selecting)
		{
			GL.PushMatrix();
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			zero.x = Mathf.Min(m_Begin.x, m_End.x);
			zero.y = Mathf.Min(m_Begin.y, m_End.y);
			zero.z = Mathf.Min(m_Begin.z, m_End.z);
			zero2.x = Mathf.Max(m_Begin.x, m_End.x);
			zero2.y = Mathf.Max(m_Begin.y, m_End.y);
			zero2.z = Mathf.Max(m_Begin.z, m_End.z);
			zero2 += dataSource.Offset;
			zero += dataSource.Offset;
			float num = 0.02f;
			if (Camera.main != null)
			{
				float magnitude = (Camera.main.transform.position - zero).magnitude;
				float magnitude2 = (Camera.main.transform.position - zero2).magnitude;
				magnitude2 = ((!(magnitude2 > magnitude)) ? magnitude : magnitude2);
				num = Mathf.Clamp(magnitude2 * 0.001f, 0.02f, 0.1f);
			}
			zero2 += new Vector3(num, num, num);
			zero -= new Vector3(num, num, num);
			Vector3[] array = new Vector3[8]
			{
				new Vector3(zero2.x, zero2.y, zero2.z),
				new Vector3(zero.x, zero2.y, zero2.z),
				new Vector3(zero.x, zero.y, zero2.z),
				new Vector3(zero2.x, zero.y, zero2.z),
				new Vector3(zero2.x, zero2.y, zero.z),
				new Vector3(zero.x, zero2.y, zero.z),
				new Vector3(zero.x, zero.y, zero.z),
				new Vector3(zero2.x, zero.y, zero.z)
			};
			for (int i = 0; i < 8; i++)
			{
				ref Vector3 reference = ref array[i];
				reference = array[i];
			}
			Color color = new Color(0f, 0.3f, 0.6f, 1f);
			Color c = new Color(0f, 0.3f, 0.6f, 1f);
			if (!m_IsValidBox)
			{
				color = new Color(0.67f, 0.1f, 0.1f);
				c = color;
			}
			color.a = 1f;
			c.a *= 0.4f + Mathf.Sin(Time.time * 6f) * 0.1f;
			for (int j = 0; j < m_Material.passCount; j++)
			{
				m_Material.SetPass(j);
				GL.Begin(1);
				GL.Color(color);
				GL.Vertex(array[0]);
				GL.Vertex(array[1]);
				GL.Vertex(array[1]);
				GL.Vertex(array[2]);
				GL.Vertex(array[2]);
				GL.Vertex(array[3]);
				GL.Vertex(array[3]);
				GL.Vertex(array[0]);
				GL.Vertex(array[4]);
				GL.Vertex(array[5]);
				GL.Vertex(array[5]);
				GL.Vertex(array[6]);
				GL.Vertex(array[6]);
				GL.Vertex(array[7]);
				GL.Vertex(array[7]);
				GL.Vertex(array[4]);
				GL.Vertex(array[0]);
				GL.Vertex(array[4]);
				GL.Vertex(array[1]);
				GL.Vertex(array[5]);
				GL.Vertex(array[2]);
				GL.Vertex(array[6]);
				GL.Vertex(array[3]);
				GL.Vertex(array[7]);
				GL.End();
				GL.Begin(7);
				GL.Color(c);
				GL.Vertex(array[0]);
				GL.Vertex(array[1]);
				GL.Vertex(array[2]);
				GL.Vertex(array[3]);
				GL.Vertex(array[4]);
				GL.Vertex(array[5]);
				GL.Vertex(array[6]);
				GL.Vertex(array[7]);
				GL.Vertex(array[0]);
				GL.Vertex(array[4]);
				GL.Vertex(array[5]);
				GL.Vertex(array[1]);
				GL.Vertex(array[1]);
				GL.Vertex(array[5]);
				GL.Vertex(array[6]);
				GL.Vertex(array[2]);
				GL.Vertex(array[2]);
				GL.Vertex(array[6]);
				GL.Vertex(array[7]);
				GL.Vertex(array[3]);
				GL.Vertex(array[3]);
				GL.Vertex(array[7]);
				GL.Vertex(array[4]);
				GL.Vertex(array[0]);
				GL.End();
			}
			GL.PopMatrix();
		}
		else if (!BSInput.s_MouseOnUI)
		{
			GL.PushMatrix();
			BSMath.DrawTarget target = m_Target;
			Vector3 point = target.rch.point;
			Vector3 point2 = target.rch.point;
			Color color2 = Color.white;
			if (Mathf.Abs(target.rch.normal.x) > 0.9f * target.ds.Scale)
			{
				point.y = Mathf.Floor(point.y * (float)dataSource.ScaleInverted) * dataSource.Scale;
				point.z = Mathf.Floor(point.z * (float)dataSource.ScaleInverted) * dataSource.Scale;
				point2.y = Mathf.Floor(point2.y * (float)dataSource.ScaleInverted) * dataSource.Scale + dataSource.Scale;
				point2.z = Mathf.Floor(point2.z * (float)dataSource.ScaleInverted) * dataSource.Scale + dataSource.Scale;
				color2 = new Color(0.9f, 0.1f, 0.2f, 1f);
			}
			else if (Mathf.Abs(target.rch.normal.y) > 0.9f * target.ds.Scale)
			{
				point.x = Mathf.Floor(point.x * (float)dataSource.ScaleInverted) * dataSource.Scale;
				point.z = Mathf.Floor(point.z * (float)dataSource.ScaleInverted) * dataSource.Scale;
				point2.x = Mathf.Floor(point2.x * (float)dataSource.ScaleInverted) * dataSource.Scale + dataSource.Scale;
				point2.z = Mathf.Floor(point2.z * (float)dataSource.ScaleInverted) * dataSource.Scale + dataSource.Scale;
				color2 = new Color(0.5f, 1f, 0.1f, 1f);
			}
			else if (Mathf.Abs(target.rch.normal.z) > 0.9f * target.ds.Scale)
			{
				point.y = Mathf.Floor(point.y * (float)dataSource.ScaleInverted) * dataSource.Scale;
				point.x = Mathf.Floor(point.x * (float)dataSource.ScaleInverted) * dataSource.Scale;
				point2.y = Mathf.Floor(point2.y * (float)dataSource.ScaleInverted) * dataSource.Scale + dataSource.Scale;
				point2.x = Mathf.Floor(point2.x * (float)dataSource.ScaleInverted) * dataSource.Scale + dataSource.Scale;
				color2 = new Color(0.1f, 0.6f, 1f, 1f);
			}
			point2 += dataSource.Offset;
			point += dataSource.Offset;
			float num2 = 0.02f;
			if (Camera.main != null)
			{
				float magnitude3 = (Camera.main.transform.position - point).magnitude;
				float magnitude4 = (Camera.main.transform.position - point2).magnitude;
				magnitude4 = ((!(magnitude4 > magnitude3)) ? magnitude3 : magnitude4);
				num2 = Mathf.Clamp(magnitude4 * 0.002f, 0.02f, 0.1f);
			}
			point2 += new Vector3(num2, num2, num2);
			point -= new Vector3(num2, num2, num2);
			Vector3[] array2 = new Vector3[8]
			{
				new Vector3(point2.x, point2.y, point2.z),
				new Vector3(point.x, point2.y, point2.z),
				new Vector3(point.x, point.y, point2.z),
				new Vector3(point2.x, point.y, point2.z),
				new Vector3(point2.x, point2.y, point.z),
				new Vector3(point.x, point2.y, point.z),
				new Vector3(point.x, point.y, point.z),
				new Vector3(point2.x, point.y, point.z)
			};
			for (int k = 0; k < 8; k++)
			{
				ref Vector3 reference2 = ref array2[k];
				reference2 = array2[k];
			}
			Color c2 = color2;
			Color c3 = color2;
			c2.a = 1f;
			c3.a *= 0.7f + Mathf.Sin(Time.time * 6f) * 0.1f;
			for (int l = 0; l < m_Material.passCount; l++)
			{
				m_Material.SetPass(l);
				GL.Begin(1);
				GL.Color(c2);
				GL.Vertex(array2[0]);
				GL.Vertex(array2[1]);
				GL.Vertex(array2[1]);
				GL.Vertex(array2[2]);
				GL.Vertex(array2[2]);
				GL.Vertex(array2[3]);
				GL.Vertex(array2[3]);
				GL.Vertex(array2[0]);
				GL.Vertex(array2[4]);
				GL.Vertex(array2[5]);
				GL.Vertex(array2[5]);
				GL.Vertex(array2[6]);
				GL.Vertex(array2[6]);
				GL.Vertex(array2[7]);
				GL.Vertex(array2[7]);
				GL.Vertex(array2[4]);
				GL.Vertex(array2[0]);
				GL.Vertex(array2[4]);
				GL.Vertex(array2[1]);
				GL.Vertex(array2[5]);
				GL.Vertex(array2[2]);
				GL.Vertex(array2[6]);
				GL.Vertex(array2[3]);
				GL.Vertex(array2[7]);
				GL.End();
				GL.Begin(7);
				GL.Color(c3);
				GL.Vertex(array2[0]);
				GL.Vertex(array2[1]);
				GL.Vertex(array2[2]);
				GL.Vertex(array2[3]);
				GL.Vertex(array2[4]);
				GL.Vertex(array2[5]);
				GL.Vertex(array2[6]);
				GL.Vertex(array2[7]);
				GL.Vertex(array2[0]);
				GL.Vertex(array2[4]);
				GL.Vertex(array2[5]);
				GL.Vertex(array2[1]);
				GL.Vertex(array2[1]);
				GL.Vertex(array2[5]);
				GL.Vertex(array2[6]);
				GL.Vertex(array2[2]);
				GL.Vertex(array2[2]);
				GL.Vertex(array2[6]);
				GL.Vertex(array2[7]);
				GL.Vertex(array2[3]);
				GL.Vertex(array2[3]);
				GL.Vertex(array2[7]);
				GL.Vertex(array2[4]);
				GL.Vertex(array2[0]);
				GL.End();
			}
			GL.PopMatrix();
		}
	}
}
