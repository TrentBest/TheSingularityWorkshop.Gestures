using System.Globalization;
using System.Numerics;
using System.Text;
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.Gestures;

// This executable is the source of docs/assets/gesture-motion.svg.
// FSM_API owns the demonstration lifecycle; GestureEvaluator owns every sampled pose.
// The SVG contains serialized evaluator samples. It does not pretend the browser runs C#.
const string group = "GestureMotionShowcase";
const double duration = 4.0;
const int sampleCount = 49;
var context = new MotionContext { DurationSeconds = duration };
var transitions = new List<string>();

FSM_API.Create.CreateProcessingGroup(group);
FSM_API.Create.CreateFiniteStateMachine("GestureMotionShowcase", processRate: 0, processingGroup: group)
    .State("Ready", onEnter: ctx =>
    {
        if (ctx is MotionContext motion) { motion.Phase = "Ready"; transitions.Add("Ready"); }
    }, onUpdate: null, onExit: null)
    .State("Running", onEnter: ctx =>
    {
        if (ctx is MotionContext motion) { motion.Phase = "Running"; transitions.Add("Running"); }
    }, onUpdate: null, onExit: null)
    .State("Complete", onEnter: ctx =>
    {
        if (ctx is MotionContext motion) { motion.Phase = "Complete"; transitions.Add("Complete"); }
    }, onUpdate: null, onExit: null)
    .WithInitialState("Ready")
    .BuildDefinition();

var handle = FSM_API.Create.CreateInstance("GestureMotionShowcase", context, group);
context.Phase = handle.CurrentState;
transitions.Add(handle.CurrentState);
handle.TransitionTo("Running");

var definition = CreateDefinition();
var samples = new List<Sample>(sampleCount);
for (var i = 0; i < sampleCount; i++)
{
    context.ElapsedSeconds = duration * i / (sampleCount - 1);
    if (i == sampleCount - 1)
        handle.TransitionTo("Complete");

    var evaluated = GestureEvaluator.Evaluate(definition, context.ElapsedSeconds, duration,
        GestureVariation.None, identitySeed: 0);
    samples.Add(new Sample(
        context.ElapsedSeconds / duration,
        evaluated.NormalizedProgress,
        evaluated.LocalTransforms.ToDictionary(
            pair => pair.Key,
            pair => new TransformSample(
                pair.Value.Position.X,
                pair.Value.Position.Y,
                pair.Value.Scale.X,
                RotationDegrees(pair.Value.Rotation))),
        evaluated.IsComplete,
        context.Phase));
}

if (context.Phase != "Complete")
    throw new InvalidOperationException($"FSM_API did not complete the demonstration. Final phase: {context.Phase}");
if (samples.Count != sampleCount || samples[0].Progress != 0d || samples[^1].Progress != 1d)
    throw new InvalidOperationException("The evaluator output did not cover both normalized endpoints.");

var output = GetOutputPath(args);
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
File.WriteAllText(output, BuildSvg(samples, transitions), new UTF8Encoding(false));
var channelsOutput = Path.Combine("docs", "assets", "gesture-channels.svg");
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(channelsOutput))!);
File.WriteAllText(channelsOutput, BuildChannelsSvg(samples), new UTF8Encoding(false));
Console.WriteLine($"Generated {output} and {channelsOutput}");
Console.WriteLine($"Lifecycle: {string.Join(" -> ", transitions)}");
Console.WriteLine($"Samples: {samples.Count}; named transforms: {string.Join(", ", samples[0].Transforms.Keys.Order())}; source: GestureEvaluator.Evaluate; coordinator: FSM_API");

