using Pathea;
using UnityEngine;

namespace TrainingScene;

public class HolotreeCutter : MonoBehaviour
{
	public int MaxHP;

	[HideInInspector]
	public int HP;

	private PeEntity mplayer;

	private AnimatorCmpt anmt;

	private Action_Fell af;

	private MotionMgrCmpt mmc;

	private HolotreeTask ht;

	private bool iscut0;

	private bool iscut1;

	private bool iscut2;

	private bool cutting;

	private bool ishited;

	private bool iscut3;

	private float ctime;

	private float hppct = 1f;

	private void Start()
	{
		if (MaxHP <= 0)
		{
			MaxHP = 1;
		}
		HP = MaxHP;
		mplayer = PeSingleton<PeCreature>.Instance.mainPlayer;
		anmt = mplayer.animCmpt;
		mmc = mplayer.motionMgr;
		af = mmc.GetAction<Action_Fell>();
		ht = HolotreeTask.Instance;
	}

	private void Update()
	{
		if ((bool)af.m_Axe && mmc.GetMaskState(PEActionMask.EquipmentHold) && !cutting && PeInput.Get(PeInput.LogicFunction.Cut))
		{
			RaycastHit[] array = Physics.RaycastAll(new Ray(mplayer.position, mplayer.forward), 2f);
			RaycastHit[] array2 = array;
			foreach (RaycastHit raycastHit in array2)
			{
				if (raycastHit.transform == base.transform.parent.parent)
				{
					mmc.EndImmediately(PEActionType.Move);
					mmc.EndImmediately(PEActionType.Sprint);
					mmc.EndImmediately(PEActionType.Rotate);
					ctime = 0f - Time.deltaTime;
					iscut0 = true;
					anmt.SetBool("Fell", value: true);
					UITreeCut.Instance.Show();
					UITreeCut.Instance.SetSliderValue(null, hppct);
					break;
				}
			}
		}
		else if (!PeInput.Get(PeInput.LogicFunction.Cut))
		{
			anmt.SetBool("Fell", value: false);
		}
		if (iscut0 && anmt.IsAnimPlaying("DoNothing", 1))
		{
			mmc.SetMaskState(PEActionMask.Fell, state: true);
			cutting = true;
			iscut0 = false;
			iscut1 = true;
		}
		if (cutting)
		{
			ctime += Time.deltaTime;
		}
		if (iscut1)
		{
			if (ctime > 0.6f)
			{
				iscut1 = false;
				iscut2 = true;
				ctime -= 0.6f;
			}
		}
		else if (iscut2)
		{
			if (ctime >= 1.1f)
			{
				ctime -= 1.1f;
				ishited = false;
				if (!anmt.GetBool("Fell"))
				{
					iscut2 = false;
					iscut3 = true;
				}
			}
			else
			{
				if (!(ctime >= 0.6f))
				{
					return;
				}
				if (!ishited)
				{
					ishited = true;
					if (null != af.m_Axe)
					{
						Transform transform = af.m_Axe.transform.Find("CutPoint");
						ApplyCut((!(transform != null)) ? Vector3.zero : transform.position);
						ht.SubHP();
					}
					else
					{
						anmt.SetBool("Fell", value: false);
						cutting = false;
						iscut0 = false;
						iscut1 = false;
						iscut2 = false;
						iscut3 = false;
						mmc.SetMaskState(PEActionMask.Fell, state: false);
						UITreeCut.Instance.Hide();
					}
				}
				if (HP == 0)
				{
					anmt.SetBool("Fell", value: false);
				}
			}
		}
		else if (iscut3 && ctime >= 0.533f)
		{
			cutting = false;
			iscut3 = false;
			mmc.SetMaskState(PEActionMask.Fell, state: false);
			UITreeCut.Instance.Hide();
		}
	}

	private void ApplyCut(Vector3 effPoint)
	{
		hppct = (float)(--HP) / (float)MaxHP;
		base.transform.GetComponent<HolotreeAppearance>().hppct = hppct;
		if (hppct < 0.01f)
		{
			UITreeCut.Instance.Hide();
		}
		else
		{
			UITreeCut.Instance.SetSliderValue(null, hppct);
		}
		if (effPoint != Vector3.zero)
		{
			GameObject gameObject = Object.Instantiate(Resources.Load("Prefab/Particle/FX_treeWood_01"), effPoint, Quaternion.identity) as GameObject;
			gameObject.AddComponent<DestroyTimer>();
			gameObject.GetComponent<DestroyTimer>().m_LifeTime = 3f;
		}
		base.transform.GetComponent<AudioSource>().volume = SystemSettingData.Instance.EffectVolume * SystemSettingData.Instance.SoundVolume;
		base.transform.GetComponent<AudioSource>().Play();
		if (HP <= 0)
		{
			PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>().Add(279, 1);
			TrainingTaskManager.Instance.CompleteMission();
			base.transform.parent.parent.GetComponent<Collider>().enabled = false;
		}
	}
}
