/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：领域信息重新抽取执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-8-19 17:02:57
└──────────────────────────────────────────────────────────────┘
*/

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI.Workflows;
using Obase.OdmGenerator.WorkFlow.Dto;
using Obase.OdmGenerator.WorkFlow.Event;

namespace Obase.OdmGenerator.WorkFlow;

/// <summary>
///     领域信息重新抽取执行器
/// </summary>
public class DomainClassInfoReExactExecutor : Executor<DomainClassReviewResult, DomainClassExtractInput>
{
    /// <summary>
    ///     不通过时 最大的重试次数
    /// </summary>
    private readonly int _maxRetry;

    /// <summary>
    ///     当前的重试次数
    /// </summary>
    private int _currentRetry;

    /// <summary>
    ///     初始化领域信息重新抽取执行器
    /// </summary>
    /// <param name="maxRetry">不通过时 最大的重试次数</param>
    public DomainClassInfoReExactExecutor(int maxRetry = 3) : base(nameof(DomainClassInfoReExactExecutor))
    {
        _maxRetry = maxRetry;
    }

    /// <summary>
    ///     处理方法
    /// </summary>
    /// <param name="input">重新抽取输入</param>
    /// <param name="context">工作流上下文</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>处理结果</returns>
    public override async ValueTask<DomainClassExtractInput> HandleAsync(DomainClassReviewResult input,
        IWorkflowContext context,
        CancellationToken cancellationToken = new())
    {
        //发布事件
        await context.AddEventAsync(new ObjectDataModelGenerateEvent("领域信息重新抽取开始执行."), cancellationToken);

        //检查是否超过最大重试次数
        if (_currentRetry < _maxRetry)
        {
            _currentRetry++;
        }
        else
        {
            await context.AddEventAsync(
                new ObjectDataModelGenerateEvent("无法生成符合规则的领域信息抽取结果,已超过最大重试次数,请调整领域信息的抽取规则或重试次数."),
                cancellationToken);
            await context.RequestHaltAsync();
        }

        //发布事件
        await context.AddEventAsync(
            new ObjectDataModelGenerateEvent($"领域信息重新抽取输入已构建,不通过原因为{input.ReviewMessage},当前重试次数为{_currentRetry}次."),
            cancellationToken);

        //重新组织输入
        var result = new ValueTask<DomainClassExtractInput>(new DomainClassExtractInput
        {
            FeedBack = input.ReviewMessage
        });

        //发布事件
        await context.AddEventAsync(new ObjectDataModelGenerateEvent("领域信息重新抽取执行完成."), cancellationToken);

        return await result;
    }
}