using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ATAS.Indicators;
using OFT.Rendering.Context;
using OFT.Rendering.Settings;
using OFT.Rendering.Tools;

namespace IntergerLevelRenderer;

[DisplayName("Interger Level Renderer")]
[Category("Custom")]
public class IntergerLevelRenderer : Indicator
{
	private static readonly HttpClient ProductVersionHttpClient = new()
	{
		Timeout = TimeSpan.FromSeconds(5)
	};

	private const string ProductSlug = "interger-level-renderer";
	private const string CurrentProductVersion = "1.0.2";
	private const string ProductVersionApiUrl = "https://tradinghubs.org/api/products/latest-version";
	private const decimal HundredStep = 100m;
	private const decimal ThousandStep = 1000m;

	private readonly RenderFont _labelFont = new RenderFont("Arial", 10f);

	private volatile bool _productVersionCheckStarted;
	private volatile bool _productUpdateAvailable;
	private string _latestProductVersion = string.Empty;
	private decimal _startPrice;
	private bool _showHundredLevels = true;
	private bool _showThousandLevels = true;
	private bool _showPriceLabels = true;
	private int _maxLines = 300;
	private PenSettings _hundredLine = CreateHundredLine();
	private PenSettings _thousandLine = CreateThousandLine();

	[Display(Name = "Start Price", GroupName = "Levels", Description = "Anchor price for integer levels. Default is 0.", Order = 10)]
	public decimal StartPrice
	{
		get => _startPrice;
		set
		{
			_startPrice = value;
			RedrawChart();
		}
	}

	[Display(Name = "Show Hundred Levels", GroupName = "Levels", Description = "Draw levels every 100 price units from Start Price.", Order = 20)]
	public bool ShowHundredLevels
	{
		get => _showHundredLevels;
		set
		{
			_showHundredLevels = value;
			RedrawChart();
		}
	}

	[Display(Name = "Show Thousand Levels", GroupName = "Levels", Description = "Draw levels every 1000 price units from Start Price.", Order = 30)]
	public bool ShowThousandLevels
	{
		get => _showThousandLevels;
		set
		{
			_showThousandLevels = value;
			RedrawChart();
		}
	}

	[Display(Name = "Show Price Labels", GroupName = "Labels", Order = 10)]
	public bool ShowPriceLabels
	{
		get => _showPriceLabels;
		set
		{
			_showPriceLabels = value;
			RedrawChart();
		}
	}

	[Display(Name = "Max Lines", GroupName = "Levels", Description = "Safety cap for rendered levels in the visible price range.", Order = 100)]
	[Range(1, 2000)]
	public int MaxLines
	{
		get => _maxLines;
		set
		{
			_maxLines = Math.Clamp(value, 1, 2000);
			RedrawChart();
		}
	}

	[Display(Name = "Hundred Line", GroupName = "Style", Description = "Line style for 100-unit levels.", Order = 10)]
	public PenSettings HundredLine
	{
		get => _hundredLine;
		set
		{
			_hundredLine = value ?? CreateHundredLine();
			RedrawChart();
		}
	}

	[Display(Name = "Thousand Line", GroupName = "Style", Description = "Line style for 1000-unit levels.", Order = 20)]
	public PenSettings ThousandLine
	{
		get => _thousandLine;
		set
		{
			_thousandLine = value ?? CreateThousandLine();
			RedrawChart();
		}
	}

	public IntergerLevelRenderer()
		: base(useCandles: true)
	{
		base.DenyToChangePanel = true;
		base.EnableCustomDrawing = true;
		base.DataSeries[0].IsHidden = true;
		SubscribeToDrawingEvents(DrawingLayouts.Final);
		MaybeCheckProductVersionAsync();
	}

	protected override void OnCalculate(int bar, decimal value)
	{
		MaybeCheckProductVersionAsync();
	}

	protected override void OnRender(RenderContext context, DrawingLayouts layout)
	{
		if (base.ChartInfo?.PriceChartContainer == null)
			return;

		IChartContainer priceContainer = base.ChartInfo.PriceChartContainer;
		decimal low = priceContainer.Low;
		decimal high = priceContainer.High;

		DrawProductUpdateNotice(context);

		if (high <= low || (!_showHundredLevels && !_showThousandLevels))
			return;

		int rendered = 0;

		if (_showHundredLevels)
		{
			rendered += DrawLevels(
				context,
				priceContainer,
				HundredStep,
				_hundredLine,
				skipThousandLevels: _showThousandLevels,
				rendered);
		}

		if (_showThousandLevels && rendered < _maxLines)
			DrawLevels(context, priceContainer, ThousandStep, _thousandLine, skipThousandLevels: false, rendered);
	}

	private int DrawLevels(
		RenderContext context,
		IChartContainer priceContainer,
		decimal step,
		PenSettings lineSettings,
		bool skipThousandLevels,
		int alreadyRendered)
	{
		if (step <= 0m || alreadyRendered >= _maxLines)
			return 0;

		decimal low = priceContainer.Low;
		decimal high = priceContainer.High;
		decimal level = GetFirstLevel(low, step);
		RenderPen pen = lineSettings.RenderObject;
		int chartWidth = base.ChartInfo.Region.Width;
		int chartHeight = base.ChartInfo.Region.Height;
		int drawn = 0;
		bool drawLabels = CanDrawLabels(context, priceContainer, step);

		while (level <= high && alreadyRendered + drawn < _maxLines)
		{
			if (!skipThousandLevels || !IsLevelAligned(level, ThousandStep))
			{
				int y = priceContainer.GetYByPrice(level, isStartOfPriceLevel: false);

				if (y >= 0 && y <= chartHeight)
				{
					context.DrawLine(pen, 0, y, chartWidth, y);

					if (drawLabels)
						DrawLabel(context, level, y, chartWidth, pen.Color);

					drawn++;
				}
			}

			level += step;
		}

		return drawn;
	}

