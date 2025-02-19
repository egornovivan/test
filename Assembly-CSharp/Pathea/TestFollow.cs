using Pathea.PeEntityExtFollow;
using UnityEngine;

namespace Pathea;

public class TestFollow : MonoBehaviour
{
	private PeEntity mEntity;

	private void Start()
	{
		mEntity = GetComponent<PeEntity>();
		Follow();
	}

	private void OnDestroy()
	{
		Defollow();
	}

	private void Follow()
	{
		mEntity.ExtFollow(PeSingleton<PeCreature>.Instance.mainPlayer);
	}

	private void Defollow()
	{
		mEntity.ExtDefollow();
	}
}
