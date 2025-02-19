using System;
using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Mono.Data.SqliteClient;
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

		public class Mgr
		{
			private static Mgr instance;

			private List<Formula> mList = new List<Formula>(50);

			public static Mgr Instance
			{
				get
				{
					if (instance == null)
					{
						instance = new Mgr();
						instance.LoadData();
					}
					return instance;
				}
			}

			private Mgr()
			{
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

			public string GetIconName(int id)
			{
				Formula formula = Find(id);
				if (formula == null)
				{
					return null;
				}
				return ItemProto.GetIconName(formula.productItemId);
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
		public int id;

		public bool flag;

		public Formula Get()
		{
			return Formula.Mgr.Instance.Find(id);
		}
	}

	private IHandler mHandler;

	private List<KnownFormula> mForumlaList = new List<KnownFormula>(5);

	public IEnumerable<KnownFormula> knowFormulas => mForumlaList;

	public Replicator(IHandler handler)
	{
		mHandler = handler;
	}

	public void AddFormula(int formulaId)
	{
		mForumlaList.Add(new KnownFormula
		{
			id = formulaId,
			flag = true
		});
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
		if (mHandler == null)
		{
			return 0;
		}
		Formula formula = Formula.Mgr.Instance.Find(formulaId);
		if (formula == null)
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
		Formula formula = Formula.Mgr.Instance.Find(formulaId);
		if (formula == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("no replicate formula with id:" + formulaId);
			}
			return false;
		}
		if (mHandler == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("no handler to handle item");
			}
			return false;
		}
		if (MaxProductCount(formulaId) < count)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("not engugh material");
			}
			return false;
		}
		if (!DeleteMaterial(formula, count))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("delete material failed");
			}
			return false;
		}
		if (!mHandler.CreateItem(formula.productItemId, formula.m_productItemCount * count))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("create item :" + formula.productItemId + ", count:" + formula.m_productItemCount * count + "failed:");
			}
			return false;
		}
		return true;
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

	public void Serialize(BinaryWriter w)
	{
		w.Write(mForumlaList.Count);
		foreach (KnownFormula mForumla in mForumlaList)
		{
			w.Write(mForumla.id);
			w.Write(mForumla.flag);
		}
	}

	public void Deserialize(BinaryReader r)
	{
		int num = r.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			mForumlaList.Add(new KnownFormula
			{
				id = r.ReadInt32(),
				flag = r.ReadBoolean()
			});
		}
	}
}
