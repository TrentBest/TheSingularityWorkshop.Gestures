using System.Globalization;
using System.Numerics;
using System.Security;
using System.Text;
using TheSingularityWorkshop.Gestures;

// Every plotted mark and tooltip is derived from the CPU baker's returned instances.
const string profile = "starport-departure";
const string catalogue = "starport-activity-catalogue-v1";
const ulong seed = 42UL;
const int count = 96;
var region = new ExtremusRegion("starport-apron", new Vector3(-100f, -1f, -60f), new Vector3(100f, 1f, 60f));
var exclusions = new[]
{
    new ExtremusExclusion("ship-landing-footprint", new Vector3(-18f, -1f, -17f), new Vector3(18f, 1f, 17f)),
    new ExtremusExclusion("service-lane", new Vector3(-100f, -1f, 25f), new Vector3(100f, 1f, 34f))
};
var request = new ExtremusBakeRequest(profile, 1, catalogue, region, seed, count,
    [new ExtremusActivity("boarding", 3d), new ExtremusActivity("walking", 4d), new ExtremusActivity("waiting", 3d)], exclusions);
var result = ExtremusCrowdBaker.Bake(request);
var output = Path.Combine("docs", "assets", "extremus-crowd-bake.svg");
for (var i = 0; i < args.Length - 1; i++)
    if (args[i] == "--output") output = args[i + 1];
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
File.WriteAllText(output, BuildSvg(result, request), new UTF8Encoding(false));
Console.WriteLine($"Generated {output} from {result.Instances.Count} ExtremusCrowdBaker outputs.");

static string BuildSvg(ExtremusBakeResult result, ExtremusBakeRequest request)
{
    const double left = 82, top = 142, width = 870, height = 510;
    static string N(double v) => v.ToString("0.###", CultureInfo.InvariantCulture);
    static string E(string s) => SecurityElement.Escape(s) ?? "";
    double X(float x) => left + (x - request.Region.Minimum.X) / (request.Region.Maximum.X - request.Region.Minimum.X) * width;
    double Y(float z) => top + (request.Region.Maximum.Z - z) / (request.Region.Maximum.Z - request.Region.Minimum.Z) * height;
    var marks = new StringBuilder();
    var grid = new StringBuilder();
    foreach (var x in new[] { -100, -50, 0, 50, 100 })
    {
        var px = X(x);
        grid.Append($"<path d=\"M{N(px)} {top}V{top + height}\" stroke=\"#2b4057\" stroke-dasharray=\"3 7\"/>");
        grid.Append($"<text x=\"{N(px)}\" y=\"{top + height + 20}\" fill=\"#9fb4cb\" font-size=\"11\" text-anchor=\"middle\">{x} m</text>");
    }
    foreach (var z in new[] { -60, -30, 0, 30, 60 })
    {
        var py = Y(z);
        grid.Append($"<path d=\"M{left} {N(py)}H{left + width}\" stroke=\"#2b4057\" stroke-dasharray=\"3 7\"/>");
        grid.Append($"<text x=\"{left - 12}\" y=\"{N(py + 4)}\" fill=\"#9fb4cb\" font-size=\"11\" text-anchor=\"end\">{z} m</text>");
    }
    foreach (var exclusion in request.Exclusions)
    {
        var x = X(exclusion.Minimum.X); var y = Y(exclusion.Maximum.Z);
        marks.Append($"<rect x=\"{N(x)}\" y=\"{N(y)}\" width=\"{N(X(exclusion.Maximum.X) - x)}\" height=\"{N(Y(exclusion.Minimum.Z) - y)}\" fill=\"#ff6f76\" fill-opacity=\".15\" stroke=\"#ff858b\" stroke-dasharray=\"6 4\"><title>{E(exclusion.Id)} — excluded envelope</title></rect>");
    }
    var colors = new Dictionary<string, string> { ["boarding"] = "#78dcff", ["walking"] = "#8fe3b0", ["waiting"] = "#e8b84b" };
    foreach (var p in result.Instances)
    {
        var x = X(p.Position.X); var y = Y(p.Position.Z);
        marks.Append($"<circle cx=\"{N(x)}\" cy=\"{N(y)}\" r=\"{N(2.4 * p.Scale)}\" fill=\"{colors[p.ActivityId]}\" stroke=\"#eaf3fc\" stroke-opacity=\".5\"><title>{E(p.StableId)} · activity={E(p.ActivityId)} · x={N(p.Position.X)} m · z={N(p.Position.Z)} m · heading={N(p.HeadingRadians)} rad · scale={N(p.Scale)} · phase={N(p.Phase)}</title></circle>");
    }
    return $$"""
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1040 770" role="img" aria-labelledby="title desc">
      <title id="title">Extremus CPU crowd population generated from the live baker</title>
      <desc id="desc">Top-down projection of {{result.Instances.Count}} actual ExtremusCrowdBaker outputs. Cyan, green, and gold indicate returned activity assignments. Red envelopes are input exclusions. This is placement data, not animation, navigation, collision, or rendering simulation.</desc>
      <defs><linearGradient id="bg" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#101827"/><stop offset="1" stop-color="#17283b"/></linearGradient></defs>
      <rect width="1040" height="770" rx="18" fill="url(#bg)"/>
      <text x="32" y="34" font-size="11" letter-spacing="2" fill="#8da7c5">THE SINGULARITY WORKSHOP / GENERATED FROM C# OUTPUT</text>
      <text x="32" y="68" font-size="25" font-weight="700" fill="#f3f6fb">Extremus · a reproducible starport population</text>
      <text x="32" y="94" font-size="13" fill="#b9c8d9">Actual CPU bake · top-down X/Z projection · hover a mark for returned instance data</text>
      <g>{{grid}}</g><rect x="{{left}}" y="{{top}}" width="{{width}}" height="{{height}}" fill="none" stroke="#6e8299"/>
      <g>{{marks}}</g>
      <text x="517" y="697" text-anchor="middle" font-size="12" fill="#b9c8d9">X position (metres)</text>
      <g font-size="12" fill="#eaf3fc"><circle cx="82" cy="730" r="5" fill="#78dcff"/><text x="94" y="734">boarding</text><circle cx="194" cy="730" r="5" fill="#8fe3b0"/><text x="206" y="734">walking</text><circle cx="302" cy="730" r="5" fill="#e8b84b"/><text x="314" y="734">waiting</text><rect x="410" y="724" width="15" height="12" fill="#ff6f76" fill-opacity=".15" stroke="#ff858b" stroke-dasharray="3 2"/><text x="434" y="734">domain exclusion</text></g>
      <text x="1008" y="734" text-anchor="end" font-size="10" fill="#9fb4cb">seed={{request.Seed}} · algorithm v{{result.Manifest.AlgorithmVersion}} · count={{result.Manifest.BakedCount}}</text>
      <text x="32" y="756" font-size="10" fill="#e8b84b">PLACEMENT DATA ONLY — no pose bake, navigation, collision, physical outcome, or LOD transition is implied.</text>
    </svg>
    """;
}
