/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：基于领域类信息进行处理的代理基类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-8-19 12:11:58
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.ClientModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Obase.OdmGenerator.Agent.Analyzer;
using Obase.OdmGenerator.Agent.Config;
using OpenAI;
using OpenAI.Chat;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace Obase.OdmGenerator.Agent;

/// <summary>
///     基于领域类信息进行处理的代理基类
/// </summary>
public abstract class BaseDomainClassInfoProcessAgent
{
    /// <summary>
    ///     域类信息
    /// </summary>
    private readonly List<DomainClassInfo> _domainClassInfos;

    /// <summary>
    ///     代理
    /// </summary>
    private AIAgent _agent;

    /// <summary>
    ///     代理建造器
    /// </summary>
    private readonly AIAgentBuilder _builder;

    /// <summary>
    ///     初始化基于领域类信息进行处理的代理基类
    /// </summary>
    /// <param name="domainClassInfos">领域类信息集合</param>
    /// <param name="config">LLM配置</param>
    /// <param name="desc">Agent描述</param>
    /// <param name="knowledge">Agent知识</param>
    /// <param name="ability">Agent能力</param>
    /// <param name="output">Agent输出</param>
    /// <param name="tools">MCP工具集合</param>
    /// <param name="useJsonOutput">是否使用Json输出</param>
    protected BaseDomainClassInfoProcessAgent(List<DomainClassInfo> domainClassInfos, IApikeyConfiguration config,
        string desc, string knowledge = null, string ability = null, string output = null, AITool[] tools = null,
        bool useJsonOutput = true)
    {
        if (domainClassInfos?.Count == 0)
            throw new ArgumentNullException(nameof(domainClassInfos), "领域类信息集合不可为空.");

        if (config == null)
            throw new ArgumentNullException(nameof(config), "LLM配置不可为空.");

        if (string.IsNullOrEmpty(config.Endpoint) || string.IsNullOrEmpty(config.ModelId) ||
            string.IsNullOrEmpty(config.ApiKey))
            throw new ArgumentException("LLM配置的Endpoint,ModelId,ApiKey均不可为空.");

        //域类信息
        _domainClassInfos = domainClassInfos;

        //创建一个OpenAI API客户端 作为基础的语言模型能力提供者
        var openAiClient = new OpenAIClient(new ApiKeyCredential(config.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri(config.Endpoint)
            });

        //加入提示词
        var instructions = new StringBuilder();
        if (!string.IsNullOrEmpty(desc))
        {
            instructions.AppendLine("## 定位");
            instructions.AppendLine(desc);
        }

        if (!string.IsNullOrEmpty(knowledge))
        {
            instructions.AppendLine("## 知识");
            instructions.AppendLine(knowledge);
        }

        if (!string.IsNullOrEmpty(ability))
        {
            instructions.AppendLine("## 能力");
            instructions.AppendLine(ability);
        }

        if (!string.IsNullOrEmpty(output))
        {
            instructions.AppendLine("## 输出");
            instructions.AppendLine(output);
        }

        //默认的MCP工具 根据属性名称查询对应的注释工具 以及根据类名称查询类的注释的工具 协助智能体更好地理解领域类的含义
        var toolList = new List<AITool>
            { AIFunctionFactory.Create(GetPropertyComment), AIFunctionFactory.Create(GetClassComment) };
        if (tools?.Length > 0)
            toolList.AddRange(tools);

        //构造一个建造器 综合使用RAG和MCP工具来判断领域类的领域类型
        _builder = openAiClient.GetChatClient(config.ModelId).AsAIAgent(new ChatClientAgentOptions
        {
            ChatOptions = new ChatOptions
            {
                //提示词
                Instructions = instructions.ToString(),
                //AI工具
                Tools = toolList,
                //设置无schema的Json输出 以兼容DeepSeek等只支持JsonObject输出模式的模型 否则使用普通Text输出
                ResponseFormat = useJsonOutput ? new ChatResponseFormatJson(null) : new ChatResponseFormatText()
            }
        }).AsBuilder();

        //向建造器重增加查询领域类信息的RAG
        _builder.UseAIContextProviders(new TextSearchProvider(SearchDomainClassInfoAsync, new TextSearchProviderOptions
        {
            //SearchAsync为RAG的具体提供者
            SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke,
            FunctionToolDescription = "查询领域类信息的工具",
            FunctionToolName = "SearchDomainClassInfo"
        }));
    }

    /// <summary>
    ///     代理
    /// </summary>
    private AIAgent Agent => _agent ??= _builder.Build();

