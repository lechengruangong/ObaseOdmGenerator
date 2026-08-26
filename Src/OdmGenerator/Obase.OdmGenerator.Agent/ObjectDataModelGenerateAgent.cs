/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象数据模型配置生成代理.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-8-19 15:34:21
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using Microsoft.Extensions.AI;
using Obase.OdmGenerator.Agent.Analyzer;
using Obase.OdmGenerator.Agent.Config;

namespace Obase.OdmGenerator.Agent;

/// <summary>
///     对象数据模型配置生成代理
/// </summary>
public class ObjectDataModelGenerateAgent : BaseDomainClassInfoProcessAgent
{
    /// <summary>
    ///     初始化对象数据模型配置审核代理
    /// </summary>
    /// <param name="domainClassInfos">领域类信息集合</param>
    /// <param name="config">LLM配置</param>
    /// <param name="output">Agent输出</param>
    /// <param name="tools">MCP工具集合</param>
    /// <param name="useJsonOutput">是否使用Json输出</param>
    /// <param name="language">语言</param>
    /// <param name="entityRule">实体型的配置规则</param>
    /// <param name="explicitlyRule">显式关联型的配置规则</param>
    /// <param name="implicitRule">隐式关联型的配置规则</param>
    public ObjectDataModelGenerateAgent(List<DomainClassInfo> domainClassInfos, IApikeyConfiguration config,
        ELanguage language, string entityRule = null, string explicitlyRule = null,
        string implicitRule = null, string output = null, AITool[] tools = null, bool useJsonOutput = true) : base(
        domainClassInfos, config, "你是一个Obase的ODM模型配置生成器,根据传入的领域类信息生成对应的ODM基础配置.",
        new ObjectDataModelGenerateRule(language, entityRule, explicitlyRule, implicitRule).ToString(),
        "使用查询领域类信息,根据属性名称查询属性的注释等工具来获取领域类的信息,并根据领域类的属性和引用关系来生成对应的ODM基础配置.", output, tools, useJsonOutput)
    {
    }
}