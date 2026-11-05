using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Numerics;
using TheSingularityWorkshop.Gestures;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

public abstract class HierarchyBenchmarkBase
{
    private StatefulJointNode _fullRoot = null!;
    private StatefulJointNode _incrementalRoot = null!;
    private StatefulJointNode _leafChangeRoot = null!;
    private StatefulHinge _leafChangeHinge = null!;
    private float _nextLeafAngle = 0.1f;

    protected abstract int NodeCount { get; }
    protected abstract StatefulJointNode BuildHierarchy(string prefix, out StatefulHinge leafHinge);

    [GlobalSetup]
    public void Setup()
    {
        _fullRoot = BuildHierarchy("full", out _);
        _incrementalRoot = BuildHierarchy("incremental", out _);
        _leafChangeRoot = BuildHierarchy("leaf-change", out _leafChangeHinge);

        _fullRoot.EvaluateFull(Vector2.Zero);
        _incrementalRoot.EvaluateIncremental(Vector2.Zero);
        _leafChangeRoot.EvaluateIncremental(Vector2.Zero);
    }

    [Benchmark(Baseline = true)]
    public int FullReevaluation()
    {
        _fullRoot.EvaluateFull(Vector2.Zero);
        return NodeCount;
    }

    [Benchmark]
    public int IncrementalNoOp() => _incrementalRoot.EvaluateIncremental(Vector2.Zero);

    [Benchmark]
    public int IncrementalLeafChange()
    {
        _nextLeafAngle = -_nextLeafAngle;
        _leafChangeHinge.Advance(_nextLeafAngle, 1f, 1f);
        return _leafChangeRoot.EvaluateIncremental(Vector2.Zero);
    }

    protected static StatefulJointNode NewNode(string name) =>
        new(name, 1f, new StatefulHinge(0f, -MathF.PI, MathF.PI));
}

[MemoryDiagnoser]
public class ChainHierarchyBenchmarks : HierarchyBenchmarkBase
{
    [Params(10, 100, 1000)]
    public int JointCount { get; set; }

    protected override int NodeCount => JointCount;

    protected override StatefulJointNode BuildHierarchy(string prefix, out StatefulHinge leafHinge)
    {
        var root = NewNode($"{prefix}-0");
        var current = root;
        leafHinge = root.Hinge;
        for (var i = 1; i < JointCount; i++)
        {
            var child = NewNode($"{prefix}-{i}");
            current.AddChild(child);
            current = child;
            leafHinge = child.Hinge;
        }

        return root;
    }
}

[MemoryDiagnoser]
public class BranchingHierarchyBenchmarks : HierarchyBenchmarkBase
{
    [Params(10, 100, 1000, 10000)]
    public int JointCount { get; set; }

    protected override int NodeCount => JointCount;

    protected override StatefulJointNode BuildHierarchy(string prefix, out StatefulHinge leafHinge)
    {
        var nodes = new StatefulJointNode[JointCount];
        for (var i = 0; i < nodes.Length; i++)
            nodes[i] = NewNode($"{prefix}-{i}");

        for (var i = 1; i < nodes.Length; i++)
            nodes[(i - 1) / 2].AddChild(nodes[i]);

        leafHinge = nodes[^1].Hinge;
        return nodes[0];
    }
}


/// <summary>
/// Measures CPU reference pose evaluation at crowd scale with samples prepared before timing.
/// This isolates evaluator cost; it does not measure keyed RNG sampling, physics, rigging, or rendering.
/// </summary>
[MemoryDiagnoser]
public class CrowdGestureEvaluationBenchmarks
{
    private GestureDefinition _march = null!;
    private GestureVariationSample[] _samples = null!;
    private readonly GestureVariation _variation = new()
    {
        MaximumSpeedDeviation = 0.08d,
        MaximumLateralOffset = 0.12f
    };

    [Params(1_000, 10_000, 100_000, 1_000_000)]
    public int AgentCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _march = new GestureDefinition("benchmark.march.reference", new[]
        {
            new GestureKeyframe(0d, new Dictionary<string, PoseTransform>
            {
                ["root"] = PoseTransform.Identity
            }),
            new GestureKeyframe(0.5d, new Dictionary<string, PoseTransform>
            {
                ["root"] = new PoseTransform(new Vector3(0f, 0.5f, 0f), Quaternion.Identity, Vector3.One)
            }),
            new GestureKeyframe(1d, new Dictionary<string, PoseTransform>
            {
                ["root"] = new PoseTransform(new Vector3(0f, 0f, 0f), Quaternion.Identity, Vector3.One)
            })
        });

        // Deterministic, bounded fixture samples are prepared outside the timed benchmark.
        // They are not an RNG implementation and do not stand in for Physics.Squirrel3Keyed.
        _samples = new GestureVariationSample[AgentCount];
        for (var i = 0; i < _samples.Length; i++)
        {
            var speedUnit = ((i * 613L) % 997L) / 997d;
            var lateralUnit = ((i * 389L + 17L) % 991L) / 991d;
            _samples[i] = new GestureVariationSample(speedUnit, lateralUnit);
        }
        _variation.Validate();
    }

    [Benchmark(Baseline = true)]
    public double SharedPoseEvaluation()
    {
        var checksum = 0d;
        for (var i = 0; i < AgentCount; i++)
            checksum += GestureEvaluator.Evaluate(_march, 0.375d, 1d).NormalizedProgress;
        return checksum;
    }

    [Benchmark]
    public double PerAgentVariationEvaluation()
    {
        var checksum = 0d;
        for (var i = 0; i < AgentCount; i++)
            checksum += GestureEvaluator.EvaluateWithSample(
                _march, 0.375d, 1d, _samples[i], _variation).NormalizedProgress;
        return checksum;
    }
}
