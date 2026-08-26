/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：领域信息审核结果.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-8-19 16:50:22
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace Obase.OdmGenerator.WorkFlow.Dto;

/// <summary>
///     领域信息审核结果
/// </summary>
public class DomainClassReviewResult
{
    /// <summary>
    ///     是否通过
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    ///     不通过时的审核信息
    /// </summary>
    public string ReviewMessage { get; set; }

    /// <summary>
    ///     通过时的领域信息提取结果
    /// </summary>
    public List<DomainClassExtractResult> DomainClassExtractResults { get; set; }

    /// <summary>
    ///     转换为字符串
    /// </summary>
    /// <returns>字符串</returns>
    public override string ToString()
    {
        return
            $"{{ \"IsValid\":{IsValid},\"ReviewMessage\":\"{ReviewMessage}\", \"DomainClassExtractResults\":{string.Join(" ", DomainClassExtractResults)} }}";
    }
}