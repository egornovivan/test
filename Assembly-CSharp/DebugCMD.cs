using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using ItemAsset;
using Pathea;
using Pathea.IO;
using PeCustom;
using PeMap;
using ScenarioRTL;
using ScenarioRTL.IO;
using UnityEngine;

public class DebugCMD : MonoBehaviour
{
	private delegate string DCmdFunc(string param);

	private const string entityTips = "param:<cmd> <id> [args]";

	public GUISkin GSkin;

	public string Tips = "[Ctrl]+[Alt]+[C][M][D] to show cmdline";

	private bool bShowCmdLine;

	private int HotKeyStep;

	private GameObject selectedGameObject;

	private Material selectedMaterial;

	private string cmdstr = string.Empty;

	private string console_text = string.Empty;

	private List<string> history_cmds = new List<string>();

	private string gui_focus_name = string.Empty;

	private bool return_key;

	private Dictionary<string, DCmdFunc> Commands;

	private Dictionary<string, GameObject> m_DisabledGOs;

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(this);
		InitCommands();
		m_DisabledGOs = new Dictionary<string, GameObject>();
	}

	private void Update()
	{
		Tips = "[Ctrl]+[Alt]+[C][M][D] to show cmdline";
		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt))
		{
			if (HotKeyStep == 0 && Input.GetKeyDown(KeyCode.C))
			{
				HotKeyStep = 1;
			}
			else if (HotKeyStep == 1 && Input.anyKeyDown)
			{
				if (Input.GetKeyDown(KeyCode.M))
				{
					HotKeyStep = 2;
				}
				else
				{
					HotKeyStep = 0;
				}
			}
			else if (HotKeyStep == 2 && Input.anyKeyDown)
			{
				if (Input.GetKeyDown(KeyCode.D))
				{
					bShowCmdLine = !bShowCmdLine;
				}
				if (Input.GetKeyDown(KeyCode.L))
				{
					bShowCmdLine = !bShowCmdLine;
				}
				HotKeyStep = 0;
			}
		}
		else
		{
			HotKeyStep = 0;
		}
		if (Application.isEditor && Input.GetKeyDown(KeyCode.BackQuote))
		{
			bShowCmdLine = !bShowCmdLine;
		}
		if (!bShowCmdLine)
		{
			return;
		}
		if (gui_focus_name == "CmdLine")
		{
			if (cmdstr.Contains("\n"))
			{
				cmdstr = cmdstr.Trim();
				cmdstr = cmdstr.Replace("\r\n", string.Empty);
				cmdstr = cmdstr.Replace("\n", string.Empty);
				if (cmdstr.Length > 0)
				{
					ExecuteCommand(cmdstr);
				}
				else
				{
					console_text += ">>\r\n";
				}
				cmdstr = string.Empty;
			}
			float axis = Input.GetAxis("Mouse ScrollWheel");
			if (axis != 0f)
			{
				int num = history_cmds.Count;
				for (int i = 0; i < history_cmds.Count; i++)
				{
					if (history_cmds[i] == cmdstr)
					{
						num = i;
						break;
					}
				}
				if (num - 1 >= 0 && axis > 0f)
				{
					cmdstr = history_cmds[num - 1];
				}
				if (num + 1 < history_cmds.Count && axis < 0f)
				{
					cmdstr = history_cmds[num + 1];
				}
			}
		}
		return_key = Input.GetKeyDown(KeyCode.Return);
	}

	private void OnGUI()
	{
		GUI.skin = GSkin;
		GUI.depth = -2000;
		if (bShowCmdLine)
		{
			GUI.SetNextControlName("CmdLine");
			cmdstr = GUI.TextArea(new Rect(10f, Screen.height - 30, Screen.width - 20, 20f), cmdstr);
			GUI.SetNextControlName("Console");
			GUI.TextArea(new Rect(10f, Screen.height - 234, Screen.width - 20, 199f), console_text.Trim(), "label");
			gui_focus_name = GUI.GetNameOfFocusedControl();
			if (return_key)
			{
				GUI.FocusControl("CmdLine");
			}
		}
	}

	private void ExecuteCommand(string cmd)
	{
		cmd = cmd.Trim();
		console_text = console_text + cmd + "\r\n";
		if (!history_cmds.Contains(cmd))
		{
			history_cmds.Add(cmd);
		}
		foreach (KeyValuePair<string, DCmdFunc> command in Commands)
		{
			string text = "-" + command.Key + " ";
			string text2 = "-" + command.Key;
			int length = text.Length;
			if (cmd.IndexOf(text) == 0)
			{
				string param = cmd.Substring(length, cmd.Length - length).Trim();
				try
				{
					string text3 = command.Value(param);
					if (text3.Trim().Length < 1)
					{
						text3 = "Command has no return.";
					}
					console_text = console_text + ">> " + text3 + "\r\n";
					return;
				}
				catch (Exception ex)
				{
					string text4 = "command error: " + ex.ToString();
					console_text = console_text + ">> " + text4 + "\r\n";
					return;
				}
			}
			if (!(cmd == text2))
			{
				continue;
			}
			try
			{
				string text5 = command.Value(string.Empty);
				if (text5.Trim().Length < 1)
				{
					text5 = "Command has no return.";
				}
				console_text = console_text + ">> " + text5 + "\r\n";
				return;
			}
			catch (Exception ex2)
			{
				string text6 = "command error: " + ex2.ToString();
				console_text = console_text + ">> " + text6 + "\r\n";
				return;
			}
		}
		console_text += ">> Bad command.\r\n";
	}

	private void InitCommands()
	{
		Commands = new Dictionary<string, DCmdFunc>();
		Commands.Add("help", CmdHelp);
		Commands.Add("activego", ActiveGO);
		Commands.Add("deactivego", DeactiveGO);
		Commands.Add("selectgo", SelectGO);
		Commands.Add("selectmat", SelectMaterial);
		Commands.Add("setshaderfile", SetShaderFromFile);
		Commands.Add("setshader", SetShader);
		Commands.Add("matprop", GetMaterialProperty);
		Commands.Add("timespeed", TimeSpeed);
		Commands.Add("passtime", PassTime);
		Commands.Add("repair", Repair);
		Commands.Add("recharge", Recharge);
		Commands.Add("entity", EntityCmd);
		Commands.Add("map", MapCmd);
		Commands.Add("fmodeditor", ToggleFMODEditor);
		Commands.Add("fmodbanklist", FMODBankList);
		Commands.Add("walkspeed", WalkingSpeed);
		Commands.Add("jumpheight", JumpingStrength);
		Commands.Add("nofall", NoFallingDamage);
		Commands.Add("whosyourdaddy", Invincible);
		Commands.Add("whosyourmommy", Uninvincible);
		Commands.Add("buildgod", BuildGod);
		Commands.Add("showbuildgui", ShowBuildGUI);
		Commands.Add("ignoremapcheck", IgnoreMapCheck);
		Commands.Add("customevt", CustomEvent);
		Commands.Add("customcdt", CustomCondition);
		Commands.Add("customact", CustomAction);
		Commands.Add("scenariotool", ToogleScenarioTool);
		Commands.Add("lagtst", LagTest);
		Commands.Add("gopointer", GoPointer);
	}

	private string CmdHelp(string param)
	{
		string text = string.Empty;
		foreach (KeyValuePair<string, DCmdFunc> command in Commands)
		{
			string text2 = "-" + command.Key + " ";
			text += text2;
		}
		return text;
	}

	private string ActiveGO(string param)
	{
		if (param == string.Empty)
		{
			return "Parameter excepted";
		}
		if (m_DisabledGOs.ContainsKey(param))
		{
			GameObject gameObject = m_DisabledGOs[param];
			gameObject.SetActive(value: true);
			return gameObject.name + " has been enabled.";
		}
		return param + " is active or not exist";
	}

	private string DeactiveGO(string param)
	{
		if (param == string.Empty)
		{
			return "Parameter excepted";
		}
		GameObject gameObject = GameObject.Find(param);
		if (gameObject != null)
		{
			gameObject.SetActive(value: false);
			m_DisabledGOs.Add(param, gameObject);
			return gameObject.name + " has been disabled.";
		}
		return "Could not find GameObject " + param;
	}

	private string SelectGO(string param)
	{
		if (param == string.Empty)
		{
			return "Parameter excepted";
		}
		selectedGameObject = GameObject.Find(param);
		if (selectedGameObject != null)
		{
			return selectedGameObject.name + " has been selected.";
		}
		return "Could not find GameObject " + param;
	}

	private string SelectMaterial(string param)
	{
		if (selectedGameObject == null)
		{
			return "Select a GameObject first";
		}
		int result = 0;
		if (param != string.Empty)
		{
			int.TryParse(param, out result);
		}
		Renderer component = selectedGameObject.GetComponent<Renderer>();
		if (component == null)
		{
			return "No material selected";
		}
		if (result >= component.materials.Length)
		{
			return "No material selected";
		}
		if (result < 0)
		{
			return "No material selected";
		}
		selectedMaterial = component.materials[result];
		if (selectedMaterial == null)
		{
			return "No material selected";
		}
		return "Material: [" + selectedMaterial.name + "] selected!";
	}

	private string SetShaderFromFile(string param)
	{
		if (selectedMaterial == null)
		{
			return "select a material first";
		}
		try
		{
			Shader shader = FileUtil.LoadShader(GameConfig.PEDataPath + param);
			if (shader == null)
			{
				return "Create shader failed";
			}
			selectedMaterial.shader = shader;
			return "Shader: [" + shader.name + "] has been set to material";
		}
		catch
		{
			return "Open shader file failed";
		}
	}

	private string SetShader(string param)
	{
		if (selectedMaterial == null)
		{
			return "select a material first";
		}
		try
		{
			Shader shader = Resources.Load<Shader>(param);
			if (shader == null)
			{
				return "Load shader failed";
			}
			selectedMaterial.shader = shader;
			return "Shader: [" + shader.name + "] has been set to material";
		}
		catch
		{
			return "Open shader file failed";
		}
	}

	private string GetMaterialProperty(string param)
	{
		if (selectedMaterial == null)
		{
			return "select a material first";
		}
		return "float: " + selectedMaterial.GetFloat(param) + "\r\nColor: " + selectedMaterial.GetColor(param).ToString() + "\r\nVector: " + selectedMaterial.GetVector(param).ToString();
	}

	private string TimeSpeed(string param)
	{
		if (param == string.Empty)
		{
			return "Parameter excepted";
		}
		float elapseSpeed = Convert.ToSingle(param);
		GameTime.Timer.ElapseSpeed = elapseSpeed;
		return "GameTime.Timer.ElapseSpeed = " + param;
	}

	private string PassTime(string param)
	{
		if (param == string.Empty)
		{
			return "Parameter excepted";
		}
		string[] array = param.Split(',');
		if (array.Length == 1)
		{
			double num = Convert.ToDouble(array[0]);
			GameTime.PassTime(num * 3600.0, 1.0);
		}
		else
		{
			double num2 = Convert.ToDouble(array[0]);
			double trueTime = Convert.ToDouble(array[1]);
			GameTime.PassTime(num2 * 3600.0, trueTime);
		}
		return "ok";
	}

	private string Repair(string param)
	{
		if (param == string.Empty)
		{
			return "Parameter excepted";
		}
		string[] array = param.Split(',');
		int id = Convert.ToInt32(array[0]) + 100000000;
		CreationData creation = CreationMgr.GetCreation(id);
		if (creation != null)
		{
			float num = creation.m_Attribute.m_Durability;
			if (array.Length != 1)
			{
				num *= Mathf.Clamp01(Convert.ToSingle(array[1]) * 0.01f);
			}
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
			LifeLimit cmpt = itemObject.GetCmpt<LifeLimit>();
			cmpt.floatValue.current = num;
		}
		return "ok";
	}

	private string Recharge(string param)
	{
		if (param == string.Empty)
		{
			return "Parameter excepted";
		}
		string[] array = param.Split(',');
		int id = Convert.ToInt32(array[0]) + 100000000;
		CreationData creation = CreationMgr.GetCreation(id);
		if (creation != null)
		{
			float num = creation.m_Attribute.m_MaxFuel;
			if (array.Length != 1)
			{
				num *= Mathf.Clamp01(Convert.ToSingle(array[1]) * 0.01f);
			}
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
			Energy cmpt = itemObject.GetCmpt<Energy>();
			cmpt.floatValue.current = num;
		}
		return "ok";
	}

	private string MapCmd(string param)
	{
		PeSingleton<StaticPoint.Mgr>.Instance.UnveilAll();
		return "ok";
	}

	private string ToggleFMODEditor(string param)
	{
		PeFmodEditor.active = !PeFmodEditor.active;
		FMODAudioSource.rteActive = PeFmodEditor.active;
		MainPlayerCmpt.gMainPlayer.m_ActionEnable = !PeFmodEditor.active;
		if (PeFmodEditor.active)
		{
			GameObject gameObject = new GameObject("FMOD Editor");
			gameObject.AddComponent<PeFmodEditor>();
		}
		return "FMOD Editor is now " + ((!PeFmodEditor.active) ? "closed" : "opened");
	}

	private string FMODBankList(string param)
	{
		if (FMOD_StudioSystem.instance == null)
		{
			return "No FMOD system found";
		}
		if (FMOD_StudioSystem.instance.System == null)
		{
			return "No FMOD system found";
		}
		Bank[] array = new Bank[0];
		FMOD_StudioSystem.instance.System.getBankList(out array);
		string text = "Bank Count: " + array.Length + "\r\n";
		Bank[] array2 = array;
		foreach (Bank bank in array2)
		{
			string path = string.Empty;
			bank.getPath(out path);
			text = text + "    Bank [" + path + "]\r\n";
			if (param.IndexOf("-event") >= 0 || param.IndexOf("-all") >= 0)
			{
				EventDescription[] array3 = new EventDescription[0];
				bank.getEventList(out array3);
				string text2 = text;
				text = text2 + "        Events: (" + array3.Length + ")\r\n";
				EventDescription[] array4 = array3;
				foreach (EventDescription eventDescription in array4)
				{
					string path2 = string.Empty;
					eventDescription.getPath(out path2);
					text = text + "            Event [" + path2 + "]\r\n";
				}
			}
			if (param.IndexOf("-vca") >= 0 || param.IndexOf("-all") >= 0)
			{
				VCA[] array5 = new VCA[0];
				bank.getVCAList(out array5);
				string text2 = text;
				text = text2 + "        VCAs: (" + array5.Length + ")\r\n";
				VCA[] array6 = array5;
				foreach (VCA vCA in array6)
				{
					string path3 = string.Empty;
					vCA.getPath(out path3);
					text = text + "            VCA [" + path3 + "]\r\n";
				}
			}
			if (param.IndexOf("-bus") >= 0 || param.IndexOf("-all") >= 0)
			{
				Bus[] array7 = new Bus[0];
				bank.getBusList(out array7);
				string text2 = text;
				text = text2 + "        Buses: (" + array7.Length + ")\r\n";
				Bus[] array8 = array7;
				foreach (Bus bus in array8)
				{
					string path4 = string.Empty;
					bus.getPath(out path4);
					text = text + "            Bus [" + path4 + "]\r\n";
				}
			}
			if (param.IndexOf("-string") < 0 && param.IndexOf("-all") < 0)
			{
				continue;
			}
			int count = 0;
			bank.getStringCount(out count);
			if (count > 0)
			{
				string text2 = text;
				text = text2 + "        Strings: (" + count + ")\r\n";
				for (int m = 0; m < count; m++)
				{
					bank.getStringInfo(m, out var id, out var path5);
					text2 = text;
					text = text2 + "            GUID {" + id.ToString() + "}    Path [" + path5 + "]\r\n";
				}
			}
		}
		Debug.Log(text);
		return text;
	}

	private string WalkingSpeed(string param)
	{
		if (param == string.Empty)
		{
			return "Parameter excepted";
		}
		if (PeSingleton<PeCreature>.Instance == null)
		{
			return "Not in game";
		}
		if (PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			return "Not in game";
		}
		HumanPhyCtrl componentInChildren = PeSingleton<PeCreature>.Instance.mainPlayer.GetComponentInChildren<HumanPhyCtrl>();
		if (componentInChildren == null)
		{
			return "HumanPhyCtrl not exist";
		}
		float result = 1f;
		if (!float.TryParse(param, out result))
		{
			return "Invalid parameter";
		}
		componentInChildren.mSpeedTimes = result;
		return "ok";
	}

	private string JumpingStrength(string param)
	{
		if (param == string.Empty)
		{
			return "Parameter excepted";
		}
		if (PeSingleton<PeCreature>.Instance == null)
		{
			return "Not in game";
		}
		if (PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			return "Not in game";
		}
		MotionMgrCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<MotionMgrCmpt>();
		if (cmpt == null)
		{
			return "MotionMgrCmpt not exist";
		}
		Action_Jump action = cmpt.GetAction<Action_Jump>();
		if (action == null)
		{
			return "Action_Jump not exist";
		}
		float result = 1f;
		if (!float.TryParse(param, out result))
		{
			return "Invalid parameter";
		}
		action.m_JumpHeight = 2f * result;
		return "ok";
	}

	private string NoFallingDamage(string param)
	{
		if (PeSingleton<PeCreature>.Instance == null)
		{
			return "Not in game";
		}
		if (PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			return "Not in game";
		}
		MainPlayerCmpt.gMainPlayer.FallDamageSpeedThreshold = 10000000f;
		return "ok";
	}

	private string Invincible(string param)
	{
		if (PeSingleton<PeCreature>.Instance == null)
		{
			return "Not in game";
		}
		if (PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			return "Not in game";
		}
		PeSingleton<PeCreature>.Instance.mainPlayer.GetComponent<BiologyViewCmpt>().ActivateInjured(value: false);
		return "ok";
	}

	private string Uninvincible(string param)
	{
		if (PeSingleton<PeCreature>.Instance == null)
		{
			return "Not in game";
		}
		if (PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			return "Not in game";
		}
		PeSingleton<PeCreature>.Instance.mainPlayer.GetComponent<BiologyViewCmpt>().ActivateInjured(value: true);
		return "ok";
	}

	private string BuildGod(string param)
	{
		PEBuildingMan.Self.IsGod = !PEBuildingMan.Self.IsGod;
		return PEBuildingMan.Self.IsGod.ToString();
	}

	private string IgnoreMapCheck(string param)
	{
		UICustomSelectWndInterpreter.ignoreIntegrityCheck = !UICustomSelectWndInterpreter.ignoreIntegrityCheck;
		return UICustomSelectWndInterpreter.ignoreIntegrityCheck.ToString();
	}

	private string ShowBuildGUI(string param)
	{
		PEBuildingMan.Self.GUITest = !PEBuildingMan.Self.GUITest;
		return PEBuildingMan.Self.GUITest.ToString();
	}

	private string CustomEvent(string param)
	{
		param = param.Trim();
		if (string.IsNullOrEmpty(param))
		{
			return "parameters needed";
		}
		string[] array = param.Split(new string[1] { "|" }, StringSplitOptions.RemoveEmptyEntries);
		string text = array[0];
		EventListener eventListener = Asm.CreateEventListenerInstance(text);
		StatementRaw statementRaw = new StatementRaw();
		if (eventListener == null)
		{
			return "cannot create statement";
		}
		statementRaw.classname = text;
		statementRaw.order = 0;
		statementRaw.parameters = new ParamRaw(array.Length - 1);
		for (int i = 1; i < array.Length; i++)
		{
			string text2 = array[i];
			string[] array2 = text2.Split(new string[1] { "=" }, StringSplitOptions.RemoveEmptyEntries);
			if (array2.Length == 2)
			{
				statementRaw.parameters.Set(i - 1, array2[0], array2[1]);
			}
			else
			{
				statementRaw.parameters.Set(i - 1, "null", "0");
			}
		}
		eventListener.Init(null, statementRaw);
		eventListener.OnPost += OnCustomEventPost;
		eventListener.Listen();
		return "[" + text + "] is listening";
	}

	private void OnCustomEventPost(EventListener evt)
	{
		Debug.LogWarning("[" + evt.classname + "] posted");
	}

	private string CustomCondition(string param)
	{
		param = param.Trim();
		if (string.IsNullOrEmpty(param))
		{
			return "parameters needed";
		}
		string[] array = param.Split(new string[1] { "|" }, StringSplitOptions.RemoveEmptyEntries);
		string text = array[0];
		Condition condition = Asm.CreateConditionInstance(text);
		StatementRaw statementRaw = new StatementRaw();
		if (condition == null)
		{
			return "cannot create statement";
		}
		statementRaw.classname = text;
		statementRaw.order = 0;
		statementRaw.parameters = new ParamRaw(array.Length - 1);
		for (int i = 1; i < array.Length; i++)
		{
			string text2 = array[i];
			string[] array2 = text2.Split(new string[1] { "=" }, StringSplitOptions.RemoveEmptyEntries);
			if (array2.Length == 2)
			{
				statementRaw.parameters.Set(i - 1, array2[0], array2[1]);
			}
			else
			{
				statementRaw.parameters.Set(i - 1, "null", "0");
			}
		}
		condition.Init(null, statementRaw);
		return "[" + text + "] check result is " + condition.Check();
	}

	private string CustomAction(string param)
	{
		param = param.Trim();
		if (string.IsNullOrEmpty(param))
		{
			return "parameters needed";
		}
		string[] array = param.Split(new string[1] { "|" }, StringSplitOptions.RemoveEmptyEntries);
		string text = array[0];
		ScenarioRTL.Action action = Asm.CreateActionInstance(text);
		StatementRaw statementRaw = new StatementRaw();
		if (action == null)
		{
			return "cannot create statement";
		}
		statementRaw.classname = text;
		statementRaw.order = 0;
		statementRaw.parameters = new ParamRaw(array.Length - 1);
		for (int i = 1; i < array.Length; i++)
		{
			string text2 = array[i];
			string[] array2 = text2.Split(new string[1] { "=" }, StringSplitOptions.RemoveEmptyEntries);
			if (array2.Length == 2)
			{
				statementRaw.parameters.Set(i - 1, array2[0], array2[1]);
			}
			else
			{
				statementRaw.parameters.Set(i - 1, "null", "0");
			}
		}
		action.Init(null, statementRaw);
		StartCoroutine(CustomActionThread(action));
		return "[" + text + "] is running";
	}

	private IEnumerator CustomActionThread(ScenarioRTL.Action act)
	{
		while (!act.Logic())
		{
			yield return 0;
		}
	}

	private string ToogleScenarioTool(string param)
	{
		PeScenario.s_ShowTools = !PeScenario.s_ShowTools;
		return "ok";
	}

	private string LagTest(string param)
	{
		param = param.Trim();
		if (string.IsNullOrEmpty(param))
		{
			return "parameters needed";
		}
		double result = 0.0;
		if (double.TryParse(param, out result))
		{
			if (result < 0.017)
			{
				result = 0.017;
			}
			LagTester.threshold = result;
			return "ok";
		}
		return "invalid parameter";
	}

	private string GoPointer(string param)
	{
		ObjectDebugInfoShower component = GetComponent<ObjectDebugInfoShower>();
		if (component != null)
		{
			component.enabled = !component.enabled;
		}
		return "ok";
	}

	private string EntityCmd(string param)
	{
		if (string.IsNullOrEmpty(param))
		{
			return "param:<cmd> <id> [args]";
		}
		string[] array = param.Split(' ');
		if (array.Length < 2)
		{
			return "param:<cmd> <id> [args]";
		}
		string funcName = array[0];
		if (!int.TryParse(array[1], out var result))
		{
			return "param:<cmd> <id> [args]";
		}
		List<string> list = new List<string>(array);
		list.RemoveRange(0, 2);
		return DoEntityCmd(result, funcName, list.ToArray());
	}

	private static bool stringToVector3(string text, out Vector3 v)
	{
		v = Vector3.zero;
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		string[] array = text.Split(',');
		if (array.Length < 3)
		{
			return false;
		}
		if (!float.TryParse(array[0], out v.x))
		{
			return false;
		}
		if (!float.TryParse(array[1], out v.y))
		{
			return false;
		}
		if (!float.TryParse(array[2], out v.z))
		{
			return false;
		}
		return true;
	}

	private string DoEntityCmd(int id, string funcName, string[] args)
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(id);
		if (null == peEntity)
		{
			return "can't find entity by id:" + id;
		}
		switch (funcName)
		{
		case "servant":
		{
			NpcCmpt cmpt2 = peEntity.GetCmpt<NpcCmpt>();
			if (cmpt2 == null)
			{
				return "no NpcCmpt.";
			}
			ServantLeaderCmpt cmpt3 = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();
			if (cmpt3 == null)
			{
				return "no ServantLeaderCmpt";
			}
			cmpt3.AddServant(cmpt2);
			cmpt2.SetServantLeader(cmpt3);
			return "ok";
		}
		case "start_skill":
		{
			if (!int.TryParse(args[0], out var result))
			{
				return "get target id failed";
			}
			PeEntity peEntity3 = PeSingleton<EntityMgr>.Instance.Get(result);
			SkAliveEntity cmpt = peEntity3.GetCmpt<SkAliveEntity>();
			if (null == cmpt)
			{
				return "target have no SkillCmpt";
			}
			if (!int.TryParse(args[1], out var result2))
			{
				return "get skill id failed";
			}
			cmpt.StartSkill(cmpt, result2);
			return "ok";
		}
		case "Kill":
		{
			PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get(id);
			if (peEntity2 == null)
			{
				return "get entity failed with id : " + id;
			}
			peEntity2.SetAttribute(AttribType.Hp, 0f, offEvent: false);
			return "ok";
		}
		default:
			return "not implementd cmd";
		}
	}
}
