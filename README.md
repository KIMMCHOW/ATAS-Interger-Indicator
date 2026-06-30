# Interger

ATAS custom indicator that draws automatic integer price levels on the main chart.

ATAS 自定义指标，用于在主图价格区域自动绘制整数关口水平线。

## Features / 功能

- Draws levels from a configurable `Start Price`, default `0`.
- Supports 100-unit levels and 1000-unit levels.
- Uses separate configurable line styles for hundred and thousand levels.
- Draws only in the visible price range through ATAS custom rendering.
- Does not place orders, change trading state, or write to ATAS installation files.

- 从可配置的 `Start Price` 起画线，默认值为 `0`。
- 支持百位整数关口和千位整数关口。
- 百位线、千位线可以分别自定义颜色、线宽、虚线样式。
- 只在当前可见价格区间内通过 ATAS 自绘层渲染。
- 不下单、不改变交易状态、不写入 ATAS 安装目录。

## Parameters / 参数

| Parameter | Default | Meaning |
| --- | ---: | --- |
| `Start Price` | `0` | Anchor price. Levels are `Start Price + n * step`. |
| `Show Hundred Levels` | `true` | Draw 100-unit levels. |
| `Show Thousand Levels` | `true` | Draw 1000-unit levels. |
| `Show Price Labels` | `true` | Show right-side price labels when there is enough vertical room. |
| `Max Lines` | `300` | Safety cap for rendered lines. |
| `Hundred Line` | gray dashed line | Style for 100-unit levels. |
| `Thousand Line` | blue solid line | Style for 1000-unit levels. |

`Show Hundred Levels` and `Show Thousand Levels` can be enabled together. When both are enabled, the 1000-unit levels are drawn once using the stronger thousand-line style.

`Show Hundred Levels` 和 `Show Thousand Levels` 可以同时打开；同时打开时，千位关口只绘制一次，并使用更醒目的千位线样式。

## Build / 构建

Run from this folder:

在本目录运行：

```powershell
dotnet build .\Interger.csproj -c Release
```

Expected artifact:

预期产物：

```text
Interger\build\Interger.dll
```

## Install / 安装

Copy the built DLL to the ATAS custom indicators folder only when you are ready to install:

需要安装时，再把 DLL 复制到 ATAS 自定义指标目录：

```text
%APPDATA%\ATAS\Indicators
```

Then restart ATAS and search for:

然后重启 ATAS，搜索：

```text
Interger
```
