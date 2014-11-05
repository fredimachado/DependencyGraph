DependencyGraph
===============

Generates dependency graphs from C# projects recursively to be pasted in yuml.me

Usage: dg <rootFolder> <outputFile>

It will write something like this in the output file:

[MyApp.UI] -> [MyApp.Core]
[MyApp.Tests] -> [MyApp.Core]
[MyApp.Core]

Now you can paste the output in http://yuml.me/diagram/scruffy/class/draw, click "Draw Diagram" and see the result:

![Example Diagram](https://raw.githubusercontent.com/Fredi/DependencyGraph/master/diagram.png)
