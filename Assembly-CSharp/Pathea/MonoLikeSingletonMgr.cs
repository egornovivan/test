using System.Collections.Generic;

namespace Pathea;

public class MonoLikeSingletonMgr : PeSingleton<MonoLikeSingletonMgr>
{
	public interface IMonoLike
	{
		void Update();

		void OnDestroy();
	}

	private List<IMonoLike> mList = new List<IMonoLike>(40);

	public void Update()
	{
		int count = mList.Count;
		for (int i = 0; i < count; i++)
		{
			mList[i].Update();
		}
	}

	public void OnDestroy()
	{
		Clear();
	}

	public void Clear()
	{
		int count = mList.Count;
		for (int i = 0; i < count; i++)
		{
			mList[i].OnDestroy();
		}
		mList.Clear();
	}

	public void Register(IMonoLike m)
	{
		mList.Add(m);
	}
}
