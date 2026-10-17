using System.Globalization;
using System.Security;
using System.Text;
using System.Numerics;
using TheSingularityWorkshop.Gestures;

// This scene serializes actual GestureExperienceProfile.Sample and GestureEvaluator outputs.
// It demonstrates only the current speed and separate root-offset channels, not joint skill.
const int sampleCount = 81;
const double duration = 4d;
var profiles = new[]
{
    new ProfileScene("NOVICE RANGE", new GestureExperienceProfile(0.05, 0.25, 0.10f, 0.50f), 101UL, "#f5a7ff"),
    new ProfileScene("VETERAN RANGE", new GestureExperienceProfile(0.01, 0.06, 0.02f, 0.12f), 202UL, "#78dcff")
};
var definition = new GestureDefinition("living-docs.seeded-variation",
[
    new GestureKeyframe(0d, new Dictionary<string, PoseTransform>
    {
        ["root"] = new PoseTransform(Vector3.Zero, Quaternion.Identity, Vector3.One)
    }),
    new GestureKeyframe(1d, new Dictionary<string, PoseTransform>
    {
        ["root"] = new PoseTransform(Vector3.UnitX, Quaternion.Identity, Vector3.One)
    })
]);
var samples = profiles.Select(profile =>
{
    var variation = profile.Profile.Sample(profile.Seed);
    var evaluated = Enumerable.Range(0, sampleCount).Select(i =>
    {
        var elapsed = duration * i / (sampleCount - 1);
        var pose = GestureEvaluator.Evaluate(definition, elapsed, duration, variation, profile.Seed);
        return new Sample(elapsed / duration, pose.NormalizedProgress, pose.RootOffset.X);
    }).ToArray();
    return new Scene(profile, variation, evaluated);
}).ToArray();

var output = args.Length >= 2 && args[0] == "--output" ? args[1] : "docs/assets/gesture-variation.svg";
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
File.WriteAllText(output, AddProvenanceLabel(BuildSvg(samples), "GENERATED FROM GESTUREEXPERIENCEPROFILE + GESTUREEVALUATOR • GESTURES • NATIVE SVG • GUI.SVG: NOT USED"), new UTF8Encoding(false));
Console.WriteLine($"Generated {output} from GestureExperienceProfile.Sample and GestureEvaluator.Evaluate.");
Console.WriteLine($"Samples per profile: {sampleCount}; identity seeds: {string.Join(", ", profiles.Select(p => p.Seed))}");


static string AddProvenanceLabel(string svg, string label)
{
    const string prefix = "viewBox=\"0 0 ";
    var viewBoxStart = svg.IndexOf(prefix, StringComparison.Ordinal);
    if (viewBoxStart < 0) throw new InvalidOperationException("SVG provenance label requires a viewBox.");
    var widthStart = viewBoxStart + prefix.Length;
    var widthEnd = svg.IndexOf(' ', widthStart);
    var heightEnd = svg.IndexOf('"', widthEnd + 1);
    if (widthEnd < 0 || heightEnd < 0) throw new InvalidOperationException("SVG viewBox is malformed.");
    var widthText = svg[widthStart..widthEnd];
    var heightText = svg[(widthEnd + 1)..heightEnd];
    var width = double.Parse(widthText, CultureInfo.InvariantCulture);
    var height = double.Parse(heightText, CultureInfo.InvariantCulture);
    var expandedViewBox = $"viewBox=\"0 0 {widthText} {NForLabel(height + 38)}\"";
    svg = svg[..viewBoxStart] + expandedViewBox + svg[(heightEnd + 1)..];
    var close = svg.LastIndexOf("</svg>", StringComparison.Ordinal);
    if (close < 0) throw new InvalidOperationException("SVG closing element was not found.");
    var safeLabel = label.Replace("&", "&amp;", StringComparison.Ordinal).Replace("<", "&lt;", StringComparison.Ordinal).Replace(">", "&gt;", StringComparison.Ordinal);
    var banner = $"<g id=\"provenance-label\" role=\"group\" aria-label=\"{safeLabel}\"><rect x=\"12\" y=\"{NForLabel(height + 3)}\" width=\"{NForLabel(width - 24)}\" height=\"27\" rx=\"6\" fill=\"#172033\"/><text x=\"{NForLabel(width / 2)}\" y=\"{NForLabel(height + 21)}\" text-anchor=\"middle\" font-family=\"system-ui,-apple-system,Segoe UI,sans-serif\" font-size=\"12\" font-weight=\"700\" fill=\"#ffffff\">{safeLabel}</text></g>";
    return svg.Insert(close, banner);
}

static string NForLabel(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);

