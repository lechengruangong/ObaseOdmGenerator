/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：ODM生成事件.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-8-19 16:52:22
└──────────────────────────────────────────────────────────────┘
*/

using Microsoft.Agents.AI.Workflows;

namespace Obase.OdmGenerator.WorkFlow.Event;

/// <summary>
///     ODM生成事件
/// </summary>
public class ObjectDataModelGenerateEvent : WorkflowEvent
{
    /// <summary>
    ///     初始化ODM生成事件
    /// </summary>
    /// <param name="message">事件消息</param>
    public ObjectDataModelGenerateEvent(string message) : base(message)
    {
    }
}