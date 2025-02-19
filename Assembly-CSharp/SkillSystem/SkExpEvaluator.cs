using System;
using System.Collections.Generic;
using Mono.CSharp;
using UnityEngine;

namespace SkillSystem;

public class SkExpEvaluator : IExpCompiler
{
	private List<ICompilableExp> _reqers = new List<ICompilableExp>();

	private List<string> _progs = new List<string>();

	public SkExpEvaluator()
	{
		Evaluator.Init(new string[0]);
		Evaluator.ReferenceAssembly(typeof(ISkAttribs).Assembly);
	}

	public void Compile()
	{
		int tickCount = Environment.TickCount;
		string text = "new System.Action<ISkAttribs, ISkAttribs, ISkAttribsModPara>[]{";
		for (int i = 0; i < _progs.Count; i++)
		{
			text += _progs[i];
		}
		text += "};";
		CompiledMethod compiledMethod = Evaluator.Compile(text);
		object retvalue = null;
		compiledMethod(ref retvalue);
		Action<ISkAttribs, ISkAttribs, ISkAttribsModPara>[] array = retvalue as Action<ISkAttribs, ISkAttribs, ISkAttribsModPara>[];
		for (int j = 0; j < _reqers.Count; j++)
		{
			_reqers[j].OnCompiled(array[j]);
		}
		_reqers.Clear();
		_progs.Clear();
		Debug.Log("[EXP]All Compiled:" + (Environment.TickCount - tickCount));
	}

	public void AddExpString(ICompilableExp op, string strExp)
	{
		string text = "new System.Action<ISkAttribs, ISkAttribs, ISkAttribsModPara>((caster, target, para) => { " + strExp + "; }),";
		try
		{
			_reqers.Add(op);
			_progs.Add(text);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to add exp string:" + text + ex);
		}
	}
}
