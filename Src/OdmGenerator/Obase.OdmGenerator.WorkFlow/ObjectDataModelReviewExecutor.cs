/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：ODM配置审核执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-8-19 17:18:10
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI.Workflows;
using Obase.OdmGenerator.Agent;
using Obase.OdmGenerator.Agent.Analyzer;
using Obase.OdmGenerator.Agent.Config;
using Obase.OdmGenerator.WorkFlow.Common;
using Obase.OdmGenerator.WorkFlow.Dto;
using Obase.OdmGenerator.WorkFlow.Event;

namespace Obase.OdmGenerator.WorkFlow;

/// <summary>
///     ODM配置审核执行器
/// </summary>
public class
    ObjectDataModelReviewExecutor : Executor<List<ObjectDataModelConfigResult>, ObjectDataModelConfigReviewResult>
{
    /// <summary>
    ///     ODM配置审核代理
    /// </summary>
    private readonly ObjectDataModelReviewAgent _agent;

    /// <summary>
    ///     初始化ODM配置审核执行器
    /// </summary>
    /// <param name="domainClassInfos">领域类信息</param>
    /// <param name="language">语言</param>
    /// <param name="entityRule">实体型的配置规则</param>
    /// <param name="explicitlyRule">显式关联型的配置规则</param>
    /// <param name="implicitRule">隐式关联型的配置规则</param>
    /// <param name="config">LLM配置</param>
    public ObjectDataModelReviewExecutor(List<DomainClassInfo> domainClassInfos, IApikeyConfiguration config,
        ELanguage language,
        string entityRule = null, string explicitlyRule = null,
        string implicitRule = null) : base(nameof(ObjectDataModelReviewExecutor))
    {
        //组织提示词
        var output = new StringBuilder("输出格式为Json格式,以下为Json对象的格式:");
        output.AppendLine("{");
        output.AppendLine("    \"isValid\": <是否相符合,如果相符合输出true,不相符合输出false>,");
        output.AppendLine("    \"reviewMessage\": <审核意见,如果不相符合则输出具体的审核意见,如果相符合则输出null> ");
        output.AppendLine("}");
        output.AppendLine("必须严格遵守如下输出要求:");
        output.AppendLine("1. 只输出一个Json对象,不要输出任何解释、说明或其他无关文字,不要使用Markdown代码块(```或```json)包裹输出内容;");
        output.AppendLine("2. 对象必须严格符合上述格式,字段名与格式示例完全一致,不要添加、删除或重命名字段;");
        output.AppendLine("3. 字符串值必须使用英文双引号(\")包裹,reviewMessage中的双引号必须转义为\\\",换行符必须表示为\\n;");
        output.AppendLine("4. 不要输出任何注释,不要输出尾随逗号,isValid必须输出布尔值true或false,不要使用引号包裹,无值的字段必须输出null;");
        output.AppendLine("5. 最终输出必须是一个可以被标准Json解析器直接解析的Json对象,不需要任何额外的说明.");
        //创建领域类预处理器代理
        _agent = new ObjectDataModelReviewAgent(domainClassInfos, config, language,
            entityRule, explicitlyRule, implicitRule, output.ToString());
    }

    /// <summary>
    ///     所使用的ODM配置审核代理
    /// </summary>
    public ObjectDataModelReviewAgent Agent => _agent;

    /// <summary>
    ///     处理方法
    /// </summary>
    /// <param name="input">ODM配置审核输入</param>
    /// <param name="context">工作流上下文</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>处理结果</returns>
    public override async ValueTask<ObjectDataModelConfigReviewResult> HandleAsync(
        List<ObjectDataModelConfigResult> input,
        IWorkflowContext context,
        CancellationToken cancellationToken = new())
    {
        //发布事件
        await context.AddEventAsync(new ObjectDataModelGenerateEvent("ODM配置审核开始执行."), cancellationToken);

        var question = new StringBuilder("请对以下ODM基础配置结果进行审核,并给出审核意见:").AppendLine();
        foreach (var value in input) question.AppendLine(value.ToString());

        //不用会话来运行任务
        var result = await _agent.RunAsync(question.ToString(), null, null, cancellationToken);

        //处理Json字符串
        var json = AgentOutputHelper.DeserializeJsonObject<ObjectDataModelConfigReviewResult>(result.Text);

        //通过
        if (json.IsValid)
            //通过了 进入输出配置流程
            json.OdmConfigurationResults = input;

        //发布事件
        await context.AddEventAsync(new ObjectDataModelGenerateEvent("ODM配置审核执行完成."), cancellationToken);

        return json;
    }
}