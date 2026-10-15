using System.Globalization;
using System.Text;

namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// Renders deterministic skill-distribution samples as a standalone illustrative SVG.
/// The output is a visualization of the supplied sample model, not a physical prediction.
/// </summary>
public static class SkillDistributionSvgRenderer
{
    private const int Width = 1200;
    private const int Height = 680;
    private const double PanelWidth = 560;
    private const double PanelTop = 100;

    /// <summary>
    /// Generates a two-panel SVG comparing narrow and broad movement spreads using the
    /// same seed, sample count, and outcome-noise setting.
    /// </summary>
    public static string RenderComparison(
        int seed,
        int sampleCount,
        double narrowMovementSpread,
        double broadMovementSpread,
        double outcomeNoise)
    {
        if (!double.IsFinite(narrowMovementSpread) || narrowMovementSpread < 0)
            throw new ArgumentOutOfRangeException(nameof(narrowMovementSpread));
        if (!double.IsFinite(broadMovementSpread) || broadMovementSpread < narrowMovementSpread)
            throw new ArgumentOutOfRangeException(nameof(broadMovementSpread), "Broad spread must be finite and at least as large as narrow spread.");

        var narrow = SkillDistributionDemo.Generate(seed, sampleCount, narrowMovementSpread, outcomeNoise);
        var broad = SkillDistributionDemo.Generate(seed, sampleCount, broadMovementSpread, outcomeNoise);
        var svg = new StringBuilder(Width * 3);
        svg.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {Width} {Height}\" role=\"img\" aria-labelledby=\"title desc\">");
        svg.AppendLine("<title id=\"title\">Deterministic skill distribution comparison</title>");
        svg.AppendLine("<desc id=\"desc\">Illustrative seeded movement samples and separately perturbed outcomes. The ghost pose and aim line mark the ideal origin. This is not physical validation.</desc>");
        svg.AppendLine("<rect width=\"1200\" height=\"680\" fill=\"#101820\" rx=\"18\"/>");
        svg.AppendLine("<text x=\"40\" y=\"45\" fill=\"#f4f7fa\" font-size=\"24\" font-family=\"sans-serif\" font-weight=\"700\">Skill is a distribution, not one number</text>");
        svg.AppendLine("<text x=\"40\" y=\"73\" fill=\"#b8c7d3\" font-size=\"14\" font-family=\"sans-serif\">Same seed and sample count · ideal pose is fixed · movement spread changes · outcome noise is independent</text>");
        AppendPanel(svg, 30, "Narrow movement envelope", narrow, narrowMovementSpread, seed);
        AppendPanel(svg, 610, "Broad movement envelope", broad, broadMovementSpread, seed);
        svg.AppendLine("<text x=\"40\" y=\"645\" fill=\"#d5e0e8\" font-size=\"13\" font-family=\"sans-serif\">Illustrative dimensionless units. Pose error ≠ movement variance ≠ outcome spread. Not a physical or ballistic model.</text>");
        svg.AppendLine("</svg>");
        return svg.ToString();
    }

    private static void AppendPanel(StringBuilder svg, int left, string title, SkillDistributionResult result, double movementSpread, int seed)
    {
        var centerX = left + PanelWidth / 2;
        var centerY = PanelTop + 210;
        const double scale = 150;
        svg.AppendLine($"<rect x=\"{F(left)}\" y=\"90\" width=\"560\" height=\"520\" rx=\"14\" fill=\"#1b2935\" stroke=\"#425565\"/>");
        svg.AppendLine($"<text x=\"{F(left + 22)}\" y=\"125\" fill=\"#f4f7fa\" font-size=\"18\" font-family=\"sans-serif\" font-weight=\"700\">{title}</text>");
        svg.AppendLine($"<text x=\"{F(left + 22)}\" y=\"150\" fill=\"#b8c7d3\" font-size=\"12\" font-family=\"sans-serif\">seed {seed} · {result.Samples.Count} samples · spread {F(movementSpread)}</text>");
        svg.AppendLine($"<line x1=\"{F(centerX - 205)}\" y1=\"{F(centerY)}\" x2=\"{F(centerX + 205)}\" y2=\"{F(centerY)}\" stroke=\"#354a5b\"/>");
        svg.AppendLine($"<line x1=\"{F(centerX)}\" y1=\"{F(centerY - 175)}\" x2=\"{F(centerX)}\" y2=\"{F(centerY + 175)}\" stroke=\"#354a5b\"/>");
        svg.AppendLine($"<line x1=\"{F(centerX)}\" y1=\"{F(centerY)}\" x2=\"{F(centerX + 145)}\" y2=\"{F(centerY - 95)}\" stroke=\"#e6edf3\" stroke-dasharray=\"6 5\" stroke-width=\"2\"/>");
        svg.AppendLine($"<circle cx=\"{F(centerX)}\" cy=\"{F(centerY)}\" r=\"13\" fill=\"none\" stroke=\"#f4f7fa\" stroke-width=\"2\" stroke-dasharray=\"3 3\"/>");
        svg.AppendLine($"<path d=\"M {F(centerX - 8)} {F(centerY)} H {F(centerX + 8)} M {F(centerX)} {F(centerY - 8)} V {F(centerY + 8)}\" stroke=\"#f4f7fa\" stroke-width=\"2\"/>");
        svg.AppendLine($"<circle cx=\"{F(centerX + 145)}\" cy=\"{F(centerY - 95)}\" r=\"28\" fill=\"none\" stroke=\"#f2bd55\" stroke-width=\"2\"/>");
        svg.AppendLine($"<circle cx=\"{F(centerX + 145)}\" cy=\"{F(centerY - 95)}\" r=\"17\" fill=\"none\" stroke=\"#f2bd55\" stroke-width=\"2\"/>");
        svg.AppendLine($"<circle cx=\"{F(centerX + 145)}\" cy=\"{F(centerY - 95)}\" r=\"5\" fill=\"#f2bd55\"/>");
        foreach (var sample in result.Samples)
        {
            var mx = centerX + sample.MovementX * scale;
            var my = centerY - sample.MovementY * scale;
            var ox = centerX + 145 + sample.OutcomeX * scale;
            var oy = centerY - 95 - sample.OutcomeY * scale;
            svg.AppendLine($"<circle cx=\"{F(mx)}\" cy=\"{F(my)}\" r=\"3.2\" fill=\"#57c8a5\" fill-opacity=\"0.72\"/>");
            svg.AppendLine($"<circle cx=\"{F(ox)}\" cy=\"{F(oy)}\" r=\"3.2\" fill=\"#f2bd55\" fill-opacity=\"0.78\"/>");
        }
        svg.AppendLine($"<text x=\"{F(left + 22)}\" y=\"535\" fill=\"#57c8a5\" font-size=\"12\" font-family=\"sans-serif\">Movement variance: {F(result.MovementVariance)}</text>");
        svg.AppendLine($"<text x=\"{F(left + 22)}\" y=\"557\" fill=\"#f2bd55\" font-size=\"12\" font-family=\"sans-serif\">Outcome spread: {F(result.OutcomeSpread)}</text>");
        svg.AppendLine($"<text x=\"{F(left + 22)}\" y=\"579\" fill=\"#d5e0e8\" font-size=\"12\" font-family=\"sans-serif\">Mean pose error: {F(result.MeanPoseError)}</text>");
    }

    private static string F(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}