using System.Collections.Generic;
using PETools;
using UnityEngine;

public class PEAnimatorFootDecal : StateMachineBehaviour
{
	private class FootDecal
	{
		public Transform parent;

		public Transform tr;

		public int soundID;

		private float m_CurHeight;

		private float m_PreHeight;

		private float m_CurVelocity;

		private float m_PreVelocity;

		private bool m_Land;

		public FootDecal(Transform argParent, Transform argTrans, int soundid)
		{
			parent = argParent;
			tr = argTrans;
			soundID = soundid;
		}

		private void OnLand()
		{
			AudioManager.instance.Create(tr.position, soundID);
		}

		private void OnFan()
		{
		}

		public void Update(float minSpeed)
		{
			if (!(tr == null))
			{
				m_CurHeight = parent.InverseTransformPoint(tr.position).y;
				m_CurVelocity = m_CurHeight - m_PreHeight;
				if (m_CurVelocity < 0f - minSpeed && m_PreVelocity < 0f - minSpeed)
				{
					m_Land = false;
				}
				if (m_CurVelocity >= 0f && !m_Land)
				{
					m_Land = true;
					OnLand();
				}
				m_PreHeight = m_CurHeight;
				m_PreVelocity = m_CurVelocity;
			}
		}
	}

	public float minSpeed;

	public string[] foots;

	private bool m_Init;

	private List<FootDecal> m_FootDecals;

	private void Init(Transform trans)
	{
		m_FootDecals = new List<FootDecal>();
		for (int i = 0; i < foots.Length; i++)
		{
			Transform child = PEUtil.GetChild(trans, foots[i]);
			if (child != null)
			{
				m_FootDecals.Add(new FootDecal(trans, child, 838));
			}
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if (!m_Init)
		{
			m_Init = true;
			Init(animator.transform);
		}
		for (int i = 0; i < m_FootDecals.Count; i++)
		{
			m_FootDecals[i].Update(minSpeed);
		}
	}
}
