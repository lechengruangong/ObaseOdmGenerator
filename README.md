# ObaseOdmGenerator
Obase 的 ODM 配置生成器，本生成器借助 LLM 完成 Obase 对象数据模型基础配置的生成。

## 已有功能

- 读取以 C# 语言编写的领域类，生成领域类的引用关系与自身属性。
- 根据领域类的引用关系与自身属性，判断其应归属的对象数据模型具体类型。
- 生成 .NET 平台下 Obase 对象数据模型的基础配置。

## 如何使用

### 引用 NuGet 包

在项目中引入如下包引用：
```
<PackageReference Include="Obase.OdmGenerator.WorkFlow" Version="X.X.X" />
```
此处的版本号可根据实际需要确定，一般情况下选择最新版本即可。

### 获取默认工作流

使用以下代码获取默认的工作流建造器：
```
//获取具体的建造器 传入领域类代码路径 编程语言 LLM配置
var workFlowBuilder =
    ObjectDataModelGenerateWorkFlowBuilder.GetDefaultWorkflowBuilder(path, ELanguage.CSharp, configuration);
```
该建造器为 Microsoft Agent Framework 框架中的工作流建造器，之后只需建造工作流并运行即可：
```
//建造工作流
var workFlow = workFlowBuilder.Build();
```
随后可以按普通方式执行，执行过程中会抛出 ObjectDataModelGenerateEvent 事件，并在最终结果中输出 ObjectDataModelConfigResult 集合：
```
//最终结果
var results = new List<ObjectDataModelConfigResult>();
//普通执行
var run = await InProcessExecution.RunAsync(workFlow, new DomainClassExtractInput());
//执行过程中的事件集合
foreach (var evt in run.NewEvents)
{
    //ODM生成事件
    if (evt is ObjectDataModelGenerateEvent odmGenEvent) Console.WriteLine($"{odmGenEvent.Data}");
    //最终结果
    if (evt is WorkflowOutputEvent outputEvt)
    {
        results = outputEvt.As<List<ObjectDataModelConfigResult>>();
        foreach (var result in results) Console.WriteLine($"{result}");
    }
}
```
也可以采用流式执行，抛出的事件与最终结果与普通执行相同：
```
//最终结果
var results = new List<ObjectDataModelConfigResult>();
//流式执行
var run = await InProcessExecution.RunStreamingAsync(workFlow, new DomainClassExtractInput());
//执行过程中的事件集合
await foreach (var evt in run.WatchStreamAsync())
{
    //ODM生成事件
    if (evt is ObjectDataModelGenerateEvent odmGenEvent) Console.WriteLine($"{odmGenEvent.Data}");
    //最终结果
    if (evt is WorkflowOutputEvent outputEvt)
    {
        results = outputEvt.As<List<ObjectDataModelConfigResult>>();
        foreach (var result in results) Console.WriteLine($"{result}");
    }
}
```

### 生成配置对象
最终获取到的 ObjectDataModelConfigResult 集合中，每个对象即为生成的一项配置，具有以下属性：
- Type，类型：0 - 实体型；1 - 显式关联型；2 - 隐式关联型。
- ConfigurationCode，配置代码。

## 计划增加

1. 读取以 Java 语言编写的领域类，生成领域类的引用关系与自身属性。
2. 生成 Java 平台下 Obase 对象数据模型的基础配置。
3. 增加对继承等配置的生成支持。
4. 增加对不同命名空间下同名类，以及单个文件内定义多个类的支持。

## 如何扩展

### 自定义执行器

- 通过 ObjectDataModelGenerateWorkFlowBuilder.GetDefaultWorkflowBuilder 方法获取的工作流建造器，其使用的执行器均由默认构造函数初始化。若希望沿用默认的工作流排布、同时自定义某些属性，可以使用 ObjectDataModelGenerateWorkFlowBuilder.GetWorkFlowWithExecutor 传入自定义参数构造的执行器，来获取保持默认排布的工作流建造器。
- 执行器 DomainClassInfoExactExecutor、DomainClassInfoReviewExecutor、ObjectDataModelGenerateExecutor、ObjectDataModelReviewExecutor 均提供 Agent 属性访问器，可在构造后使用该访问器的 Use、UseLogging 方法注册中间件与日志工厂，以协助调试。

### 自定义工作流

- 使用 ObjectDataModelGenerateWorkFlowBuilder 中方法获得的 Microsoft Agent Framework 框架的 WorkflowBuilder 可以自定义工作流的排布，具体请参考相关文档。
- 直接使用Microsoft Agent Framework 框架的 WorkflowBuilder 来定义工作流的排布，具体请参考相关文档。

### 扩展领域信息解析器

- 执行器 DomainClassInfoExactExecutor、DomainClassInfoReviewExecutor、ObjectDataModelGenerateExecutor、ObjectDataModelReviewExecutor 在初始化时需要传入 DomainClassInfo 列表，合理构造该对象中的各个属性即可向 LLM 提供领域信息。因此可以自行实现解析器，在构造执行器时将 DomainClassInfo 列表传入即可。
