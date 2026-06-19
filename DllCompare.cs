using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

public class DllCompare {
    public static void Main(string[] args) {
        var origPath = args[0];
        var ourPath = args[1];
        
        var runtimeDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
        var resolver = new PathAssemblyResolver(Directory.GetFiles(runtimeDir, "*.dll").Concat(new[]{origPath}).ToArray());
        
        using var mlcOrig = new MetadataLoadContext(resolver);
        var origAsm = mlcOrig.LoadFromAssemblyPath(origPath);
        
        var resolver2 = new PathAssemblyResolver(Directory.GetFiles(runtimeDir, "*.dll").Concat(new[]{ourPath}).ToArray());
        using var mlcOur = new MetadataLoadContext(resolver2);
        var ourAsm = mlcOur.LoadFromAssemblyPath(ourPath);
        
        // Compare DebugMotherboard type
        var origType = origAsm.GetTypes().FirstOrDefault(t => t.Name == "DebugMotherboard");
        var ourType = ourAsm.GetTypes().FirstOrDefault(t => t.Name == "DebugMotherboard");
        
        if (origType == null) { Console.WriteLine("DebugMotherboard not in original!"); return; }
        if (ourType == null) { Console.WriteLine("DebugMotherboard not in ours!"); return; }
        
        // Get all fields
        var origFields = origType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .Select(f => f.Name).OrderBy(n => n).ToList();
        var ourFields = ourType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .Select(f => f.Name).OrderBy(n => n).ToList();
        
        var onlyOrig = origFields.Except(ourFields).ToList();
        var onlyOurs = ourFields.Except(origFields).ToList();
        
        Console.WriteLine($"Original field count: {origFields.Count}");
        Console.WriteLine($"Our field count: {ourFields.Count}");
        
        if (onlyOrig.Any()) {
            Console.WriteLine("\nFields ONLY in original:");
            foreach (var f in onlyOrig) Console.WriteLine($"  - {f}");
        }
        if (onlyOurs.Any()) {
            Console.WriteLine("\nFields ONLY in ours:");
            foreach (var f in onlyOurs) Console.WriteLine($"  + {f}");
        }
        
        // Also compare all types
        var origTypes = origAsm.GetTypes().Select(t => t.FullName).OrderBy(n => n).ToList();
        var ourTypes = ourAsm.GetTypes().Select(t => t.FullName).OrderBy(n => n).ToList();
        
        var onlyOrigTypes = origTypes.Except(ourTypes).ToList();
        var onlyOurTypes = ourTypes.Except(origTypes).ToList();
        
        if (onlyOrigTypes.Any()) {
            Console.WriteLine("\nTypes ONLY in original:");
            foreach (var t in onlyOrigTypes) Console.WriteLine($"  - {t}");
        }
        if (onlyOurTypes.Any()) {
            Console.WriteLine("\nTypes ONLY in ours:");
            foreach (var t in onlyOurTypes) Console.WriteLine($"  + {t}");
        }
    }
}
