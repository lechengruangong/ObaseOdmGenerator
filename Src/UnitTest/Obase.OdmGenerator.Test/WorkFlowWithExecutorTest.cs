using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Obase.OdmGenerator.Agent.Analyzer;
using Obase.OdmGenerator.Test.Configuration;
using Obase.OdmGenerator.WorkFlow;
using Obase.OdmGenerator.WorkFlow.Dto;
using Obase.OdmGenerator.WorkFlow.Event;

namespace Obase.OdmGenerator.Test;

/// <summary>
///     自定义执行器的工作流的测试
/// </summary>
[TestFixture]
public class WorkFlowWithExecutorTest
{
    /// <summary>
    ///     测试流式方法
    /// </summary>
    /// <returns>无</returns>
    [Test]
    public async ValueTask Test()
    {
        //读取配置
        var path = ConfigurationManager.DomainClassPath;
        var configuration = ConfigurationManager.ApikeyLlmConfiguration;

        //抽取领域信息
        var analyzer = DomainInfoAnalyzerFactory.CreateAnalyzer(path, ELanguage.CSharp);
        var infos = analyzer.Analyze();

        //日志工厂（基于文件的日志，输出到输出目录下的logs文件夹，按天滚动）
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddFile(Path.Combine(logDirectory, "agents-{Date}.log"), minimumLevel:LogLevel.Trace,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}"));

        //各个执行器
        var exactExecutor = new DomainClassInfoExactExecutor(infos, configuration);
        exactExecutor.Agent.UseLogging(loggerFactory);
        exactExecutor.Agent.Use(null, RunStreamingFunc);
        exactExecutor.Agent.Use(Callback);

        var exactReviewExecutor = new DomainClassInfoReviewExecutor(infos, configuration);
        exactReviewExecutor.Agent.UseLogging(loggerFactory);
        exactReviewExecutor.Agent.Use(null, RunStreamingFunc);

        var reExactExecutor = new DomainClassInfoReExactExecutor();

        var genExecutor = new ObjectDataModelGenerateExecutor(infos, configuration, ELanguage.CSharp);
        genExecutor.Agent.UseLogging(loggerFactory);
        genExecutor.Agent.Use(null, RunStreamingFunc);

        var genReviewExecutor = new ObjectDataModelReviewExecutor(infos, configuration, ELanguage.CSharp);
        genReviewExecutor.Agent.UseLogging(loggerFactory);
        genReviewExecutor.Agent.Use(null, RunStreamingFunc);

        var reGenExecutor = new ObjectDataModelReGenerateExecutor();

        var outputExecutor = new OutputExecutor();

        //获取具体的建造器
        var workFlowBuilder =
            ObjectDataModelGenerateWorkFlowBuilder.GetWorkFlowWithExecutor(exactExecutor, exactReviewExecutor,
                reExactExecutor, genExecutor, genReviewExecutor, reGenExecutor, outputExecutor,
                infos, ELanguage.CSharp, configuration);

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
    ///     流式执行中间件
    /// </summary>
    /// <param name="messages">当前轮之前的消息</param>
    /// <param name="session">会话</param>
    /// <param name="options">选项</param>
    /// <param name="innerAgent">内部Agent</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>下一步</returns>
    private IAsyncEnumerable<AgentResponseUpdate> RunStreamingFunc(IEnumerable<ChatMessage> messages,
        AgentSession session, AgentRunOptions options, AIAgent innerAgent, CancellationToken cancellationToken)
    {
        //记录对话
        var chatMessages = messages as ChatMessage[] ?? messages.ToArray();
        Console.WriteLine($"[MAF.ChatMessage] {string.Join(Environment.NewLine, chatMessages.Select(p => p.Text))}");
        var response = innerAgent.RunStreamingAsync(chatMessages, session, options, cancellationToken);

        return response;
    }

    /// <summary>
    ///     Tools调用中间件
    /// </summary>
    /// <param name="innerAgent">内部Agent</param>
    /// <param name="context">上下文</param>
    /// <param name="next">下一节</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>结果</returns>
    private async ValueTask<object> Callback(AIAgent innerAgent, FunctionInvocationContext context,
        Func<FunctionInvocationContext, CancellationToken, ValueTask<object>> next, CancellationToken cancellationToken)
    {
        //记录工具调用
        Console.WriteLine($"[MAF.Function] ▶ {context.Function.Name}(...) | iter={context.Iteration}");
        var result = await next(context, cancellationToken);
        Console.WriteLine($"[MAF.Function] ◀ {context.Function.Name} => ...");
        return result;
    }
}