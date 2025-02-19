using Pathea;
using UnityEngine;
using UnityEngine.UI;

namespace TrainingScene;

public class MoveTaskPoint : MonoBehaviour
{
	private Transform playerTransform;

	private Camera maincmr;

	private Vector3 scrPos;

	private RectTransform targetSign;

	private MoveTaskScreen screenBorder;

	[SerializeField]
	private Transform marker;

	[SerializeField]
	private string title;

	[SerializeField]
	private string titleC;

	[SerializeField]
	private string discription;

	[SerializeField]
	private string discriptionC;

	[SerializeField]
	private float detectDistancePow = 1f;

	private void Start()
	{
		playerTransform = PeSingleton<PeCreature>.Instance.mainPlayer.peTrans.trans;
		maincmr = Camera.main;
		screenBorder = MoveTask.Instance.screenBorder;
	}

	public void InitStr()
	{
		targetSign = MoveTask.Instance.targetSign;
		targetSign.Find("Text").GetComponent<Text>().text = discriptionC;
		targetSign.Find("Title").GetComponent<Text>().text = titleC;
	}

	private void Update()
	{
		scrPos = maincmr.WorldToScreenPoint(marker.position);
		if (0f < scrPos.z)
		{
			targetSign.position = new Vector3(scrPos.x + 100f, scrPos.y + 25f, 0f);
		}
		else
		{
			targetSign.position = Vector3.one * 10000f;
		}
		if ((base.transform.position - playerTransform.position).sqrMagnitude < detectDistancePow)
		{
			screenBorder.gameObject.SetActive(value: true);
			screenBorder.FadeOnScreen();
			MoveTask.Instance.NextPoint();
		}
	}
}
