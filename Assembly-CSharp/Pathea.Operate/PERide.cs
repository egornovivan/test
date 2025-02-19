using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;

namespace Pathea.Operate;

public class PERide : Operation_Single
{
	[SerializeField]
	[HideInInspector]
	public string Ainm = "RideMax";

	[HideInInspector]
	[SerializeField]
	public float MonsterScaleMin = 0.6f;

	[SerializeField]
	[HideInInspector]
	public float MonsterScaleMax = 1.5f;

	[HideInInspector]
	[SerializeField]
	public AnimationCurve IkLeapCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public Vector3 RidePos => base.transform.position;

	public Quaternion RideRotation => base.transform.rotation;

	private void Start()
	{
		AdjustIkByMonsterScale();
	}

	public override bool Do(IOperator oper)
	{
		PEActionParamVQSNS param = PEActionParamVQSNS.param;
		param.vec = base.transform.position;
		param.q = base.transform.rotation;
		param.strAnima = Ainm;
		PeEntity componentInParent = GetComponentInParent<PeEntity>();
		param.enitytID = ((!(null == componentInParent)) ? componentInParent.Id : (-1));
		param.boneStr = base.transform.name;
		return Do(oper, param);
	}

	public override bool CanOperate(Transform trans)
	{
		return null == base.Operator;
	}

	public override bool Do(IOperator oper, PEActionParam para)
	{
		return oper.DoAction(PEActionType.Ride, para);
	}

	public override bool UnDo(IOperator oper)
	{
		oper.EndAction(PEActionType.Ride);
		return true;
	}

	public override bool CanOperateMask(EOperationMask mask)
	{
		return base.CanOperateMask(mask);
	}

	public override EOperationMask GetOperateMask()
	{
		return EOperationMask.Ride;
	}

	public override bool StartOperate(IOperator oper, EOperationMask mask)
	{
		if (oper == null || oper.Equals(null))
		{
			return false;
		}
		if (m_Mask == mask)
		{
			base.Operator = oper;
			oper.Operate = this;
			if (!Do(oper))
			{
				base.Operator = null;
				oper.Operate = null;
				return false;
			}
			return true;
		}
		return false;
	}

	public override bool StopOperate(IOperator oper, EOperationMask mask)
	{
		if (oper == null || oper.Equals(null))
		{
			return false;
		}
		if (m_Mask == mask)
		{
			if (UnDo(oper))
			{
				base.Operator = null;
				oper.Operate = null;
				return true;
			}
			return false;
		}
		return true;
	}

	[ContextMenu("AdjustIkByMonsterScale")]
	public void AdjustIkByMonsterScale()
	{
		PeEntity componentInParent = base.transform.GetComponentInParent<PeEntity>();
		if ((bool)componentInParent)
		{
			if ((bool)componentInParent.biologyViewCmpt && (bool)componentInParent.biologyViewCmpt.biologyViewRoot)
			{
				FullBodyBipedIK componentInChildren = componentInParent.GetComponentInChildren<FullBodyBipedIK>();
				if ((bool)componentInChildren)
				{
					Transform transform = componentInChildren.transform.FindChild("BeRideMin");
					Transform transform2 = componentInChildren.transform.FindChild("BeRideMax");
					Transform transform3 = componentInChildren.transform.FindChild("BeRide");
					if ((bool)transform && (bool)transform2 && (bool)transform3)
					{
						Dictionary<FullBodyBipedEffector, Transform> bonesDic2;
						Dictionary<FullBodyBipedEffector, Transform> bonesDic;
						Dictionary<FullBodyBipedEffector, Transform> bonesDic3 = (bonesDic2 = (bonesDic = null));
						GetIkTransByRoot(transform, out bonesDic3);
						GetIkTransByRoot(transform2, out bonesDic2);
						GetIkTransByRoot(transform3, out bonesDic);
						if (bonesDic3 != null && bonesDic3.Count > 0 && bonesDic2 != null && bonesDic2.Count > 0 && bonesDic != null && bonesDic.Count > 0)
						{
							float time = (componentInParent.biologyViewCmpt.biologyViewRoot.transform.localScale.x - MonsterScaleMin) / (MonsterScaleMax - MonsterScaleMin);
							time = Mathf.Clamp01(IkLeapCurve.Evaluate(time));
							{
								foreach (KeyValuePair<FullBodyBipedEffector, Transform> item in bonesDic)
								{
									Transform transform4 = null;
									if (bonesDic3.ContainsKey(item.Key))
									{
										if (bonesDic2.ContainsKey(item.Key))
										{
											Transform transform5 = bonesDic3[item.Key];
											transform4 = bonesDic2[item.Key];
											item.Value.localPosition = Vector3.Lerp(transform5.localPosition, transform4.localPosition, time);
											item.Value.localRotation = Quaternion.Lerp(transform5.localRotation, transform4.localRotation, time);
										}
										else
										{
											Debug.LogFormat("PERide:{0}-> maxIKTransDic->{1} not exist ! ", componentInParent.name, item.Key.ToString());
										}
									}
									else
									{
										Debug.LogFormat("PERide:{0}-> minIKTransDic->{1} not exist ! ", componentInParent.name, item.Key.ToString());
									}
								}
								return;
							}
						}
						Debug.LogFormat("PERide:{0} ik trans not full ! ", componentInParent.name);
					}
					else
					{
						Debug.LogFormat("PERide:{0} ik configura not completed ! ", componentInParent.name);
					}
				}
				else
				{
					Debug.LogFormat("PERide: {0} not have FullBodyBipedIK ! ", componentInParent.name);
				}
			}
			else
			{
				Debug.LogFormat("PERide: {0} not have biologyViewCmpt ! ", componentInParent.name);
			}
		}
		else
		{
			Debug.LogFormat("PERide: monsterEntity is null!");
		}
	}

	private void GetIkTransByRoot(Transform transRoot, out Dictionary<FullBodyBipedEffector, Transform> bonesDic)
	{
		bonesDic = new Dictionary<FullBodyBipedEffector, Transform>();
		InteractionTarget[] componentsInChildren = transRoot.GetComponentsInChildren<InteractionTarget>(includeInactive: true);
		if (componentsInChildren != null && componentsInChildren.Length > 0)
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				bonesDic.Add(componentsInChildren[i].effectorType, componentsInChildren[i].transform);
			}
		}
	}
}
