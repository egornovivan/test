using System;
using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Mono.Data.SqliteClient;
using PeEvent;
using UnityEngine;

namespace Pathea;

public class Replicator
{
	public class Formula
	{
		public class Material
		{
			public int itemId;

			public int itemCount;
		}

		public class Mgr : MonoLikeSingleton<Mgr>
		{
			private List<Formula> mList = new List<Formula>(50);

			protected override void OnInit()
			{
				base.OnInit();
				LoadData();
			}

			private void LoadData()
			{
				SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("synthesis_skill");
				while (sqliteDataReader.Read())
				{
					Formula formula = new Formula();
					formula.id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
					formula.productItemId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("object_ID")));
					formula.m_productItemCount = Convert.ToInt16(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("num_each_product")));
					formula.timeNeed = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("need_time")));
					int ordinal = sqliteDataReader.GetOrdinal("material1");
					formula.materials = new List<Material>();
					for (int i = 0; i < 6; i++)
					{
						int num = Convert.ToInt32(sqliteDataReader.GetString(ordinal + 1 + i * 2));
						if (num <= 0)
						{
							break;
						}
						formula.materials.Add(new Material
						{
							itemId = Convert.ToInt32(sqliteDataReader.GetString(ordinal + i * 2)),
							itemCount = num
						});
					}
					formula.workSpace = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("workspace")));
					mList.Add(formula);
				}
			}

			private bool MatchId(Formula iter, int id)
			{
				return iter.id == id;
			}

			public Formula FindByProductId(int productId)
			{
				return mList.Find((Formula hh) => hh.productItemId == productId);
			}

			public List<Formula> FindAllByProDuctID(int productId)
			{
				return mList.FindAll((Formula item) => item.productItemId == productId);
			}

			public Formula Find(int id)
			{
				return mList.Find((Formula iterSkill1) => MatchId(iterSkill1, id));
			}

			public string GetName(int id)
			{
				Formula formula = Find(id);
				if (formula == null)
				{
					return string.Empty;
				}
				return ItemProto.GetName(formula.productItemId);
			}
		}

		public int id;

		public int productItemId;

		public float timeNeed;

		public short m_productItemCount;

		public List<Material> materials;

		public int workSpace;

		private Formula()
		{
		}
	}

	public interface IHandler
	{
		int ItemCount(int itemId);

		bool DeleteItem(int itemId, int count);

		bool CreateItem(int itemId, int count);

		bool HasEnoughPackage(int itemId, int count);
	}

	public class KnownFormula
	{
		public bool flag;

		private int _id;

		private Formula _formula;

		public int id => _id;

		public KnownFormula(int formulaId, bool formulaFlag)
		{
			_id = formulaId;
			flag = formulaFlag;
			_formula = PeSingleton<Formula.Mgr>.Instance.Find(_id);
		}

		public Formula Get()
		{
			return _formula;
		}
	}

	public class RunningReplicate
	{
		public int formulaID;

		public int requestCount;

		public int leftCount;

		public int finishCount;

		public float runningTime;

		public float lastReciveTime;

		private Formula m_Formula;

		public Formula formula
		{
			get
			{
				if (m_Formula == null)
				{
					m_Formula = PeSingleton<Formula.Mgr>.Instance.Find(formulaID);
				}
				return m_Formula;
			}
		}
	}

	public class EventArg : PeEvent.EventArg
	{
		public int formulaId;
	}

	private const float ReciveInterval = 5f;

	public float needTimeScale = 1f;

	private IHandler mHandler;

	private List<KnownFormula> mForumlaList = new List<KnownFormula>(5);

	private RunningReplicate mRunningReplicate;

	public Action onReplicateEnd;

	private Event<EventArg> mEventor = new Event<EventArg>();

	public RunningReplicate runningReplicate => mRunningReplicate;

	public Event<EventArg> eventor => mEventor;

	public IEnumerable<KnownFormula> knowFormulas => mForumlaList;

	public int knowFormulaCount => mForumlaList.Count;

	public Replicator(IHandler handler)
	{
		mHandler = handler;
	}

	public bool AddFormula(int formulaId)
	{
		KnownFormula knownFormula = GetKnownFormula(formulaId);
		if (knownFormula != null)
		{
			return false;
		}
		mForumlaList.Add(new KnownFormula(formulaId, formulaFlag: true));
		eventor.Dispatch(new EventArg
		{
			formulaId = formulaId
		});
		return true;
	}

	public bool HasEnoughPackage(int itemId, int count)
	{
		return mHandler.HasEnoughPackage(itemId, count);
	}

	public int GetItemCount(int itemId)
	{
		if (mHandler == null)
		{
			return 0;
		}
		return mHandler.ItemCount(itemId);
	}

	public int MaxProductCount(int formulaId)
	{
		Formula formula = PeSingleton<Formula.Mgr>.Instance.Find(formulaId);
		return MaxProductCount(formula);
	}

	private int MaxProductCount(Formula formula)
	{
		if (mHandler == null || formula == null)
		{
			return 0;
		}
		int num = 9999;
		foreach (Formula.Material material in formula.materials)
		{
			int num2 = mHandler.ItemCount(material.itemId) / material.itemCount;
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}

	public int MinProductCount(int formulaId)
	{
		if (MaxProductCount(formulaId) <= 0)
		{
			return 0;
		}
		return 1;
	}

	private bool DeleteMaterial(Formula formula, int count)
	{
		foreach (Formula.Material material in formula.materials)
		{
			if (!mHandler.DeleteItem(material.itemId, material.itemCount * count))
			{
				return false;
			}
		}
		return true;
	}

	public bool Run(int formulaId, int count)
	{
		Formula formula = PeSingleton<Formula.Mgr>.Instance.Find(formulaId);
		if (formula == null)
		{
			Debug.LogError("no replicate formula with id:" + formulaId);
			return false;
		}
		if (mHandler == null)
		{
			Debug.LogError("no handler to handle item");
			return false;
		}
		if (MaxProductCount(formulaId) < count)
		{
			Debug.LogError("not engugh material");
			return false;
		}
		if (!DeleteMaterial(formula, count))
		{
			Debug.LogError("delete material failed");
			return false;
		}
		if (!mHandler.CreateItem(formula.productItemId, formula.m_productItemCount * count))
		{
			Debug.LogError("create item :" + formula.productItemId + ", count:" + formula.m_productItemCount * count + "failed:");
			return false;
		}
		return true;
	}

	public int[] GetProductItemIds()
	{
		List<int> list = new List<int>(10);
		foreach (KnownFormula knowFormula in knowFormulas)
		{
			Formula formula = knowFormula.Get();
			if (!list.Contains(formula.productItemId))
			{
				list.Add(formula.productItemId);
			}
		}
		return list.ToArray();
	}

	public KnownFormula[] GetKnowFormulasByProductItemId(int productItemId)
	{
		List<KnownFormula> list = new List<KnownFormula>(1);
		foreach (KnownFormula knowFormula in knowFormulas)
		{
			Formula formula = knowFormula.Get();
			if (formula != null && formula.productItemId == productItemId)
			{
				list.Add(knowFormula);
			}
		}
		return list.ToArray();
	}

	public int[] GetProductItems()
	{
		List<int> list = new List<int>(1);
		foreach (KnownFormula knowFormula in knowFormulas)
		{
			Formula formula = knowFormula.Get();
			if (!list.Contains(formula.productItemId))
			{
				list.Add(formula.productItemId);
			}
		}
		return list.ToArray();
	}

	public KnownFormula GetKnownFormula(int id)
	{
		return mForumlaList.Find((KnownFormula kf) => (kf.id == id) ? true : false);
	}

	public void SetKnownFormulaFlag(int id)
	{
		KnownFormula knownFormula = GetKnownFormula(id);
		if (knownFormula != null)
		{
			knownFormula.flag = false;
		}
	}

	public bool StartReplicate(int formulaID, int count)
	{
		if (mRunningReplicate != null)
		{
			return false;
		}
		mRunningReplicate = new RunningReplicate();
		mRunningReplicate.formulaID = formulaID;
		mRunningReplicate.requestCount = count;
		mRunningReplicate.leftCount = count;
		mRunningReplicate.lastReciveTime = Time.time;
		return true;
	}

	public bool CancelReplicate(int formulaID)
	{
		if (mRunningReplicate.finishCount > 0)
		{
			if (!HasEnoughPackage(mRunningReplicate.formula.productItemId, mRunningReplicate.formula.m_productItemCount * (mRunningReplicate.finishCount + 1)))
			{
				return false;
			}
			if (GameConfig.IsMultiMode && MaxProductCount(mRunningReplicate.formula) >= mRunningReplicate.finishCount)
			{
				PlayerNetwork.mainPlayer.RequestMergeSkill(mRunningReplicate.formulaID, mRunningReplicate.finishCount);
			}
			else
			{
				mHandler.CreateItem(mRunningReplicate.formula.productItemId, mRunningReplicate.formula.m_productItemCount * mRunningReplicate.finishCount);
			}
		}
		mRunningReplicate.finishCount = 0;
		mRunningReplicate.lastReciveTime = Time.time;
		mRunningReplicate = null;
		if (onReplicateEnd != null)
		{
			onReplicateEnd();
		}
		return true;
	}

	public void UpdateReplicate()
	{
		if (mRunningReplicate == null)
		{
			return;
		}
		mRunningReplicate.runningTime += Time.deltaTime;
		if (!(mRunningReplicate.runningTime >= mRunningReplicate.formula.timeNeed * needTimeScale))
		{
			return;
		}
		if (!HasEnoughPackage(mRunningReplicate.formula.productItemId, mRunningReplicate.formula.m_productItemCount * (mRunningReplicate.finishCount + 1)))
		{
			mRunningReplicate.runningTime = 0f;
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000050));
			return;
		}
		int num = MaxProductCount(mRunningReplicate.formula);
		if ((!GameConfig.IsMultiMode) ? (num > 0) : (num > mRunningReplicate.finishCount))
		{
			mRunningReplicate.runningTime -= mRunningReplicate.formula.timeNeed;
			mRunningReplicate.leftCount--;
			mRunningReplicate.finishCount++;
			if (!GameConfig.IsMultiMode)
			{
				DeleteMaterial(mRunningReplicate.formula, 1);
			}
			if (mRunningReplicate.leftCount == 0 || Time.time - mRunningReplicate.lastReciveTime > 5f)
			{
				if (GameConfig.IsMultiMode)
				{
					PlayerNetwork.mainPlayer.RequestMergeSkill(mRunningReplicate.formulaID, mRunningReplicate.finishCount);
				}
				else
				{
					mHandler.CreateItem(mRunningReplicate.formula.productItemId, mRunningReplicate.formula.m_productItemCount * mRunningReplicate.finishCount);
				}
				mRunningReplicate.finishCount = 0;
				mRunningReplicate.lastReciveTime = Time.time;
			}
			if (mRunningReplicate.leftCount == 0)
			{
				mRunningReplicate = null;
				if (onReplicateEnd != null)
				{
					onReplicateEnd();
				}
			}
		}
		else
		{
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000689));
			if (!GameConfig.IsMultiMode)
			{
				mHandler.CreateItem(mRunningReplicate.formula.productItemId, mRunningReplicate.formula.m_productItemCount * mRunningReplicate.finishCount);
			}
			mRunningReplicate = null;
			if (onReplicateEnd != null)
			{
				onReplicateEnd();
			}
		}
	}

	public void Serialize(BinaryWriter w)
	{
		w.Write(-1);
		w.Write(mForumlaList.Count);
		foreach (KnownFormula mForumla in mForumlaList)
		{
			w.Write(mForumla.id);
			w.Write(mForumla.flag);
		}
		if (mRunningReplicate != null)
		{
			w.Write(value: true);
			w.Write(mRunningReplicate.formulaID);
			w.Write(mRunningReplicate.requestCount);
			w.Write(mRunningReplicate.leftCount);
			w.Write(mRunningReplicate.finishCount);
			w.Write(mRunningReplicate.runningTime);
		}
		else
		{
			w.Write(value: false);
		}
	}

	public void Deserialize(BinaryReader r)
	{
		int num = r.ReadInt32();
		int num2 = num;
		if (num <= -1)
		{
			num2 = r.ReadInt32();
		}
		for (int i = 0; i < num2; i++)
		{
			int formulaId = r.ReadInt32();
			bool formulaFlag = r.ReadBoolean();
			mForumlaList.Add(new KnownFormula(formulaId, formulaFlag));
		}
		if (num <= -1 && r.ReadBoolean())
		{
			mRunningReplicate = new RunningReplicate();
			mRunningReplicate.formulaID = r.ReadInt32();
			mRunningReplicate.requestCount = r.ReadInt32();
			mRunningReplicate.leftCount = r.ReadInt32();
			mRunningReplicate.finishCount = r.ReadInt32();
			mRunningReplicate.runningTime = r.ReadSingle();
		}
	}
}
