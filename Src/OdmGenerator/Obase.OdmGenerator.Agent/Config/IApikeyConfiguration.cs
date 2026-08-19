/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：基于ApiKey的LLM接入配置.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-8-19 11:54:40
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.OdmGenerator.Agent.Config;

/// <summary>
///     基于ApiKey的LLM接入配置
/// </summary>
public interface IApikeyConfiguration
{
    /// <summary>
    ///     API KEY
    ///     鉴权凭证
    /// </summary>
    string ApiKey { get; }

    /// <summary>
    ///     API接入点
    /// </summary>
    string Endpoint { get; }

    /// <summary>
    ///     模型ID
    /// </summary>
    public string ModelId { get; }
}