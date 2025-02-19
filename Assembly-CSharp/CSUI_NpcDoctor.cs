using System.Collections.Generic;
using UnityEngine;

public class CSUI_NpcDoctor : MonoBehaviour
{
	[SerializeField]
	private CSUI_WorkRoom WorkRoomUIPrefab;

	[SerializeField]
	private UIGrid m_WorkRoomRootUI;

	private List<CSUI_WorkRoom> m_WorkRooms = new List<CSUI_WorkRoom>();

	private CSPersonnel m_RefNpc;

	private bool m_Active = true;

	public CSPersonnel RefNpc
	{
		get
		{
			return m_RefNpc;
		}
		set
		{
			m_RefNpc = value;
			UpdateWorkRoom();
		}
	}

	private void UpdateWorkRoom()
	{
		foreach (CSUI_WorkRoom workRoom in m_WorkRooms)
		{
			workRoom.m_RefNpc = m_RefNpc;
		}
	}

	public void Init()
	{
	}

	public void Activate(bool active)
	{
		if (m_Active != active)
		{
			m_Active = active;
			_activate();
		}
		else
		{
			m_Active = active;
		}
	}

	private void _activate()
	{
		for (int i = 0; i < m_WorkRooms.Count; i++)
		{
			m_WorkRooms[i].Activate(m_Active);
		}
	}

	private void OnEnable()
	{
		CSCreator creator = CSUI_MainWndCtrl.Instance.Creator;
		if (creator == null)
		{
			return;
		}
		Dictionary<int, CSCommon> commonEntities = creator.GetCommonEntities();
		foreach (KeyValuePair<int, CSCommon> item in commonEntities)
		{
			if (item.Value.Assembly != null && item.Value.WorkerMaxCount > 0 && item.Value is CSHealth)
			{
				CSUI_WorkRoom cSUI_WorkRoom = Object.Instantiate(WorkRoomUIPrefab);
				cSUI_WorkRoom.transform.parent = m_WorkRoomRootUI.transform;
				cSUI_WorkRoom.transform.localPosition = Vector3.zero;
				cSUI_WorkRoom.transform.localRotation = Quaternion.identity;
				cSUI_WorkRoom.transform.localScale = Vector3.one;
				cSUI_WorkRoom.m_RefCommon = item.Value;
				cSUI_WorkRoom.m_RefNpc = RefNpc;
				m_WorkRooms.Add(cSUI_WorkRoom);
			}
		}
		m_WorkRoomRootUI.repositionNow = true;
	}

	private void OnDisable()
	{
		foreach (CSUI_WorkRoom workRoom in m_WorkRooms)
		{
			Object.Destroy(workRoom.gameObject);
		}
		m_WorkRooms.Clear();
	}

	private void Start()
	{
		_activate();
	}
}
