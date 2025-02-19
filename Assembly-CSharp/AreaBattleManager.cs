using System;
using System.Collections.Generic;
using AiAsset;
using UnityEngine;

public class AreaBattleManager : MonoBehaviour
{
	public class AreaUnit
	{
		public List<BattleUnit> mDefenderList = new List<BattleUnit>();

		public List<BattleUnit> mEnemyList = new List<BattleUnit>();

		public List<BattleUnit> mUnInstantiateEnemy = new List<BattleUnit>();

		private Bounds mBounds;

		private AreaBattleManager mParent;

		private List<AreaUnit> mNearArea = new List<AreaUnit>();

		private float DefenderBE;

		private float EnemyBE;

		public float mDefenderForce = 100f;

		public bool mPlayerCtrlArea;

		private float mUpdateInterval = 5f;

		public float mUpdatePassTime;

		private static float DamageScale = 1f;

		public float UsableDefenderBE()
		{
			if (IsInBattle() && EnemyBE > 0f)
			{
				return DefenderBE * DefenderBE / EnemyBE;
			}
			return DefenderBE;
		}

		public bool IsInBattle()
		{
			return mDefenderList.Count > 0 && mEnemyList.Count > 0;
		}

		public float DefendWeight()
		{
			return (!IsInBattle()) ? 1f : (DefenderBE / (DefenderBE + EnemyBE));
		}

		public void SetRange(Vector3 min, Vector3 max, AreaBattleManager parent)
		{
			mBounds.SetMinMax(min, max);
			mParent = parent;
		}

		public void AddNearArea(AreaUnit areaUnit)
		{
			mNearArea.Add(areaUnit);
		}

		public bool IsInArea(Vector3 pos)
		{
			return mBounds.Contains(pos);
		}

		public void AddBattleUnit(BattleUnit battleUnit)
		{
			if (battleUnit.mPlayerForce)
			{
				if (!mPlayerCtrlArea)
				{
					mDefenderList.Add(battleUnit);
				}
			}
			else if (mPlayerCtrlArea)
			{
				mUnInstantiateEnemy.Add(battleUnit);
			}
			else
			{
				mEnemyList.Add(battleUnit);
			}
		}

