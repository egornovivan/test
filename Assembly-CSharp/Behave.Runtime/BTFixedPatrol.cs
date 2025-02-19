using System.Collections.Generic;
using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTFixedPatrol), "FixedPatrol")]
public class BTFixedPatrol : BTNormal
{
	private class Data
	{
		[Behave]
		public string PlayState;

		[Behave]
		public string PlayAnim;

		[Behave]
		public string PlayTimes;

		[Behave]
		public float IntervalTime;

		[Behave]
		public float MinTime;

		[Behave]
		public float MaxTime;

		[Behave]
		public string TriggerTimes;

		[Behave]
		public float Probability;

		public List<Vector3> PathWalk;

		public PEPathData PathData;

		public RandIntDb RandTriggerTimes;

		public RandIntDb RandPlayTimes;

		public string[] RandAnims;

		public float AnimTime;

		public float AnimStartTime;

		public float AnimStartIntervalTime;

		public bool IsActive;

		public int curPath;

		private bool mInit;

		public void Init()
		{
			if (!mInit)
			{
				RandAnims = PEUtil.ToArrayString(PlayAnim, ',');
				int[] array = PEUtil.ToArrayInt32(TriggerTimes, ',');
				if (array != null)
				{
					RandTriggerTimes = new RandIntDb(array[0], array[1]);
				}
				array = PEUtil.ToArrayInt32(PlayTimes, ',');
				if (array != null)
				{
					RandPlayTimes = new RandIntDb(array[0], array[1]);
				}
				if (PathWalk == null)
				{
					PathWalk = new List<Vector3>();
				}
				mInit = true;
				curPath = 0;
			}
		}

		public void Realse()
		{
			if (PathWalk != null)
			{
				PathWalk.Clear();
				PathWalk = null;
			}
			mInit = false;
		}

		public bool RandPathData(Camp camp, int curpath)
		{
			if (camp != null && camp.mPaths != null)
			{
				string path = camp.GetPath(curpath);
				if (path == null || path == "0")
				{
					return false;
				}
				if (PeSingleton<PEPathSingleton>.Instance == null)
				{
					return false;
				}
				PathData = CampPathDb.GetPathData(path);
				if (PathData.warpMode == EPathMode.Pingpong)
				{
					PathWalk.Clear();
					for (int i = 0; i < PathData.path.Length; i++)
					{
						PathWalk.Add(PathData.path[i]);
					}
					for (int num = PathData.path.Length - 2; num > 0; num--)
					{
						PathWalk.Add(PathData.path[num]);
					}
				}
				else if (PathData.warpMode == EPathMode.Loop)
				{
					PathWalk.Clear();
					for (int j = 0; j < PathData.path.Length; j++)
					{
						PathWalk.Add(PathData.path[j]);
					}
				}
				return true;
			}
			return false;
		}

		public bool IsReached(Vector3 pos, Vector3 targetPos, float radiu = 1f)
		{
			float num = PEUtil.SqrMagnitudeH(pos, targetPos);
			return num < radiu * radiu;
		}

		public int GetClosetPointIndex(PEPathData pathData, Vector3 position)
		{
			int result = -1;
			float num = float.PositiveInfinity;
			for (int i = 0; i < pathData.path.Length; i++)
			{
				float num2 = PEUtil.SqrMagnitudeH(pathData.path[i], position);
				if (num2 < num)
				{
					num = num2;
					result = i;
				}
			}
			return result;
		}
	}

	private Data m_Data;

	private int mIndex;

	private int mTriggerTimes;

	private int mPlayTimes;

	private int mActiveAnim;

	private int loopTimes;

	private string curAnim;

