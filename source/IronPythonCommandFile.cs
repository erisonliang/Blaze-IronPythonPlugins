﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SystemCore.CommonTypes;
using SystemCore.SystemAbstraction.WindowManagement;
using ContextLib;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace IronPythonPlugins
{
    public class IronPythonCommandFile : IEnumerable<Command>
    {
        private readonly ScriptEngine _engine;
        private readonly FileInfo _pythonFile;
        private readonly List<Command> _localCommands;

        public IronPythonCommandFile(ScriptEngine engine, FileInfo pythonFile)
            : this(engine)
        {
            _pythonFile = pythonFile;

            var script = engine.CreateScriptSourceFromFile(pythonFile.FullName);

            FillCommandsFromScriptSource(script);
        }

        public IronPythonCommandFile(ScriptEngine engine, string sourceCode)
            :this(engine)
        {
            _engine = engine;
            var script = engine.CreateScriptSourceFromString(sourceCode, SourceCodeKind.File);
            FillCommandsFromScriptSource(script);
        }

        private IronPythonCommandFile(ScriptEngine engine)
        {
            _engine = engine;
            _localCommands = new List<Command>();
        }

        private void FillCommandsFromScriptSource(ScriptSource script)
        {
            CompiledCode code = script.Compile();
            ScriptScope scope = _engine.CreateScope();

            scope.SetVariable("IIronPythonCommand", ClrModule.GetPythonType(typeof(IIronPythonCommand)));
            scope.SetVariable("BaseIronPythonCommand", ClrModule.GetPythonType(typeof(BaseIronPythonCommand)));
            scope.SetVariable("UserContext", UserContext.Instance);
            scope.SetVariable("WindowUtility", WindowUtility.Instance);
            scope.SetVariable("clr", _engine.GetClrModule());
            code.Execute(scope);


            var pluginClasses = scope.GetItems()
                .Where(kvp => kvp.Value is IronPython.Runtime.Types.PythonType)
                .Where(
                    kvp =>
                    typeof(IIronPythonCommand).IsAssignableFrom(((IronPython.Runtime.Types.PythonType)kvp.Value).__clrtype__()))
                .Where(kvp => kvp.Key != "BaseIronPythonCommand" && kvp.Key != "IIronPythonCommand");


            foreach (var nameAndClass in pluginClasses)
            {
                var plugin = (IIronPythonCommand)_engine.Operations.Invoke(nameAndClass.Value, new object[] { });
                if (plugin as BaseIronPythonCommand != null)
                {
                    // retrieving the class name from the python time is a bit trickier without access to the engine
                    // so we pass this here
                    var commandName = CamelToSpaced(nameAndClass.Key);
                    ((BaseIronPythonCommand) plugin).SetDefaultName(commandName);
                }
                var command = new IronPythonPluginCommand(_pythonFile, plugin);

                _localCommands.Add(command);
            }
        }

        private string CamelToSpaced(string name)
        {
            return System.Text.RegularExpressions.Regex.Replace(name, "(?<l>[A-Z])", " ${l}").Trim();
        }

        public IEnumerator<Command> GetEnumerator()
        {
            return _localCommands.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}