using System.Globalization;
using System.Security;
using System.Text;
using TheSingularityWorkshop.Gestures;

// This tool serializes actual CrowdResponseScheduler outputs into a reviewable SVG.
// The scene's participant IDs and policy are declared fixture inputs; plotted times,
// variants, and intensities are produced by the library's real scheduler.
const string policyId = "balcony-reveal";
const int policyVersion = 1;
const int participantCount = 18;
var policy = new CrowdResponsePolicy(
    policyId, policyVersion,
    minimumDelaySeconds: 0.25, maximumDelaySeconds: 5.5,
    minimumVariant: 1, maximumVariant: 6,
    minimumIntensity: 0.2, maximumIntensity: 0.9);

var schedules = Enumerable.Range(1, participantCount)
    .Select(index => CrowdResponseScheduler.Schedule(
        new CrowdResponseRequest("speech-01", 12d, $"soldier-{index:000}", 42UL),
        policy))
    .OrderBy(schedule => schedule.IntentTimeSeconds)
    .ThenBy(schedule => schedule.ParticipantId, StringComparer.Ordinal)
    .ToArray();

if (schedules.Length != participantCount ||
    schedules.Any(schedule => schedule.IntentTimeSeconds < 12.25d || schedule.IntentTimeSeconds > 17.5d ||
                              schedule.Variant is < 1 or > 6 ||
                              schedule.Intensity is < 0.2d or > 0.9d))
    throw new InvalidOperationException("Scheduler output violated the declared scene policy.");

var output = GetOutputPath(args);
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
File.WriteAllText(output, BuildSvg(schedules), new UTF8Encoding(false));
Console.WriteLine($"Generated {output} from {schedules.Length} CrowdResponseScheduler outputs.");
Console.WriteLine($"Event=speech-01; policy={policyId}/v{policyVersion}; intent range={schedules[0].IntentTimeSeconds.ToString("0.000", CultureInfo.InvariantCulture)}..{schedules[^1].IntentTimeSeconds.ToString("0.000", CultureInfo.InvariantCulture)} seconds.");

static string GetOutputPath(string[] args)
{
    for (var i = 0; i < args.Length; i++)
        if (args[i] == "--output" && i + 1 < args.Length)
            return args[i + 1];
    return Path.Combine("docs", "assets", "crowd-response-schedule.svg");
}

static string BuildSvg(IReadOnlyList<CrowdResponseSchedule> schedules)
{
    const double left = 184d, right = 866d, top = 154d, rowHeight = 22d;
    const double startTime = 12d, endTime = 18d;
    static string N(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
    static string E(string value) => SecurityElement.Escape(value) ?? string.Empty;
    double X(double time) => left + (time - startTime) / (endTime - startTime) * (right - left);
    var rows = new StringBuilder();
    var grid = new StringBuilder();
    for (var second = 12; second <= 18; second++)
    {
        var x = X(second);
        grid.Append($"<path d=\"M{N(x)} 142V{N(top + schedules.Count * rowHeight)}\" stroke=\"#31465e\" stroke-dasharray=\"3 6\"/>");
        grid.Append($"<text x=\"{N(x)}\" y=\"132\" text-anchor=\"middle\" fill=\"#b9c8d9\" font-size=\"11\">{second}.0s</text>");
    }

    for (var i = 0; i < schedules.Count; i++)
    {
        var schedule = schedules[i];
        var y = top + i * rowHeight;
        var x = X(schedule.IntentTimeSeconds);
        var intensityWidth = 16d + schedule.Intensity * 74d;
        var color = schedule.Variant switch
        {
            1 => "#78dcff",
            2 => "#8fe3b0",
            3 => "#e8b84b",
            4 => "#f5a7ff",
            5 => "#ff9878",
            _ => "#a9b8ff"
        };
        rows.Append($$"""
        <g>
          <text x="34" y="{{N(y + 4)}}" fill="#d9e7f5" font-size="11" font-family="Consolas,monospace">{{E(schedule.ParticipantId)}}</text>
          <path d="M{{N(left)}} {{N(y)}}H{{N(right)}}" stroke="#25384d" stroke-width="1"/>
          <path d="M{{N(X(startTime))}} {{N(y)}}H{{N(x)}}" stroke="{{color}}" stroke-width="2" opacity=".62"/>
          <circle cx="{{N(x)}}" cy="{{N(y)}}" r="5" fill="{{color}}" stroke="#f3f6fb" stroke-width="1"/>
          <rect x="888" y="{{N(y - 5)}}" width="{{N(intensityWidth)}}" height="10" rx="3" fill="{{color}}" opacity=".9"/>
          <text x="974" y="{{N(y + 4)}}" fill="#d9e7f5" font-size="10">{{schedule.Variant}}</text>
          <title>{{E(schedule.ParticipantId)}}: intent {{N(schedule.IntentTimeSeconds)}} s; delay {{N(schedule.DelaySeconds)}} s; variant {{schedule.Variant}}; intensity {{N(schedule.Intensity)}}</title>
        </g>
        """);
    }

    return $$"""
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1040 690" role="img" aria-labelledby="title desc">
      <title id="title">Living crowd-response schedule generated from CrowdResponseScheduler</title>
      <desc id="desc">Eighteen stable soldier identities scheduled by the actual CPU scheduler for event speech-01 and policy balcony-reveal version 1. Each row plots its returned intent time. The colored bar indicates the returned intensity and the number is the returned gesture variant. This is intent data, not a simulation of physical movement or acoustic arrival.</desc>
      <defs>
        <linearGradient id="bg" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#101827"/><stop offset="1" stop-color="#17283b"/></linearGradient>
        <style>text{font-family:"Segoe UI",Arial,sans-serif}.mono{font-family:Consolas,monospace}</style>
      </defs>
      <rect width="1040" height="690" rx="18" fill="url(#bg)"/>
      <text x="34" y="34" font-size="11" letter-spacing="2" fill="#8da7c5">THE SINGULARITY WORKSHOP / GENERATED FROM C# OUTPUT</text>
      <text x="34" y="68" font-size="25" font-weight="700" fill="#f3f6fb">One balcony reveal. Eighteen individual response times.</text>
      <text x="34" y="94" font-size="13" fill="#b9c8d9">Real CrowdResponseScheduler results · sorted for display only · deterministic input fixture</text>
      <g font-size="11" fill="#d9e7f5">
        <text x="34" y="119">STABLE PARTICIPANT</text>
        <text x="{{N(left)}}" y="119">INTENT TIME (EVENT ORIGIN = 12.0 s)</text>
        <text x="888" y="119">INTENSITY</text>
        <text x="974" y="119">VAR.</text>
      </g>
      {{grid}}
      {{rows}}
      <path d="M34 570H1006" stroke="#31465e" stroke-width="2"/>
      <text x="34" y="597" font-size="12" fill="#78dcff" letter-spacing="1.5">POLICY / REPRODUCTION INPUTS</text>
      <text x="34" y="620" font-size="12" fill="#f3f6fb">event=speech-01 · policy=balcony-reveal/v1 · stable seed=42 per participant request</text>
      <text x="34" y="642" font-size="12" fill="#b9c8d9">Delay bounds: 0.25–5.5 s · variants: 1–6 · intensity: 0.2–0.9 · participants: {{schedules.Count}}</text>
      <text x="34" y="665" font-size="11" fill="#e8b84b">INTENT ONLY — no completed gesture, physical outcome, or acoustic arrival is implied.</text>
    </svg>
    """;
}
