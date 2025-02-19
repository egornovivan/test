using UnityEngine;

namespace TrainingScene;

public class HoloherbTask : MonoBehaviour
{
	private static HoloherbTask s_instance;

	[SerializeField]
	private HoloherbGather[] gather;

	[SerializeField]
	private HoloherbAppearance[] appearance;

	[SerializeField]
	private GameObject herbsBase;

	[SerializeField]
	private int herbCount;

	private int herbDead;

	public static HoloherbTask Instance => s_instance;

	private void Awake()
	{
		s_instance = this;
	}

	public void InitScene()
	{
		appearance[0].transform.parent.gameObject.SetActive(value: true);
		herbDead = 0;
		HoloherbGather[] array = gather;
		foreach (HoloherbGather holoherbGather in array)
		{
			holoherbGather.isDead = false;
		}
		HoloCameraControl.Instance.renderObjs1.Add(herbsBase);
		HoloherbAppearance[] array2 = appearance;
		foreach (HoloherbAppearance holoherbAppearance in array2)
		{
			holoherbAppearance.produce = true;
		}
	}

	public void DestroyScene()
	{
		Invoke("CloseMission", appearance[0].fadeTime + 0.2f);
	}

	private void CloseMission()
	{
		HoloCameraControl.Instance.renderObjs1.Clear();
		appearance[0].transform.parent.gameObject.SetActive(value: false);
	}

	public void SubHerb(HoloherbAppearance ha)
	{
		ha.destroy = true;
		Object.Destroy(ha.col);
		if (++herbDead == herbCount)
		{
			TrainingTaskManager.Instance.CompleteMission();
			DestroyScene();
		}
	}
}
