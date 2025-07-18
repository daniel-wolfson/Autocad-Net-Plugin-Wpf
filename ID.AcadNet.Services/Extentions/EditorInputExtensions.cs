using System;
using System.Linq.Expressions;
using System.Reflection;
using Autodesk.AutoCAD.EditorInput;

namespace Intellidesk.AcadNet.Services.Extentions
{
    /// <summary>
    /// Editor of document input extensions
    /// </summary>
    public static class EditorInputExtensions
    {
        static readonly Func<Editor, object[], PromptStatus> runCommand = GenerateRunCommand();
        public static PromptStatus Cmd(this Editor editor, params object[] args)
        {
            if (editor == null)
                throw new ArgumentNullException("editor");
            return runCommand(editor, args);
        }

        static Func<Editor, object[], PromptStatus> GenerateRunCommand()
        {
            MethodInfo method = typeof(Editor).GetMethod("RunCommand",
             BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            ParameterExpression instance = Expression.Parameter(typeof(Editor), "ed"); // <- FW 3.5
            ParameterExpression args = Expression.Parameter(typeof(object[]), "args"); // <- FW 3.5
            return Expression
                    .Lambda<Func<Editor, object[], PromptStatus>>(Expression.Call(instance, method, args), instance, args)
                    .Compile();
        }
    }
}