static string BuildSvg(IReadOnlyList<Scene> scenes)
{
    const double left = 250, right = 900, top = 132, laneGap = 112;
    static string N(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
    static string E(string value) => SecurityElement.Escape(value) ?? string.Empty;
    var b = new StringBuilder();
    b.Append("""<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1000 390" role="img" aria-labelledby="title desc">""");
    b.Append("""<title id="title">Deterministic experience-profile variation</title><desc id="desc">Two fixed identity seeds are sampled through the actual Gestures profile and evaluator. Animated markers show evaluated progress and separately returned root offset; declared ranges and limits are labeled.</desc>""");
    b.Append("""<defs><linearGradient id="bg" x1="0" y1="0" x2="1" y2="1"><stop stop-color="#101827"/><stop offset="1" stop-color="#1c2940"/></linearGradient><filter id="glow"><feGaussianBlur stdDeviation="3" result="blur"/><feMerge><feMergeNode in="blur"/><feMergeNode in="SourceGraphic"/></feMerge></filter></defs>""");
    b.Append("""<rect width="1000" height="390" rx="22" fill="url(#bg)"/><text x="38" y="42" fill="#f3f6fb" font-family="Segoe UI, sans-serif" font-size="23" font-weight="700">SAME PROCEDURE. STABLE INDIVIDUAL INPUTS.</text><text x="38" y="68" fill="#b8c6d9" font-family="Segoe UI, sans-serif" font-size="13">Actual CPU reference outputs · fixed seeds · no per-frame randomness</text>""");
    for (var i = 0; i < scenes.Count; i++)
    {
        var scene = scenes[i];
        var y = top + i * laneGap;
        var color = scene.Profile.Color;
        var times = string.Join(";", scene.Samples.Select(s => N(s.Time)));
        var speed = 1d + ((ToUnit(scene.Profile.Seed, 0UL) * 2d - 1d) * scene.Variation.MaximumSpeedDeviation);
        b.Append($"<text x=\"38\" y=\"{N(y-10)}\" fill=\"{color}\" font-family=\"Segoe UI, sans-serif\" font-size=\"15\" font-weight=\"700\">{E(scene.Profile.Name)}</text>");
        b.Append($"<text x=\"38\" y=\"{N(y+12)}\" fill=\"#c4d0df\" font-family=\"Segoe UI, sans-serif\" font-size=\"12\">seed {scene.Profile.Seed} · speed ×{N(speed)}</text>");
        b.Append($"<line x1=\"{left}\" y1=\"{y}\" x2=\"{right}\" y2=\"{y}\" stroke=\"#63748b\" stroke-width=\"2\" stroke-dasharray=\"5 7\"/>");
        b.Append($"<line x1=\"{left}\" y1=\"{y-18}\" x2=\"{right}\" y2=\"{y-18}\" stroke=\"{color}\" stroke-opacity=\".24\" stroke-width=\"1\"/>");
        b.Append($"<polyline points=\"{string.Join(" ", scene.Samples.Select(s => $"{N(left+s.Progress*(right-left))},{N(y+s.Offset*36)}"))}\" fill=\"none\" stroke=\"{color}\" stroke-opacity=\".65\" stroke-width=\"1.5\"/>");
        b.Append($"<circle r=\"7\" fill=\"{color}\" filter=\"url(#glow)\"><animate attributeName=\"cx\" values=\"{string.Join(";", scene.Samples.Select(s => N(left+s.Progress*(right-left))))}\" keyTimes=\"{times}\" dur=\"4s\" repeatCount=\"indefinite\"/><animate attributeName=\"cy\" values=\"{string.Join(";", scene.Samples.Select(s => N(y+s.Offset*36)))}\" keyTimes=\"{times}\" dur=\"4s\" repeatCount=\"indefinite\"/></circle>");
        b.Append($"<text x=\"{right+15}\" y=\"{y+4}\" fill=\"#dbe5f1\" font-family=\"Segoe UI, sans-serif\" font-size=\"11\">offset ±{N(scene.Variation.MaximumLateralOffset)}</text>");
    }
    b.Append("""<text x="250" y="348" fill="#b8c6d9" font-family="Segoe UI, sans-serif" font-size="12">0% · START</text><text x="900" y="348" text-anchor="end" fill="#b8c6d9" font-family="Segoe UI, sans-serif" font-size="12">100% · COMPLETE</text><text x="38" y="376" fill="#93a5bb" font-family="Segoe UI, sans-serif" font-size="11">Illustrative profile ranges, not calibrated skill scores. Offset is separate output—not baked into named transforms.</text></svg>""");
    return b.ToString();
}

static double ToUnit(ulong seed, ulong salt)
{
    var value = seed ^ salt;
    value += 0x9E3779B97F4A7C15UL;
    value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
    value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
    value ^= value >> 31;
    return (value >> 11) * (1.0 / (1UL << 53));
}

sealed record ProfileScene(string Name, GestureExperienceProfile Profile, ulong Seed, string Color);
sealed record Sample(double Time, double Progress, double Offset);
sealed record Scene(ProfileScene Profile, GestureVariation Variation, Sample[] Samples);
