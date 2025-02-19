using System.Collections;
using System.Collections.Generic;
using PETools;
using SkillSystem;
using UnityEngine;

namespace Pathea;

public class PeNpcGroup : MonoBehaviour
{
	private static PeNpcGroup mInstance;

	private List<Camp> mCamps;

	private List<PeEntity> mCSNpcs;

	private CSCreator mCScreator;

	private CSNpcTeam mCSnpcTeam;

	public static PeNpcGroup Instance => mInstance;

	public bool InDanger => mCSnpcTeam != null && mCSnpcTeam.mIndanger;

	private IEnumerator CSTeam(float time)
	{
		if (SingleGameStory.curType == SingleGameStory.StoryScene.TrainingShip)
		{
			yield break;
		}
		yield return new WaitForSeconds(time);
		while (true)
		{
			mCScreator = CSMain.GetCreator(0);
			if (mCScreator != null)
			{
				mCSNpcs = CSMain.GetCSNpcs(mCScreator);
				mCSnpcTeam.setCSCreator(mCScreator);
				if (mCScreator.Assembly != null && mCSNpcs.Count > 0)
				{
					mCSnpcTeam.InitTeam();
					mCSnpcTeam.AddInTeam(mCSNpcs);
					mCSnpcTeam.ReFlashTeam();
				}
			}
			yield return new WaitForSeconds(time);
		}
	}

	private IEnumerator CampTeam(float time)
	{
		yield return new WaitForSeconds(time);
	}

	private IEnumerator Tracking(PeEntity entity, float time)
	{
		while (true)
		{
			yield return new WaitForSeconds(time);
		}
	}

	public void OnCSAttackEnmey(PeEntity entity, PeEntity enmey)
	{
		if (PEUtil.CanAttackReputation(entity, enmey))
		{
			mCSnpcTeam.OnAlertInform(enmey);
			if (mCScreator != null)
			{
				NpcSupply.SupplyNpcsByCSAssembly(mCSNpcs, mCScreator.Assembly, ESupplyType.Ammo);
			}
		}
	}

	public void OnCsAttackEnd()
	{
		mCSnpcTeam.OnClearAlert();
	}

	public void OnCsAttackTypeChange(PeEntity entity, AttackType oldType, AttackType newType)
	{
		if (mCScreator != null && mCScreator.Assembly != null && mCSnpcTeam != null)
		{
			mCSnpcTeam.OnAttackTypeChange(entity, oldType, newType);
		}
	}

	public void OnCSAddDamageHaterd(PeEntity enmey, PeEntity beDamage, float hater)
	{
		if (!(mCScreator == null) && mCScreator.Assembly != null && !(beDamage == null) && !(enmey == null) && !(beDamage.NpcCmpt == null) && !(beDamage.NpcCmpt.Creater == null) && beDamage.NpcCmpt.Creater.Assembly != null)
		{
			float num = PEUtil.MagnitudeH(mCScreator.Assembly.Position, enmey.position);
			if (num <= mCScreator.Assembly.Radius * 1.5f)
			{
				OnCSAttackEnmey(beDamage, enmey);
			}
		}
	}

	public void OnCsLineChange(PeEntity member, ELineType oldType, ELineType newType)
	{
		if (mCScreator != null && mCScreator.Assembly != null && mCSnpcTeam != null)
		{
			mCSnpcTeam.OnLineChange(member, oldType, newType);
		}
	}

	public void OnCsJobChange(PeEntity member, ENpcJob oldJob, ENpcJob newJob)
	{
		if (mCScreator != null && mCScreator.Assembly != null && mCSnpcTeam != null)
		{
			mCSnpcTeam.OnCsNpcJobChange(member, oldJob, newJob);
		}
	}

	public void OnRemoveCsNpc(PeEntity member)
	{
		if (mCScreator != null && mCScreator.Assembly != null && mCSnpcTeam != null)
		{
			mCSnpcTeam.OnCsNPcRemove(member);
		}
	}

	public void OnEnemyLost(Enemy enemy)
	{
		if (mCScreator != null && mCScreator.Assembly != null && mCSnpcTeam != null)
		{
			mCSnpcTeam.OnTargetLost(enemy.entityTarget);
		}
	}

	public void OnAssemblyHpChange(SkEntity caster, float hpChange)
	{
		if (mCScreator != null && mCScreator.Assembly != null && mCSnpcTeam != null)
		{
			mCSnpcTeam.OnAssemblyHpChange(caster, hpChange);
		}
	}

	private void Awake()
	{
		mInstance = this;
		if (mCSnpcTeam == null)
		{
			mCSnpcTeam = new CSNpcTeam();
		}
	}

	private void Start()
	{
		StartCoroutine(CSTeam(5f));
		StartCoroutine(CampTeam(5f));
	}
}
