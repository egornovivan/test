using System.Collections.Generic;
using UnityEngine;

public class BSPointBrush : BSBrush
{
	public BSGizmoCubeMesh GizmoCube;

	public int MaxOffset = 5;

	public Color addModeGizmoColor = Color.white;

	public Color removeModeGizmoColor = Color.white;

	[SerializeField]
	private BSGizmoTriggerEvent GizmoTrigger;

	private GameObject Gizmo;

	private BSMath.DrawTarget m_Target;

	private Vector3 m_Cursor;

	private int m_Rot;

	private Vector3 m_FocusOffset;

	private BSPattern prvePattern;

	private Vector3 _prveMousePos = Vector3.zero;

	public override Bounds brushBound => GizmoTrigger.boxCollider.bounds;

	private void Start()
	{
		m_Target = default(BSMath.DrawTarget);
		pattern = BSPattern.DefaultV1;
	}

	private void Update()
	{
		if (pattern == null || dataSource == null || GameConfig.IsInVCE)
		{
			return;
		}
		if (BSInput.s_MouseOnUI)
		{
			if (Gizmo != null)
			{
				Gizmo.SetActive(value: false);
			}
			GizmoCube.gameObject.SetActive(value: false);
			return;
		}
		if (BSInput.s_Alt)
		{
			mode = EBSBrushMode.Subtract;
			GizmoCube.color = removeModeGizmoColor;
		}
		else
		{
			mode = EBSBrushMode.Add;
			GizmoCube.color = addModeGizmoColor;
		}
		if (GizmoTrigger.RayCast)
		{
			GizmoCube.color = removeModeGizmoColor;
		}
		if (mode == EBSBrushMode.Add)
		{
			AddMode();
		}
		else if (mode == EBSBrushMode.Subtract)
		{
			SubtractMode();
		}
	}

	private void AddMode()
	{
		if (pattern != prvePattern)
		{
			if (Gizmo != null)
			{
				Object.Destroy(Gizmo.gameObject);
				Gizmo = null;
			}
			if (pattern.MeshPath != null && pattern.MeshPath != string.Empty)
			{
				Gizmo = Object.Instantiate(Resources.Load(pattern.MeshPath)) as GameObject;
				Gizmo.transform.parent = base.transform;
			}
			prvePattern = pattern;
			m_Rot = 0;
		}
		if (pattern.MeshMat != null && Gizmo != null)
		{
			Renderer component = Gizmo.GetComponent<Renderer>();
			if (component != null)
			{
				component.material = pattern.MeshMat;
			}
		}
		if (pattern.type == EBSVoxelType.Block && Input.GetKeyDown(KeyCode.T))
		{
			m_Rot = ((++m_Rot <= 3) ? m_Rot : 0);
		}
		Vector3 vector = Vector3.zero;
		if (Gizmo != null)
		{
			Quaternion quaternion = Quaternion.Euler(0f, 90 * m_Rot, 0f);
			Gizmo.transform.rotation = Quaternion.Euler(0f, 90 * m_Rot, 0f);
			float num = (float)pattern.size * 0.5f;
			vector = (quaternion * new Vector3(0f - num, 0f - num, 0f - num) + new Vector3(num, num, num)) * dataSource.Scale;
			Gizmo.transform.localScale = new Vector3(pattern.size, pattern.size, pattern.size) * dataSource.Scale;
		}
		GizmoCube.m_VoxelSize = dataSource.Scale;
		GizmoCube.CubeSize = new IntVector3(pattern.size, pattern.size, pattern.size);
		Vector3 vector2 = new Vector3((float)GizmoCube.CubeSize.x * dataSource.Scale, (float)GizmoCube.CubeSize.y * dataSource.Scale, (float)GizmoCube.CubeSize.z * dataSource.Scale);
		GizmoTrigger.boxCollider.size = vector2;
		GizmoTrigger.boxCollider.center = vector2 * 0.5f;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (BSMath.RayCastDrawTarget(ray, dataSource, out m_Target, minvol, ignoreDiagonal: true, BuildingMan.Datas))
		{
			if (Gizmo != null)
			{
				Gizmo.SetActive(value: true);
			}
			GizmoCube.gameObject.SetActive(value: true);
			m_Cursor = BSBrush.CalcCursor(m_Target, dataSource, pattern.size);
			FocusAjust();
			m_Cursor += m_FocusOffset * dataSource.Scale;
			if (Gizmo != null)
			{
				Gizmo.transform.position = m_Cursor + dataSource.Offset + vector;
			}
			GizmoCube.transform.position = m_Cursor + dataSource.Offset;
			if (Input.GetMouseButtonDown(0) && !GizmoTrigger.RayCast)
			{
				Do();
			}
		}
	}

