#! "netcoreapp5.0"

#r "nuget:Nuke.Common,5.0.2"
#r "nuget:Svg.Skia,0.5.0"
#r "nuget:SkiaSharp,2.80.2"

#load "nuget:Dotnet.Build, 0.7.1"


using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Nuke.Common.IO;
using SkiaSharp;
using Svg.Skia;
using static Nuke.Common.IO.FileSystemTasks;

using static FileUtils;

var ScriptDir = (AbsolutePath)GetScriptFolder();
var RepoDir = ScriptDir / "..";
var IconCollectionSrcDir = RepoDir / "IconCollections";
var WebIconCollectionsDir = RepoDir / "WebIconCollections";
var IconCollectionOutDir = WebIconCollectionsDir/ "wwwroot" / "images" / "IconCollections";

var iconsCollection = new Dictionary<string, string[]>();

var SVG = new SKSvg();
var BgColor = new SKColor();

Console.WriteLine("Collecting Icons");

foreach(var collSrcDir in IconCollectionSrcDir.GlobDirectories("*")) {
    var collName = Path.GetFileName(collSrcDir);

    var allFiles = new List<string>();

    foreach(var csvFile in collSrcDir.GlobFiles("*.svg"))
        allFiles.Add(Path.GetFileNameWithoutExtension(csvFile));
    
    if(allFiles.Count > 0)
        iconsCollection.Add(collName, allFiles.ToArray());
}


Console.WriteLine("Converting Icons");

/*foreach(var kv in iconsCollection) {
    var collDestDir = IconCollectionOutDir / kv.Key;
    var collSrcDir = IconCollectionSrcDir / kv.Key;

    Directory.CreateDirectory(collDestDir);

    foreach(var iconName in kv.Value) {
        
        var destSvg = collDestDir / iconName + ".svg";
        var srcSvg = collSrcDir / iconName + ".svg";

        Console.WriteLine(srcSvg);


        File.Copy(srcSvg, destSvg);

        if (SVG.Load(destSvg) is { }) {

            { // 128
                var scale = 128.0f / (SVG.Picture?.CullRect.Width ?? 16.0f);
                var outFile = collDestDir / iconName + ".128.png";
                SVG.Save(outFile, BgColor, SKEncodedImageFormat.Png, 100, scale, scale);
            }

            { // 64
                var scale = 64.0f / (SVG.Picture?.CullRect.Width ?? 16.0f);
                var outFile = collDestDir / iconName + ".64.png";
                SVG.Save(outFile, BgColor, SKEncodedImageFormat.Png, 100, scale, scale);
            }

            { // 32
                var scale = 32.0f / (SVG.Picture?.CullRect.Width ?? 16.0f);
                var outFile = collDestDir / iconName + ".32.png";
                SVG.Save(outFile, BgColor, SKEncodedImageFormat.Png, 100, scale, scale);
            }

            { // 16
                var scale = 16.0f / (SVG.Picture?.CullRect.Width ?? 16.0f);
                var outFile = collDestDir / iconName + ".16.png";
                SVG.Save(outFile, BgColor, SKEncodedImageFormat.Png, 100, scale, scale);
            }

        } else {
            throw new Exception(destSvg);
        }
    }
}*/

System.IO.StringWriter baseTextWriter = new System.IO.StringWriter();
var tw = new System.CodeDom.Compiler.IndentedTextWriter(baseTextWriter, "    ");

tw.WriteLine(@"
using System.Collections.Generic;

namespace WebIconCollections
{
    public static class Collections
    {
        public static readonly IReadOnlyDictionary<string, string[]> IconCollections = new Dictionary<string, string[]>
        {
");
tw.Indent = 3;

foreach(var kv in iconsCollection) {
    tw.WriteLine("{ \"" + kv.Key + "\", new string[] {");
    tw.Indent++;
    foreach(var iconName in kv.Value)
        tw.WriteLine($"\"{iconName}\",");
    tw.Indent--;
    tw.WriteLine("}},");
}

tw.Indent = 0;
tw.WriteLine(@"
        };
    }
}
");
File.WriteAllText(WebIconCollectionsDir / "Collections.cs", baseTextWriter.ToString().Replace("\r\n", "\n"));
