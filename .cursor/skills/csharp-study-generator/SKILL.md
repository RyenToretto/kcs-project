---
name: csharp-study-generator
description: >-
  Generate structured C#/.NET learning modules for kcs-project (00_csharp_basics …
  19_architecture). Produces NN_ dirs, L00_*.cs + L00_*.csproj lessons with Chinese
  comments, RunTests()+Bootstrap()+Main, and scripts/verify_modules.sh. Use when
  initializing or expanding any NN_xxx stage, running full-auto roadmap generation,
  or auditing lessons.
---

# C# Study Generator (`kcs-project`)

为 `kcs-project` 生成可直接运行的 C# 学习课：中文注释、直观版 + 进阶版、`RunTests` + `Bootstrap` + `Main`、`./scripts/verify_modules.sh` 可过。

**终点目标（硬性）**：按顺序学完后，不止可就业 C#/.NET 后端，还要沿精通线达到各方向**顶尖精通**（语法/异步/Web+EF/中间件/测试可观测/生产工程/CLR/云原生/架构）。禁止浅尝辄止的「只贴框架皮毛」。

## 何时启用

- 「按 csharp-study-generator / kcs 学习规范初始化 `00_csharp_basics` / `06_aspnet_core` …」
- 「全自动生成下一阶段 / 把路线图全部做完 / 推进 14→15→16→17→18→19」
- 在 `kcs-project` 内新增编号学习目录或审计课是否合格

## Workflow

1. **读路线图，定目录名与课表**
   - 根 [README.md](../../../README.md)、[references/roadmap.md](references/roadmap.md)、[references/resources.md](references/resources.md)。
   - 阶段目录：`<repo-root>/NN_<slug>/`（命名可增补，顺序勿乱）。
   - 起点：**C# 小白**；先就业主线 `00_`～`13_`，再精通线 **`14→15→16→17→18→19`**。
   - 工具链：.NET 最新稳定 SDK（建议 ≥ 8）。
   - 每阶段课表以 roadmap「课表明细」为准；可增补，不可擅自删减核心知识点。

2. **读模板再写代码**
   - 必读 [references/templates.md](references/templates.md)。
   - 必读 [.cursor/rules/kcs-learning-standards.mdc](../../rules/kcs-learning-standards.mdc)。

3. **源码落盘约定**
   - 优先：`NN_slug/<topic>/L00_TopicName.cs` + 同目录 `L00_TopicName.csproj`。
   - 生产框架阶段：可用纯 C# 迷你实现讲心智，README 写明与 ASP.NET Core / EF Core / 真实中间件对照。

4. **生成内容约束**
   - 顶部主题块、中文注释、有价值时直观版 + 进阶版。
   - `RunTests()` 失败须抛异常；打印 `✅ ...通过`。
   - `Bootstrap()` 演示后调用 `RunTests()`；`Main` → `Bootstrap()`。
   - 离线可测；新依赖写入该课 `csproj`。
   - 更新阶段 README 与根 README「已生成」表。
   - **外链**：明文完整 URL；须来自 resources.md 或同次 HTTP 核验。
   - 阶段 README 必须含：章节进度表 + 本阶段知识点资料包。

5. **分批提交（用户要求全自动时）**
   - 每完成一个 `NN_xxx`：verify 通过 → 单独 commit；全部完成再 push（用户明确要求时）。
   - 不要 force push；不要无关重构。

6. **验证**

```bash
./scripts/verify_modules.sh 00_csharp_basics
./scripts/verify_modules.sh 00_csharp_basics/00_hello
```

## 用户一句话触发示例

```text
按 csharp-study-generator / kcs 学习规范初始化 00_csharp_basics
```

```text
全自动把 README 阶段路线里还没生成的目录都做完，分批次提交并 push
```

## Additional Resources

- [references/templates.md](references/templates.md)
- [references/roadmap.md](references/roadmap.md)
- [references/resources.md](references/resources.md)
- 仓库 `scripts/verify_modules.sh`