		public void Update()
		{
			mUpdatePassTime += Time.deltaTime;
			if (!(mUpdatePassTime > mUpdateInterval))
			{
				return;
			}
			mUpdatePassTime -= mUpdateInterval;
			if (mPlayerCtrlArea)
			{
				CreatEnemyMode();
				return;
			}
			DefenderBE = 0f;
			EnemyBE = 0f;
			bool flag = IsInBattle();
			for (int num = mDefenderList.Count - 1; num >= 0; num--)
			{
				if (mDefenderList[num].mHp > 0f)
				{
					if (flag && mEnemyList.Count > 0 && Convert.ToBoolean(mDefenderList[num].mType & 0x20))
					{
						switch (mDefenderList[num].mAtkRange)
						{
						case EffectRange.Single:
						{
							BattleUnit battleUnit2 = mEnemyList[UnityEngine.Random.Range(0, mEnemyList.Count)];
							float num4 = mDefenderList[num].mAtk * (1f - battleUnit2.mDef / (battleUnit2.mDef + DamageScale)) * AiDamageTypeData.GetDamageScale(mDefenderList[num].mAtkType, battleUnit2.mDefType);
							battleUnit2.mHp -= num4 * mUpdateInterval / mDefenderList[num].mAtkInterval;
							break;
						}
						case EffectRange.Range:
							foreach (BattleUnit mEnemy in mEnemyList)
							{
								float num3 = mDefenderList[num].mAtk * (1f - mEnemy.mDef / (mEnemy.mDef + DamageScale)) * AiDamageTypeData.GetDamageScale(mDefenderList[num].mAtkType, mEnemy.mDefType);
								mEnemy.mHp -= num3 * mUpdateInterval / mDefenderList[num].mAtkInterval;
							}
							break;
						case EffectRange.spread:
						{
							BattleUnit battleUnit = mEnemyList[UnityEngine.Random.Range(0, mEnemyList.Count)];
							float num2 = mDefenderList[num].mAtk * (1f - battleUnit.mDef / (battleUnit.mDef + DamageScale)) * AiDamageTypeData.GetDamageScale(mDefenderList[num].mAtkType, battleUnit.mDefType);
							battleUnit.mHp -= num2 * mUpdateInterval / mDefenderList[num].mAtkInterval;
							foreach (BattleUnit mEnemy2 in mEnemyList)
							{
								if (battleUnit != mEnemy2)
								{
									num2 = mDefenderList[num].mAtk * (1f - mEnemy2.mDef / (mEnemy2.mDef + DamageScale)) * AiDamageTypeData.GetDamageScale(mDefenderList[num].mAtkType, mEnemy2.mDefType) * mDefenderList[num].mSpreadFactor;
									mEnemy2.mHp -= num2 * mUpdateInterval / mDefenderList[num].mAtkInterval;
								}
							}
							break;
						}
						}
					}
					if (Convert.ToBoolean(mDefenderList[num].mType & 0x40))
					{
						switch (mDefenderList[num].mHealRange)
						{
						case EffectRange.Single:
						{
							BattleUnit battleUnit4 = null;
							float num6 = 0f;
							foreach (BattleUnit mDefender in mDefenderList)
							{
								if (mDefender.mHp < mDefender.mMaxHp && Convert.ToBoolean(mDefender.mType & mDefenderList[num].mHealType) && num6 < mDefender.mMaxHp - mDefender.mHp)
								{
									num6 = mDefender.mMaxHp - mDefender.mHp;
									battleUnit4 = mDefender;
								}
							}
							if ((bool)battleUnit4)
							{
								battleUnit4.mHp = Mathf.Clamp(battleUnit4.mHp + mDefenderList[num].mHealPs * mUpdateInterval, 0f, battleUnit4.mMaxHp);
							}
							break;
						}
						case EffectRange.Range:
							foreach (BattleUnit mDefender2 in mDefenderList)
							{
								if (mDefender2.mHp < mDefender2.mMaxHp && Convert.ToBoolean(mDefender2.mType & mDefenderList[num].mHealType))
								{
									mDefender2.mHp = Mathf.Clamp(mDefender2.mHp + mDefenderList[num].mHealPs * mUpdateInterval, 0f, mDefender2.mMaxHp);
								}
							}
							break;
						case EffectRange.spread:
						{
							BattleUnit battleUnit3 = null;
							float num5 = 0f;
							foreach (BattleUnit mDefender3 in mDefenderList)
							{
								if (mDefender3.mHp < mDefender3.mMaxHp && Convert.ToBoolean(mDefender3.mType & mDefenderList[num].mHealType) && num5 < mDefender3.mMaxHp - mDefender3.mHp)
								{
									num5 = mDefender3.mMaxHp - mDefender3.mHp;
									battleUnit3 = mDefender3;
								}
							}
							if ((bool)battleUnit3)
							{
								battleUnit3.mHp = Mathf.Clamp(battleUnit3.mHp + mDefenderList[num].mHealPs * mUpdateInterval, 0f, battleUnit3.mMaxHp);
							}
							foreach (BattleUnit mDefender4 in mDefenderList)
							{
								if (mDefender4 != battleUnit3 && mDefender4.mHp < mDefender4.mMaxHp && Convert.ToBoolean(mDefender4.mType & mDefenderList[num].mHealType))
								{
									mDefender4.mHp = Mathf.Clamp(mDefender4.mHp + mDefenderList[num].mHealPs * mUpdateInterval * mDefenderList[num].mSpreadFactor, 0f, mDefender4.mMaxHp);
								}
							}
							break;
						}
						}
					}
				}
			}
			for (int num7 = mEnemyList.Count - 1; num7 >= 0; num7--)
			{
				if (mEnemyList[num7].mHp > 0f)
				{
					if (flag && mDefenderList.Count > 0 && Convert.ToBoolean(mEnemyList[num7].mType & 0x20))
					{
						switch (mEnemyList[num7].mAtkRange)
						{
						case EffectRange.Single:
						{
							BattleUnit battleUnit6 = mDefenderList[UnityEngine.Random.Range(0, mDefenderList.Count)];
							float num10 = mEnemyList[num7].mAtk * (1f - battleUnit6.mDef / (battleUnit6.mDef + DamageScale)) * AiDamageTypeData.GetDamageScale(mEnemyList[num7].mAtkType, battleUnit6.mDefType);
							battleUnit6.mHp -= num10 * mUpdateInterval / mEnemyList[num7].mAtkInterval;
							break;
						}
						case EffectRange.Range:
							foreach (BattleUnit mDefender5 in mDefenderList)
							{
								float num9 = mEnemyList[num7].mAtk * (1f - mDefender5.mDef / (mDefender5.mDef + DamageScale)) * AiDamageTypeData.GetDamageScale(mEnemyList[num7].mAtkType, mDefender5.mDefType);
								mDefender5.mHp -= num9 * mUpdateInterval / mEnemyList[num7].mAtkInterval;
							}
							break;
						case EffectRange.spread:
						{
							BattleUnit battleUnit5 = mDefenderList[UnityEngine.Random.Range(0, mDefenderList.Count)];
							float num8 = mEnemyList[num7].mAtk * (1f - battleUnit5.mDef / (battleUnit5.mDef + DamageScale)) * AiDamageTypeData.GetDamageScale(mEnemyList[num7].mAtkType, battleUnit5.mDefType);
							battleUnit5.mHp -= num8 * mUpdateInterval / mEnemyList[num7].mAtkInterval;
							foreach (BattleUnit mDefender6 in mDefenderList)
							{
								if (mDefender6 != battleUnit5)
								{
									num8 = mEnemyList[num7].mAtk * (1f - mDefender6.mDef / (mDefender6.mDef + DamageScale)) * AiDamageTypeData.GetDamageScale(mEnemyList[num7].mAtkType, mDefender6.mDefType);
									mDefender6.mHp -= num8 * mUpdateInterval / mEnemyList[num7].mAtkInterval;
								}
							}
							break;
						}
						}
					}
					if (Convert.ToBoolean(mEnemyList[num7].mType & 0x40))
					{
						switch (mEnemyList[num7].mHealRange)
						{
						case EffectRange.Single:
						{
							BattleUnit battleUnit8 = null;
							float num12 = 0f;
							foreach (BattleUnit mEnemy3 in mEnemyList)
							{
								if (mEnemy3.mHp < mEnemy3.mMaxHp && Convert.ToBoolean(mEnemy3.mType & mEnemyList[num7].mHealType) && num12 < mEnemy3.mMaxHp - mEnemy3.mHp)
								{
									num12 = mEnemy3.mMaxHp - mEnemy3.mHp;
									battleUnit8 = mEnemy3;
								}
							}
							if ((bool)battleUnit8)
							{
								battleUnit8.mHp = Mathf.Clamp(battleUnit8.mHp + mEnemyList[num7].mHealPs * mUpdateInterval, 0f, battleUnit8.mMaxHp);
							}
							break;
						}
						case EffectRange.Range:
							foreach (BattleUnit mEnemy4 in mEnemyList)
							{
								if (mEnemy4.mHp < mEnemy4.mMaxHp && Convert.ToBoolean(mEnemy4.mType & mEnemyList[num7].mHealType))
								{
									mEnemy4.mHp = Mathf.Clamp(mEnemy4.mHp + mEnemyList[num7].mHealPs * mUpdateInterval, 0f, mEnemy4.mMaxHp);
								}
							}
							break;
						case EffectRange.spread:
						{
							BattleUnit battleUnit7 = null;
							float num11 = 0f;
							foreach (BattleUnit mEnemy5 in mEnemyList)
							{
								if (mEnemy5.mHp < mEnemy5.mMaxHp && Convert.ToBoolean(mEnemy5.mType & mEnemyList[num7].mHealType) && num11 < mEnemy5.mMaxHp - mEnemy5.mHp)
								{
									num11 = mEnemy5.mMaxHp - mEnemy5.mHp;
									battleUnit7 = mEnemy5;
								}
							}
							if ((bool)battleUnit7)
							{
								battleUnit7.mHp = Mathf.Clamp(battleUnit7.mHp + mEnemyList[num7].mHealPs * mUpdateInterval, 0f, battleUnit7.mMaxHp);
							}
							foreach (BattleUnit mEnemy6 in mEnemyList)
							{
								if (mEnemy6 != battleUnit7 && mEnemy6.mHp < mEnemy6.mMaxHp && Convert.ToBoolean(mEnemy6.mType & mEnemyList[num7].mHealType))
								{
									mEnemy6.mHp = Mathf.Clamp(mEnemy6.mHp + mEnemyList[num7].mHealPs * mUpdateInterval * mEnemyList[num7].mSpreadFactor, 0f, mEnemy6.mMaxHp);
								}
							}
							break;
						}
						}
					}
				}
			}
			for (int num13 = mDefenderList.Count - 1; num13 >= 0; num13--)
			{
				if (mDefenderList[num13].mHp > 0f)
				{
					DefenderBE += mDefenderList[num13].mBE;
				}
				else
				{
					if (mDefenderList[num13] == mParent.mSelfUnit)
					{
						mParent.OnBattleFail();
					}
					RemoveFromDragItemMgr(mDefenderList[num13].gameObject);
				}
			}
			for (int num14 = mEnemyList.Count - 1; num14 >= 0; num14--)
			{
				if (mEnemyList[num14].mHp > 0f)
				{
					EnemyBE += mEnemyList[num14].mBE;
				}
				else
				{
					UnityEngine.Object.Destroy(mEnemyList[num14].gameObject);
					mEnemyList.RemoveAt(num14);
				}
			}
			if (DefenderBE == 0f && EnemyBE > 0f)
			{
				AreaUnit areaUnit = null;
				float num15 = 100000000f;
				foreach (AreaUnit item in mNearArea)
				{
					if (item.mDefenderForce > mDefenderForce && item.mDefenderForce < num15)
					{
						areaUnit = item;
						num15 = item.mDefenderForce;
					}
				}
				if (areaUnit == null)
				{
					num15 = 0f;
					foreach (AreaUnit item2 in mNearArea)
					{
						if (item2.mDefenderForce > num15)
						{
							areaUnit = item2;
							num15 = item2.mDefenderForce;
						}
					}
				}
				for (int num16 = mEnemyList.Count - 1; num16 >= 0; num16--)
				{
					if (Convert.ToBoolean(mEnemyList[num16].mType & 1) && mEnemyList[num16].CanMove())
					{
						mEnemyList[num16].Move();
						areaUnit.AddBattleUnit(mEnemyList[num16]);
						mEnemyList.RemoveAt(num16);
					}
				}
			}
			else if (EnemyBE == 0f && !(DefenderBE > 0f))
			{
			}
		}

