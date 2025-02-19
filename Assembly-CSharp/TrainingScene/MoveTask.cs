using UnityEngine;

namespace TrainingScene;

public class MoveTask : MonoBehaviour
{
	private static MoveTask s_instance;

	[SerializeField]
	private MoveTaskPoint[] points;

	public RectTransform targetSign;

	public MoveTaskScreen screenBorder;

	private int currentPoint;

	public static MoveTask Instance => s_instance;

	private void Awake()
	{
		s_instance = this;
	}

	public void InitScene()
	{
		targetSign.gameObject.SetActive(value: true);
		points[0].gameObject.SetActive(value: true);
		points[0].InitStr();
	}

	public void DestroyScene()
	{
		currentPoint = 0;
		targetSign.gameObject.SetActive(value: false);
		MoveTaskPoint[] array = points;
		foreach (MoveTaskPoint moveTaskPoint in array)
		{
			moveTaskPoint.gameObject.SetActive(value: false);
		}
	}

	private void CloseMission()
	{
	}

	public void NextPoint()
	{
		points[currentPoint].gameObject.SetActive(value: false);
		if (++currentPoint != points.Length)
		{
			points[currentPoint].gameObject.SetActive(value: true);
			points[currentPoint].InitStr();
		}
		else
		{
			DestroyScene();
		}
	}
}
