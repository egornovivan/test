using System.Collections.Generic;
using System.IO;
using ItemAsset;
using PETools;

namespace Pathea;

public abstract class PackageCmpt : PeCmpt, EquipmentCmpt.Receiver
{
	private Money mMoney;

	public Money money
	{
		get
		{
			if (mMoney == null)
			{
				mMoney = new Money(this);
			}
			return mMoney;
		}
	}

	bool EquipmentCmpt.Receiver.CanAddItemList(List<ItemObject> items)
	{
		return CanAddItemList(items);
	}

	void EquipmentCmpt.Receiver.AddItemList(List<ItemObject> items)
	{
		AdditemFromEquip(items);
	}

	public override void Start()
	{
		base.Start();
		EquipmentCmpt cmpt = base.Entity.GetCmpt<EquipmentCmpt>();
		cmpt.mItemReciver = this;
	}

	public abstract bool Add(ItemObject item, bool isNew = false);

	public abstract bool Remove(ItemObject item);

	public abstract bool Contain(ItemObject item);

	public abstract bool CanAddItemList(List<ItemObject> items);

	public abstract bool AddItemList(List<ItemObject> items);

	public virtual bool AdditemFromEquip(List<ItemObject> items)
	{
		return true;
	}

	public abstract int GetItemCount(int protoId);

	public abstract bool ContainsItem(int protoId);

	public abstract int GetCountByEditorType(int editorType);

	public abstract int GetAllItemsCount();

	public abstract bool Destory(int protoId, int count);

	public abstract bool DestroyItem(int instanceId, int count);

	public abstract bool DestroyItem(ItemObject item, int count);

	public abstract bool Add(int protoId, int count);

	public abstract bool Set(int protoId, int count);

	public override void Deserialize(BinaryReader r)
	{
		base.Deserialize(r);
		byte[] data = PETools.Serialize.ReadBytes(r);
		money.Import(data);
	}

	public override void Serialize(BinaryWriter w)
	{
		base.Serialize(w);
		PETools.Serialize.WriteBytes(money.Export(), w);
	}
}
