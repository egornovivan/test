using UnityEngine;

namespace Pathfinding.RVO;

[AddComponentMenu("Pathfinding/Local Avoidance/Square Obstacle")]
public class RVOSquareObstacle : RVOObstacle
{
	public float height = 1f;

	public Vector2 size = Vector3.one;

	public Vector2 center = Vector3.one;

	protected override bool StaticObstacle => false;

	protected override bool ExecuteInEditor => true;

	protected override bool LocalCoordinates => true;

	protected override float Height => height;

	protected override bool AreGizmosDirty()
	{
		return false;
	}

	protected override void CreateObstacles()
	{
		size.x = Mathf.Abs(size.x);
		size.y = Mathf.Abs(size.y);
		height = Mathf.Abs(height);
		Vector3[] array = new Vector3[4]
		{
			new Vector3(1f, 0f, -1f),
			new Vector3(1f, 0f, 1f),
			new Vector3(-1f, 0f, 1f),
			new Vector3(-1f, 0f, -1f)
		};
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Scale(new Vector3(size.x * 0.5f, 0f, size.y * 0.5f));
			array[i] += new Vector3(center.x, 0f, center.y);
		}
		AddObstacle(array, height);
	}
}
