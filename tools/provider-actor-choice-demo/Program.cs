using System.Security;
using System.Text;
using System.Text.Json;

// Tool-local executable prototype; not a frozen public API.
// Provider capabilities offer possibilities. Actor goals filter and score them.
// The fixture emits both an SVG and a machine-readable trace from the same records.
var provider = new Provider("pet-store-window", 1, new[]
{
    new Capability("look-at-puppies", "curious", 2),
    new Capability("enter-store", "store-interest", 3),
    new Capability("buy-puppy", "companion-goal", 10),
    new Capability("buy-dog-food", "owns-dog", 5),
    new Capability("visit-dog-park", "owns-dog", 4)
});
var actors = new[]
{
    new Actor("digiten-mira", 42UL, new[] { "companion", "care" }, 80, true, false),
    new Actor("digiten-kellan", 42UL, new[] { "work" }, 15, true, false)
};
var decisions = new List<Decision>();
foreach (var actor in actors)
{
    var candidates = provider.Capabilities.Select(c =>
    {
        var reason = Reject(actor, c);
        return new Decision(actor.Id, c.Id, reason == "eligible", reason,
            reason == "eligible" ? Score(actor, c) : int.MinValue, false);
    }).ToList();
    var chosen = candidates.Where(x => x.Eligible)
        .OrderByDescending(x => x.Score)
        .ThenBy(x => StableTie(actor.Seed, x.CapabilityId))
        .ThenBy(x => x.CapabilityId, StringComparer.Ordinal).FirstOrDefault();
    if (chosen is not null)
    {
        var index = candidates.FindIndex(x => x.CapabilityId == chosen.CapabilityId);
        candidates[index] = chosen with { Selected = true };
    }
    decisions.AddRange(candidates);
}
var outcomes = new[]
{
    new Outcome("digiten-mira", "enter-store", "attempted", "front-door gate evaluated"),
    new Outcome("digiten-mira", "approach-counter", "attempted", "puppy-counter anchor resolved"),
    new Outcome("digiten-mira", "purchase-puppy", "succeeded", "fixture outcome: payment accepted; ownership event emitted"),
    new Outcome("digiten-kellan", "provider-choice", "no-purchase", "work goal did not select puppy purchase")
};
if (!decisions.Any(x => x.ActorId == "digiten-mira" && x.CapabilityId == "buy-puppy" && x.Selected) ||
    decisions.Any(x => x.ActorId == "digiten-kellan" && x.Selected) ||
    decisions.Any(x => x.CapabilityId is "buy-dog-food" or "visit-dog-park" && x.Eligible))
    throw new InvalidOperationException("Actor-choice fixture violated its expected filters or selection.");
var replay = actors.Select(a => Select(provider, a)).ToArray();
if (!replay.SequenceEqual(actors.Select(a => Select(provider, a))))
    throw new InvalidOperationException("Selection was not deterministic.");
