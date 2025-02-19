using Pathea;

namespace Behave.Runtime;

[BehaveAction(typeof(BTIsWaitJob), "IsWaitJob")]
public class BTIsWaitJob : BTNormal
{
	private class Data
	{
		[Behave]
		public int Jobtype;

		public ENpcJob mNpcJobType => (ENpcJob)Jobtype;
	}

	private Data m_Data;

	private bool StopWait()
	{
		return m_Data.mNpcJobType switch
		{
			ENpcJob.Trainer => base.WorkEntity != null && base.WorkEntity.workTrans != null && base.NpcTrainerType != 0 && base.IsNpcTrainning, 
			ENpcJob.Worker => base.Work != null && !base.Work.Equals(null), 
			ENpcJob.Processor => base.IsNpcProcessing, 
			ENpcJob.Follower => base.entity != null && base.entity.NpcCmpt != null && base.entity.NpcCmpt.IsServant, 
			ENpcJob.Doctor => base.Cured != null && !base.Cured.Equals(null) && base.WorkEntity != null && base.WorkEntity.workTrans != null, 
			ENpcJob.Farmer => IsStopFormerWait(), 
			_ => false, 
		};
	}

	private bool IsStopFormerWait()
	{
		if (!base.IsNpcBase || base.NpcJob != ENpcJob.Farmer)
		{
			return true;
		}
		if (ContainsTitle(ENpcTitle.Plant))
		{
			return !CSUtils.FarmPlantReady(base.entity) && CSUtils.FindPlantPosNewChunk(base.entity) != null;
		}
		if (ContainsTitle(ENpcTitle.Manage))
		{
			return (!CSUtils.FarmWaterReady(base.entity) && CSUtils.FindPlantToWater(base.entity) != null) || (!CSUtils.FarmCleanReady(base.entity) && CSUtils.FindPlantToClean(base.entity) != null);
		}
		if (ContainsTitle(ENpcTitle.Harvest))
		{
			return CSUtils.FindPlantGet(base.entity) != null || CSUtils.FindPlantRemove(base.entity) != null;
		}
		return true;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.NpcJob != m_Data.mNpcJobType)
		{
			return BehaveResult.Failure;
		}
		if (StopWait())
		{
			return BehaveResult.Failure;
		}
		return BehaveResult.Success;
	}
}
