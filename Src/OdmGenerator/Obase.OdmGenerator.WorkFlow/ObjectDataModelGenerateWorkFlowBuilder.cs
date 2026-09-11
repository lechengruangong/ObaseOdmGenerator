/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：Obase的ODM模型生成工作流建造器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-8-19 17:35:35
└──────────────────────────────────────────────────────────────┘
*/


using System;
using System.Collections.Generic;
using Microsoft.Agents.AI.Workflows;
using Obase.OdmGenerator.Agent.Analyzer;
using Obase.OdmGenerator.Agent.Config;
using Obase.OdmGenerator.WorkFlow.Dto;

namespace Obase.OdmGenerator.WorkFlow;

/// <summary>
///     Obase的ODM模型生成工作流建造器
/// </summary>
public static class ObjectDataModelGenerateWorkFlowBuilder
{
    /// <summary>
    ///     获取使用默认排布和默认执行器的工作流建造器
    /// </summary>
    /// <param name="path">领域类代码所在路径</param>
    /// <param name="language">编程语言</param>
    /// <param name="configuration">LLM配置</param>
    /// <returns>工作流建造器</returns>
    public static WorkflowBuilder GetDefaultWorkflowBuilder(string path, ELanguage language,
        IApikeyConfiguration configuration)
    {
        var analyzer = DomainInfoAnalyzerFactory.CreateAnalyzer(path, language);
        return GetDefaultWorkflowBuilder(analyzer, language, configuration);
    }

    /// <summary>
    ///     获取使用默认排布和默认执行器的工作流建造器
    /// </summary>
    /// <param name="analyzer">领域类信息分析器</param>
    /// <param name="language">编程语言</param>
    /// <param name="configuration">LLM配置</param>
    /// <returns>工作流建造器</returns>
    public static WorkflowBuilder GetDefaultWorkflowBuilder(IDomainInfoAnalyzer analyzer, ELanguage language,
        IApikeyConfiguration configuration)
    {
        //抽取领域信息
        var infos = analyzer.Analyze();

        //默认排布 执行器均不指定 使用默认构造的执行器
        return GetWorkFlowWithExecutor(domainClassInfos: infos, language: language, configuration: configuration);
    }

    /// <summary>
    ///     使用自定义的执行器获取默认排布工作流建造器
    /// </summary>
    /// <param name="exactExecutor">领域信息抽取执行器,不指定时使用默认的执行器</param>
    /// <param name="exactReviewExecutor">领域信息审核执行器,不指定时使用默认的执行器</param>
    /// <param name="reExactExecutor">领域信息再抽取执行器,不指定时使用默认的执行器</param>
    /// <param name="genExecutor">ODM生成执行器,不指定时使用默认的执行器</param>
    /// <param name="genReviewExecutor">ODM生成审核执行器,不指定时使用默认的执行器</param>
    /// <param name="reGenExecutor">ODM再生成执行器,不指定时使用默认的执行器</param>
    /// <param name="outputExecutor">输出执行器,不指定时使用默认的执行器</param>
    /// <param name="domainClassInfos">领域类信息,不指定的执行器需要使用此项构造默认的执行器</param>
    /// <param name="language">编程语言,不指定的执行器需要使用此项构造默认的执行器</param>
    /// <param name="configuration">LLM配置,不指定的执行器需要使用此项构造默认的执行器</param>
    /// <returns></returns>
    public static WorkflowBuilder GetWorkFlowWithExecutor(
        DomainClassInfoExactExecutor exactExecutor = null,
        DomainClassInfoReviewExecutor exactReviewExecutor = null,
        DomainClassInfoReExactExecutor reExactExecutor = null, ObjectDataModelGenerateExecutor genExecutor = null,
        ObjectDataModelReviewExecutor genReviewExecutor = null,
        ObjectDataModelReGenerateExecutor reGenExecutor = null, OutputExecutor outputExecutor = null,
        List<DomainClassInfo> domainClassInfos = null, ELanguage language = ELanguage.CSharp,
        IApikeyConfiguration configuration = null)
    {
        //不指定的执行器需要使用领域类信息与LLM配置构造默认的执行器 故这两项不可为空
        if (domainClassInfos == null || domainClassInfos.Count == 0)
            throw new ArgumentNullException(nameof(domainClassInfos), "领域类信息集合不可为空.");

        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration), "LLM配置不可为空.");

        //未指定的执行器使用默认的构造方法构造
        exactExecutor ??= new DomainClassInfoExactExecutor(domainClassInfos, configuration);
        exactReviewExecutor ??= new DomainClassInfoReviewExecutor(domainClassInfos, configuration);
        reExactExecutor ??= new DomainClassInfoReExactExecutor();
        genExecutor ??= new ObjectDataModelGenerateExecutor(domainClassInfos, configuration, language);
        genReviewExecutor ??= new ObjectDataModelReviewExecutor(domainClassInfos, configuration, language);
        reGenExecutor ??= new ObjectDataModelReGenerateExecutor();
        outputExecutor ??= new OutputExecutor();

        //组合工作流
        //输入 → 抽取
        var workflowBuilder = new WorkflowBuilder(exactExecutor)
            //抽取 → 审核
            .AddEdge(exactExecutor, exactReviewExecutor)
            //审核不通过 → 重新抽取
            .AddEdge(exactReviewExecutor, reExactExecutor, (DomainClassReviewResult result) => !result.IsValid)
            //重新生成 → 抽取
            .AddEdge(reExactExecutor, exactExecutor)
            //通过 → 生成ODM配置
            .AddEdge(exactReviewExecutor, genExecutor, (DomainClassReviewResult result) => result.IsValid)
            //生成ODM配置 → 审核
            .AddEdge(genExecutor, genReviewExecutor)
            //审核不通过 → 重新生成
            .AddEdge(genReviewExecutor, reGenExecutor, (ObjectDataModelConfigReviewResult result) => !result.IsValid)
            //重新生成 → 生成
            .AddEdge(reGenExecutor, genExecutor)
            //通过 → 输出
            .AddEdge(genReviewExecutor, outputExecutor, (ObjectDataModelConfigReviewResult result) => result.IsValid)
            //指定输出来源
            .WithOutputFrom(outputExecutor);

        return workflowBuilder;
    }
}