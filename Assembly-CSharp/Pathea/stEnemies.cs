using System.Collections.Generic;

namespace Pathea;

public class stEnemies
{
	public PeEntity mTarget;

	private List<PeEntity> mChaseLists;

	public int ChasesCout => (mChaseLists != null) ? mChaseLists.Count : 0;

	public stEnemies(PeEntity target)
	{
		mTarget = target;
		mChaseLists = new List<PeEntity>();
	}

	public void AddInChaseLists(PeEntity chase)
	{
		mChaseLists.Add(chase);
	}

	public bool RemoveChaseLists(PeEntity chase)
	{
		return mChaseLists.Remove(chase);
	}

	public bool BeTarget(PeEntity enemey)
	{
		return !(mTarget == null) && mTarget.Equals(enemey);
	}

	public bool InChaseLists(PeEntity entity)
	{
		return mChaseLists != null && mChaseLists.Contains(entity);
	}

	public void ClearChaseLists()
	{
		mChaseLists.Clear();
	}
}
