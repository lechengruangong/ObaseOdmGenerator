/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：领域信息提取器工厂.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-8-19 11:43:22
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.OdmGenerator.Agent.Analyzer;

/// <summary>
///     领域信息提取器工厂
/// </summary>
public static class DomainInfoAnalyzerFactory
{
    /// <summary>
    ///     创建领域信息提取器
    /// </summary>
    /// <param name="codePath">代码文件所在路径</param>
    /// <param name="language">编程语言</param>
    /// <returns>领域信息提取器</returns>
    public static IDomainInfoAnalyzer CreateAnalyzer(string codePath, ELanguage language)
    {
        return language switch
        {
            ELanguage.CSharp => new DotNetDomainInfoAnalyzer(codePath),
            ELanguage.Java => throw new NotImplementedException("Java语言的领域信息分析器未实现."),
            _ => throw new ArgumentOutOfRangeException(nameof(language), language, "未知的编程语言类型.")
        };
    }
}