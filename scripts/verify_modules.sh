#!/usr/bin/env bash
# 递归查找含 L*.csproj 的学习课，dotnet run --project 执行。
# 用法:
#   ./scripts/verify_modules.sh 00_csharp_basics
#   ./scripts/verify_modules.sh 00_csharp_basics/00_hello
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "❌ 需要安装 .NET SDK（建议最新稳定版 ≥ 8）。安装: https://dotnet.microsoft.com/en-us/download"
  echo "   文档: https://learn.microsoft.com/en-us/dotnet/core/install/"
  exit 1
fi

echo "dotnet toolchain: $(dotnet --version 2>/dev/null || true)"

if [[ $# -lt 1 ]]; then
  echo "用法: $0 <目录...>"
  exit 1
fi

total=0
passed=0
failed=0

run_one() {
  local proj="$1"
  total=$((total + 1))
  echo "—— 运行: $proj"
  if ! dotnet run --project "$proj" -v q --nologo; then
    echo "❌ 运行失败: $proj"
    failed=$((failed + 1))
    return
  fi
  passed=$((passed + 1))
}

for target in "$@"; do
  abs="$ROOT/$target"
  if [[ ! -e "$abs" ]]; then
    echo "❌ 不存在: $target"
    failed=$((failed + 1))
    continue
  fi
  echo ""
  echo "===== 验证目录: $target ====="
  while IFS= read -r -d '' file; do
    run_one "$file"
  done < <(find "$abs" -type f -name 'L*.csproj' -print0 | sort -z)
done

echo ""
echo "全部汇总: 通过 $passed / $total ，失败 $failed"
if [[ "$failed" -gt 0 ]] || [[ "$total" -eq 0 ]]; then
  exit 1
fi
