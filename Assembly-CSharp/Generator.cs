using System.Text;
using DunGen;
using UnityEngine;

public class Generator : MonoBehaviour
{
	public RuntimeDungeon DungeonGenerator;

	private StringBuilder infoText = new StringBuilder();

	private bool showStats = true;

	private float keypressDelay = 0.1f;

	private float timeSinceLastPress;

	private bool allowHold;

	private bool isKeyDown;

	private void Start()
	{
		DungeonGenerator = GetComponentInChildren<RuntimeDungeon>();
		DungeonGenerator.Generator.OnGenerationStatusChanged += OnGenerationStatusChanged;
		GenerateRandom();
	}

	private void OnGenerationStatusChanged(DungeonGenerator generator, GenerationStatus status)
	{
		if (status == GenerationStatus.Complete)
		{
			infoText.Length = 0;
			infoText.AppendLine("Seed: " + generator.ChosenSeed);
			infoText.AppendLine();
			infoText.Append("## TIME TAKEN ##");
			infoText.AppendFormat("\n\tPre-Processing:\t\t{0:0.00} ms", generator.GenerationStats.PreProcessTime);
			infoText.AppendFormat("\n\tMain Path Generation:\t{0:0.00} ms", generator.GenerationStats.MainPathGenerationTime);
			infoText.AppendFormat("\n\tBranch Path Generation:\t{0:0.00} ms", generator.GenerationStats.BranchPathGenerationTime);
			infoText.AppendFormat("\n\tPost-Processing:\t\t{0:0.00} ms", generator.GenerationStats.PostProcessTime);
			infoText.Append("\n\t-------------------------------------------------------");
			infoText.AppendFormat("\n\tTotal:\t\t\t{0:0.00} ms", generator.GenerationStats.TotalTime);
			infoText.AppendLine();
			infoText.AppendLine();
			infoText.AppendLine("## ROOM COUNT ##");
			infoText.AppendFormat("\n\tMain Path: {0}", generator.GenerationStats.MainPathRoomCount);
			infoText.AppendFormat("\n\tBranch Paths: {0}", generator.GenerationStats.BranchPathRoomCount);
			infoText.Append("\n\t-------------------");
			infoText.AppendFormat("\n\tTotal: {0}", generator.GenerationStats.TotalRoomCount);
			infoText.AppendLine();
			infoText.AppendLine();
			infoText.AppendFormat("Retry Count: {0}", generator.GenerationStats.TotalRetries);
			infoText.AppendLine();
			infoText.AppendLine();
			infoText.AppendLine("Press 'F1' to toggle this information");
			infoText.AppendLine("Press 'R' to generate a new layout");
			infoText.AppendLine("Press 'C' to switch between camera modes");
		}
	}

	public void GenerateRandom()
	{
		DungeonGenerator.Generator.Generate();
	}

	private void Update()
	{
		timeSinceLastPress += Time.deltaTime;
		if (Input.GetKeyDown(KeyCode.R))
		{
			timeSinceLastPress = 0f;
			isKeyDown = true;
			GenerateRandom();
		}
		if (Input.GetKeyUp(KeyCode.R))
		{
			isKeyDown = false;
			allowHold = false;
		}
		if (!allowHold && isKeyDown && timeSinceLastPress >= keypressDelay)
		{
			allowHold = true;
			timeSinceLastPress = 0f;
		}
		if (allowHold && Input.GetKey(KeyCode.R) && timeSinceLastPress >= keypressDelay)
		{
			GenerateRandom();
			timeSinceLastPress = 0f;
		}
		if (Input.GetKeyDown(KeyCode.F1))
		{
			showStats = !showStats;
		}
	}

	private void OnGUI()
	{
		if (showStats)
		{
			GUILayout.Label(infoText.ToString());
		}
	}
}
