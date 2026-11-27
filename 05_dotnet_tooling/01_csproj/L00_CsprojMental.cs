using System;
using System.Collections.Generic;
using System.Xml.Linq;

/*
[主题]: csproj 心智 —— SDK 风格项目文件在读什么
[难度]: Easy
[前置]: 05_dotnet_tooling/00_sdk

一、主题说明
现代项目用 `<Project Sdk="Microsoft.NET.Sdk">`，大部分约定隐式导入。
关键节点：`OutputType`、`TargetFramework`、`PackageReference`、`Nullable`。

二、写法对比
1. 直观版：在 IDE 里点选，不看 XML。
2. 进阶版：能读 csproj，知道改哪影响编译/输出/包。

三、真实用途
- 升级 TFM：`net8.0` → `net9.0`。
- 开关可空：`Nullable` enable。
- 加包：`<PackageReference Include="..." Version="..." />`。
资料: https://learn.microsoft.com/en-us/dotnet/core/tools/
      https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/
      https://dotnet.microsoft.com/en-us/download
*/
static class Program
{
    /** 迷你 csproj 字符串（与课前模板一致）。 */
    const string SampleCsproj = """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <OutputType>Exe</OutputType>
            <TargetFramework>net8.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
          </PropertyGroup>
        </Project>
        """;

    /** 直观版：字符串 Contains 判断。 */
    static bool IsExecutableNaive(string xml) =>
        xml.Contains("<OutputType>Exe</OutputType>", StringComparison.Ordinal);

    /** 进阶版：XDocument 解析。 */
    static Dictionary<string, string> ReadProperties(string xml)
    {
        var doc = XDocument.Parse(xml);
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pg in doc.Descendants("PropertyGroup"))
        {
            foreach (var el in pg.Elements())
            {
                map[el.Name.LocalName] = el.Value;
            }
        }
        return map;
    }

    static void RunTests()
    {
        if (!IsExecutableNaive(SampleCsproj)) throw new Exception("应为 Exe");

        var props = ReadProperties(SampleCsproj);
        if (props["TargetFramework"] != "net8.0") throw new Exception("TFM");
        if (props["Nullable"] != "enable") throw new Exception("Nullable");

        const string withPackage = """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
              </ItemGroup>
            </Project>
            """;
        var pkg = XDocument.Parse(withPackage)
            .Descendants("PackageReference")
            .First();
        if ((string?)pkg.Attribute("Include") != "Newtonsoft.Json") throw new Exception("包名");

        Console.WriteLine("✅ CsprojMental 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 CsprojMental ************\n");
        var props = ReadProperties(SampleCsproj);
        Console.WriteLine($"TFM={props["TargetFramework"]} Output={props["OutputType"]} Nullable={props["Nullable"]}");
        RunTests();
    }

    static void Main() => Bootstrap();
}