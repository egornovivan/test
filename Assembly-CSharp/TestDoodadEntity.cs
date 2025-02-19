using Pathea;
using UnityEngine;

public class TestDoodadEntity : MonoBehaviour
{
	[SerializeField]
	private int id = 111111111;

	[SerializeField]
	private int protoId = 55;

	[SerializeField]
	private bool create;

	private void Update()
	{
		if (create)
		{
			create = false;
			PeEntity peEntity = PeSingleton<PeEntityCreator>.Instance.CreateDoodad(id, protoId, base.transform.position, base.transform.rotation, base.transform.localScale);
		}
	}
}
