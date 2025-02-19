using System;
using System.Collections.Generic;
using Pathea;
using PETools;
using UnityEngine;

public class ItemDraggingColony : ItemDraggingArticle
{
	[Serializable]
	public class FieldInfo
	{
		public Vector3 center;

		public float radius;

		public PeGameMgr.ESceneMode mode;
	}

	public List<FieldInfo> m_LimitedField;

	private CSEntityObject m_CEO;

	private bool m_NotInWaterOrCave = true;

	private bool bLimited;

	private bool bPut;

	private void Awake()
	{
		m_CEO = GetComponentInChildren<CSEntityObject>();
		if (null != m_CEO)
		{
			m_CEO.m_BoundState = 0;
		}
	}

	public override bool OnPutDown()
	{
		if (PeGameMgr.IsMulti && VArtifactUtil.IsInTownBallArea(base.transform.position))
		{
			new PeTipMsg(PELocalization.GetString(8000864), PeTipMsg.EMsgLevel.Warning);
			return true;
		}
		return base.OnPutDown();
	}

	public override bool OnDragging(Ray cameraRay)
	{
		if (SingleGameStory.curType != 0 || RandomDungenMgrData.InDungeon)
		{
			return false;
		}
		bool flag = base.OnDragging(cameraRay);
		bLimited = false;
		for (int i = 0; i < m_LimitedField.Count; i++)
		{
			if (PeGameMgr.sceneMode == m_LimitedField[i].mode)
			{
				Vector3 position = base.transform.position;
				float num = m_LimitedField[i].radius * m_LimitedField[i].radius;
				if ((position - m_LimitedField[i].center).sqrMagnitude < num)
				{
					bLimited = true;
					break;
				}
			}
		}
		CSCreator creator = CSMain.GetCreator(0);
		if (creator != null)
		{
			flag = flag && creator.CanCreate((int)m_CEO.m_Type, base.transform.position) == 4 && !bLimited;
		}
		m_NotInWaterOrCave = base.transform.position == Vector3.zero || (!CheckPosInCave() && !PEUtil.CheckPositionUnderWater(base.transform.position));
		if (flag)
		{
			flag = flag && m_NotInWaterOrCave;
		}
		if (null != itemBounds)
		{
			itemBounds.activeState = flag;
		}
		return flag;
	}

	private bool CheckPosInCave()
	{
		int num = 0;
		int num2 = 4;
		int num3 = 8;
		float num4 = 0.7f;
		float num5 = 60f;
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num3; j++)
			{
				Vector3 b = Quaternion.AngleAxis((float)(360 * j) / (float)num3, Vector3.up) * Vector3.forward;
				b = Vector3.Slerp(Vector3.up, b, (float)i * num5 / (float)num2 / 90f);
				RaycastHit[] array = Physics.RaycastAll(base.transform.position, b, 100f, AiUtil.voxelLayer);
				for (int k = 0; k < array.Length; k++)
				{
					if (null == array[k].collider.GetComponent<Block45ChunkGo>())
					{
						num++;
						break;
					}
				}
			}
		}
		if ((float)num >= (float)(num2 * num3) * num4)
		{
			return true;
		}
		return false;
	}

	public override bool OnCheckPutDown()
	{
		if (!m_NotInWaterOrCave)
		{
			MessageBox_N.ShowOkBox(PELocalization.GetString(82209011));
			return false;
		}
		if (m_CEO != null)
		{
			if (bLimited)
			{
				MessageBox_N.ShowOkBox(PELocalization.GetString(3002311));
				return false;
			}
			CSCreator creator = CSMain.GetCreator(0);
			switch (creator.CanCreate((int)m_CEO.m_Type, base.transform.position))
			{
			case 1:
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000087));
				return false;
			case 0:
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000088));
				return false;
			case 3:
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000105));
				return false;
			case 2:
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000089));
				return false;
			case 6:
				MessageBox_N.ShowOkBox(PELocalization.GetString(82209010));
				return false;
			case 7:
				MessageBox_N.ShowOkBox(PELocalization.GetString(82209009));
				return false;
			case 8:
				MessageBox_N.ShowOkBox(PELocalization.GetString(82209008));
				return false;
			case 9:
				return false;
			}
			m_CEO.m_BoundState = 0;
		}
		bPut = true;
		return true;
	}
}
