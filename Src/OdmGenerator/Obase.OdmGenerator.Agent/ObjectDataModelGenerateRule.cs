/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象数据模型生成规则.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-8-19 15:37:32
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Text;
using Obase.OdmGenerator.Agent.Analyzer;
using Obase.OdmGenerator.Agent.Cons;

namespace Obase.OdmGenerator.Agent;

/// <summary>
///     对象数据模型生成规则
/// </summary>
internal sealed class ObjectDataModelGenerateRule
{
    /// <summary>
    ///     实体型的配置规则
    /// </summary>
    private readonly string _entityRule;

    /// <summary>
    ///     显式关联型的配置规则
    /// </summary>
    private readonly string _explicitlyRule;

    /// <summary>
    ///     隐式关联型的配置规则
    /// </summary>
    private readonly string _implicitRule;

    /// <summary>
    ///     初始化ODM配置生成规则
    /// </summary>
    /// <param name="language">语言</param>
    /// <param name="entityRule">实体型的配置规则</param>
    /// <param name="explicitlyRule">显式关联型的配置规则</param>
    /// <param name="implicitRule">隐式关联型的配置规则</param>
    public ObjectDataModelGenerateRule(ELanguage language, string entityRule = null, string explicitlyRule = null,
        string implicitRule = null)
    {
        switch (language)
        {
            case ELanguage.CSharp:
                _entityRule = entityRule ?? DefaultPrompts.GetDefaultOdmGenerateCSharpEntityRule();
                _explicitlyRule = explicitlyRule ?? DefaultPrompts.GetDefaultOdmGenerateCSharpExplicitlyRule();
                _implicitRule = implicitRule ?? DefaultPrompts.GetDefaultOdmGenerateCSharpImplicitRule();
                break;
            case ELanguage.Java:
                _entityRule = entityRule ?? DefaultPrompts.GetDefaultOdmGenerateJavaEntityRule();
                _explicitlyRule = explicitlyRule ?? DefaultPrompts.GetDefaultOdmGenerateJavaExplicitlyRule();
                _implicitRule = implicitRule ?? DefaultPrompts.GetDefaultOdmGenerateJavaImplicitRule();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(language), language, "不支持的编程语言.");
        }
    }

    /// <summary>
    ///     转换为字符串
    /// </summary>
    /// <returns>字符串</returns>
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.AppendLine("当用户要求生成Obase的ODM基础配置时,请根据以下规则生成:");
        builder.AppendLine(
            "1. ODM的基础配置包括实体型、显式关联型和隐式关联型三种类型.分别对应规则entity-rule,explicitly-association-rule和implicit-association-rule.");
        builder.AppendLine("2. 需要读取具体的资源来获取配置规则后再根据具体的规则和步骤生成相应的配置.");
        builder.AppendLine(
            "3. ODM的配置默认都是位于Obase的CreateModel配置方法中的,此方法的参数为ModelBuilder类型的modelBuilder,以下的具体配置规则中的建模器或者modelBuilder均指代此参数.");
        builder.AppendLine(
            "4. 在生成的配置最前面,加上配置的类型注释.如果是实体型和显式关联型,则注释内容为实体型或者显式关联型 + 类名 + 配置;如果是隐式关联型,则注释内容为 类名1 + 和 + 类型2 + 之间的关系配置.");
        builder.AppendLine("## 规则entity-rule");
        builder.AppendLine($"{_entityRule}");
        builder.AppendLine("## 规则explicitly-association-rule");
        builder.AppendLine($"{_explicitlyRule}");
        builder.AppendLine("## 规则implicit-association-rule");
        builder.AppendLine($"{_implicitRule}");
        return builder.ToString();
    }
}