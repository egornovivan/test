using Pathfinding.RVO.Sampled;
using UnityEngine;

namespace Pathfinding.RVO;

[AddComponentMenu("Pathfinding/Local Avoidance/RVO Simulator")]
public class RVOSimulator : MonoBehaviour
{
	[Tooltip("Calculate local avoidance in between frames.\nThis can increase jitter in the agents' movement so use it only if you really need the performance boost. It will also reduce the responsiveness of the agents to the commands you send to them.")]
	public bool doubleBuffering;

	[Tooltip("Interpolate positions between simulation timesteps")]
	public bool interpolation = true;

	[Tooltip("Desired FPS for rvo simulation. It is usually not necessary to run a crowd simulation at a very high fps.\nUsually 10-30 fps is enough, but can be increased for better quality.\nThe rvo simulation will never run at a higher fps than the game")]
	public int desiredSimulationFPS = 20;

	[Tooltip("Number of RVO worker threads. If set to None, no multithreading will be used.")]
	public ThreadCount workerThreads = ThreadCount.Two;

	[Tooltip("[GradientDecent][unitless][0...1] A higher value will result in lower quality local avoidance but faster calculations.")]
	public float qualityCutoff = 0.05f;

	[Tooltip("[GradientDecent][unitless][0...2] How large steps to take when searching for a minimum to the penalty function. Larger values will make it faster, but less accurate, too low values (near 0) can also give large inaccuracies. Values around 0.5-1.5 work the best.")]
	public float stepScale = 1.5f;

	[Tooltip("[0...infinity] Higher values will raise the penalty for agent-agent intersection")]
	public float incompressibility = 30f;

	[Tooltip("Thickness of RVO obstacle walls.\nIf obstacles are passing through obstacles, try a larger value, if they are getting stuck near small obstacles, try reducing it")]
	public float wallThickness = 1f;

	[Tooltip("[unitless][0...infinity] How much an agent will try to reach the desired velocity. A higher value will yield a more aggressive behaviour")]
	public float desiredVelocityWeight = 0.1f;

	[Tooltip("What sampling algorithm to use. GradientDecent is a bit more agressive but makes it easier for agents to intersect.")]
	public Simulator.SamplingAlgorithm algorithm = Simulator.SamplingAlgorithm.GradientDecent;

	[Tooltip("Run multiple simulation steps per step. Much slower, but may lead to slightly higher quality local avoidance.")]
	public bool oversampling;

	private Simulator simulator;

	public Simulator GetSimulator()
	{
		if (simulator == null)
		{
			Awake();
		}
		return simulator;
	}

	private void Awake()
	{
		if (desiredSimulationFPS < 1)
		{
			desiredSimulationFPS = 1;
		}
		if (simulator == null)
		{
			int workers = AstarPath.CalculateThreadCount(workerThreads);
			simulator = new Simulator(workers, doubleBuffering);
			simulator.Interpolation = interpolation;
			simulator.DesiredDeltaTime = 1f / (float)desiredSimulationFPS;
		}
	}

	private void Update()
	{
		if (desiredSimulationFPS < 1)
		{
			desiredSimulationFPS = 1;
		}
		Agent.DesiredVelocityWeight = desiredVelocityWeight;
		Agent.GlobalIncompressibility = incompressibility;
		Simulator simulator = GetSimulator();
		simulator.DesiredDeltaTime = 1f / (float)desiredSimulationFPS;
		simulator.Interpolation = interpolation;
		simulator.stepScale = stepScale;
		simulator.qualityCutoff = qualityCutoff;
		simulator.algorithm = algorithm;
		simulator.Oversampling = oversampling;
		simulator.WallThickness = wallThickness;
		simulator.Update();
	}

	private void OnDestroy()
	{
		if (simulator != null)
		{
			simulator.OnDestroy();
		}
	}
}
