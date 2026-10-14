using System.Globalization;
using System.Numerics;
using System.Text;
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.Gestures;

// This executable is the source of docs/assets/gesture-motion.svg.
// FSM_API owns the demonstration lifecycle; GestureEvaluator owns every sampled pose.
// The SVG contains the serialized evaluator samples. It does not pretend the browser runs C#.
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
    .Transition("Ready", "Running", ctx => ctx is MotionContext motion && motion.StartRequested)
    .Transition("Running", "Complete", ctx => ctx is MotionContext motion && motion.ElapsedSeconds >= motion.DurationSeconds)
    .BuildDefinition();

var handle = FSM_API.Create.CreateInstance("GestureMotionShowcase", context, group);
context.StartRequested = true;
handle.Update(group);

var definition = CreateDefinition();
var samples = new List<Sample>(sampleCount);
for (var i = 0; i < sampleCount; i++)
{
    context.ElapsedSeconds = duration * i / (sampleCount - 1);
    handle.Update(group);
    var evaluated = GestureEvaluator.Evaluate(definition, context.ElapsedSeconds, duration,
        GestureVariation.None, identitySeed: 0);
    var root = evaluated.LocalTransforms["root"];
    samples.Add(new Sample(
        context.ElapsedSeconds / duration,
        evaluated.NormalizedProgress,
        root.Position.X,
        root.Position.Y,
        root.Scale.X,
        RotationDegrees(root.Rotation),
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
Console.WriteLine($"Generated {output}");
Console.WriteLine($"Lifecycle: {string.Join(" -> ", transitions)}");
Console.WriteLine($"Samples: {samples.Count}; source: GestureEvaluator.Evaluate; coordinator: FSM_API");

static GestureDefinition CreateDefinition()
{
    // X/Y are the root transform's two-dimensional projection coordinates for this scene.
    // These keyframes are data passed to the real evaluator, not an SVG-authored path.
    static PoseTransform Pose(float x, float y, float degrees, float scale) =>
        new(new Vector3(x, y, 0), Quaternion.CreateFromAxisAngle(Vector3.UnitZ, degrees * MathF.PI / 180f),
            new Vector3(scale, scale, 1));

    return new GestureDefinition("living-docs.root-traverse",
    [
        new GestureKeyframe(0, new Dictionary<string, PoseTransform> { ["root"] = Pose(0.08f, 0.14f, -8, 0.85f) }),
        new GestureKeyframe(0.25, new Dictionary<string, PoseTransform> { ["root"] = Pose(0.29f, 0.33f, 10, 0.95f) }),
        new GestureKeyframe(0.5, new Dictionary<string, PoseTransform> { ["root"] = Pose(0.51f, 0.20f, -14, 1.10f) }),
        new GestureKeyframe(0.75, new Dictionary<string, PoseTransform> { ["root"] = Pose(0.73f, 0.38f, 16, 0.98f) }),
        new GestureKeyframe(1, new Dictionary<string, PoseTransform> { ["root"] = Pose(0.92f, 0.16f, 0, 1.00f) })
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
    var points = string.Join(" ", samples.Select(s => $"{N(X(s.X))},{N(Y(s.Y))}"));
    var xValues = string.Join(";", samples.Select(s => N(X(s.X))));
    var yValues = string.Join(";", samples.Select(s => N(Y(s.Y))));
    var progressValues = string.Join(";", samples.Select(s => N(left + s.ElapsedFraction * width)));
    var keyTimes = string.Join(";", samples.Select(s => N(s.ElapsedFraction)));
    var start = samples[0];
    var mid = samples[samples.Count / 2];
    var end = samples[^1];
    var keyframeDots = new StringBuilder();
    foreach (var sample in samples.Where((_, i) => i % 12 == 0 || i == samples.Count - 1))
        keyframeDots.Append($"<circle cx=\"{N(X(sample.X))}\" cy=\"{N(Y(sample.Y))}\" r=\"3\" fill=\"#e8b84b\" opacity=\".8\"/>");

    return $$"""
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 960 480" role="img" aria-labelledby="title desc">
  <title id="title">GestureEvaluator output coordinated by FSM_API</title>
  <desc id="desc">This SVG was generated by running C# code. FSM_API advances Ready to Running to Complete. GestureEvaluator evaluates 49 samples from the same five-keyframe GestureDefinition; the chart and animated marker use those returned root positions. The straight segments reflect the evaluator's linear position interpolation.</desc>
  <defs>
    <linearGradient id="bg" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#101827"/><stop offset="1" stop-color="#17283b"/></linearGradient>
    <filter id="glow" x="-100%" y="-100%" width="300%" height="300%"><feGaussianBlur stdDeviation="3" result="b"/><feMerge><feMergeNode in="b"/><feMergeNode in="SourceGraphic"/></feMerge></filter>
    <style>
      text{font-family:"Segoe UI",Arial,sans-serif} .muted{fill:#b9c8d9;font-size:12px} .grid{stroke:#31465e;stroke-width:1;stroke-dasharray:3 7} .motion{fill:none;stroke:#78dcff;stroke-width:3;stroke-linecap:round;stroke-linejoin:round} .static{display:none}
      @media(prefers-reduced-motion:reduce){\n        .animated{display:none}\n        .static{display:inline}\n      }
    </style>
  </defs>
  <rect width="960" height="480" rx="18" fill="url(#bg)"/>
  <text x="34" y="38" font-size="12" letter-spacing="2" fill="#8da7c5">THE SINGULARITY WORKSHOP / GENERATED FROM C#</text>
  <text x="34" y="72" font-size="25" font-weight="700" fill="#f3f6fb">A real evaluated pose, made visible</text>
  <text x="34" y="97" font-size="13" fill="#b9c8d9">FSM_API controls lifecycle · GestureEvaluator computes the samples · this SVG records the result</text>
  <g class="grid">
    <path d="M82 156H878 M82 202H878 M82 248H878 M82 294H878 M82 340H878"/>
    <path d="M82 156V340 M281 156V340 M480 156V340 M679 156V340 M878 156V340"/>
  </g>
  <path d="M82 156H878 M82 340H878" stroke="#51657b" stroke-width="1.5"/>
  <polyline points="{{points}}" class="motion"/>
  {{keyframeDots}}
  <g fill="#b9c8d9" font-size="11">
    <text x="82" y="362">progress 0</text><text x="480" y="362" text-anchor="middle">normalized progression</text><text x="878" y="362" text-anchor="end">progress 1</text>
  </g>
  <g class="animated" filter="url(#glow)">
    <circle r="9" fill="#e8b84b" stroke="#fff0b9" stroke-width="2">
      <animate attributeName="cx" values="{{xValues}}" keyTimes="{{keyTimes}}" dur="4s" repeatCount="indefinite"/>
      <animate attributeName="cy" values="{{yValues}}" keyTimes="{{keyTimes}}" dur="4s" repeatCount="indefinite"/>
    </circle>
  </g>
  <circle class="static" cx="{{N(X(mid.X))}}" cy="{{N(Y(mid.Y))}}" r="9" fill="#e8b84b" stroke="#fff0b9" stroke-width="2"/>
  <path d="M82 394H878" stroke="#31465e" stroke-width="2"/>
  <circle class="animated" r="5" fill="#78dcff">
    <animate attributeName="cx" values="{{progressValues}}" keyTimes="{{keyTimes}}" dur="4s" repeatCount="indefinite"/>
    <animate attributeName="cy" values="{{string.Join(";", samples.Select(_ => "394"))}}" dur="4s" repeatCount="indefinite"/>
  </circle>
  <circle class="static" cx="480" cy="394" r="5" fill="#78dcff"/>
  <text x="34" y="426" font-size="11" letter-spacing="1.5" fill="#78dcff">OBSERVED C# OUTPUT</text>
  <text x="34" y="448" font-size="13" fill="#f3f6fb">49 evaluator samples · linear position interpolation · quaternion rotation interpolation · scale interpolation</text>
  <text x="926" y="426" text-anchor="end" font-size="11" letter-spacing="1.5" fill="#e8b84b">FSM_API LIFECYCLE</text>
  <text x="926" y="448" text-anchor="end" font-size="13" fill="#f3f6fb">{{Esc(string.Join(" → ", transitions))}}</text>
</svg>
""";
}

internal sealed class MotionContext : IStateContext
{
    public string Name { get; set; } = "Gesture motion documentation";
    public bool IsValid { get; set; } = true;
    public bool StartRequested { get; set; }
    public double DurationSeconds { get; set; }
    public double ElapsedSeconds { get; set; }
    public string Phase { get; set; } = "Created";
}

internal sealed record Sample(
    double ElapsedFraction,
    double Progress,
    double X,
    double Y,
    double Scale,
    double RotationDegrees,
    bool IsComplete,
    string Phase);
