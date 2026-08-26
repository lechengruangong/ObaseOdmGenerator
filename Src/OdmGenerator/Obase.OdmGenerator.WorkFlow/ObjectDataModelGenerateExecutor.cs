/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：ODM配置生成执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-8-19 17:10:39
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
///     ODM配置生成执行器
/// </summary>
public class ObjectDataModelGenerateExecutor : Executor<DomainClassReviewResult, List<ObjectDataModelConfigResult>>
{
    /// <summary>
    ///     ODM配置生成代理
    /// </summary>
    private readonly ObjectDataModelGenerateAgent _agent;

    /// <summary>
    ///     ODM配置生成代理的会话
    /// </summary>
    private AgentSession _agentSession;

    /// <summary>
    ///     初始化ODM配置生成执行器
    /// </summary>
    /// <param name="domainClassInfos">领域类信息</param>
    /// <param name="language">语言</param>
    /// <param name="entityRule">实体型的配置规则</param>
    /// <param name="explicitlyRule">显式关联型的配置规则</param>
    /// <param name="implicitRule">隐式关联型的配置规则</param>
    /// <param name="config">LLM配置</param>
    public ObjectDataModelGenerateExecutor(List<DomainClassInfo> domainClassInfos, IApikeyConfiguration config,
        ELanguage language,
        string entityRule = null, string explicitlyRule = null,
        string implicitRule = null) : base(
        nameof(ObjectDataModelGenerateExecutor))
    {
        //组织提示词
        var output = new StringBuilder("输出格式为Json数组,每个生成的ODM基础配置对应一个Json对象,最终组成一个Json数组,以下为每个ODM基础配置代码的Json对象格式:");
        output.AppendLine("{");
        output.AppendLine("    \"type\": <类型,如果是实体型此值为0,如果是显式关联型此值为1,如果是隐式关联型此值为2>,");
        output.AppendLine("    \"configurationCode\": <配置代码,生成的具体配置代码> ");
        output.AppendLine("}");
        output.AppendLine("必须严格遵守如下输出要求:");
        output.AppendLine("1. 只输出一个Json数组,不要输出任何解释、说明或其他无关文字,不要使用Markdown代码块(```或```json)包裹输出内容;");
        output.AppendLine("2. 数组中的每个对象都必须严格符合上述格式,字段名与格式示例完全一致,不要添加、删除或重命名字段;");
        output.AppendLine("3. 字符串值必须使用英文双引号(\")包裹,configurationCode中的双引号必须转义为\\\",换行符必须表示为\\n;");
        output.AppendLine("4. 不要输出任何注释,不要输出尾随逗号,type为数字不要使用引号包裹,无值的字段必须输出null;");
        output.AppendLine("5. 最终输出必须是一个可以被标准Json解析器直接解析的Json数组,不需要任何额外的说明.");
        //创建领域类预处理器代理
        _agent = new ObjectDataModelGenerateAgent(domainClassInfos, config, language,
            entityRule, explicitlyRule, implicitRule, output.ToString());
    }

    /// <summary>
    ///     所使用的ODM配置生成代理
    /// </summary>
    public ObjectDataModelGenerateAgent Agent => _agent;

    /// <summary>
    ///     处理方法
    /// </summary>
    /// <param name="input">ODM配置生输入</param>
    /// <param name="context">工作流上下文</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>处理结果</returns>
    public override async ValueTask<List<ObjectDataModelConfigResult>> HandleAsync(DomainClassReviewResult input,
        IWorkflowContext context,
        CancellationToken cancellationToken = new())
    {
        //发布事件
        await context.AddEventAsync(new ObjectDataModelGenerateEvent("ODM配置生成开始执行."), cancellationToken);

        //没有创建会话就创建一个会话
        _agentSession ??= await _agent.CreateSessionAsync(cancellationToken);

        StringBuilder question;

        if (!string.IsNullOrEmpty(input.ReviewMessage))
        {
            //重新做
            question = new StringBuilder("之前执行的生成Obase的ODM基础配置任务的反馈如下,请进行相应的修正使结果更准确:");
            question.AppendLine($"{input.ReviewMessage}");
        }
        else
        {
            question = new StringBuilder("请根据以下领域类型分析结果生成Obase的ODM基础配置:").AppendLine();

            //将领域类型分析结果转换为字符串
            foreach (var extractResult in input.DomainClassExtractResults)
                question.AppendLine(extractResult.ToString());
        }

        //运行任务
        var result = await _agent.RunAsync(question.ToString(), _agentSession, null, cancellationToken);

        //处理Json字符串
        var json = AgentOutputHelper.DeserializeJsonArray<ObjectDataModelConfigResult>(result.Text);

        //发布事件
        await context.AddEventAsync(new ObjectDataModelGenerateEvent("ODM配置生成执行完成."), cancellationToken);


        return json;
    }
}