using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Agents.AI.Workflows;
using Obase.OdmGenerator.Agent.Analyzer;
using Obase.OdmGenerator.Test.Configuration;
using Obase.OdmGenerator.WorkFlow;
using Obase.OdmGenerator.WorkFlow.Dto;
using Obase.OdmGenerator.WorkFlow.Event;

namespace Obase.OdmGenerator.Test;

/// <summary>
///     默认工作流的测试
/// </summary>
[TestFixture]
public class DefaultWorkFlowTest
{
    /// <summary>
    ///     测试流式方法
    /// </summary>
    /// <returns>无</returns>
    [Test]
    public async ValueTask StreamingTest()
    {
        //读取配置
        var path = ConfigurationManager.DomainClassPath;
        var configuration = ConfigurationManager.ApikeyLlmConfiguration;

        //获取具体的建造器
        var workFlowBuilder =
            ObjectDataModelGenerateWorkFlowBuilder.GetDefaultWorkflowBuilder(path, ELanguage.CSharp, configuration);
        //建造工作流
        var workFlow = workFlowBuilder.Build();

        var results = new List<ObjectDataModelConfigResult>();

        //流式执行
        var run = await InProcessExecution.RunStreamingAsync(workFlow, new DomainClassExtractInput());
        await foreach (var evt in run.WatchStreamAsync())
        {
            if (evt is ObjectDataModelGenerateEvent odmGenEvent) Console.WriteLine($"{odmGenEvent.Data}");

            if (evt is WorkflowOutputEvent outputEvt)
            {
                results = outputEvt.As<List<ObjectDataModelConfigResult>>();
                foreach (var result in results) Console.WriteLine($"{result}");
            }
        }

        //校验结果
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Any(), Is.True);
    }

    /// <summary>
    ///     测试普通方法
    /// </summary>
    /// <returns>无</returns>
    [Test]
    public async ValueTask Test()
    {
        //读取配置
        var path = ConfigurationManager.DomainClassPath;
        var configuration = ConfigurationManager.ApikeyLlmConfiguration;

        var analyzer = new DotNetDomainInfoAnalyzer(path);

        //获取具体的建造器
        var workFlowBuilder =
            ObjectDataModelGenerateWorkFlowBuilder.GetDefaultWorkflowBuilder(analyzer, ELanguage.CSharp, configuration);
        //建造工作流
        var workFlow = workFlowBuilder.Build();

        var results = new List<ObjectDataModelConfigResult>();

        //普通执行
        var run = await InProcessExecution.RunAsync(workFlow, new DomainClassExtractInput());
        foreach (var evt in run.NewEvents)
        {
            if (evt is ObjectDataModelGenerateEvent odmGenEvent) Console.WriteLine($"{odmGenEvent.Data}");

            if (evt is WorkflowOutputEvent outputEvt)
            {
                results = outputEvt.As<List<ObjectDataModelConfigResult>>();
                foreach (var result in results) Console.WriteLine($"{result}");
            }
        }

        //校验结果
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Any(), Is.True);
    }
}