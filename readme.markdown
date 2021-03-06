Iron Python Plugins for [Blaze][1]
========

This is a simple hosting facility to allow for anyone to write plugins for blaze using ironpython.

Main highlights are:

  - Simple (iron) python syntax for plugins. Subclass *BaseIronPythonCommand*, implement *Name* and *Execute* and you're done.
  - Autoreloading in runtime. Edit the plugin .ipy file and blaze reloads it automatically for you.
  - Use [ContextLib.UserContext][3] to access the user's current scope, including selected files in explorer, volume and opened windows. 

Download now
------------

From the downloads tab you can find:

  - A version of blaze packaged with the hosting plugin and several iron python scripts
  - A version of the plugin ready to be dropped into blaze (also with scripts)

Try it! 

Creating a plugin
-----------------

Just create a .ipy file in *plugins/IronPythonPlugins* like this:

    def NewPlugin(args):
        pass
    
This creates a new plugin, with the name "New Plugin", that does nothing.
A simple plugin that launches the command line would be:

    def LaunchCommandLine(args):
        return "cmd"

A string returned from the plugin is interpreted as the command we want to execute, in this case, *cmd*.
Notice that the name is camel cased, and is translated to the name "Launch Command Line".


Examples
--------

Look into [Sample.ipy][2] a plugin created via a class:

    import System
    clr.AddReference("System.Windows.Forms")
    
    class MustScream(BaseIronPythonCommand):
        @property
        def Name(self):
            return "MustScream"
    
        def Execute(self, args):
            System.Windows.Forms.MessageBox.Show(args, "Shout!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning)


Here's a plugin that picks up the current path from the topmost explorer window and copies it into the clipboard:

    clr.AddReference("System.Windows.Forms")
    from System.Windows.Forms import Clipboard

    class CopyPathToClipboard(BaseIronPythonCommand):
        @property
        def Name(self):
            return "path to clipboard"
    
        def Execute(self, args):
            path = UserContext.GetExplorerPath(True)
            if path != None:
                Clipboard.SetText(path)

For a sample with autocomplete, check out the [putty plugin][4]

[1]: http://blaze-wins.sourceforge.net/
[2]: http://github.com/brunomlopes/Blaze-IronPythonPlugins/blob/master/source/IronPythonPlugins/Sample.ipy
[3]: http://github.com/brunomlopes/Blaze-IronPythonPlugins/raw/master/lib/blaze/Docs/ContextLib.chm
[4]: http://github.com/brunomlopes/Blaze-IronPythonPlugins/blob/master/source/IronPythonPlugins/Putty.ipy