/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：领域信息提取结果.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-8-19 16:50:01
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.OdmGenerator.WorkFlow.Dto;

/// <summary>
///     领域信息提取结果
/// </summary>
public class DomainClassExtractResult
{
    /// <summary>
    ///     类型
    ///     0 - 实体型 1 - 显式关联型 2 - 隐式关联型
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    ///     主键
    /// </summary>
    public string[] Identity { get; set; }

    /// <summary>
    ///     类名
    ///     如果是实体型或者显式关联型 此处为类型的名称
    ///     如果是隐式关联型 此处为空
    /// </summary>
    public string ClassName { get; set; }

    /// <summary>
    ///     参与隐式关联的类名1
    ///     如果是隐式关联型 此处为参与隐式关联的其中一个类名
    /// </summary>
    public string EndName1 { get; set; }

    /// <summary>
    ///     参与隐式关联的类名2
    ///     如果是隐式关联型 此处为参与隐式关联的另外一个类名
    /// </summary>
    public string EndName2 { get; set; }

    /// <summary>
    ///     转为字符串
    /// </summary>
    /// <returns>字符串</returns>
    public override string ToString()
    {
        return Type switch
        {
            0 => $"{ClassName}是实体型,主键是{string.Join(",", Identity)}.",
            1 => $"{ClassName}是显式关联型.",
            2 => $"{EndName1}和{EndName2}之间存在隐式关联型.",
            _ => throw new ArgumentOutOfRangeException(nameof(Type), $"未知类型: {Type}")
        };
    }
}