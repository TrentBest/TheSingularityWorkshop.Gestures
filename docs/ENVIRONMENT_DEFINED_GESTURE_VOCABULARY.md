# Environment-Defined Gesture Vocabulary

**Status: architecture example, not an implemented soccer simulation.** This document explains a reusable design pattern for the Workshop. The soccer actions below are illustrative; they are not claims that this repository currently contains a complete soccer gesture catalog, match rules, player AI, ball physics, or a Renderer integration.

## The idea

The environment supplies an acceptable vocabulary of physical procedures. Participants select from that vocabulary to pursue their own objectives.

A soccer field, for example, can expose a soccer-domain capability set: receive, trap, pass, dribble, shoot, clear, jockey, block a lane, intercept, tackle, and goalkeeper actions. Teams and players choose when and why to request those procedures based on their tactics, roles, assignments, and current match state.

The field does not need to dictate one fixed performance for every player. It supplies the domain and the available actions. A team can choose a strategy; a player can receive an assignment; a provider can parameterize a reusable procedure for a particular target, body rig, or interaction anchor.

**The environment defines what is available; the participant chooses what to do with it; the domain decides what actually happened.**

## Example: a pass

1. **Soccer domain / field:** publishes the available passing procedure and the contextual requirements for requesting it. The domain owns field boundaries, ball state, legal interactions, and match rules.
2. **Team strategy:** chooses a passing option based on the team's objective and current situation.
3. **Player role and task:** selects a receiver or target area and requests a pass procedure, if the request is valid in the current state.
4. **Procedure provider:** supplies the relevant ball/foot/target anchors, parameters, and gesture definition.
5. **Gesture evaluator:** evaluates the requested procedure at the current progress into an `EvaluatedPose`. The same procedure data can be reused by different players with separate identity, progress, and bounded variation.
6. **Host rig / Renderer:** maps the evaluated pose to the actual rig and chooses how to present it. The repository does not yet define a universal humanoid rig or a completed Renderer pose adapter.
7. **Soccer simulation:** owns the authoritative contact, ball impulse/trajectory, interception, possession, and resulting match state. The foot reaching the expected pose is not proof that the ball was struck or that the pass succeeded.

The same separation applies to a tackle: a tackle gesture can describe the intended movement, but the soccer simulation decides whether contact occurred, whether the ball changed possession, and whether a foul occurred.

## Ownership boundaries

| Layer | Responsibility |
| --- | --- |
| Environment / domain provider | Available gesture vocabulary, semantic anchors, contextual constraints, and domain-specific procedure data |
| Team / player decision-making | Strategy, role, target selection, and choice among available procedures |
| Router / orchestrator | Selects the next task or procedure from assignment, prerequisites, and current state |
| Gestures | Deterministic reference evaluation of pose-procedure data into an `EvaluatedPose` |
| Host rig adapter / Renderer | Maps named transforms to a concrete rig and presents the evaluated pose |
| Authoritative domain simulation | Rules, collision/contact, ball or equipment state, task outcomes, and success/failure evidence |

FSM_API or FSM_COS may be used by a consumer to orchestrate state transitions and task progression, but Gestures does not need either package as a core dependency. Likewise, the soccer domain need not be built into Gestures itself.

## A vocabulary is not a script

An available gesture is a reusable capability, not an instruction that every participant must perform it. Different teams can select different actions from the same domain vocabulary, and different players can use the same procedure with different targets and timing. The same team can choose another procedure when the situation changes.

The provider should describe the procedure and its valid inputs; the decision layer should choose it; the authoritative domain should validate requests and report outcomes. Avoid embedding a team's entire strategy into the low-level gesture definition.

Some procedures may be composed from smaller reusable procedures. For example, a player may orient, approach, receive the ball, and then choose to pass or dribble. That sequence belongs to a task/behavior plan, not necessarily to one indivisible Gesture.

## Reuse across domains

The same architecture pattern extends beyond soccer:

- **Star Port:** the station supplies task/procedure capabilities; the router sends ground and flight crew to role-specific providers; the host confirms equipment and occupancy outcomes.
- **Soccer:** the field/domain supplies the available sports actions; teams and players choose procedures; the match simulation owns ball and rule outcomes.
- **Robotics or industrial work:** the workcell exposes permitted operations and interaction anchors; the planner selects a procedure; sensors and the authoritative controller confirm the operation's result.

The gesture evaluator stays domain-neutral. Domain-specific meaning is carried by providers, parameters, anchors, task contracts, and outcome evidence.

## Design and validation rules

- Keep each participant's identity, assignment, progress, and interruption state independent even when procedure definitions are shared.
- Validate procedure requests against the current domain state; an available gesture is not permission to perform it in every circumstance.
- Keep physical outcomes distinct from pose progression. A completed `EvaluatedPose` does not authenticate contact, possession, successful interaction, or task completion.
- Make missing, failed, delayed, and indeterminate observations explicit. Do not silently convert missing evidence into success.
- Keep the core library independent of soccer, Star Port, a universal skeleton, a specific engine, and a GPU API.
- Treat this document as an architectural example until a concrete consumer, test fixture, rig contract, and authoritative outcome integration exist.