static GestureDefinition CreateDefinition()
{
    // These named transforms are independently interpolated by the current evaluator.
    // Their 2-D positions are scene coordinates; this demo does not compose a rig hierarchy.
    static PoseTransform Pose(float x, float y, float degrees, float scale) =>
        new(new Vector3(x, y, 0), Quaternion.CreateFromAxisAngle(Vector3.UnitZ, degrees * MathF.PI / 180f),
            new Vector3(scale, scale, 1));

    return new GestureDefinition("living-docs.named-transform-traverse",
    [
        new GestureKeyframe(0, new Dictionary<string, PoseTransform>
        {
            ["root"] = Pose(0.08f, 0.14f, -8, 0.85f),
            ["tool"] = Pose(0.12f, 0.39f, 12, 0.70f),
            ["load"] = Pose(0.06f, 0.08f, -3, 1.15f)
        }),
        new GestureKeyframe(0.25, new Dictionary<string, PoseTransform>
        {
            ["root"] = Pose(0.29f, 0.33f, 10, 0.95f),
            ["tool"] = Pose(0.30f, 0.45f, -18, 0.82f),
            ["load"] = Pose(0.24f, 0.12f, 8, 1.05f)
        }),
        new GestureKeyframe(0.5, new Dictionary<string, PoseTransform>
        {
            ["root"] = Pose(0.51f, 0.20f, -14, 1.10f),
            ["tool"] = Pose(0.48f, 0.31f, 24, 0.95f),
            ["load"] = Pose(0.47f, 0.10f, 16, 0.92f)
        }),
        new GestureKeyframe(0.75, new Dictionary<string, PoseTransform>
        {
            ["root"] = Pose(0.73f, 0.38f, 16, 0.98f),
            ["tool"] = Pose(0.69f, 0.46f, -10, 0.78f),
            ["load"] = Pose(0.70f, 0.15f, -12, 1.08f)
        }),
        new GestureKeyframe(1, new Dictionary<string, PoseTransform>
        {
            ["root"] = Pose(0.92f, 0.16f, 0, 1.00f),
            ["tool"] = Pose(0.88f, 0.36f, 8, 0.88f),
            ["load"] = Pose(0.91f, 0.11f, 0, 1.00f)
        })
    ]);
}

static double RotationDegrees(Quaternion q)
{
    var normalized = Quaternion.Normalize(q);
    var sin = 2d * (normalized.W * normalized.Z + normalized.X * normalized.Y);
    var cos = 1d - 2d * (normalized.Y * normalized.Y + normalized.Z * normalized.Z);
    return Math.Atan2(sin, cos) * 180d / Math.PI;
}

static string GetOutputPath(string[] args)
{
    for (var i = 0; i < args.Length; i++)
    {
        if (args[i] == "--output" && i + 1 < args.Length)
            return args[i + 1];
    }
    return Path.Combine("docs", "assets", "gesture-motion.svg");
}