foreach (var actor in actors)
{
    var recorded = decisions.FirstOrDefault(x => x.ActorId == actor.Id && x.Selected)?.CapabilityId ?? "none";
    if (recorded != Select(provider, actor))
        throw new InvalidOperationException($"Decision trace disagrees with replay for {actor.Id}.");
}
var output = GetOutputPath(args);
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
File.WriteAllText(output, Svg(provider, actors, decisions, outcomes), new UTF8Encoding(false));
File.WriteAllText(Path.ChangeExtension(output, ".json"), JsonSerializer.Serialize(new
{
    fixture = "pet-store-actor-choice", status = "prototype, not public API",
    provider, actors, decisions, outcomes
}, new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
Console.WriteLine($"Generated {output} and JSON trace from {decisions.Count} actor/capability evaluations.");
Console.WriteLine("Checks passed: deterministic selection, distinct actor intent, ownership gates, nested outcomes.");

static string Reject(Actor a, Capability c) => c.Requirement switch
{
    "curious" when !a.Goals.Contains("companion") && !a.Goals.Contains("care") => "filtered: no relevant goal",
    "store-open" when !a.StoreOpen => "blocked: store closed",
    "store-interest" when !a.Goals.Contains("companion") && !a.Goals.Contains("care") => "filtered: no relevant store goal",
    "store-interest" when !a.StoreOpen => "blocked: store closed",
    "companion-goal" when !a.Goals.Contains("companion") => "filtered: no companion goal",
    "companion-goal" when a.Cash < 50 => "blocked: insufficient cash",
    "owns-dog" when !a.OwnsDog => "blocked: actor does not own a dog",
    _ => "eligible"
};
static int Score(Actor a, Capability c) => (c.Id, a.Goals.Contains("companion"), a.Goals.Contains("care")) switch
{
    ("buy-puppy", true, _) => 100,
    ("buy-dog-food", _, true) => 80,
    ("visit-dog-park", _, true) => 70,
    ("look-at-puppies", _, true) => 30,
    ("look-at-puppies", true, _) => 30,
    ("enter-store", true, _) => 25,
    ("enter-store", _, true) => 25,
    _ => c.Priority
};
static ulong StableTie(ulong seed, string id)
{
    var hash = 14695981039346656037UL ^ seed;
    foreach (var b in Encoding.UTF8.GetBytes(id)) hash = (hash ^ b) * 1099511628211UL;
    return hash;
}
static string Select(Provider p, Actor a) => p.Capabilities.Where(c => Reject(a,c) == "eligible")
    .OrderByDescending(c => Score(a,c)).ThenBy(c => StableTie(a.Seed,c.Id))
    .ThenBy(c => c.Id,StringComparer.Ordinal).Select(c => c.Id).FirstOrDefault() ?? "none";
static string GetOutputPath(string[] a) {
    for (var i=0;i<a.Length-1;i++) if (a[i]=="--output") return a[i+1];
    return Path.Combine("docs","assets","provider-actor-choice.svg");
}
static string Svg(Provider p, IReadOnlyList<Actor> actors, IReadOnlyList<Decision> ds, IReadOnlyList<Outcome> os) {
    static string E(string s)=>SecurityElement.Escape(s)??"";
    var b=new StringBuilder("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 1120 760\" role=\"img\" aria-labelledby=\"title desc\">");
    b.Append("<title id=\"title\">Provider offers filtered by actor perspective</title><desc id=\"desc\">Generated from deterministic decision and outcome records.</desc><rect width=\"1120\" height=\"760\" rx=\"18\" fill=\"#101827\"/>");
    b.Append("<text x=\"32\" y=\"38\" fill=\"#8da7c5\" font-size=\"11\">GESTURES / EXECUTABLE REFERENCE FIXTURE</text><text x=\"32\" y=\"74\" fill=\"#f3f6fb\" font-size=\"25\">One provider. Two actors. Different choices.</text>");
    b.Append($"<text x=\"32\" y=\"103\" fill=\"#b9c8d9\" font-size=\"13\">Provider {E(p.Id)} v{p.Version} · deterministic seed fixture · generated from decision records</text>");
    b.Append("<text x=\"32\" y=\"145\" fill=\"#78dcff\" font-size=\"13\">ACTOR FILTER / CANDIDATE DECISIONS</text>");
    var y=178;
    foreach(var a in actors) {
        b.Append($"<text x=\"32\" y=\"{y}\" fill=\"#e8b84b\" font-size=\"13\">{E(a.Id)} · goals: {E(string.Join(", ",a.Goals))}</text>"); y+=22;
        foreach(var d in ds.Where(x=>x.ActorId==a.Id)) {
            var color=d.Selected?"#8fe3b0":d.Eligible?"#78dcff":"#f5a7ff";
            b.Append($"<rect x=\"24\" y=\"{y-15}\" width=\"1072\" height=\"23\" rx=\"4\" fill=\"#17283b\"/><text x=\"40\" y=\"{y}\" fill=\"#f3f6fb\" font-size=\"12\">{E(d.CapabilityId)}</text><text x=\"280\" y=\"{y}\" fill=\"{color}\" font-size=\"11\">{(d.Selected?"SELECTED":d.Eligible?"ELIGIBLE":"EXCLUDED")}: {E(d.Reason)}</text><text x=\"970\" y=\"{y}\" fill=\"{color}\" font-size=\"11\">{(d.Eligible?d.Score.ToString():"—")}</text>");
            y+=27;
        } y+=10;
    }
    b.Append($"<text x=\"32\" y=\"{y}\" fill=\"#8fe3b0\" font-size=\"13\">NESTED PROCEDURE / AUTHORITATIVE OUTCOME</text>");
    foreach(var o in os) { y+=20; b.Append($"<text x=\"40\" y=\"{y}\" fill=\"#d9e7f5\" font-size=\"11\">{E(o.ActorId)} · {E(o.Step)} · {E(o.Status)} — {E(o.Evidence)}</text>"); }
    return b.Append("</svg>").ToString();
}
internal sealed record Capability(string Id,string Requirement,int Priority);
internal sealed record Provider(string Id,int Version,IReadOnlyList<Capability> Capabilities);
internal sealed record Actor(string Id,ulong Seed,string[] Goals,int Cash,bool StoreOpen,bool OwnsDog);
internal sealed record Decision(string ActorId,string CapabilityId,bool Eligible,string Reason,int Score,bool Selected);
internal sealed record Outcome(string ActorId,string Step,string Status,string Evidence);
