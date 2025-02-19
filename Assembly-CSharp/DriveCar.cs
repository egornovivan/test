using System.Collections.Generic;
using PETools;
using UnityEngine;
using UnityEngine.UI;
using WhiteCat;
using WhiteCat.BitwiseOperationExtension;

public class DriveCar : MonoBehaviour
{
	private struct Barrier
	{
		public readonly float location;

		public readonly GameObject gameObject;

		public Barrier(float distance, GameObject gameObject)
		{
			location = distance;
			this.gameObject = gameObject;
		}
	}

	public TweenInterpolator playingUIInterpolator;

	public ObjectPool barrierPool;

	public Text scoreText;

	public Text bestScoreText;

	[Space(8f)]
	public float acceleration = 5f;

	public float maxSpeed = 20f;

	[Space(8f)]
	public float slideDuration = 0.25f;

	public float maxSlideAngle = 15f;

	[Space(8f)]
	public Vector3 cameraPosition = new Vector3(0f, 2.5f, -5f);

	public Vector3 cameraForward = new Vector3(0f, -2f, 8f);

	public float cameraRotateDamping = 4f;

	[Space(8f)]
	public float recycleLength = 10f;

	public float generateLength = 100f;

	public float minDistance = 10f;

	public float maxDistance = 40f;

	private Transform mainCamera;

	private Transform carTransform;

	private Transform driverTransform;

	private PathDriver driver;

	private int slideDirection;

	private bool slideTwice;

	private float slideTime;

	private float slideFrom;

	private float slideTo;

	private bool ready;

	private float speed;

	private int score;

	private int bestScore;

	private Vector3 originalCarLocalPosition;

	private Vector3 carLocalPosition;

	private Quaternion originalCarLocalRotation;

	private Quaternion carTargetRotation;

	private List<Barrier> barriers;

	private float nextGenerateDistance;

	private void OnEnable()
	{
		mainCamera = PEUtil.MainCamTransform;
		carTransform = SelectCar.carTransform;
		driverTransform = SelectCar.carDriver.transform;
		driver = SelectCar.carDriver;
		originalCarLocalPosition = carTransform.localPosition;
		originalCarLocalRotation = carTransform.localRotation;
		ready = false;
		speed = 0f;
		score = 0;
		scoreText.text = "0m";
		slideDirection = 0;
		barriers = new List<Barrier>(16);
		nextGenerateDistance = generateLength;
	}

	private void FixedUpdate()
	{
		speed += acceleration * Time.deltaTime;
		if (speed >= maxSpeed)
		{
			speed = maxSpeed;
			if (!ready)
			{
				ready = true;
				SelectCar.HideUnselected();
				playingUIInterpolator.Record();
				playingUIInterpolator.isPlaying = true;
			}
		}
		driver.location += speed * Time.deltaTime;
		int num = (int)driver.location / 10 * 10;
		if (num != score)
		{
			score = num;
			scoreText.text = score + "m";
		}
		SlideCar();
		SetCameraTransform();
		ManageBarriers();
	}

	private void Update()
	{
		if (!ready)
		{
			return;
		}
		if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
		{
			if (slideDirection == 0)
			{
				if (carTransform.localPosition.x > -1f)
				{
					slideDirection = -1;
					slideTwice = false;
					slideTime = 0f;
					slideFrom = carTransform.localPosition.x;
					slideTo = slideFrom - 2f;
				}
			}
			else if (slideDirection == 1)
			{
				slideDirection = -1;
				slideTwice = false;
				slideTime = 1f - slideTime;
				Utility.Swap(ref slideFrom, ref slideTo);
			}
			else if (slideTo > -1f)
			{
				slideTwice = true;
			}
		}
		if (!Input.GetMouseButtonDown(1) && !Input.GetKeyDown(KeyCode.RightArrow) && !Input.GetKeyDown(KeyCode.D))
		{
			return;
		}
		if (slideDirection == 0)
		{
			if (carTransform.localPosition.x < 1f)
			{
				slideDirection = 1;
				slideTwice = false;
				slideTime = 0f;
				slideFrom = carTransform.localPosition.x;
				slideTo = slideFrom + 2f;
			}
		}
		else if (slideDirection == -1)
		{
			slideDirection = 1;
			slideTwice = false;
			slideTime = 1f - slideTime;
			Utility.Swap(ref slideFrom, ref slideTo);
		}
		else if (slideTo < 1f)
		{
			slideTwice = true;
		}
	}

	private void SlideCar()
	{
		if (slideDirection == 0)
		{
			return;
		}
		slideTime += Time.deltaTime / slideDuration;
		if (slideTime >= 1f)
		{
			if (slideTwice)
			{
				slideTwice = false;
				slideTime = 0f;
				slideFrom = slideTo;
				slideTo += slideDirection * 2;
			}
			else
			{
				slideDirection = 0;
				slideTime = 1f;
			}
		}
		carLocalPosition.x = Utility.Interpolate(slideFrom, slideTo, slideTime, Interpolation.EaseInEaseOut);
		carTransform.localPosition = carLocalPosition;
		carTransform.localRotation = Quaternion.Euler(0f, Interpolation.Bell(slideTime) * maxSlideAngle * (float)slideDirection, 0f);
	}

	private void SetCameraTransform()
	{
		if (ready)
		{
			mainCamera.position = driverTransform.TransformPoint(cameraPosition);
		}
		else
		{
			mainCamera.position = Vector3.Lerp(mainCamera.position, driverTransform.TransformPoint(cameraPosition), Mathf.Pow(speed / maxSpeed, 3f));
		}
		mainCamera.rotation = Quaternion.Slerp(mainCamera.rotation, Quaternion.LookRotation(driverTransform.TransformVector(cameraForward), driverTransform.up), speed / maxSpeed * cameraRotateDamping * Time.deltaTime);
	}

	private void ManageBarriers()
	{
		while (barriers.Count > 0 && barriers[0].location + recycleLength < driver.location)
		{
			ObjectPool.Recycle(barriers[0].gameObject);
			barriers.RemoveAt(0);
		}
		if (!(driver.location + generateLength > nextGenerateDistance))
		{
			return;
		}
		int splineIndex = -1;
		float splineTime = 0f;
		driver.path.GetPathPositionAtPathLength(nextGenerateDistance, ref splineIndex, ref splineTime);
		Vector3 splinePoint = driver.path.GetSplinePoint(splineIndex, splineTime);
		Quaternion splineRotation = driver.path.GetSplineRotation(splineIndex, splineTime);
		int value = Random.Range(1, 7);
		for (int i = 0; i < 3; i++)
		{
			if (value.GetBit(i))
			{
				GameObject gameObject = barrierPool.TakeOut();
				gameObject.transform.position = splinePoint + splineRotation * new Vector3((i - 1) * 2, 0f, 0f);
				gameObject.transform.rotation = splineRotation;
				barriers.Add(new Barrier(nextGenerateDistance, gameObject));
			}
		}
		nextGenerateDistance += Random.Range(minDistance, maxDistance);
	}

	private void OnDisable()
	{
		try
		{
			Time.timeScale = 1f;
			for (int i = 0; i < barriers.Count; i++)
			{
				ObjectPool.Recycle(barriers[i].gameObject);
			}
			if (score > bestScore)
			{
				bestScore = score;
				bestScoreText.text = "<color=ffffff>" + "Best Score".ToLocalizationString() + "</color> " + score;
			}
			driver.location = 0f;
			carTransform.localPosition = originalCarLocalPosition;
			carTransform.localRotation = originalCarLocalRotation;
		}
		catch
		{
		}
	}
}