static string BuildSvg(IReadOnlyList<Sample> samples, IReadOnlyList<string> transitions)
{
    const double left = 82, top = 156, width = 796, height = 184;
    static string N(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
    static string Esc(string value) => System.Security.SecurityElement.Escape(value) ?? string.Empty;
    double X(double x) => left + Math.Clamp(x, 0, 1) * width;
    double Y(double y) => top + (0.5 - y) * height;
    var tracks = new[]
    {
        new Track("root", "#78dcff", "ROOT", 7),
        new Track("tool", "#f5a7ff", "TOOL", 6),
        new Track("load", "#e8b84b", "LOAD", 5)
    };
    var keyTimes = string.Join(";", samples.Select(s => N(s.ElapsedFraction)));
    var paths = new StringBuilder();
    var dots = new StringBuilder();
    var markers = new StringBuilder();
    var legend = new StringBuilder();

    foreach (var track in tracks)
    {
        var values = samples.Select(s => s.Transforms[track.Name]).ToArray();
        var points = string.Join(" ", values.Select(s => $"{N(X(s.X))},{N(Y(s.Y))}"));
        var positionValues = string.Join(";", values.Select(s => $"{N(X(s.X))} {N(Y(s.Y))}"));
        var rotationValues = string.Join(";", values.Select(s => N(s.RotationDegrees)));
        var scaleValues = string.Join(";", values.Select(s => $"{N(s.Scale)} {N(s.Scale)}"));
        paths.Append($"<polyline points=\"{points}\" fill=\"none\" stroke=\"{track.Color}\" stroke-width=\"2.5\" stroke-linecap=\"round\" stroke-linejoin=\"round\" opacity=\".82\"/>");
        foreach (var sample in samples.Where((_, i) => i % 12 == 0 || i == samples.Count - 1))
        {
            var value = sample.Transforms[track.Name];
            dots.Append($"<circle cx=\"{N(X(value.X))}\" cy=\"{N(Y(value.Y))}\" r=\"3\" fill=\"{track.Color}\" opacity=\".9\"/>");
        }
        markers.Append($$"""
        <g class="animated" filter="url(#glow)">
          <g>
            <animateTransform attributeName="transform" type="translate" values="{{positionValues}}" keyTimes="{{keyTimes}}" dur="4s" repeatCount="indefinite"/>
            <g>
              <animateTransform attributeName="transform" type="rotate" values="{{rotationValues}}" keyTimes="{{keyTimes}}" dur="4s" repeatCount="indefinite"/>
              <g>
                <animateTransform attributeName="transform" type="scale" values="{{scaleValues}}" keyTimes="{{keyTimes}}" dur="4s" repeatCount="indefinite"/>
                <circle r="{{track.Radius}}" fill="{{track.Color}}" stroke="#f3f6fb" stroke-width="1.5"/>
                <path d="M0 0 L16 0" stroke="{{track.Color}}" stroke-width="3" stroke-linecap="round"/>
              </g>
            </g>
          </g>
        </g>
        """);
        var middle = values[values.Length / 2];
        markers.Append($$"""
        <g class="static" transform="translate({{N(X(middle.X))}} {{N(Y(middle.Y))}}) rotate({{N(middle.RotationDegrees)}}) scale({{N(middle.Scale)}})">
          <circle r="{{track.Radius}}" fill="{{track.Color}}" stroke="#f3f6fb" stroke-width="1.5"/>
          <path d="M0 0 L16 0" stroke="{{track.Color}}" stroke-width="3" stroke-linecap="round"/>
        </g>
        """);
        legend.Append($$"""<g transform="translate({{track.LegendX}} 128)"><circle r="5" fill="{{track.Color}}"/><text x="11" y="4" font-size="11" fill="#f3f6fb" letter-spacing="1">{{track.Name.ToUpperInvariant()}}</text></g>""");
    }

    var start = samples[0];
    var end = samples[^1];
    return $$"""
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 960 480" role="img" aria-labelledby="title desc">
  <title id="title">Three real named transform tracks evaluated by GestureEvaluator and coordinated by FSM_API</title>
  <desc id="desc">Generated by running C# code. FSM_API advances Ready to Running to Complete. GestureEvaluator evaluates 49 samples of one GestureDefinition containing root, tool, and load transforms. All three paths, marker positions, rotations, and scales come from returned transform data. The transforms are independently interpolated and are not composed into a rig hierarchy. Position interpolation is linear.</desc>
  <defs>
    <linearGradient id="bg" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#101827"/><stop offset="1" stop-color="#17283b"/></linearGradient>
    <filter id="glow" x="-100%" y="-100%" width="300%" height="300%"><feGaussianBlur stdDeviation="3" result="b"/><feMerge><feMergeNode in="b"/><feMergeNode in="SourceGraphic"/></feMerge></filter>
    <style>
      text{font-family:"Segoe UI",Arial,sans-serif}.grid{stroke:#31465e;stroke-width:1;stroke-dasharray:3 7}.static{display:none}
      @media(prefers-reduced-motion:reduce){.animated{display:none}.static{display:inline}}
    </style>
  </defs>
  <rect width="960" height="480" rx="18" fill="url(#bg)"/>
  <text x="34" y="38" font-size="12" letter-spacing="2" fill="#8da7c5">THE SINGULARITY WORKSHOP / GENERATED FROM C#</text>
  <text x="34" y="72" font-size="25" font-weight="700" fill="#f3f6fb">One procedure. Three evaluated transform tracks.</text>
  <text x="34" y="97" font-size="13" fill="#b9c8d9">Real GestureEvaluator output · lifecycle coordinated by FSM_API · browser replays serialized samples</text>
  {{legend}}
  <g class="grid">
    <path d="M82 156H878 M82 202H878 M82 248H878 M82 294H878 M82 340H878"/>
    <path d="M82 156V340 M281 156V340 M480 156V340 M679 156V340 M878 156V340"/>
  </g>
  <path d="M82 156H878 M82 340H878" stroke="#51657b" stroke-width="1.5"/>
  {{paths}}
  {{dots}}
  {{markers}}
  <g fill="#b9c8d9" font-size="11">
    <text x="82" y="362">START · {{N(start.Progress)}}</text><text x="480" y="362" text-anchor="middle">INDEPENDENT NAMED TRANSFORMS · NOT A RIG HIERARCHY</text><text x="878" y="362" text-anchor="end">END · {{N(end.Progress)}}</text>
  </g>
  <path d="M82 394H878" stroke="#31465e" stroke-width="2"/>
  <circle class="animated" r="5" fill="#78dcff">
    <animate attributeName="cx" values="{{string.Join(";", samples.Select(s => N(left + s.ElapsedFraction * width)))}}" keyTimes="{{keyTimes}}" dur="4s" repeatCount="indefinite"/>
    <animate attributeName="cy" values="{{string.Join(";", samples.Select(_ => "394"))}}" dur="4s" repeatCount="indefinite"/>
  </circle>
  <circle class="static" cx="480" cy="394" r="5" fill="#78dcff"/>
  <text x="34" y="426" font-size="11" letter-spacing="1.5" fill="#78dcff">OBSERVED C# OUTPUT</text>
  <text x="34" y="448" font-size="13" fill="#f3f6fb">49 samples · linear position/scale interpolation · quaternion rotation interpolation</text>
  <text x="926" y="426" text-anchor="end" font-size="11" letter-spacing="1.5" fill="#e8b84b">FSM_API LIFECYCLE</text>
  <text x="926" y="448" text-anchor="end" font-size="13" fill="#f3f6fb">{{Esc(string.Join(" → ", transitions))}}</text>
</svg>
""";
}


static string BuildChannelsSvg(IReadOnlyList<Sample> samples)
{
    const double left = 120, width = 720, panelHeight = 58, gap = 25, firstTop = 122;
    static string N(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
    double TimeX(double fraction) => left + Math.Clamp(fraction, 0, 1) * width;
    var root = samples.Select(s => s.Transforms["root"]).ToArray();
    var panels = new[]
    {
        new ChannelPanel("POSITION X", "normalized", "#78dcff", root.Select(s => s.X).ToArray(), 0, 1),
        new ChannelPanel("POSITION Y", "normalized", "#f5a7ff", root.Select(s => s.Y).ToArray(), 0, .5),
        new ChannelPanel("ROTATION", "degrees", "#e8b84b", root.Select(s => s.RotationDegrees).ToArray(), -20, 25),
        new ChannelPanel("SCALE", "uniform factor", "#91e6a7", root.Select(s => s.Scale).ToArray(), .7, 1.2)
    };
    var grid = new StringBuilder();
    var curves = new StringBuilder();
    var labels = new StringBuilder();
    for (var panelIndex = 0; panelIndex < panels.Length; panelIndex++)
    {
        var panel = panels[panelIndex];
        var top = firstTop + panelIndex * (panelHeight + gap);
        double ValueY(double value) => top + panelHeight * (1 - (value - panel.Min) / (panel.Max - panel.Min));
        var points = string.Join(" ", panel.Values.Select((value, i) => $"{N(TimeX(samples[i].ElapsedFraction))},{N(ValueY(value))}"));
        grid.Append($"<path d=\"M{N(left)} {N(top)}H{N(left + width)} M{N(left)} {N(top + panelHeight / 2)}H{N(left + width)} M{N(left)} {N(top + panelHeight)}H{N(left + width)}\"/>");
        curves.Append($"<polyline points=\"{points}\" fill=\"none\" stroke=\"{panel.Color}\" stroke-width=\"2.5\" stroke-linejoin=\"round\" stroke-linecap=\"round\"/>");
        foreach (var sampleIndex in new[] { 0, 12, 24, 36, 48 })
            curves.Append($"<circle cx=\"{N(TimeX(samples[sampleIndex].ElapsedFraction))}\" cy=\"{N(ValueY(panel.Values[sampleIndex]))}\" r=\"3\" fill=\"{panel.Color}\"/>");
        labels.Append($"<text x=\"34\" y=\"{N(top + 15)}\" font-size=\"12\" font-weight=\"700\" letter-spacing=\"1.2\" fill=\"{panel.Color}\">{panel.Title}</text>");
        labels.Append($"<text x=\"34\" y=\"{N(top + 33)}\" font-size=\"10\" fill=\"#b9c8d9\">{panel.Unit}</text>");
        labels.Append($"<text x=\"{N(left - 10)}\" y=\"{N(top + 4)}\" text-anchor=\"end\" font-size=\"10\" fill=\"#b9c8d9\">{N(panel.Max)}</text>");
        labels.Append($"<text x=\"{N(left - 10)}\" y=\"{N(top + panelHeight)}\" text-anchor=\"end\" font-size=\"10\" fill=\"#b9c8d9\">{N(panel.Min)}</text>");
    }
    var keyTimes = string.Join(";", samples.Select(s => N(s.ElapsedFraction)));
    var cursorValues = string.Join(";", samples.Select(s => N(TimeX(s.ElapsedFraction))));
    var middleX = N(TimeX(samples[samples.Count / 2].ElapsedFraction));
    var endY = N(firstTop + panels.Length * (panelHeight + gap) - gap + 18);
    return $"""
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 900 480" role="img" aria-labelledby="channels-title channels-desc">
  <title id="channels-title">Live data channels from the evaluated root transform</title>
  <desc id="channels-desc">Generated from the same 49 C# GestureEvaluator samples as the motion scene. Four plots show root X position, root Y position, rotation in degrees, and scale against normalized time. A cursor moves across the plots. These are evaluated keyframe interpolation values, not physics telemetry.</desc>
  <defs>
    <linearGradient id="channels-bg" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#101827"/><stop offset="1" stop-color="#17283b"/></linearGradient>
    <style>
      text{font-family:"Segoe UI",Arial,sans-serif}.grid{stroke:#31465e;stroke-width:1;stroke-dasharray:3 6}.static{display:none}
      @media(prefers-reduced-motion:reduce){.animated{display:none}.static{display:inline}}
    </style>
  </defs>
  <rect width="900" height="480" rx="16" fill="url(#channels-bg)"/>
  <text x="30" y="34" font-size="11" letter-spacing="2" fill="#8da7c5">THE SINGULARITY WORKSHOP / C# EVALUATOR DATA</text>
  <text x="30" y="66" font-size="24" font-weight="700" fill="#f3f6fb">A pose is more than a point on a path.</text>
  <text x="30" y="89" font-size="12" fill="#b9c8d9">The same evaluated root transform, separated into measurable channels.</text>
  <g class="grid">{{grid}}</g>
  {{curves}}
  {{labels}}
  <g class="animated" opacity=".9">
    <path d="M0 112V{{endY}}" stroke="#f3f6fb" stroke-width="1.5" stroke-dasharray="4 4">
      <animate attributeName="d" values="{{string.Join(";", samples.Select(s => $"M{N(TimeX(s.ElapsedFraction))} 112V{endY}"))}}" keyTimes="{{keyTimes}}" dur="4s" repeatCount="indefinite"/>
    </path>
    <circle r="4" fill="#f3f6fb">
      <animate attributeName="cx" values="{{cursorValues}}" keyTimes="{{keyTimes}}" dur="4s" repeatCount="indefinite"/>
      <animate attributeName="cy" values="{{string.Join(";", samples.Select(_ => N(firstTop - 9)))}}" dur="4s" repeatCount="indefinite"/>
    </circle>
  </g>
  <g class="static">
    <path d="M{{middleX}} 112V{{endY}}" stroke="#f3f6fb" stroke-width="1.5" stroke-dasharray="4 4"/>
    <circle cx="{{middleX}}" cy="{{N(firstTop - 9)}}" r="4" fill="#f3f6fb"/>
  </g>
  <text x="{{N(left)}}" y="447" font-size="11" fill="#b9c8d9">START · 0%</text>
  <text x="{{N(left + width / 2)}}" y="447" text-anchor="middle" font-size="11" fill="#b9c8d9">NORMALIZED PROGRESS</text>
  <text x="{{N(left + width)}}" y="447" text-anchor="end" font-size="11" fill="#b9c8d9">100% · COMPLETE</text>
  <text x="30" y="468" font-size="10" fill="#8da7c5">Linear position/scale interpolation · quaternion rotation interpolation · not a physics solver</text>
</svg>
""";
}

internal sealed record ChannelPanel(string Title, string Unit, string Color, double[] Values, double Min, double Max);

internal sealed class MotionContext : IStateContext
{
    public string Name { get; set; } = "Gesture motion documentation";
    public bool IsValid { get; set; } = true;
    public double DurationSeconds { get; set; }
    public double ElapsedSeconds { get; set; }
    public string Phase { get; set; } = "Created";
}

internal sealed record TransformSample(double X, double Y, double Scale, double RotationDegrees);
internal sealed record Sample(double ElapsedFraction, double Progress, Dictionary<string, TransformSample> Transforms, bool IsComplete, string Phase);
internal sealed record Track(string Name, string Color, string Label, int Radius)
{
    public int LegendX => Name switch { "root" => 82, "tool" => 200, _ => 310 };
}
