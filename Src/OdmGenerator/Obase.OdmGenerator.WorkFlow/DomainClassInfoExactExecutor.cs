/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：领域信息抽取执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-8-19 17:00:19
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Obase.OdmGenerator.Agent;
using Obase.OdmGenerator.Agent.Analyzer;
using Obase.OdmGenerator.Agent.Config;
using Obase.OdmGenerator.WorkFlow.Common;
using Obase.OdmGenerator.WorkFlow.Dto;
using Obase.OdmGenerator.WorkFlow.Event;

namespace Obase.OdmGenerator.WorkFlow;

/// <summary>
///     领域信息抽取执行器
/// </summary>
public class DomainClassInfoExactExecutor : Executor<DomainClassExtractInput, List<DomainClassExtractResult>>
{
    /// <summary>
    ///     领域信息抽取工作代理
    /// </summary>
    private readonly DomainClassInfoExactAgent _agent;

    /// <summary>
    ///     领域信息抽取代理的会话
    /// </summary>
    private AgentSession _agentSession;

    /// <summary>
    ///     初始化领域信息抽取执行器
    /// </summary>
    /// <param name="domainClassInfos">领域类信息</param>
    /// <param name="knowledge">如何确定领域类型的知识</param>
    /// <param name="config">LLM配置</param>
    public DomainClassInfoExactExecutor(List<DomainClassInfo> domainClassInfos, IApikeyConfiguration config,
        string knowledge = null) : base(
        nameof(DomainClassInfoExactExecutor))
    {
        //组织提示词
        var output = new StringBuilder("输出格式为Json数组,每个提取出来的领域信息对应一个Json对象,最终组成一个Json数组,以下为每个Json对象的格式:");
        output.AppendLine("{");
        output.AppendLine("    \"type\": <类型,如果是实体型此值为0,如果是显式关联型此值为1,如果是隐式关联型此值为2>,");
        output.AppendLine("    \"className\": <类名,如果是实体型或者显式关联型此处为类型的名称,如果是隐式关联型此处为null>, ");
        output.AppendLine("    \"identity\": <主键,一个字符串数组,如果是实体型此处为所有主键的名称组成的数组,例如[\"Id\",\"Code\"],否则此处为null>, ");
        output.AppendLine("    \"endName1\": <参与隐式关联的类名1,如果是隐式关联型此处为参与隐式关联的其中一个类名,否则此处为null>,");
        output.AppendLine("    \"endName2\": <参与隐式关联的类名2,如果是隐式关联型此处为参与隐式关联的另外一个类名,否则此处为null>");
        output.AppendLine("}");
        output.AppendLine("必须严格遵守如下输出要求:");
        output.AppendLine("1. 只输出一个Json数组,不要输出任何解释、说明或其他无关文字,不要使用Markdown代码块(```或```json)包裹输出内容;");
        output.AppendLine("2. 数组中的每个对象都必须严格符合上述格式,字段名与格式示例完全一致,不要添加、删除或重命名字段;");
        output.AppendLine("3. 字符串值必须使用英文双引号(\")包裹,identity必须输出为字符串数组,例如[\"Id\",\"Code\"],不要输出为单个字符串;");
        output.AppendLine("4. 不要输出任何注释,不要输出尾随逗号,type为数字不要使用引号包裹,无值的字段必须输出null;");
        output.AppendLine("5. 最终输出必须是一个可以被标准Json解析器直接解析的Json数组,不需要任何额外的说明.");
        //创建领域类预处理器代理
        _agent = new DomainClassInfoExactAgent(domainClassInfos, config, knowledge, output.ToString());
    }

    /// <summary>
    ///     所使用的领域信息抽取工作代理
    /// </summary>
    public DomainClassInfoExactAgent Agent => _agent;

    /// <summary>
    ///     处理方法
    /// </summary>
    /// <param name="input">领域信息提取输入</param>
    /// <param name="context">工作流上下文</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>处理结果</returns>
    public override async ValueTask<List<DomainClassExtractResult>> HandleAsync(DomainClassExtractInput input,
        IWorkflowContext context,
        CancellationToken cancellationToken = new())
    {
        //发布事件
        await context.AddEventAsync(new ObjectDataModelGenerateEvent("领域信息抽取开始执行."), cancellationToken);

        //没有创建会话就创建一个会话
        _agentSession ??= await _agent.CreateSessionAsync(cancellationToken);

        //如果有反馈信息,则将反馈信息附加到问题中,以便智能体根据反馈修正提取结果
        var question = new StringBuilder();

        question.AppendLine("分析领域内的领域类型,输出具体的分析结果.");

        if (!string.IsNullOrEmpty(input.FeedBack))
        {
            question = new StringBuilder("之前执行的分析领域内的领域类型任务的反馈如下,请进行相应的修正使结果更准确:");
            question.AppendLine($"{input.FeedBack}");
        }

        //用同一个会话来运行任务
        var result = await _agent.RunAsync(question.ToString(), _agentSession, null, cancellationToken);

        //处理Json字符串
        var json = AgentOutputHelper.DeserializeJsonArray<DomainClassExtractResult>(result.Text);

        //发布事件
        await context.AddEventAsync(new ObjectDataModelGenerateEvent("领域信息抽取执行完成."), cancellationToken);

        return json;
    }
}