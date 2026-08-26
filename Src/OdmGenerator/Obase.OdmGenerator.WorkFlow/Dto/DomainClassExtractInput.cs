/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：领域信息提取输入.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-8-19 16:48:42
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.OdmGenerator.WorkFlow.Dto;

/// <summary>
///     领域信息提取输入
/// </summary>
public class DomainClassExtractInput
{
    /// <summary>
    ///     针对上次提取结果的反馈信息
    ///     用于指导下一次提取的方向和重点
    /// </summary>
    internal string FeedBack { get; set; }

    /// <summary>
    ///     转换为字符串
    /// </summary>
    /// <returns>字符串</returns>
    public override string ToString()
    {
        return $"{{ \"FeedBack\":\"{FeedBack}\" }}";
    }
}