    /// <summary>
    ///     领域类信息查询RAG工具:根据查询文本查询领域类信息集合
    /// </summary>
    /// <param name="query">查询文本</param>
    /// <param name="token">取消Token</param>
    /// <returns>RAG结果</returns>
    private Task<IEnumerable<TextSearchProvider.TextSearchResult>> SearchDomainClassInfoAsync(string query,
        CancellationToken token)
    {
        var results = new List<TextSearchProvider.TextSearchResult>();

        foreach (var item in _domainClassInfos)
        {
            var refs = item.ReferencedTypes.Count > 0
                ? string.Join(". ", item.ReferencedTypes.Select(p => $"属性类型:{p.Item1},属性名称:{p.Item2}"))
                : "没有";
            var props = item.PropertyList.Count > 0
                ? string.Join(". ", item.PropertyList.Select(p => $"属性类型:{p.Item1},属性名称:{p.Item2}"))
                : "没有";
            var classComment = string.IsNullOrEmpty(item.ClassComment) ? "没有" : item.ClassComment;

            results.Add(new TextSearchProvider.TextSearchResult
            {
                SourceName = "域类信息集合",
                Text =
                    $"类名:{item.ClassName},类的注释:{classComment},引用的其他领域类型列表:[{string.Join(" ", refs)}],自身属性列表:[{string.Join(" ", props)}]."
            });
        }

        return Task.FromResult(results.AsEnumerable());
    }

    /// <summary>
    ///     MCP工具:根据领域类名称属性名称查询属性的注释
    /// </summary>
    /// <param name="className">领域类名称</param>
    /// <param name="propName">要查询的属性名称</param>
    /// <returns>属性的注释 </returns>
    [Description("根据领域类名称和属性名称查询属性的注释工具.")]
    private string GetPropertyComment([Description("要查询的领域类名称.")] string className,
        [Description("要查询的属性名称.")] string propName)
    {
        var cla = _domainClassInfos.FirstOrDefault(p => p.ClassName == className);
        if (cla == null)
            return "无此领域类型.";
        var prop = cla.PropertyComments.TryGetValue(propName, out var comment);
        return prop ? comment : "无此属性的注释.";
    }

    /// <summary>
    ///     MCP工具:根据领域类名称查询类的注释
    /// </summary>
    /// <param name="className">领域类名称</param>
    /// <returns>类的注释</returns>
    [Description("根据领域类名称查询类的注释工具.")]
    private string GetClassComment([Description("要查询的领域类名称.")] string className)
    {
        var cla = _domainClassInfos.FirstOrDefault(p => p.ClassName == className);
        if (cla == null)
            return "无此领域类型.";
        return string.IsNullOrEmpty(cla.ClassComment) ? "无此类的注释." : cla.ClassComment;
    }


    /// <summary>
    ///     创建一个新的Agent会话
    /// </summary>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>一个新的Agent会话</returns>
    public async Task<AgentSession> CreateSessionAsync(CancellationToken cancellationToken = default)
    {
        return await Agent.CreateSessionAsync(cancellationToken);
    }

    /// <summary>
    ///     执行一次与LLM的交互
    /// </summary>
    /// <param name="input">输入问题</param>
    /// <param name="session">Agent会话</param>
    /// <param name="options">运行选项</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>Agent响应</returns>
    public async Task<AgentResponse> RunAsync(string input, AgentSession session = null, AgentRunOptions options = null,
        CancellationToken cancellationToken = default)
    {
        return await Agent.RunAsync(input, session, options, cancellationToken);
    }

    /// <summary>
    ///     注册执行中间件
    /// </summary>
    /// <param name="runFunc">普通执行中间件委托</param>
    /// <param name="runStreamingFunc">流式执行中间件委托</param>
    /// <returns>自身</returns>
    public BaseDomainClassInfoProcessAgent Use(
        Func<IEnumerable<ChatMessage>, AgentSession, AgentRunOptions, AIAgent, CancellationToken, Task<AgentResponse>>
            runFunc,
        Func<IEnumerable<ChatMessage>, AgentSession, AgentRunOptions, AIAgent, CancellationToken,
            IAsyncEnumerable<AgentResponseUpdate>> runStreamingFunc)
    {
        _builder.Use(runFunc, runStreamingFunc);
        return this;
    }

    /// <summary>
    ///     注册工具调用中间件
    /// </summary>
    /// <param name="callback">工具调用中间件</param>
    /// <returns>自身</returns>
    public BaseDomainClassInfoProcessAgent Use(
        Func<AIAgent, FunctionInvocationContext, Func<FunctionInvocationContext, CancellationToken, ValueTask<object>>,
            CancellationToken, ValueTask<object>> callback)
    {
        _builder.Use(callback);
        return this;
    }

    /// <summary>
    ///     注册日志工厂
    /// </summary>
    /// <param name="loggerFactory">日志工厂</param>
    /// <param name="configure">日志配置委托</param>
    /// <returns>自身</returns>
    public BaseDomainClassInfoProcessAgent UseLogging(ILoggerFactory loggerFactory,
        Action<LoggingAgent> configure = null)
    {
        _builder.UseLogging(loggerFactory, configure);
        return this;
    }
}