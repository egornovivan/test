using UnityEngine;

public interface ISceneObjAgent
{
	int Id { get; set; }

	int ScenarioId { get; set; }

	GameObject Go { get; }

	Vector3 Pos { get; }

	IBoundInScene Bound { get; }

	bool NeedToActivate { get; }

	bool TstYOnActivate { get; }

	void OnConstruct();

	void OnActivate();

	void OnDeactivate();

	void OnDestruct();
}
