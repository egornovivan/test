using ItemAsset;
using Pathea;
using UnityEngine;
using WhiteCat;

public class PEEquipment : MonoBehaviour, ICloneModelHelper
{
	public string m_EquipAnim = string.Empty;

	public ItemObject m_ItemObj;

	protected PeEntity m_Entity;

	protected BiologyViewCmpt m_View;

	public bool showOnVehicle = true;

	protected Renderer[] m_Renderers;

	protected bool[] m_RendersEnable;

	private bool[] m_HideMask = new bool[4];

	private bool m_HideState;

	private CreationController m_CreationController;

	public EquipType equipType => m_ItemObj.protoData.equipType;

	public virtual void ResetView()
	{
		for (int i = 0; i < m_HideMask.Length; i++)
		{
			m_HideMask[i] = false;
		}
		UpdateHideState();
	}

	public virtual void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		m_Entity = entity;
		m_ItemObj = itemObj;
		m_View = m_Entity.biologyViewCmpt;
		m_Renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
		m_RendersEnable = new bool[m_Renderers.Length];
		for (int i = 0; i < m_RendersEnable.Length; i++)
		{
			m_RendersEnable[i] = m_Renderers[i].enabled;
		}
		m_CreationController = GetComponent<CreationController>();
	}

	public virtual void RemoveEquipment()
	{
		m_View.DetachObject(base.gameObject);
		Object.Destroy(base.gameObject);
	}

	public virtual bool CanTakeOff()
	{
		return true;
	}

	public virtual void SetActiveState(bool active)
	{
	}

	public void HideEquipmentByVehicle(bool hide)
	{
		if (!showOnVehicle)
		{
			m_HideMask[0] = hide;
			UpdateHideState();
		}
	}

	public void HideEquipmentByFirstPerson(bool hide)
	{
		m_HideMask[1] = hide;
		UpdateHideState();
	}

	public void HidEquipmentByUnderWater(bool hide)
	{
		m_HideMask[2] = hide;
		UpdateHideState();
	}

	public void HidEquipmentByRagdoll(bool hide)
	{
		m_HideMask[3] = hide;
		UpdateHideState();
	}

	protected virtual void UpdateHideState()
	{
		if (m_Renderers == null || m_HideMask == null)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < m_HideMask.Length; i++)
		{
			if (m_HideMask[i])
			{
				flag = true;
				break;
			}
		}
		if (flag == m_HideState)
		{
			return;
		}
		m_HideState = flag;
		if (null != m_CreationController)
		{
			m_CreationController.visible = !m_HideState;
		}
		for (int j = 0; j < m_Renderers.Length; j++)
		{
			if (m_Renderers[j] != null && m_Renderers[j].enabled != !m_HideState)
			{
				m_Renderers[j].enabled = !m_HideState && m_RendersEnable[j];
			}
		}
	}
}
