using System.Collections.Generic;
using Pathea;
using UnityEngine;

namespace SkillSystem;

public class SkEntityAttribsUpdater : MonoLikeSingleton<SkEntityAttribsUpdater>
{
	private const int UpdateFrameInterval = 8;

	private int _prevFrmCnt;

	private List<SkEntity> _reqEntities = new List<SkEntity>();

	public override void Update()
	{
		if (Time.frameCount < _prevFrmCnt + 8)
		{
			return;
		}
		int count = _reqEntities.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			if (_reqEntities[num] == null || _reqEntities[num].Equals(null))
			{
				_reqEntities.RemoveAt(num);
			}
			else
			{
				_reqEntities[num].UpdateAttribs();
			}
		}
		_prevFrmCnt = Time.frameCount;
	}

	public void Register(SkEntity entity)
	{
		_reqEntities.Add(entity);
	}

	public void Unregister(SkEntity entity)
	{
		_reqEntities.Remove(entity);
	}
}
