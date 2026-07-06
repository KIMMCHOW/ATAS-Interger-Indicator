# Interger Level Renderer

## Overview / 概览

English: Interger Level Renderer is an ATAS custom indicator that draws automatic integer price levels on the main chart. It helps users keep 100-unit and 1000-unit price levels visible while reviewing market structure.

中文：Interger Level Renderer 是用于 ATAS 的自定义指标，会在主图上自动绘制整数价格水平线，帮助用户在观察行情结构时清晰查看 100 单位和 1000 单位价格关口。

## Features / 功能

English:

- Draws levels from a configurable `Start Price`, default `0`.
- Supports 100-unit levels and 1000-unit levels.
- Uses separate configurable line styles for hundred and thousand levels.
- Draws only in the visible price range through ATAS custom rendering.
- Does not place orders, change trading state, or write to ATAS installation files.

中文：

- 可从 `Start Price` 设置的锚定价格开始绘制，默认值为 `0`。
- 支持 100 单位整数关口和 1000 单位整数关口。
- 100 单位线和 1000 单位线可分别配置颜色、线宽和虚线样式。
- 仅在当前可见价格区间内通过 ATAS 自绘层渲染。
- 不下单、不改变交易状态，也不写入 ATAS 安装目录。

## Update Prompt / 更新提示

English: On load, the indicator silently checks TradingHub latest-version metadata. It shows no controls, settings, placeholders, or buttons by default; only when a newer/different version exists, it draws one small text prompt on the chart.

中文：指标加载时会静默检查 TradingHub 最新版本元数据。默认不显示控件、设置、占位文字或按钮；只有存在更新或不同版本时，才会在图表上显示一行小号文字提示。

## Parameters / 参数

| Parameter | Default | English | 中文 |
| --- | ---: | --- | --- |
| `Start Price` | `0` | Anchor price. Levels are `Start Price + n * step`. | 锚定价格，水平线按 `Start Price + n * step` 生成。 |
| `Show Hundred Levels` | `true` | Draw 100-unit levels. | 显示 100 单位整数关口。 |
| `Show Thousand Levels` | `true` | Draw 1000-unit levels. | 显示 1000 单位整数关口。 |
| `Show Price Labels` | `true` | Show right-side price labels when there is enough vertical room. | 在垂直空间足够时显示右侧价格标签。 |
| `Max Lines` | `300` | Safety cap for rendered lines. | 渲染水平线数量上限。 |
| `Hundred Line` | Gray dashed line | Style for 100-unit levels. | 100 单位线样式。 |
| `Thousand Line` | Blue solid line | Style for 1000-unit levels. | 1000 单位线样式。 |

English: `Show Hundred Levels` and `Show Thousand Levels` can be enabled together. When both are enabled, 1000-unit levels are drawn once using the stronger thousand-line style.

中文：`Show Hundred Levels` 和 `Show Thousand Levels` 可以同时开启。两者同时开启时，1000 单位关口只绘制一次，并使用更醒目的千位线样式。

## Build / 构建

English: Run from this folder:

中文：在本目录运行：

```powershell
dotnet build .\IntergerLevelRenderer.csproj -c Release
```

Expected artifact / 预期产物：

```text
Products\IntergerLevelRenderer\build\IntergerLevelRenderer.dll
```

## Download Mirror / 下载镜像

English: The customer-facing stable release mirror is stored under:

中文：面向客户的稳定版下载镜像位于：

```text
Products\IntergerLevelRenderer\IntergerLevelRenderer-Downloads
```

English: The TradingHub website serves the current stable DLL through the signed-in download API at `/download/interger-level-renderer`.

中文：TradingHub 网站通过登录后的下载 API 在 `/download/interger-level-renderer` 提供当前稳定版 DLL。

## Install / 安装

English: Copy the built DLL to the ATAS custom indicators folder when you are ready to install:

中文：需要安装时，将构建好的 DLL 复制到 ATAS 自定义指标目录：

```text
%APPDATA%\ATAS\Indicators
```

English: Restart ATAS, then search for:

中文：重新启动 ATAS 后搜索：

```text
Interger Level Renderer
```
