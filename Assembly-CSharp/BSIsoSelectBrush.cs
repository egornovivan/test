using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BSIsoSelectBrush : BSBrush
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

	private BSMath.DrawTarget m_Target;

	private EPhase m_Phase;

	private Vector3 m_Begin;

	private Vector3 m_End;

	public BSBoundGizmo gizmoBox;

	public ECoordPlane DragPlane = ECoordPlane.XZ;

	private Vector3 m_PointBeforeAdjustHeight;

	protected Vector3 m_Cursor = Vector3.zero;

	private Vector3 _beginPos = Vector3.zero;

	private Vector3 _prevMousePos = Vector3.zero;

	public override Bounds brushBound
	{
		get
		{
			if (gizmoTrigger != null)
			{
				return gizmoTrigger.boxCollider.bounds;
			}
			Bounds result = default(Bounds);
			result.min = Min;
			result.max = Max;
			return result;
		}
	}

	protected Vector3 Min => new Vector3(Mathf.Min(m_Begin.x, m_End.x), Mathf.Min(m_Begin.y, m_End.y), Mathf.Min(m_Begin.z, m_End.z));

	protected Vector3 Max => new Vector3(Mathf.Max(m_Begin.x, m_End.x), Mathf.Max(m_Begin.y, m_End.y), Mathf.Max(m_Begin.z, m_End.z));

	public Vector3 Size => (Max - Min) * dataSource.ScaleInverted;

	public List<IntVector3> GetSelectionPos()
	{
		IntVector3 intVector = Min * dataSource.ScaleInverted;
		IntVector3 intVector2 = Max * dataSource.ScaleInverted;
		List<IntVector3> list = new List<IntVector3>();
		for (int i = intVector.x; i < intVector2.x; i++)
		{
			for (int j = intVector.y; j < intVector2.y; j++)
			{
				for (int k = intVector.z; k < intVector2.z; k++)
				{
					BSVoxel voxel = dataSource.Read(i, j, k);
					if (!dataSource.VoxelIsZero(voxel, 0f))
					{
						list.Add(new IntVector3(i, j, k));
					}
				}
			}
		}
		return list;
	}

	public bool SaveToIso(string IsoName, byte[] icon_tex, out BSIsoData outData)
	{
		outData = null;
		List<IntVector3> selectionPos = GetSelectionPos();
		if (selectionPos.Count == 0)
		{
			return false;
		}
		if (pattern.type != EBSVoxelType.Block)
		{
			Debug.LogWarning("The iso is not support the Voxel");
			return false;
		}
		BSIsoData bSIsoData = new BSIsoData();
		bSIsoData.Init(pattern.type);
		bSIsoData.m_HeadInfo.Name = IsoName;
		IntVector3 intVector = new IntVector3(selectionPos[0]);
		IntVector3 intVector2 = new IntVector3(selectionPos[0]);
		for (int i = 1; i < selectionPos.Count; i++)
		{
			intVector.x = ((intVector.x <= selectionPos[i].x) ? intVector.x : selectionPos[i].x);
			intVector.y = ((intVector.y <= selectionPos[i].y) ? intVector.y : selectionPos[i].y);
			intVector.z = ((intVector.z <= selectionPos[i].z) ? intVector.z : selectionPos[i].z);
			intVector2.x = ((intVector2.x >= selectionPos[i].x) ? intVector2.x : selectionPos[i].x);
			intVector2.y = ((intVector2.y >= selectionPos[i].y) ? intVector2.y : selectionPos[i].y);
			intVector2.z = ((intVector2.z >= selectionPos[i].z) ? intVector2.z : selectionPos[i].z);
		}
		bSIsoData.m_HeadInfo.xSize = intVector2.x - intVector.x + 1;
		bSIsoData.m_HeadInfo.ySize = intVector2.y - intVector.y + 1;
		bSIsoData.m_HeadInfo.zSize = intVector2.z - intVector.z + 1;
		bSIsoData.m_HeadInfo.IconTex = icon_tex;
		for (int j = 0; j < selectionPos.Count; j++)
		{
			BSVoxel value = dataSource.SafeRead(selectionPos[j].x, selectionPos[j].y, selectionPos[j].z);
			int key = BSIsoData.IPosToKey(selectionPos[j].x - intVector.x, selectionPos[j].y - intVector.y, selectionPos[j].z - intVector.z);
			bSIsoData.m_Voxels.Add(key, value);
		}
		bSIsoData.CaclCosts();
		string file_path = GameConfig.GetUserDataPath() + BuildingMan.s_IsoPath;
		SaveFile(file_path, bSIsoData);
		if (SaveFile(file_path, bSIsoData))
		{
			outData = bSIsoData;
			return true;
		}
		return false;
	}

	private bool SaveFile(string file_path, BSIsoData iso)
	{
		if (!Directory.Exists(file_path))
		{
			Directory.CreateDirectory(file_path);
		}
		file_path = file_path + iso.m_HeadInfo.Name + BuildingMan.s_IsoExt;
		try
		{
			using (FileStream output = new FileStream(file_path, FileMode.Create, FileAccess.Write))
			{
				BinaryWriter binaryWriter = new BinaryWriter(output);
				byte[] buffer = iso.Export();
				binaryWriter.Write(buffer);
				binaryWriter.Close();
			}
			Debug.Log("Save building ISO successfully");
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private void Update()
	{
		if (dataSource == null || GameConfig.IsInVCE)
		{
			return;
		}
		if (m_Phase == EPhase.Free)
		{
			if (BSInput.s_MouseOnUI)
			{
				gizmoBox.gameObject.SetActive(value: false);
				return;
			}
			if (BSInput.s_Cancel)
			{
				ResetDrawing();
			}
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			BSMath.DrawTarget target = default(BSMath.DrawTarget);
			BSMath.DrawTarget target2 = default(BSMath.DrawTarget);
			bool flag = BSMath.RayCastDrawTarget(ray, dataSource, out target, minvol, true);
			bool flag2 = BSMath.RayCastDrawTarget(ray, dataSource, out target2, minvol, true, BuildingMan.Voxels);
			if (flag || flag2)
			{
				if (!gizmoBox.gameObject.activeSelf)
				{
					gizmoBox.gameObject.SetActive(value: true);
				}
				Vector3 vector = m_Target.cursor;
				if (flag && flag2)
				{
					if (target.rch.distance <= target2.rch.distance)
					{
						m_Target = target;
						vector = BSBrush.CalcSnapto(m_Target, dataSource, pattern);
						m_Cursor = m_Target.rch.point;
					}
					else
					{
						m_Target = target2;
						vector = BSBrush.CalcCursor(m_Target, dataSource, 1);
						m_Cursor = m_Target.rch.point;
					}
				}
				else if (flag)
				{
					m_Target = target;
					vector = BSBrush.CalcSnapto(m_Target, dataSource, pattern);
					m_Cursor = m_Target.rch.point;
				}
				else if (flag2)
				{
					m_Target = target2;
					vector = BSBrush.CalcCursor(m_Target, dataSource, 1);
					m_Cursor = m_Target.rch.point;
				}
				gizmoBox.size = Vector3.one * dataSource.Scale;
				gizmoBox.position = vector + dataSource.Offset;
				float scale = dataSource.Scale;
				Vector3 vector2 = vector;
				Vector3 end = vector2 + new Vector3(scale, scale, scale);
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
			if (!BSMath.RayCastCoordPlane(ray2, DragPlane, position, out var rch))
			{
				return;
			}
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
				gizmoBox.position = Min + dataSource.Offset;
				gizmoBox.size = Size * dataSource.Scale;
			}
			if (Input.GetMouseButtonUp(0))
			{
				m_Phase = EPhase.AdjustHeight;
				_prevMousePos = new Vector3(-100f, -100f, -100f);
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
				m_End.y = outPut7;
			}
			else if (DragPlane == ECoordPlane.XY)
			{
				float height2 = m_PointBeforeAdjustHeight.z;
				BSMath.RayAdjustHeight(ray3, ECoordAxis.Z, m_PointBeforeAdjustHeight, out height2);
				float outPut8 = 0f;
				m_Begin.z = CalcValue(height2, _beginPos.z, out outPut8);
				m_End.z = outPut8;
			}
			else if (DragPlane == ECoordPlane.ZY)
			{
				float height3 = m_PointBeforeAdjustHeight.x;
				BSMath.RayAdjustHeight(ray3, ECoordAxis.X, m_PointBeforeAdjustHeight, out height3);
				float outPut9 = 0f;
				m_Begin.x = CalcValue(height3, _beginPos.x, out outPut9);
				m_End.x = outPut9;
			}
			m_End = Clamp(m_Begin, m_End);
			gizmoBox.position = Min + dataSource.Offset;
			gizmoBox.size = Size * dataSource.Scale;
			if (PeInput.Get(PeInput.LogicFunction.Build))
			{
				Do();
				m_Phase = EPhase.Drawing;
			}
		}
		else if (m_Phase == EPhase.Drawing)
		{
			if (BSInput.s_Cancel)
			{
				ResetDrawing();
			}
			else if (BSInput.s_Delete)
			{
				DeleteVoxels();
			}
		}
	}

	public void DeleteVoxels()
	{
		List<IntVector3> selectionPos = GetSelectionPos();
		if (selectionPos.Count != 0)
		{
			List<BSVoxel> list = new List<BSVoxel>();
			List<BSVoxel> list2 = new List<BSVoxel>();
			for (int i = 0; i < selectionPos.Count; i++)
			{
				BSVoxel item = dataSource.Read(selectionPos[i].x, selectionPos[i].y, selectionPos[i].z);
				list.Add(default(BSVoxel));
				list2.Add(item);
			}
			BSAction bSAction = new BSAction();
			BSVoxelModify bSVoxelModify = new BSVoxelModify(selectionPos.ToArray(), list2.ToArray(), list.ToArray(), dataSource, EBSBrushMode.Subtract);
			bSAction.AddModify(bSVoxelModify);
			bSVoxelModify.Redo();
			BSHistory.AddAction(bSAction);
			ResetDrawing();
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
		gizmoBox.size = Vector3.one * dataSource.Scale;
		gizmoBox.gameObject.SetActive(value: false);
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
}
