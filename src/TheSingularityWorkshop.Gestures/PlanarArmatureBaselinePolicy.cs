using System.Numerics;

namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// Deterministic, non-learned baseline that converts an end-effector target into joint-angle targets
/// using the existing constrained two-link inverse-kinematics reference.
/// </summary>
public static class PlanarArmatureBaselinePolicy
{
    /// <summary>
    /// Produces the next joint-target action for a measured armature observation.
    /// The returned action is a kinematic command; it does not represent force or torque.
    /// </summary>
    public static PlanarArmatureAction CreateAction(
        PlanarArmatureDefinition definition,
        PlanarArmatureObservation observation,
        Vector2 worldTarget)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (observation.ContractVersion != StatefulPlanarArmature.CurrentContractVersion)
            throw new ArgumentException("Unsupported observation contract version.", nameof(observation));
        if (!StringComparer.Ordinal.Equals(observation.ScenarioId, definition.ScenarioId))
            throw new ArgumentException("Observation scenario does not match the armature definition.", nameof(observation));
        if (string.IsNullOrWhiteSpace(observation.EpisodeId))
            throw new ArgumentException("Observation must identify an active episode.", nameof(observation));
        if (observation.StepIndex < 0)
            throw new ArgumentException("Observation step index must not be negative.", nameof(observation));
        if (!StringComparer.Ordinal.Equals(observation.ShoulderJointId, definition.ShoulderJointId)
            || !StringComparer.Ordinal.Equals(observation.ElbowJointId, definition.ElbowJointId)
            || !StringComparer.Ordinal.Equals(observation.EndEffectorId, definition.EndEffectorId))
            throw new ArgumentException("Observation identities do not match the armature definition.", nameof(observation));
        if (observation.RootAnchor != definition.RootAnchor)
            throw new ArgumentException("Observation root anchor does not match the armature definition.", nameof(observation));
        if (!float.IsFinite(worldTarget.X) || !float.IsFinite(worldTarget.Y))
            throw new ArgumentOutOfRangeException(nameof(worldTarget), "Target coordinates must be finite.");

        var localTarget = worldTarget - definition.RootAnchor;
        if (!float.IsFinite(localTarget.X) || !float.IsFinite(localTarget.Y))
            throw new ArgumentOutOfRangeException(nameof(worldTarget), "Target offset must be finite.");

        var solution = PlanarKinematics.SolveTwoLink(
            definition.ShoulderLinkLength,
            definition.ElbowLinkLength,
            localTarget,
            definition.ShoulderLimits,
            definition.ElbowLimits);

        return new PlanarArmatureAction(
            StatefulPlanarArmature.CurrentContractVersion,
            observation.EpisodeId,
            observation.StepIndex,
            PlanarArmatureActionMode.JointAngleTargets,
            solution.ShoulderRadians,
            solution.ElbowRadians);
    }
}
