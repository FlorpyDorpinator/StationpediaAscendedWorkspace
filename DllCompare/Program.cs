using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

var origPath = args[0];
var ourPath = args[1];
var runtimeDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();

var origPaths = Directory.GetFiles(runtimeDir, "*.dll").Append(origPath).ToArray();
var resolver1 = new PathAssemblyResolver(origPaths);
using var mlc1 = new MetadataLoadContext(resolver1);
var origAsm = mlc1.LoadFromAssemblyPath(origPath);

var ourPaths = Directory.GetFiles(runtimeDir, "*.dll").Append(ourPath).ToArray();
var resolver2 = new PathAssemblyResolver(ourPaths);
using var mlc2 = new MetadataLoadContext(resolver2);
var ourAsm = mlc2.LoadFromAssemblyPath(ourPath);

var origType = origAsm.GetTypes().FirstOrDefault(t => t.Name == "DebugMotherboard");
var ourType = ourAsm.GetTypes().FirstOrDefault(t => t.Name == "DebugMotherboard");

Console.WriteLine($"Orig DebugMotherboard: {origType?.FullName ?? "NOT FOUND"}");
Console.WriteLine($"Ours DebugMotherboard: {ourType?.FullName ?? "NOT FOUND"}");

if (origType != null && ourType != null) {
    var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
    var origFields = origType.GetFields(bf).Select(f => f.Name).OrderBy(n => n).ToList();
    var ourFields = ourType.GetFields(bf).Select(f => f.Name).OrderBy(n => n).ToList();
    Console.WriteLine($"\nOrig field count: {origFields.Count}");
    Console.WriteLine($"Ours field count: {ourFields.Count}");
    var onlyOrig = origFields.Except(ourFields).ToList();
    var onlyOurs = ourFields.Except(origFields).ToList();
    if (onlyOrig.Any()) { Console.WriteLine("\nFields ONLY in original:"); foreach (var f in onlyOrig) Console.WriteLine($"  - {f}"); }
    if (onlyOurs.Any()) { Console.WriteLine("\nFields ONLY in ours:"); foreach (var f in onlyOurs) Console.WriteLine($"  + {f}"); }
    if (!onlyOrig.Any() && !onlyOurs.Any()) Console.WriteLine("\nAll fields match!");
    
    var origMethods = origType.GetMethods(bf).Select(m => m.Name).OrderBy(n => n).ToList();
    var ourMethods = ourType.GetMethods(bf).Select(m => m.Name).OrderBy(n => n).ToList();
    var onlyOrigM = origMethods.Except(ourMethods).ToList();
    var onlyOursM = ourMethods.Except(origMethods).ToList();
    if (onlyOrigM.Any()) { Console.WriteLine("\nMethods ONLY in original:"); foreach (var m in onlyOrigM) Console.WriteLine($"  - {m}"); }
    if (onlyOursM.Any()) { Console.WriteLine("\nMethods ONLY in ours:"); foreach (var m in onlyOursM) Console.WriteLine($"  + {m}"); }
}

// Compare all types
var origTypes = origAsm.GetTypes().Select(t => t.FullName).OrderBy(n => n).ToList();
var ourTypes = ourAsm.GetTypes().Select(t => t.FullName).OrderBy(n => n).ToList();
var onlyOrigT = origTypes.Except(ourTypes).ToList();
var onlyOurT = ourTypes.Except(origTypes).ToList();
if (onlyOrigT.Any()) { Console.WriteLine("\nTypes ONLY in original:"); foreach (var t in onlyOrigT) Console.WriteLine($"  - {t}"); }
if (onlyOurT.Any()) { Console.WriteLine("\nTypes ONLY in ours:"); foreach (var t in onlyOurT) Console.WriteLine($"  + {t}"); }
if (!onlyOrigT.Any() && !onlyOurT.Any()) Console.WriteLine("\nAll types match!");
