namespace DunGen;

public enum GenerationStatus
{
	NotStarted,
	PreProcessing,
	TileInjection,
	MainPath,
	Branching,
	PostProcessing,
	Complete,
	Failed
}
