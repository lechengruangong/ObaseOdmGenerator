/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：领域信息提取器接口.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-8-19 11:39:41
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace Obase.OdmGenerator.Agent.Analyzer;

/// <summary>
///     领域信息提取器接口
/// </summary>
public interface IDomainInfoAnalyzer
{
    /// <summary>
    ///     提取领域信息方法
    /// </summary>
    /// <returns>领域类信息列表</returns>
    List<DomainClassInfo> Analyze();
}