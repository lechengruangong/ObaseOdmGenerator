/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：最终输出执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-8-19 17:23:21
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI.Workflows;
using Obase.OdmGenerator.WorkFlow.Dto;
using Obase.OdmGenerator.WorkFlow.Event;

namespace Obase.OdmGenerator.WorkFlow;

/// <summary>
///     最终输出执行器
/// </summary>
public class OutputExecutor : Executor<ObjectDataModelConfigReviewResult, List<ObjectDataModelConfigResult>>
{
    /// <summary>
    ///     初始化最终输出执行器
    /// </summary>
    public OutputExecutor() : base(nameof(OutputExecutor))
    {
    }

    /// <summary>
    ///     处理方法
    /// </summary>
    /// <param name="input">最终输出输入</param>
    /// <param name="context">工作流上下文</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>处理结果</returns>
    public override async ValueTask<List<ObjectDataModelConfigResult>> HandleAsync(
        ObjectDataModelConfigReviewResult input,
        IWorkflowContext context,
        CancellationToken cancellationToken = new())
    {
        //发布事件
        await context.AddEventAsync(new ObjectDataModelGenerateEvent("最终输出开始执行."), cancellationToken);

        //输出排序
        var result =
            new ValueTask<List<ObjectDataModelConfigResult>>(
                input.OdmConfigurationResults.OrderBy(x => x.Type).ToList());

        //发布事件
        await context.AddEventAsync(new ObjectDataModelGenerateEvent("最终输出执行完成."), cancellationToken);

        return await result;
    }
}