using Pathea;
using Pathea.Operate;
using UnityEngine;
using WhiteCat;

public class SleepController : MonoBehaviour, FastTravelMgr.IEnable
{
	private OperateCmpt character;

	private PESleep peSleep;

	private float maxSleepTime;

	private float timeCount;

	private float actionTime;

	private bool isMainPlayer;

	bool FastTravelMgr.IEnable.Enable()
	{
		return peSleep == null;
	}

	public static void StartSleep(PESleep peSleep, PeEntity character, float hours = 12f)
	{
		if (!(null != character))
		{
			return;
		}
		SleepController sc = character.GetComponent<SleepController>();
		if (sc == null)
		{
			sc = character.gameObject.AddComponent<SleepController>();
			sc.isMainPlayer = PeSingleton<PeCreature>.Instance.mainPlayer == character;
			if (sc.isMainPlayer)
			{
				MotionMgrCmpt cmpt = character.GetCmpt<MotionMgrCmpt>();
				Action_Sleep action = cmpt.GetAction<Action_Sleep>();
				action.startSleepEvt += sc.MainPlayerStartSleep;
				action.startSleepEvt += delegate
				{
					PeSingleton<FastTravelMgr>.Instance.Add(sc);
				};
				action.endSleepEvt += sc.MainPlayerEndSleep;
				action.endSleepEvt += delegate
				{
					PeSingleton<FastTravelMgr>.Instance.Remove(sc);
				};
			}
		}
		sc.character = character.GetComponent<OperateCmpt>();
		sc.maxSleepTime = hours * 3600f;
		sc.timeCount = 0f;
		sc.peSleep = peSleep;
		sc.enabled = true;
		sc.actionTime = 2f;
		peSleep.StartOperate(sc.character, EOperationMask.Sleep);
	}

	private void MainPlayerStartSleep(int buffID)
	{
		PeSingleton<MousePicker>.Instance.enable = false;
		GameUI.Instance.mItemOp.ShowSleepingUI(() => 1f - timeCount / maxSleepTime);
		if (!PeGameMgr.IsMulti)
		{
			GameTime.Timer.ElapseSpeedBak = GameTime.Timer.ElapseSpeed;
			GameTime.Timer.ElapseSpeed = PEVCConfig.instance.sleepTimeScale;
			GameTime.AbnormalityTimePass = true;
		}
	}

	private void MainPlayerEndSleep(int buffID)
	{
		PeSingleton<MousePicker>.Instance.enable = true;
		GameUI.Instance.mItemOp.HideSleepingUI();
		if (!PeGameMgr.IsMulti)
		{
			if (GameTime.Timer.ElapseSpeedBak >= 0f)
			{
				GameTime.Timer.ElapseSpeed = GameTime.Timer.ElapseSpeedBak;
				GameTime.Timer.ElapseSpeedBak = -1f;
			}
			GameTime.AbnormalityTimePass = false;
		}
		peSleep = null;
	}

	private void Update()
	{
		actionTime -= Time.deltaTime;
		timeCount += GameTime.DeltaTime;
		if (timeCount >= maxSleepTime || (isMainPlayer && actionTime < 0f && (PeInput.Get(PeInput.LogicFunction.InteractWithItem) || EntityMonsterBeacon.IsRunning())))
		{
			if (peSleep != null)
			{
				peSleep.StopOperate(character, EOperationMask.Sleep);
			}
			base.enabled = false;
		}
	}
}
