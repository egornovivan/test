using System;
using System.Collections.Generic;
using PEIK;
using PETools;
using RootMotion.FinalIK;
using UnityEngine;

namespace Pathea;

public class IKCmpt : PeCmpt, IPeMsg
{
	private PeTrans m_Trans;

	[HideInInspector]
	public IKAnimEffectCtrl m_AnimEffectCtrl;

	[HideInInspector]
	public IKAimCtrl m_IKAimCtrl;

	[HideInInspector]
	public IKFlashLight m_IKFlashLight;

	private FullBodyBipedIK m_FBBIK;

	private GrounderFBBIK m_GroundFBBIK;

	private IK[] m_IKArray;

	private float m_DefaultMappingValue;

	private float m_DefaultSpineBend;

	private float m_EnableFBBIKSqrDis = 1024f;

	private bool m_AutoCloseFBBIK;

	private bool m_IKEnable = true;

	private List<Type> m_SpineMask = new List<Type>();

	private bool m_FlashLightActive;

	private bool m_SmoothAim;

	public bool aimActive
	{
		get
		{
			if (null != m_IKAimCtrl)
			{
				return m_IKAimCtrl.active;
			}
			return false;
		}
		set
		{
			if (null != m_IKAimCtrl)
			{
				m_IKAimCtrl.SetActive(value);
			}
		}
	}

	public bool flashLightActive
	{
		get
		{
			return m_FlashLightActive;
		}
		set
		{
			m_FlashLightActive = value;
			if (null != m_IKFlashLight)
			{
				m_IKFlashLight.m_Active = m_FlashLightActive;
			}
		}
	}

	public Vector3 aimTargetPos
	{
		get
		{
			if (null != m_IKAimCtrl)
			{
				return m_IKAimCtrl.targetPos;
			}
			return Vector3.zero;
		}
		set
		{
			if (null != m_IKFlashLight)
			{
				m_IKFlashLight.targetPos = value;
			}
			if (null != m_IKAimCtrl)
			{
				if (flashLightActive && null != m_IKFlashLight)
				{
					m_IKAimCtrl.targetPos = m_IKFlashLight.targetPos;
				}
				else
				{
					m_IKAimCtrl.targetPos = value;
				}
			}
		}
	}

	public Transform aimTargetTrans
	{
		get
		{
			if (null != m_IKAimCtrl)
			{
				return m_IKAimCtrl.m_Target;
			}
			return null;
		}
		set
		{
			if (null != m_IKAimCtrl)
			{
				m_IKAimCtrl.m_Target = value;
			}
		}
	}

	public Ray aimRay
	{
		get
		{
			if (null != m_IKAimCtrl)
			{
				return m_IKAimCtrl.aimRay;
			}
			return new Ray(Vector3.zero, Vector3.zero);
		}
	}

	public IKAimCtrl iKAimCtrl => m_IKAimCtrl;

	public bool aimed
	{
		get
		{
			if (null != m_IKAimCtrl)
			{
				return m_IKAimCtrl.aimed;
			}
			return false;
		}
	}

	public bool ikEnable
	{
		set
		{
			m_IKEnable = value;
			if (null != m_FBBIK)
			{
				m_FBBIK.solver.SetIKPositionWeight(m_IKEnable ? 1 : 0);
			}
			if (null != m_GroundFBBIK)
			{
				m_GroundFBBIK.weight = (m_IKEnable ? 1 : 0);
			}
			if (m_IKArray == null)
			{
				return;
			}
			IK[] iKArray = m_IKArray;
			foreach (IK iK in iKArray)
			{
				if (null != iK)
				{
					iK.enabled = m_IKEnable;
				}
			}
		}
	}

	public bool EnableGroundFBBIK
	{
		set
		{
			if (null != m_GroundFBBIK)
			{
				m_GroundFBBIK.weight = (m_IKEnable ? 1 : 0);
			}
		}
	}

