using System.Collections.Generic;

public class InputFoceManager
{
	private static InputFoceManager mInstance;

	private Dictionary<int, List<UIInput>> mInputWnds = new Dictionary<int, List<UIInput>>();

	public static InputFoceManager Instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = new InputFoceManager();
			}
			return mInstance;
		}
	}

	public void AddInput(UIInput addInput)
	{
		if (!mInputWnds.ContainsKey(addInput.mMainGroupID))
		{
			mInputWnds[addInput.mMainGroupID] = new List<UIInput>();
		}
		for (int i = 0; i < mInputWnds[addInput.mMainGroupID].Count - 1; i++)
		{
			if (addInput.mSortScore > mInputWnds[addInput.mMainGroupID][i].mSortScore && addInput.mSortScore < mInputWnds[addInput.mMainGroupID][i + 1].mSortScore)
			{
				mInputWnds[addInput.mMainGroupID].Insert(i + 1, addInput);
				return;
			}
		}
		mInputWnds[addInput.mMainGroupID].Add(addInput);
	}

	public void RemoveInput(UIInput removeInput)
	{
		if (mInputWnds.ContainsKey(removeInput.mMainGroupID))
		{
			mInputWnds[removeInput.mMainGroupID].Remove(removeInput);
		}
	}

	public void MoveNext(UIInput current)
	{
		if (mInputWnds[current.mMainGroupID].Count != 1)
		{
			int num = mInputWnds[current.mMainGroupID].IndexOf(current);
			num = ((num != mInputWnds[current.mMainGroupID].Count - 1) ? (num + 1) : 0);
			UICamera.selectedObject = mInputWnds[current.mMainGroupID][num].gameObject;
		}
	}
}
