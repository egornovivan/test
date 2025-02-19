using System.Collections.Generic;

namespace ItemAsset;

public class NewFlagMgr
{
	private class NewItemIndex
	{
		public float time;

		public int index;
	}

	private static readonly float NewFlagTime = 300f;

	private List<NewItemIndex> mNewItemIndexList = new List<NewItemIndex>(10);

	public bool IsNew(int index)
	{
		return null != mNewItemIndexList.Find((NewItemIndex item) => (item.index == index) ? true : false);
	}

	public void Add(int pkgIndex)
	{
		mNewItemIndexList.Add(new NewItemIndex
		{
			index = pkgIndex,
			time = NewFlagTime
		});
	}

	public void Remove(int index)
	{
		mNewItemIndexList.RemoveAll((NewItemIndex item) => (item.index == index) ? true : false);
	}

	public void RemoveAll()
	{
		mNewItemIndexList.Clear();
	}

	public void Update(float deltaTime)
	{
		NewItemIndex newItemIndex = null;
		for (int num = mNewItemIndexList.Count - 1; num >= 0; num--)
		{
			newItemIndex = mNewItemIndexList[num];
			newItemIndex.time -= deltaTime;
			if (newItemIndex.time < float.Epsilon)
			{
				mNewItemIndexList.RemoveAt(num);
				break;
			}
		}
	}
}