	private bool enableArmMap
	{
		set
		{
			if (null != m_FBBIK)
			{
				m_FBBIK.solver.leftArmMapping.weight = ((!value) ? 0f : m_DefaultMappingValue);
				m_FBBIK.solver.rightArmMapping.weight = ((!value) ? 0f : m_DefaultMappingValue);
			}
		}
	}

	private bool enableGrounderSpineEffect
	{
		set
		{
			if (null != m_GroundFBBIK)
			{
				m_GroundFBBIK.spineBend = ((!value) ? 0f : m_DefaultSpineBend);
			}
		}
	}

	void IPeMsg.OnMsg(EMsg msg, params object[] args)
	{
		if (msg == EMsg.View_Prefab_Build)
		{
			BiologyViewRoot biologyViewRoot = args[1] as BiologyViewRoot;
			m_AnimEffectCtrl = biologyViewRoot.ikAnimEffectCtrl;
			m_IKAimCtrl = biologyViewRoot.ikAimCtrl;
			m_IKFlashLight = biologyViewRoot.ikFlashLight;
			m_FBBIK = biologyViewRoot.fbbik;
			m_GroundFBBIK = biologyViewRoot.grounderFBBIK;
			m_IKArray = biologyViewRoot.ikArray;
			if (null != m_IKAimCtrl)
			{
				m_IKAimCtrl.SetSmoothMoveState(m_SmoothAim);
			}
			m_AutoCloseFBBIK = null == m_GroundFBBIK;
			if (null != m_FBBIK)
			{
				m_FBBIK.Disable();
				m_FBBIK.solver.iterations = 1;
				m_DefaultMappingValue = m_FBBIK.solver.leftArmMapping.weight;
			}
			if (null != m_GroundFBBIK)
			{
				m_DefaultSpineBend = m_GroundFBBIK.spineBend;
			}
			ikEnable = m_IKEnable;
			flashLightActive = m_FlashLightActive;
			enableArmMap = m_SpineMask.Count == 0;
			enableGrounderSpineEffect = m_SpineMask.Count == 0;
		}
	}

	public override void Start()
	{
		base.Start();
		m_Trans = base.Entity.peTrans;
		m_SmoothAim = base.Entity.GetCmpt<MainPlayerCmpt>() == null;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		UpdateFlashLightIK();
		UpdateFBBIKState();
	}

	private void UpdateFlashLightIK()
	{
		if (flashLightActive && null != m_IKFlashLight && null != m_IKAimCtrl)
		{
			m_IKAimCtrl.targetPos = m_IKFlashLight.targetPos;
		}
	}

	private void UpdateFBBIKState()
	{
		if (!(null != m_FBBIK))
		{
			return;
		}
		bool flag = false;
		if (null != m_AnimEffectCtrl)
		{
			float num = Vector3.SqrMagnitude(m_Trans.position - PEUtil.MainCamTransform.position);
			if (num < m_EnableFBBIKSqrDis)
			{
				if (!m_AutoCloseFBBIK)
				{
					flag = true;
				}
				else if (m_AnimEffectCtrl.m_MoveEffect.isRunning || m_AnimEffectCtrl.m_HitReaction.isRunning)
				{
					flag = true;
				}
			}
			if (!flag && m_AnimEffectCtrl.m_HitReaction.isRunning)
			{
				flag = true;
			}
		}
		if (m_FBBIK.enabled != flag)
		{
			m_FBBIK.enabled = flag;
		}
	}

	public void SetSpineEffectDeactiveState(Type type, bool deactive)
	{
		if (!deactive)
		{
			m_SpineMask.Remove(type);
		}
		else if (!m_SpineMask.Contains(type))
		{
			m_SpineMask.Add(type);
		}
		enableArmMap = m_SpineMask.Count == 0;
		enableGrounderSpineEffect = m_SpineMask.Count == 0;
	}
}
