using System;
using System.Collections.Generic;
using UnityEngine;

public class BSIsoBrush : BSBrush
{
	public delegate void VoidEvent();

	public static Action onBrushDo;

	public string File_Name = "No Name";

	private BIsoCursor m_Cursor;

	public bool Gen;

	private BSMath.DrawTarget m_Target;

	private int m_Rot;

	private List<BSVoxel> m_NewVoxel = new List<BSVoxel>();

	private List<IntVector3> m_Indexes = new List<IntVector3>();

	private List<BSVoxel> m_OldVoxel = new List<BSVoxel>();

	private Dictionary<IntVector3, int> m_VoxelMap = new Dictionary<IntVector3, int>();

	private Vector3 _prveMousePos = Vector3.zero;

	private Vector3 m_FocusOffset;

	public event VoidEvent onCancelClick;

	private void OnDestroy()
	{
		if (m_Cursor != null)
		{
			UnityEngine.Object.Destroy(m_Cursor.gameObject);
			m_Cursor = null;
		}
	}

	private void Update()
	{
		if (GameConfig.IsInVCE)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.O))
		{
			Gen = true;
		}
		if (Gen)
		{
			if (m_Cursor != null)
			{
				UnityEngine.Object.Destroy(m_Cursor.gameObject);
			}
			m_Cursor = BIsoCursor.CreateIsoCursor(GameConfig.GetUserDataPath() + BSSaveIsoBrush.s_IsoPath + File_Name + BSSaveIsoBrush.s_Ext);
			m_Rot = 0;
			Gen = false;
		}
		if (m_Cursor == null)
		{
			return;
		}
		if (BSInput.s_Cancel)
		{
			Cancel();
			if (this.onCancelClick != null)
			{
				this.onCancelClick();
			}
			return;
		}
		if (m_Cursor != null)
		{
			m_Cursor.gameObject.SetActive(value: true);
		}
		if (m_Cursor.ISO.m_HeadInfo.Mode == EBSVoxelType.Block)
		{
			if (BSMath.RayCastDrawTarget(BSInput.s_PickRay, BuildingMan.Blocks, out m_Target, 128, ignoreDiagonal: true, BuildingMan.Datas))
			{
				m_Cursor.gameObject.SetActive(value: true);
				Vector3 zero = Vector3.zero;
				zero.x = (float)(m_Cursor.ISO.m_HeadInfo.xSize / 2) * BSBlock45Data.s_Scale;
				zero.z = (float)(m_Cursor.ISO.m_HeadInfo.zSize / 2) * BSBlock45Data.s_Scale;
				if (Input.GetKeyDown(KeyCode.T))
				{
					m_Rot = ((++m_Rot <= 3) ? m_Rot : 0);
				}
				m_Cursor.SetOriginOffset(-zero);
				m_Cursor.transform.rotation = Quaternion.Euler(0f, 90 * m_Rot, 0f);
				FocusAjust();
				m_Cursor.transform.position = m_Target.cursor + m_FocusOffset * BSBlock45Data.s_Scale;
				if (!Input.GetMouseButtonDown(0) || BSInput.s_MouseOnUI || m_Cursor.gizmoTrigger.RayCast)
				{
					return;
				}
				m_OldVoxel.Clear();
				m_NewVoxel.Clear();
				m_Indexes.Clear();
				m_VoxelMap.Clear();
				m_Cursor.OutputVoxels(Vector3.zero, OnOutputBlocks);
				BSBrush.FindExtraExtendableVoxels(dataSource, m_NewVoxel, m_OldVoxel, m_Indexes, m_VoxelMap);
				BSAction bSAction = new BSAction();
				BSVoxelModify bSVoxelModify = new BSVoxelModify(m_Indexes.ToArray(), m_OldVoxel.ToArray(), m_NewVoxel.ToArray(), BuildingMan.Blocks, EBSBrushMode.Add);
				if (!bSVoxelModify.IsNull())
				{
					bSAction.AddModify(bSVoxelModify);
				}
				if (bSAction.Do())
				{
					BSHistory.AddAction(bSAction);
					if (onBrushDo != null)
					{
						onBrushDo();
					}
				}
			}
			else
			{
				m_Cursor.gameObject.SetActive(value: false);
			}
		}
		else if (m_Cursor.ISO.m_HeadInfo.Mode == EBSVoxelType.Block)
		{
			if (BSMath.RayCastDrawTarget(BSInput.s_PickRay, BuildingMan.Voxels, out m_Target, 1, ignoreDiagonal: true, BuildingMan.Datas))
			{
				m_Cursor.gameObject.SetActive(value: false);
				Debug.Log("Draw building Iso dont support the voxel");
			}
			else
			{
				m_Cursor.gameObject.SetActive(value: false);
			}
		}
	}

	private void OnOutputBlocks(Dictionary<int, BSVoxel> voxels, Vector3 originalPos)
	{
		List<IntVector3> list = new List<IntVector3>();
		List<B45Block> list2 = new List<B45Block>();
		foreach (KeyValuePair<int, BSVoxel> voxel in voxels)
		{
			list.Add(BSIsoData.KeyToIPos(voxel.Key));
			list2.Add(voxel.Value.ToBlock());
		}
		B45Block.RepositionBlocks(list, list2, m_Rot, originalPos);
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			IntVector3 intVector = list[i];
			m_Indexes.Add(intVector);
			m_OldVoxel.Add(BuildingMan.Blocks.SafeRead(intVector.x, intVector.y, intVector.z));
			m_NewVoxel.Add(new BSVoxel(list2[i]));
			m_VoxelMap[intVector] = 0;
		}
	}

	protected override void Do()
	{
	}

	public override void Cancel()
	{
		if (m_Cursor != null)
		{
			UnityEngine.Object.Destroy(m_Cursor.gameObject);
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
}
