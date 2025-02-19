using UnityEngine;

namespace PeMap;

public interface ILabel
{
	int GetIcon();

	Vector3 GetPos();

	string GetText();

	bool FastTravel();

	new ELabelType GetType();

	bool NeedArrow();

	float GetRadius();

	EShow GetShow();

	bool CompareTo(ILabel label);
}
