using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

/// <summary>
/// Test-backed reference scenario for the VR magic-weaving concept. Gesture evaluation
/// is real; the weave rules below are a deliberately small consumer-owned acceptance model.
/// </summary>
public sealed class MagicSpellGestureTests
{
    [Fact]
    public void ElementReachAndChannelProceduresUseTheRealGestureEvaluator()
    {
        var reach = CreateProcedure("magic.reach-element", "hand", "element-node");
        var channel = CreateProcedure("magic.channel-to-elbow", "forearm", "elbow");
        var arc = CreateProcedure("magic.arc-to-casting-sphere", "hand", "casting-sphere");

        Assert.Equal("element-node", Assert.Single(reach.Anchors).Name);
        Assert.Equal("elbow", Assert.Single(channel.Anchors).Name);
        Assert.Equal("casting-sphere", Assert.Single(arc.Anchors).Name);

        var reachPose = GestureEvaluator.Evaluate(reach, 0.5d, 1d);
        var channelPose = GestureEvaluator.Evaluate(channel, 0.5d, 1d);
        var arcPose = GestureEvaluator.Evaluate(arc, 0.5d, 1d);

        Assert.Equal(0.5d, reachPose.NormalizedProgress);
        Assert.Equal(0.5d, channelPose.NormalizedProgress);
        Assert.Equal(0.5d, arcPose.NormalizedProgress);
        Assert.Contains("hand", reachPose.LocalTransforms.Keys);
        Assert.Contains("forearm", channelPose.LocalTransforms.Keys);
        Assert.Contains("hand", arcPose.LocalTransforms.Keys);
        Assert.False(reachPose.IsComplete);
    }

    [Fact]
    public void SpellRequiresAllFiveElementsAndAValidSpiritAlignment()
    {
        var spell = CreateSpell();
        var incomplete = new[]
        {
            new SpellThread(MagicElement.Earth, "#9A6A3A", 0.5f),
            new SpellThread(MagicElement.Air, "#DDEEFF", 0.5f),
            new SpellThread(MagicElement.Water, "#2588DD", 0.5f),
            new SpellThread(MagicElement.Fire, "#FF5A22", 0.5f)
        };

        Assert.False(spell.TryAccept(incomplete, triggerReleased: true, out _));
        Assert.False(spell.TryAccept(
            [.. incomplete, new SpellThread(MagicElement.Spirit, "#C7A7FF", 0.5f, (SpiritAlignment)99)],
            triggerReleased: true, out _));
    }

    [Fact]
    public void TriggerReleaseRequestsCastButDoesNotBypassThicknessRules()
    {
        var spell = CreateSpell();
        var weave = ValidWeave();
        var rejected = weave.Select(thread => thread.Element == MagicElement.Fire
            ? thread with { Thickness = 1.4f }
            : thread).ToArray();

        Assert.False(spell.TryAccept(weave, triggerReleased: false, out _));
        Assert.True(spell.TryAccept(weave, triggerReleased: true, out _));
        Assert.False(spell.TryAccept(rejected, triggerReleased: true, out _));
    }

    [Fact]
    public void PermittedThicknessVariationCanRepresentAmplification()
    {
        var spell = CreateSpell();
        var low = ValidWeave(thickness: 0.4f);
        var high = ValidWeave(thickness: 0.9f);

        Assert.True(spell.TryAccept(low, triggerReleased: true, out _));
        Assert.True(spell.TryAccept(high, triggerReleased: true, out _));
        Assert.True(high.Sum(thread => thread.Thickness) > low.Sum(thread => thread.Thickness));
    }

    [Fact]
    public void NonFiniteThicknessIsRejected()
    {
        var invalid = ValidWeave().Select(thread => thread.Element == MagicElement.Water
            ? thread with { Thickness = float.NaN }
            : thread).ToArray();

        Assert.False(CreateSpell().TryAccept(invalid, triggerReleased: true, out _));
    }

    private static GestureDefinition CreateProcedure(string id, string transformName, string anchorName)
    {
        return new GestureDefinition(id,
        [
            new GestureKeyframe(0d, new Dictionary<string, PoseTransform>
            {
                [transformName] = At(0f, 0f, 0f)
            }),
            new GestureKeyframe(1d, new Dictionary<string, PoseTransform>
            {
                [transformName] = At(0.4f, 0.6f, 0.2f)
            })
        ],
        [new GestureAnchor(anchorName, transformName, At(0f, 0f, 0f))]);
    }

    private static SpellWeaveDefinition CreateSpell() => new(
        minimumThickness: 0.25f,
        maximumThickness: 1.0f,
        requiredElements: [MagicElement.Earth, MagicElement.Air, MagicElement.Water, MagicElement.Fire, MagicElement.Spirit]);

    private static SpellThread[] ValidWeave(float thickness = 0.5f) =>
    [
        new(MagicElement.Earth, "#9A6A3A", thickness),
        new(MagicElement.Air, "#DDEEFF", thickness),
        new(MagicElement.Water, "#2588DD", thickness),
        new(MagicElement.Fire, "#FF5A22", thickness),
        new(MagicElement.Spirit, "#C7A7FF", thickness, SpiritAlignment.Neutral)
    ];

    private static PoseTransform At(float x, float y, float z) =>
        new(new Vector3(x, y, z), Quaternion.Identity, Vector3.One);
}

internal enum MagicElement { Earth, Air, Water, Fire, Spirit }
internal enum SpiritAlignment { Good, Neutral, Evil }

internal sealed record SpellThread(
    MagicElement Element,
    string Color,
    float Thickness,
    SpiritAlignment? Alignment = null);

internal sealed class SpellWeaveDefinition(
    float minimumThickness,
    float maximumThickness,
    IReadOnlyList<MagicElement> requiredElements)
{
    public bool TryAccept(
        IReadOnlyList<SpellThread> threads,
        bool triggerReleased,
        out string reason)
    {
        if (!triggerReleased)
        {
            reason = "Trigger has not been released.";
            return false;
        }

        if (threads.Count != requiredElements.Count ||
            !threads.Select(thread => thread.Element).SequenceEqual(requiredElements))
        {
            reason = "Weave does not contain the required element sequence.";
            return false;
        }

        foreach (var thread in threads)
        {
            if (!float.IsFinite(thread.Thickness) ||
                thread.Thickness < minimumThickness ||
                thread.Thickness > maximumThickness)
            {
                reason = $"Thread thickness is outside [{minimumThickness}, {maximumThickness}].";
                return false;
            }

            if (thread.Element == MagicElement.Spirit &&
                thread.Alignment is not (SpiritAlignment.Good or SpiritAlignment.Neutral or SpiritAlignment.Evil))
            {
                reason = "Spirit requires good, neutral, or evil alignment.";
                return false;
            }

            if (thread.Element != MagicElement.Spirit && thread.Alignment is not null)
            {
                reason = "Only Spirit may carry an alignment.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(thread.Color))
            {
                reason = "Thread color metadata is required.";
                return false;
            }
        }

        reason = "Accepted.";
        return true;
    }
}