	private void SubtractMode()
	{
		m_Rot = 0;
		prvePattern = null;
		if (Gizmo != null)
		{
			Object.Destroy(Gizmo.gameObject);
			Gizmo = null;
		}
		GizmoCube.gameObject.SetActive(value: true);
		GizmoCube.m_VoxelSize = dataSource.Scale;
		GizmoCube.CubeSize = new IntVector3(pattern.size, pattern.size, pattern.size);
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (BSMath.RayCastDrawTarget(ray, dataSource, out m_Target, minvol, false))
		{
			if (Gizmo != null)
			{
				Gizmo.SetActive(value: true);
			}
			if (!GizmoCube.gameObject.activeSelf)
			{
				GizmoCube.gameObject.SetActive(value: true);
			}
			m_Cursor = BSBrush.CalcSnapto(m_Target, dataSource, pattern);
			FocusAjust();
			m_Cursor += m_FocusOffset * dataSource.Scale;
			if (Gizmo != null)
			{
				Gizmo.transform.position = m_Cursor + dataSource.Offset;
			}
			GizmoCube.transform.position = m_Cursor + dataSource.Offset;
			if (Input.GetMouseButtonDown(0))
			{
				Do();
			}
		}
		else
		{
			if (Gizmo != null)
			{
				Gizmo.SetActive(value: false);
			}
			if (GizmoCube.gameObject.activeSelf)
			{
				GizmoCube.gameObject.SetActive(value: false);
			}
		}
	}

	private void FocusAjust()
	{
		if (_prveMousePos == Input.mousePosition)
		{
			if (BSInput.s_Up)
			{
				m_FocusOffset += Vector3.up;
			}
			else if (BSInput.s_Down)
			{
				m_FocusOffset -= Vector3.up;
			}
			else if (BSInput.s_Right)
			{
				m_FocusOffset += Vector3.right;
			}
			else if (BSInput.s_Left)
			{
				m_FocusOffset += Vector3.left;
			}
			else if (BSInput.s_Forward)
			{
				m_FocusOffset += Vector3.forward;
			}
			else if (BSInput.s_Back)
			{
				m_FocusOffset += Vector3.back;
			}
		}
		else
		{
			_prveMousePos = Input.mousePosition;
			m_FocusOffset.x = 0f;
			m_FocusOffset.y = 0f;
			m_FocusOffset.z = 0f;
		}
	}

	protected override void Do()
	{
		int size = pattern.size;
		Vector3 vector = new Vector3((float)(size - 1) / 2f, 0f, (float)(size - 1) / 2f);
		if (mode == EBSBrushMode.Add)
		{
			List<BSVoxel> list = new List<BSVoxel>();
			List<IntVector3> list2 = new List<IntVector3>();
			List<BSVoxel> list3 = new List<BSVoxel>();
			for (int i = 0; i < size; i++)
			{
				for (int j = 0; j < size; j++)
				{
					for (int k = 0; k < size; k++)
					{
						Vector3 vector2 = Quaternion.Euler(0f, 90 * m_Rot, 0f) * new Vector3((float)i - vector.x, (float)j - vector.y, (float)k - vector.z) + vector;
						IntVector3 intVector = new IntVector3((float)Mathf.FloorToInt(m_Cursor.x * (float)dataSource.ScaleInverted) + vector2.x, (float)Mathf.FloorToInt(m_Cursor.y * (float)dataSource.ScaleInverted) + vector2.y, (float)Mathf.FloorToInt(m_Cursor.z * (float)dataSource.ScaleInverted) + vector2.z);
						BSVoxel item = pattern.voxelList[i, j, k];
						item.materialType = materialType;
						item.blockType = BSVoxel.MakeBlockType(item.blockType >> 2, ((item.blockType & 3) + m_Rot) % 4);
						BSVoxel item2 = dataSource.SafeRead(intVector.x, intVector.y, intVector.z);
						list.Add(item);
						list3.Add(item2);
						list2.Add(intVector);
					}
				}
			}
			if (list2.Count != 0)
			{
				BSAction bSAction = new BSAction();
				BSVoxelModify modify = new BSVoxelModify(list2.ToArray(), list3.ToArray(), list.ToArray(), dataSource, mode);
				bSAction.AddModify(modify);
				if (bSAction.Do())
				{
					BSHistory.AddAction(bSAction);
				}
			}
		}
		else
		{
			if (mode != EBSBrushMode.Subtract)
			{
				return;
			}
			List<BSVoxel> list4 = new List<BSVoxel>();
			List<IntVector3> list5 = new List<IntVector3>();
			List<BSVoxel> list6 = new List<BSVoxel>();
			for (int l = 0; l < size; l++)
			{
				for (int m = 0; m < size; m++)
				{
					for (int n = 0; n < size; n++)
					{
						IntVector3 intVector2 = new IntVector3(Mathf.FloorToInt(m_Cursor.x * (float)dataSource.ScaleInverted) + l, Mathf.FloorToInt(m_Cursor.y * (float)dataSource.ScaleInverted) + m, Mathf.FloorToInt(m_Cursor.z * (float)dataSource.ScaleInverted) + n);
						BSVoxel item3 = dataSource.Read(intVector2.x, intVector2.y, intVector2.z);
						list4.Add(default(BSVoxel));
						list5.Add(intVector2);
						list6.Add(item3);
					}
				}
			}
			if (list5.Count != 0)
			{
				BSAction bSAction2 = new BSAction();
				BSVoxelModify modify2 = new BSVoxelModify(list5.ToArray(), list6.ToArray(), list4.ToArray(), dataSource, mode);
				bSAction2.AddModify(modify2);
				if (bSAction2.Do())
				{
					BSHistory.AddAction(bSAction2);
				}
			}
		}
	}
}
