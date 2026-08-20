/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：生成的ODM配置结果.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-8-19 16:50:43
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.OdmGenerator.WorkFlow.Dto;

/// <summary>
///     生成的ODM配置结果
/// </summary>
public class ObjectDataModelConfigResult
{
    /// <summary>
    ///     类型
    ///     0 - 实体型 1 - 显式关联型 2 - 隐式关联型
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    ///     配置代码
    /// </summary>
    public string ConfigurationCode { get; set; }

    /// <summary>
    ///     转为字符串
    /// </summary>
    /// <returns>字符串</returns>
    public override string ToString()
    {
        return Type switch
        {
            0 => $"实体型的配置为{ConfigurationCode}",
            1 => $"显式关联型的配置为{ConfigurationCode}",
            2 => $"隐式关联型的配置为{ConfigurationCode}",
            _ => throw new ArgumentOutOfRangeException(nameof(Type), $"未知类型: {Type}")
        };
    }
}