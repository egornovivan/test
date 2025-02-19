using Pathea;
using UnityEngine;

public class RandomNpcCreator : MonoBehaviour
{
	[SerializeField]
	private int m_randomTemplateId = 1;

	private void Start()
	{
		int id = PeSingleton<WorldInfoMgr>.Instance.FetchNonRecordAutoId();
		Create(id, m_randomTemplateId);
		Object.Destroy(base.gameObject);
	}

	private void Create(int id, int randomTemplateId)
	{
		PeSingleton<PeEntityCreator>.Instance.CreateRandomNpc(randomTemplateId, id, base.transform.position, Quaternion.identity, Vector3.one);
	}
}
