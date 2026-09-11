/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：领域类信息，包括类名、引用的类型和文件路.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-8-19 11:37:25
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Obase.OdmGenerator.Agent.Analyzer;

/// <summary>
///     领域类信息，包括类名、引用的类型和文件路径
/// </summary>
public class DomainClassInfo
{
    /// <summary>
    ///     类名
    /// </summary>
    public string ClassName { get; init; }

    /// <summary>
    ///     类的注释，取类声明前的文档注释内容
    /// </summary>
    public string ClassComment { get; set; } = string.Empty;

    /// <summary>
    ///     引用的其他类型列表
    ///     仅包含属性类型 类型仅限于当前领域内 不包括系统类型
    /// </summary>
    public List<Tuple<string, string>> ReferencedTypes { get; set; } = [];

    /// <summary>
    ///     类的属性列表，每个属性包含类型和名称
    /// </summary>
    public List<Tuple<string, string>> PropertyList { get; set; } = [];

    /// <summary>
    ///     类的属性注释字典，键为属性名称，值为属性注释
    /// </summary>
    public Dictionary<string, string> PropertyComments { get; set; } = new();

    /// <summary>
    ///     转换为字符串表示，方便输出和调试
    /// </summary>
    /// <returns>字符串</returns>
    public override string ToString()
    {
        var refs = ReferencedTypes.Count > 0
            ? string.Join(", ", ReferencedTypes.Select(p => $"{p.Item1} {p.Item2}"))
            : "None";
        var props = PropertyList.Count > 0
            ? string.Join(", ", PropertyList.Select(p => $"{p.Item1} {p.Item2}"))
            : "None";
        var comments = PropertyComments.Count > 0
            ? string.Join(", ", PropertyComments.Select(p => $"{p.Key} {p.Value}"))
            : "None";
        return
            $"{{ Class: {ClassName}, ClassComment: {ClassComment}, References: [{refs}], Properties: [{props}], PropertyComments: [{comments}] }}";
    }
}