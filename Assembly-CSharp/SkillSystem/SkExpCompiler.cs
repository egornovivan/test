using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SkillSystem;

public class SkExpCompiler : IExpCompiler
{
	private List<ICompilableExp> _reqs = new List<ICompilableExp>();

	private string _srcCode = string.Empty;

	private static Assembly CompileExpressionToMethod(string sourceCode)
	{
		CompilerParameters compilerParameters = new CompilerParameters();
		compilerParameters.GenerateExecutable = false;
		compilerParameters.GenerateInMemory = true;
		compilerParameters.IncludeDebugInformation = false;
		compilerParameters.ReferencedAssemblies.Add(typeof(ISkAttribs).Assembly.Location);
		CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider("CSharp");
		try
		{
			CompilerResults compilerResults = codeDomProvider.CompileAssemblyFromSource(compilerParameters, sourceCode);
			if (compilerResults.Errors.HasErrors)
			{
				throw new InvalidOperationException("Expression has a syntax error.");
			}
			return compilerResults.CompiledAssembly;
		}
		catch (Exception ex)
		{
			Debug.LogError("[CompileFailed]" + sourceCode + ":" + ex);
			return null;
		}
	}

	public void Compile()
	{
		int tickCount = Environment.TickCount;
		Assembly assembly = CompileExpressionToMethod(_srcCode);
		int count = _reqs.Count;
		for (int i = 0; i < count; i++)
		{
			MethodInfo method = assembly.GetType("Func" + i).GetMethod("func");
			Action<ISkAttribs, ISkAttribs, ISkAttribsModPara> action = (Action<ISkAttribs, ISkAttribs, ISkAttribsModPara>)Delegate.CreateDelegate(typeof(Action<ISkAttribs, ISkAttribs, ISkAttribsModPara>), null, method);
			_reqs[i].OnCompiled(action);
		}
		Debug.Log("[EXP]All Compiled:" + (Environment.TickCount - tickCount));
		_reqs.Clear();
		_srcCode = string.Empty;
	}

	public void AddExpString(ICompilableExp op, string strExp)
	{
		int count = _reqs.Count;
		string text = string.Format("public static class Func" + count + "{{ public static void func(ISkAttribs caster, ISkAttribs target, ISkAttribsModPara para){{ {0};}}}}", strExp);
		_reqs.Add(op);
		_srcCode += text;
	}
}
