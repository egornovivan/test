using System;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class PEFootDecal : MonoBehaviour
{
	[Serializable]
	public class FootDecal
	{
		public Transform tr;

		private Vector3 m_CurPosition;

		private Vector3 m_PrePosition;

		private Vector3 m_CurVelocity;

		private bool m_Dirty;

		private Stack<Vector3> m_UpHeights = new Stack<Vector3>();

		private Stack<Vector3> m_DownHeights = new Stack<Vector3>();

		public bool Dirty
		{
			set
			{
				m_Dirty = value;
			}
		}

		private void UpdateLand(int musicID)
		{
			float y = m_CurVelocity.y;
			float num = Mathf.Sqrt(m_CurVelocity.x * m_CurVelocity.x + m_CurVelocity.z * m_CurVelocity.z);
			if (!m_Dirty)
			{
				m_DownHeights.Clear();
				if (y > float.Epsilon && num > 0.05f)
				{
					m_UpHeights.Push(m_CurVelocity);
					return;
				}
				m_Dirty = m_UpHeights.Count > 5;
				m_UpHeights.Clear();
				return;
			}
			m_UpHeights.Clear();
			if (y < -1E-45f)
			{
				m_DownHeights.Push(m_CurVelocity);
				return;
			}
			if (m_DownHeights.Count > 5)
			{
				m_Dirty = false;
				OnLand(musicID);
			}
			m_DownHeights.Clear();
		}

		private void UpdateFan(int musicID)
		{
			if (!m_Dirty)
			{
				if (m_CurVelocity.y < -1E-45f)
				{
					m_DownHeights.Push(m_CurVelocity);
				}
				else
				{
					m_DownHeights.Clear();
				}
				m_Dirty = m_DownHeights.Count > 5;
				if (m_Dirty)
				{
					m_UpHeights.Clear();
				}
			}
			else if (m_CurVelocity.y > float.Epsilon)
			{
				m_UpHeights.Push(m_CurVelocity);
			}
			else
			{
				if (m_UpHeights.Count > 5)
				{
					m_Dirty = false;
					m_DownHeights.Clear();
					OnFan(musicID);
				}
				m_UpHeights.Clear();
			}
		}

		private bool CanAddFootStep()
		{
			int num = 0;
			PeEntity mainPlayer = PeSingleton<PeCreature>.Instance.mainPlayer;
			if (mainPlayer != null)
			{
				int count = AudioManager.instance.m_animFootStepAudios.Count;
				for (int i = 0; i < count; i++)
				{
					AudioController audioController = AudioManager.instance.m_animFootStepAudios[i];
					if (audioController != null && audioController.mAudio != null)
					{
						float num2 = Vector3.Distance(mainPlayer.position, audioController.transform.position);
						if (num2 < audioController.mAudio.maxDistance)
						{
							num++;
						}
					}
				}
			}
			return num <= 3;
		}

		private void OnLand(int musicID)
		{
			if (CanAddFootStep() && UnityEngine.Random.value < 0.6f)
			{
				AudioManager.instance.CreateFootStepAudio(tr.position, musicID);
			}
		}

		private void OnFan(int musicID)
		{
			if (CanAddFootStep() && UnityEngine.Random.value < 0.6f)
			{
				AudioManager.instance.CreateFootStepAudio(tr.position, musicID);
			}
		}

		public void Update(Transform parent, int musicID, bool isSky)
		{
			m_CurPosition = tr.position;
			m_CurPosition.y = parent.InverseTransformPoint(tr.position).y;
			m_CurVelocity = m_CurPosition - m_PrePosition;
			if (isSky)
			{
				UpdateFan(musicID);
			}
			else
			{
				UpdateLand(musicID);
			}
			m_PrePosition = m_CurPosition;
		}
	}

	public static int LandForward = Animator.StringToHash("Base Layer.Locomotion.Forward");

	public static int LandBackward = Animator.StringToHash("Base Layer.Locomotion.Backward");

	public static int SkyForward = Animator.StringToHash("Base Layer.Locomotion_Sky.Forward");

	public static int SkyBackward = Animator.StringToHash("Base Layer.Locomotion_Sky.Backward");

	public static int SkyRotLeft = Animator.StringToHash("Base Layer.Locomotion_Sky.RotLeft");

	public static int SkyRotRight = Animator.StringToHash("Base Layer.Locomotion_Sky.RotRight");

	public static int SkyIdle = Animator.StringToHash("Base Layer.Locomotion_Sky.Idle");

	public static int SkyIdleAttack = Animator.StringToHash("Base Layer.Locomotion_Sky.AttackIdle");

	public bool isSky;

	public int musicID;

	public FootDecal[] foots;

	private Animator m_Animator;

	private PeEntity m_Entity;

	private bool IsUpdate()
	{
		if (m_Entity == null || m_Entity.IsDeath() || m_Entity.isRagdoll)
		{
			return false;
		}
		if (m_Animator == null)
		{
			m_Animator = GetComponent<Animator>();
		}
		if (m_Animator == null || m_Animator.IsInTransition(0))
		{
			return false;
		}
		int fullPathHash = m_Animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
		if (isSky)
		{
			return fullPathHash == SkyForward || fullPathHash == SkyBackward || fullPathHash == SkyRotLeft || fullPathHash == SkyRotRight || fullPathHash == SkyIdle || fullPathHash == SkyIdleAttack;
		}
		return fullPathHash == LandForward || fullPathHash == LandBackward;
	}

	private void Start()
	{
		m_Entity = GetComponentInParent<PeEntity>();
	}

	private void LateUpdate()
	{
		if (IsUpdate())
		{
			for (int i = 0; i < foots.Length; i++)
			{
				foots[i].Update(base.transform, musicID, isSky);
			}
		}
	}
}
