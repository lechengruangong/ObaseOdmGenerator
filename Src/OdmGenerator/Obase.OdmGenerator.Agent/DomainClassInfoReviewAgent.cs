/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：领域类信息审核代理.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-8-19 15:23:19
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using Microsoft.Extensions.AI;
using Obase.OdmGenerator.Agent.Analyzer;
using Obase.OdmGenerator.Agent.Config;
using Obase.OdmGenerator.Agent.Cons;

namespace Obase.OdmGenerator.Agent;

/// <summary>
///     领域类信息审核代理
/// </summary>
public class DomainClassInfoReviewAgent : BaseDomainClassInfoProcessAgent
{
    /// <summary>
    ///     初始化领域类信息审核代理
    /// </summary>
    /// <param name="domainClassInfos">领域类信息集合</param>
    /// <param name="config">LLM配置</param>
    /// <param name="knowledge">Agent知识</param>
    /// <param name="output">Agent输出</param>
    /// <param name="tools">MCP工具集合</param>
    /// <param name="useJsonOutput">是否使用Json输出</param>
    public DomainClassInfoReviewAgent(List<DomainClassInfo> domainClassInfos, IApikeyConfiguration config,
        string knowledge = null, string output = null, AITool[] tools = null, bool useJsonOutput = true) : base(
        domainClassInfos, config, "你是一个领域类的类型审核器,负责根据领域信息来判断领域类型的结果是否与领域信息相符合.",
        string.IsNullOrEmpty(knowledge) ? DefaultPrompts.GetDefaultDomainInfoKnowledge() : knowledge,
        "使用查询领域类信息,根据属性名称查询属性的注释等工具来获取领域类的信息,并根据领域类的属性和引用关系来分析领域类型是否与领域类型的结果相符合.", output, tools, useJsonOutput)
    {
    }
}