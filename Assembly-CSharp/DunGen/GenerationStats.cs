using System.Diagnostics;

namespace DunGen;

public sealed class GenerationStats
{
	private Stopwatch stopwatch = new Stopwatch();

	private GenerationStatus generationStatus;

	public int MainPathRoomCount { get; private set; }

	public int BranchPathRoomCount { get; private set; }

	public int TotalRoomCount { get; private set; }

	public int MaxBranchDepth { get; private set; }

	public int TotalRetries { get; private set; }

	public float PreProcessTime { get; private set; }

	public float MainPathGenerationTime { get; private set; }

	public float BranchPathGenerationTime { get; private set; }

	public float PostProcessTime { get; private set; }

	public float TotalTime { get; private set; }

	internal void Clear()
	{
		int num2 = (TotalRetries = 0);
		num2 = (MaxBranchDepth = num2);
		num2 = (TotalRoomCount = num2);
		num2 = (BranchPathRoomCount = num2);
		MainPathRoomCount = num2;
		float num7 = (TotalTime = 0f);
		num7 = (PostProcessTime = num7);
		num7 = (BranchPathGenerationTime = num7);
		num7 = (MainPathGenerationTime = num7);
		PreProcessTime = num7;
	}

	internal void IncrementRetryCount()
	{
		TotalRetries++;
	}

	internal void SetRoomStatistics(int mainPathRoomCount, int branchPathRoomCount, int maxBranchDepth)
	{
		MainPathRoomCount = mainPathRoomCount;
		BranchPathRoomCount = branchPathRoomCount;
		MaxBranchDepth = maxBranchDepth;
		TotalRoomCount = MainPathRoomCount + BranchPathRoomCount;
	}

	internal void BeginTime(GenerationStatus status)
	{
		if (stopwatch.IsRunning)
		{
			EndTime();
		}
		generationStatus = status;
		stopwatch.Reset();
		stopwatch.Start();
	}

	internal void EndTime()
	{
		stopwatch.Stop();
		float num = (float)stopwatch.Elapsed.TotalMilliseconds;
		switch (generationStatus)
		{
		case GenerationStatus.PreProcessing:
			PreProcessTime += num;
			break;
		case GenerationStatus.MainPath:
			MainPathGenerationTime += num;
			break;
		case GenerationStatus.Branching:
			BranchPathGenerationTime += num;
			break;
		case GenerationStatus.PostProcessing:
			PostProcessTime += num;
			break;
		}
		TotalTime = PreProcessTime + MainPathGenerationTime + BranchPathGenerationTime + PostProcessTime;
	}

	public GenerationStats Clone()
	{
		GenerationStats generationStats = new GenerationStats();
		generationStats.MainPathRoomCount = MainPathRoomCount;
		generationStats.BranchPathRoomCount = BranchPathRoomCount;
		generationStats.TotalRoomCount = TotalRoomCount;
		generationStats.MaxBranchDepth = MaxBranchDepth;
		generationStats.TotalRetries = TotalRetries;
		generationStats.PreProcessTime = PreProcessTime;
		generationStats.MainPathGenerationTime = MainPathGenerationTime;
		generationStats.BranchPathGenerationTime = BranchPathGenerationTime;
		generationStats.PostProcessTime = PostProcessTime;
		generationStats.TotalTime = TotalTime;
		return generationStats;
	}
}
