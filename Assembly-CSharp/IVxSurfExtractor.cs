public interface IVxSurfExtractor
{
	bool IsIdle { get; }

	bool IsAllClear { get; }

	bool Pause { get; set; }

	void Init();

	void Exec();

	int OnFin();

	void Reset();

	void CleanUp();

	bool AddSurfExtractReq(IVxSurfExtractReq req);
}
