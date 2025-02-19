using System;

namespace WhiteCat;

public class State
{
	public Action onEnter;

	public Action onExit;

	public Action<float> onUpdate;
}
