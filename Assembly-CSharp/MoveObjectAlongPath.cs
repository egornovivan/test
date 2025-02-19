using UnityEngine;
using WhiteCat;

[RequireComponent(typeof(PathDriver))]
public class MoveObjectAlongPath : MonoBehaviour
{
	public float speed = 10f;

	private PathDriver driver;

	private void Awake()
	{
		driver = GetComponent<PathDriver>();
	}

	private void Update()
	{
		driver.location += speed * Time.deltaTime;
	}
}