		private void RemoveFromDragItemMgr(GameObject obj)
		{
			ItemScript component = obj.GetComponent<ItemScript>();
			if (null == component)
			{
				Debug.LogError("has no itemscript");
			}
			else
			{
				DragArticleAgent.Destory(component.id);
			}
		}

		public void ActiveAsPlayerCtrl()
		{
			mPlayerCtrlArea = true;
			SynchBattleUnit();
		}

		public void UnactiveAsAutoCtrl()
		{
			mPlayerCtrlArea = false;
			RecountBattleUnit();
		}

		public void CreatEnemyMode()
		{
			Dictionary<int, List<BattleUnit>> dictionary = new Dictionary<int, List<BattleUnit>>();
			foreach (BattleUnit item in mUnInstantiateEnemy)
			{
				if (dictionary.ContainsKey(item.mID))
				{
					dictionary[item.mID].Add(item);
					continue;
				}
				dictionary[item.mID] = new List<BattleUnit>();
				dictionary[item.mID].Add(item);
			}
			foreach (int key in dictionary.Keys)
			{
				if (mParent.CanCreateModeNum() > 0)
				{
					BattleUnitData battleUnitData = BattleUnitData.GetBattleUnitData(key);
					int num = ((dictionary[key].Count > mParent.CanCreateModeNum()) ? mParent.CanCreateModeNum() : dictionary[key].Count);
					for (int i = 0; i < num; i++)
					{
						string[] array = battleUnitData.mPerfabPath.Split(',');
						GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load(array[1])) as GameObject;
						gameObject.transform.parent = mParent.transform;
						gameObject.transform.position = GetSaveCreatePos();
						gameObject.transform.rotation = Quaternion.identity;
						mParent.CreatMode(gameObject.GetComponent<BattleUnit>());
						mUnInstantiateEnemy.Remove(dictionary[key][i]);
						UnityEngine.Object.Destroy(dictionary[key][i].gameObject);
					}
					continue;
				}
				break;
			}
		}