	private bool CanDrawLabels(RenderContext context, IChartContainer priceContainer, decimal step)
	{
		if (!_showPriceLabels)
			return false;

		int y1 = priceContainer.GetYByPrice(_startPrice, isStartOfPriceLevel: false);
		int y2 = priceContainer.GetYByPrice(_startPrice + step, isStartOfPriceLevel: false);
		int labelHeight = context.MeasureString("0", _labelFont).Height;

		return Math.Abs(y1 - y2) > labelHeight + 2;
	}

	private void DrawLabel(RenderContext context, decimal level, int y, int chartWidth, Color color)
	{
		string text = base.ChartInfo.GetPriceString(level);
		Size size = context.MeasureString(text, _labelFont);
		int x = Math.Max(0, chartWidth - size.Width - 2);
		int top = Math.Max(0, y - size.Height);
		Rectangle rectangle = new Rectangle(x, top, size.Width, size.Height);

		context.DrawString(text, _labelFont, color, rectangle);
	}

	private decimal GetFirstLevel(decimal low, decimal step)
	{
		decimal offsetSteps = decimal.Ceiling((low - _startPrice) / step);
		return _startPrice + offsetSteps * step;
	}

	private bool IsLevelAligned(decimal level, decimal step)
	{
		if (step <= 0m)
			return false;

		return (level - _startPrice) % step == 0m;
	}

	private void MaybeCheckProductVersionAsync()
	{
		if (_productVersionCheckStarted)
			return;

		_productVersionCheckStarted = true;
		_ = CheckProductVersionAsync();
	}

	private async Task CheckProductVersionAsync()
	{
		try
		{
			string url = $"{ProductVersionApiUrl}?product_slug={Uri.EscapeDataString(ProductSlug)}&current_version={Uri.EscapeDataString(CurrentProductVersion)}";
			using var response = await ProductVersionHttpClient.GetAsync(url).ConfigureAwait(false);
			string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(body))
				return;

			using var doc = JsonDocument.Parse(body);
			JsonElement root = doc.RootElement;
			if (!TryReadString(root, "latest_version", out string latestVersion))
				return;

			bool updateAvailable = TryReadBool(root, "update_available", out bool apiUpdateAvailable)
				? apiUpdateAvailable
				: CompareProductVersions(latestVersion, CurrentProductVersion) != 0;
			if (!updateAvailable)
				return;

			_latestProductVersion = latestVersion;
			_productUpdateAvailable = true;
			TryRedrawChart();
		}
		catch
		{
		}
	}

	private void DrawProductUpdateNotice(RenderContext context)
	{
		if (!_productUpdateAvailable || string.IsNullOrWhiteSpace(_latestProductVersion) || base.ChartInfo == null)
			return;

		string text = $"Interger Level Renderer {_latestProductVersion} update available / 可更新";
		Size size = context.MeasureString(text, _labelFont);
		context.DrawString(text, _labelFont, Color.Orange, new Rectangle(8, 8, size.Width, size.Height));
	}

	private static bool TryReadString(JsonElement root, string propertyName, out string value)
	{
		value = string.Empty;
		if (!root.TryGetProperty(propertyName, out JsonElement property) || property.ValueKind != JsonValueKind.String)
			return false;

		value = property.GetString() ?? string.Empty;
		return !string.IsNullOrWhiteSpace(value);
	}

	private static bool TryReadBool(JsonElement root, string propertyName, out bool value)
	{
		value = false;
		if (!root.TryGetProperty(propertyName, out JsonElement property))
			return false;

		if (property.ValueKind == JsonValueKind.True || property.ValueKind == JsonValueKind.False)
		{
			value = property.GetBoolean();
			return true;
		}

		return property.ValueKind == JsonValueKind.String && bool.TryParse(property.GetString(), out value);
	}

	private static int CompareProductVersions(string left, string right)
	{
		string normalizedLeft = NormalizeProductVersion(left);
		string normalizedRight = NormalizeProductVersion(right);

		if (Version.TryParse(normalizedLeft, out Version leftVersion) &&
			Version.TryParse(normalizedRight, out Version rightVersion))
		{
			return leftVersion.CompareTo(rightVersion);
		}

		return string.Compare(normalizedLeft, normalizedRight, StringComparison.OrdinalIgnoreCase);
	}

	private static string NormalizeProductVersion(string version)
	{
		if (string.IsNullOrWhiteSpace(version))
			return "0.0.0";

		Match match = Regex.Match(version.Trim(), @"\d+(?:\.\d+){0,3}");
		return match.Success ? match.Value : version.Trim();
	}

	private void TryRedrawChart()
	{
		try
		{
			RedrawChart();
		}
		catch
		{
		}
	}

	private static PenSettings CreateHundredLine()
	{
		return new PenSettings
		{
			Color = System.Windows.Media.Colors.DimGray,
			Width = 1,
			LineDashStyle = LineDashStyle.Dash
		};
	}

	private static PenSettings CreateThousandLine()
	{
		return new PenSettings
		{
			Color = System.Windows.Media.Colors.DeepSkyBlue,
			Width = 2,
			LineDashStyle = LineDashStyle.Solid
		};
	}
}