	private BehaveResult Init(Tree sender)
	{
		if (!base.IsNpcCampsite)
		{
			return BehaveResult.Failure;
		}
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		m_Data.Init();
		if (base.hasAnyRequest || !Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Stroll))
		{
			return BehaveResult.Failure;
		}
		if (base.entity.NpcCmpt.NpcInAlert)
		{
			return BehaveResult.Failure;
		}
		if (base.entity.IsNpcInSleepTime || base.entity.IsNpcInDinnerTime)
		{
			return BehaveResult.Failure;
		}
		if (Random.value <= m_Data.Probability)
		{
			return BehaveResult.Failure;
		}
		if (!m_Data.RandPathData(base.Campsite, m_Data.curPath))
		{
			return BehaveResult.Failure;
		}
		if (m_Data.PathData.Equals(null) || m_Data.PathData.path == null)
		{
			return BehaveResult.Failure;
		}
		mIndex = m_Data.GetClosetPointIndex(m_Data.PathData, base.position);
		m_Data.curPath++;
		if (base.Campsite.mPaths != null && m_Data.curPath >= base.Campsite.mPaths.Length)
		{
			m_Data.curPath = 0;
		}
		loopTimes = Random.Range(1, 3);
		mTriggerTimes = m_Data.RandTriggerTimes.Random();
		m_Data.AnimTime = Random.Range(m_Data.MinTime, m_Data.MaxTime);
		mActiveAnim = Random.Range(0, m_Data.PathWalk.Count);
		SetBool(m_Data.PlayState, value: true);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!base.IsNpcCampsite)
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest || !Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Stroll))
		{
			return BehaveResult.Failure;
		}
		if (base.entity.NpcCmpt.NpcInAlert)
		{
			return BehaveResult.Failure;
		}
		if (base.entity.IsNpcInSleepTime || base.entity.IsNpcInDinnerTime)
		{
			return BehaveResult.Failure;
		}
		if (GetBool("OperateMed"))
		{
			SetBool("OperateMed", value: false);
		}
		if (GetBool("OperateCom"))
		{
			SetBool("OperateCom", value: false);
		}
		if (GetBool("FixLifeboat"))
		{
			SetBool("FixLifeboat", value: false);
		}
		if (mIndex == mActiveAnim && !m_Data.IsActive)
		{
			mTriggerTimes--;
			if (mTriggerTimes > 0)
			{
				mActiveAnim += 3;
			}
			else
			{
				mTriggerTimes = m_Data.RandTriggerTimes.Random();
			}
			if (mActiveAnim > m_Data.PathWalk.Count)
			{
				mActiveAnim = m_Data.PathWalk.Count - mActiveAnim;
			}
			StopMove();
			m_Data.IsActive = true;
			m_Data.AnimStartTime = Time.time;
			mPlayTimes = m_Data.RandPlayTimes.Random();
		}
		if (!m_Data.IsActive)
		{
			if (m_Data.IsReached(base.position, m_Data.PathWalk[mIndex]))
			{
				mIndex++;
				if (mIndex == m_Data.PathWalk.Count)
				{
					mIndex = 0;
					loopTimes--;
				}
				if (loopTimes <= 0)
				{
					return BehaveResult.Success;
				}
			}
			if (Stucking(3f))
			{
				SetPosition(m_Data.PathWalk[mIndex]);
			}
			MoveToPosition(m_Data.PathWalk[mIndex]);
		}
		else if (Time.time - m_Data.AnimStartTime >= m_Data.AnimTime)
		{
			m_Data.IsActive = false;
			SetBool(curAnim, value: false);
			mIndex++;
			if (mIndex == m_Data.PathWalk.Count)
			{
				mIndex = 0;
				loopTimes--;
			}
		}
		else
		{
			if (Time.time - m_Data.AnimStartIntervalTime >= m_Data.IntervalTime)
			{
				m_Data.AnimStartIntervalTime = Time.time;
				if (mPlayTimes > 0)
				{
					curAnim = m_Data.RandAnims[Random.Range(0, m_Data.RandAnims.Length)];
					SetBool(curAnim, value: true);
					mPlayTimes--;
				}
			}
			FaceDirection(base.position - base.Campsite.Pos);
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (m_Data == null)
		{
			return;
		}
		SetBool(m_Data.PlayState, value: false);
		for (int i = 0; i < m_Data.RandAnims.Length; i++)
		{
			if (GetBool(m_Data.RandAnims[i]))
			{
				SetBool(m_Data.RandAnims[i], value: false);
			}
		}
		m_Data.Realse();
	}
}