		private Vector3 GetSaveCreatePos()
		{
			return Vector3.zero;
		}

		private void SynchBattleUnit()
		{
			mUnInstantiateEnemy.AddRange(mEnemyList);
			mEnemyList.Clear();
			mDefenderList.Clear();
		}

		public void RecountBattleUnit()
		{
			mDefenderList.Clear();
			List<BattleUnit> mCurrentShowMode = mParent.mCurrentShowMode;
			for (int num = mCurrentShowMode.Count - 1; num >= 0; num--)
			{
				if ((bool)mCurrentShowMode[num] && mBounds.Contains(mCurrentShowMode[num].transform.position))
				{
					BattleUnit battleUnit = UnityEngine.Object.Instantiate(mParent.mPerfab);
					battleUnit.SetData(BattleUnitData.GetBattleUnitData(mCurrentShowMode[num].mID));
					UnityEngine.Object.Destroy(mCurrentShowMode[num].gameObject);
					mCurrentShowMode.RemoveAt(num);
					mEnemyList.Add(battleUnit);
				}
			}
		}
	}

	public class AreaCube
	{
		public AreaUnit mAreaUnit;

		public GameObject mCube;

		public void Update()
		{
			if (mAreaUnit != null && null != mCube)
			{
				float num = mAreaUnit.DefendWeight();
				mCube.GetComponent<Renderer>().material.color = new Color(1f - num, num, 0f, Mathf.Clamp01(mAreaUnit.mDefenderForce / 10000000f));
			}
		}
	}

	private static AreaBattleManager mInstance;

	public BattleUnit mSelfUnit;

	public int mSingleAreaLength;

	public int mAreaNum_OneSide;

	public int mActiveAreaRadius;

	private int mAreaBattleLength;

	private int mLongestLength;

	public float mUpdateInterval;

	private float mPassTime;

	public int mEnemyTotal;

	private int mFreeEnemyNum;

	public int mEnemyMax;

	private int mCurrentEnemyNum;

	public float mEnemyRecoverPS;

	private float mReadyEnemyNum;

	public float mGenInterval;

	private float mGenPassTime;

	public int mEnemyGeneratedMax;

	public int mEnemyGeneratedMin;

	private bool mActive;

	public BattleUnit mPerfab;

	public List<int> mGenEnemyID;

	public List<int> mGenEnemyP;

	public int mMaxShowModelNum;

	[HideInInspector]
	public List<BattleUnit> mCurrentShowMode;

	private List<BattleUnit> mEnergyUnitList;

	private List<BattleUnit> mAmmoUnitList;

	public bool mStart;

	private AreaUnit[,] mAreaUnitList;

	private AreaCube[,] mAreaCubeList;

	private List<AreaUnit> mBorderlineArea;

	public static AreaBattleManager Instance => mInstance;

	private void Start()
	{
		mSelfUnit = GetComponent<BattleUnit>();
		mInstance = this;
	}

	public void InitBattle()
	{
		mActive = true;
		mAreaBattleLength = mAreaNum_OneSide * mSingleAreaLength;
		mLongestLength = (mAreaNum_OneSide - 1) * 2;
		mFreeEnemyNum = mEnemyTotal;
		mAreaUnitList = new AreaUnit[mAreaNum_OneSide, mAreaNum_OneSide];
		if (Application.isEditor)
		{
			mAreaCubeList = new AreaCube[mAreaNum_OneSide, mAreaNum_OneSide];
		}
		mBorderlineArea = new List<AreaUnit>();
		for (int i = 0; i < mAreaNum_OneSide; i++)
		{
			for (int j = 0; j < mAreaNum_OneSide; j++)
			{
				mAreaUnitList[i, j] = new AreaUnit();
				Vector3 zero = Vector3.zero;
				zero.x = base.transform.position.x + ((float)(i - mAreaNum_OneSide / 2) - 0.5f) * (float)mSingleAreaLength;
				zero.z = base.transform.position.z + ((float)(j - mAreaNum_OneSide / 2) - 0.5f) * (float)mSingleAreaLength;
				zero.y = 0f;
				Vector3 max = new Vector3(zero.x + (float)mSingleAreaLength, 3000f, zero.z + (float)mSingleAreaLength);
				mAreaUnitList[i, j].SetRange(zero, max, this);
				mAreaUnitList[i, j].mUpdatePassTime = (float)i * 0.9f;
				mAreaUnitList[i, j].RecountBattleUnit();
				if (i == 0 || i == mAreaNum_OneSide - 1 || j == 0 || j == mAreaNum_OneSide - 1)
				{
					mBorderlineArea.Add(mAreaUnitList[i, j]);
				}
				if (Application.isEditor)
				{
					mAreaCubeList[i, j] = new AreaCube();
					mAreaCubeList[i, j].mAreaUnit = mAreaUnitList[i, j];
					mAreaCubeList[i, j].mCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
					mAreaCubeList[i, j].mCube.GetComponent<Renderer>().material = new Material(Shader.Find("Transparent/Diffuse"));
					mAreaCubeList[i, j].mCube.GetComponent<Renderer>().material.color = new Color(0f, 1f, 0f, 0.25f);
					UnityEngine.Object.Destroy(mAreaCubeList[i, j].mCube.GetComponent<BoxCollider>());
					mAreaCubeList[i, j].mCube.transform.parent = base.transform;
					mAreaCubeList[i, j].mCube.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
					mAreaCubeList[i, j].mCube.transform.position = base.transform.position + new Vector3(5f / (float)mAreaNum_OneSide * (float)(i - mAreaNum_OneSide / 2), 2f, 5f / (float)mAreaNum_OneSide * (float)(j - mAreaNum_OneSide / 2));
					mAreaCubeList[i, j].Update();
				}
			}
		}
		for (int k = 0; k < mAreaNum_OneSide; k++)
		{
			for (int l = 0; l < mAreaNum_OneSide; l++)
			{
				if (k > 0)
				{
					mAreaUnitList[k, l].AddNearArea(mAreaUnitList[k - 1, l]);
				}
				if (k < mAreaNum_OneSide - 1)
				{
					mAreaUnitList[k, l].AddNearArea(mAreaUnitList[k + 1, l]);
				}
				if (l > 0)
				{
					mAreaUnitList[k, l].AddNearArea(mAreaUnitList[k, l - 1]);
				}
				if (l < mAreaNum_OneSide - 1)
				{
					mAreaUnitList[k, l].AddNearArea(mAreaUnitList[k, l + 1]);
				}
			}
		}
		mPassTime = 0f;
	}

	public int CanCreateModeNum()
	{
		return mMaxShowModelNum - mCurrentShowMode.Count;
	}

	public void CreatMode(BattleUnit bu)
	{
		mCurrentShowMode.Add(bu);
	}

	private void Update()
	{
		if (mActive)
		{
			GeneratNewEnemy();
			UpdateAreaUnit();
			ClearDeathEnemy();
			mPassTime += Time.deltaTime;
			if (mPassTime > mUpdateInterval)
			{
				mPassTime -= mUpdateInterval;
				UpdatePlayerCtrlAreaState();
				UpdateAreaForce();
			}
		}
		if (mStart)
		{
			mStart = false;
			if (!mActive)
			{
				InitBattle();
			}
		}
	}

	private void GeneratNewEnemy()
	{
		mReadyEnemyNum += mEnemyRecoverPS * mUpdateInterval;
		mGenPassTime += Time.deltaTime;
		if (!(mGenPassTime >= mGenInterval))
		{
			return;
		}
		int num = 0;
		if (mReadyEnemyNum >= (float)mFreeEnemyNum)
		{
			num = mFreeEnemyNum;
			mFreeEnemyNum = 0;
		}
		else if (mReadyEnemyNum >= (float)mEnemyGeneratedMax)
		{
			num = mEnemyGeneratedMax;
			mFreeEnemyNum -= num;
		}
		else
		{
			if (!(mReadyEnemyNum >= (float)mEnemyGeneratedMin))
			{
				return;
			}
			num = (int)((float)mFreeEnemyNum - mReadyEnemyNum);
			mFreeEnemyNum -= num;
		}
		mGenPassTime -= mGenInterval;
		float num2 = 0f;
		for (int i = 0; i < mBorderlineArea.Count; i++)
		{
			num2 += 1f / mBorderlineArea[i].mDefenderForce;
		}
		float num3 = UnityEngine.Random.Range(0f, 1f);
		int index = 0;
		float num4 = 0f;
		for (int j = 0; j < mBorderlineArea.Count; j++)
		{
			if (num4 < num3 && num4 + 1f / mBorderlineArea[j].mDefenderForce / num2 >= num3)
			{
				index = j;
				break;
			}
			num4 += 1f / mBorderlineArea[j].mDefenderForce / num2;
		}
		int num5 = 0;
		int num6 = 0;
		for (int k = 0; k < mGenEnemyP.Count; k++)
		{
			num5 += mGenEnemyP[k];
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		for (int l = 0; l < num; l++)
		{
			num6 = UnityEngine.Random.Range(0, num5);
			num4 = 0f;
			for (int m = 0; m < mGenEnemyP.Count; m++)
			{
				if (num4 <= (float)num6 && num4 + (float)mGenEnemyP[m] > (float)num6)
				{
					if (dictionary.ContainsKey(m))
					{
						Dictionary<int, int> dictionary2;
						Dictionary<int, int> dictionary3 = (dictionary2 = dictionary);
						int key;
						int key2 = (key = m);
						key = dictionary2[key];
						dictionary3[key2] = key + 1;
					}
					else
					{
						dictionary[m] = 1;
					}
					break;
				}
				num4 += (float)mGenEnemyP[m];
			}
		}
		foreach (int key3 in dictionary.Keys)
		{
			BattleUnit battleUnit = UnityEngine.Object.Instantiate(mPerfab);
			battleUnit.transform.parent = base.transform;
			battleUnit.gameObject.SetActive(value: false);
			battleUnit.SetData(BattleUnitData.GetBattleUnitData(mGenEnemyID[key3]));
			mBorderlineArea[index].AddBattleUnit(battleUnit);
		}
	}

	private void UpdatePlayerCtrlAreaState()
	{
	}

	private void UpdateAreaUnit()
	{
		for (int i = 0; i < mAreaNum_OneSide; i++)
		{
			for (int j = 0; j < mAreaNum_OneSide; j++)
			{
				mAreaUnitList[i, j].Update();
			}
		}
	}

	private void UpdateAreaForce()
	{
		for (int i = 0; i < mAreaNum_OneSide; i++)
		{
			for (int j = 0; j < mAreaNum_OneSide; j++)
			{
				mAreaUnitList[i, j].mDefenderForce = 0f;
			}
		}
		for (int k = 0; k < mAreaNum_OneSide; k++)
		{
			for (int l = 0; l < mAreaNum_OneSide; l++)
			{
				float num = mAreaUnitList[k, l].UsableDefenderBE();
				if (!(num > 0f))
				{
					continue;
				}
				for (int m = 0; m < mAreaNum_OneSide; m++)
				{
					for (int n = 0; n < mAreaNum_OneSide; n++)
					{
						mAreaUnitList[m, n].mDefenderForce += num * (float)(mLongestLength - Mathf.Abs(m - k) - Mathf.Abs(n - l)) / (float)mLongestLength;
					}
				}
			}
		}
		if (!Application.isEditor)
		{
			return;
		}
		for (int num2 = 0; num2 < mAreaNum_OneSide; num2++)
		{
			for (int num3 = 0; num3 < mAreaNum_OneSide; num3++)
			{
				mAreaCubeList[num2, num3].Update();
			}
		}
	}

	private void ClearDeathEnemy()
	{
		for (int num = mCurrentShowMode.Count - 1; num >= 0; num--)
		{
			if (mCurrentShowMode[num] == null)
			{
				mCurrentShowMode.RemoveAt(num);
			}
		}
	}

	public void OnBattleFail()
	{
		mActive = false;
	}

	public void AddTower(BattleUnit bU)
	{
		for (int i = 0; i < mAreaNum_OneSide; i++)
		{
			for (int j = 0; j < mAreaNum_OneSide; j++)
			{
				if (mAreaUnitList[i, j].IsInArea(bU.transform.position))
				{
					mAreaUnitList[i, j].AddBattleUnit(bU);
					return;
				}
			}
		}
	}
}
