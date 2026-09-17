/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象数据模型配置审核代理.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-8-19 15:41:08
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.AI;
using Obase.OdmGenerator.Agent.Analyzer;
using Obase.OdmGenerator.Agent.Config;
using Obase.OdmGenerator.Agent.Cons;

namespace Obase.OdmGenerator.Agent;

/// <summary>
///     对象数据模型配置审核代理
/// </summary>
public class ObjectDataModelReviewAgent : BaseDomainClassInfoProcessAgent
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
    /// <param name="reviewRule">审核要求,不指定时使用默认的审核要求</param>
    public ObjectDataModelReviewAgent(List<DomainClassInfo> domainClassInfos, IApikeyConfiguration config,
        ELanguage language, string entityRule = null, string explicitlyRule = null,
        string implicitRule = null, string reviewRule = null, string output = null, AITool[] tools = null,
        bool useJsonOutput = true) : base(
        domainClassInfos, config, "你是一个Obase的ODM模型配置审核器,根据传入的ODM基础配置代码审核是否符合配置规则.",
        string.IsNullOrEmpty(reviewRule)
            ? BuildDefaultReviewRule(language, entityRule, explicitlyRule, implicitRule)
            : reviewRule,
        "使用查询领域类信息,根据属性名称查询属性的注释等工具来获取领域类的信息,并根据领域类的属性和引用关系来分析领域信息是否与ODM基础配置代码结果相符合,同时应当核对分析结果中的类型与隐式关联是否均已在配置中体现.", output, tools, useJsonOutput)
    {
    }

    /// <summary>
    ///     组织默认的审核要求
    ///     由配置生成的规则与配置审核的检查项组成
    /// </summary>
    /// <param name="language">语言</param>
    /// <param name="entityRule">实体型的配置规则</param>
    /// <param name="explicitlyRule">显式关联型的配置规则</param>
    /// <param name="implicitRule">隐式关联型的配置规则</param>
    /// <returns>默认的审核要求</returns>
    private static string BuildDefaultReviewRule(ELanguage language, string entityRule, string explicitlyRule,
        string implicitRule)
    {
        //审核时以配置生成的规则作为审核依据
        var generateRule = new ObjectDataModelGenerateRule(language, entityRule, explicitlyRule, implicitRule)
            .ToRuleString();
        var reviewRuleBuilder = new StringBuilder(generateRule);
        //追加审核的检查项
        reviewRuleBuilder.AppendLine("## 审核检查项");
        reviewRuleBuilder.AppendLine($"{DefaultPrompts.GetDefaultOdmReviewCheckItem()}");
        return reviewRuleBuilder.ToString();
    }
}
