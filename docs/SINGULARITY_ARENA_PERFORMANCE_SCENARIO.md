# Singularity Arena — Performance, Karaoke, and Audience Gestures

**Status: domain scenario and implementation plan.** This document extends the environment-defined Gesture vocabulary into a second venue event type: live performance. The current Gestures evaluator is a CPU reference for named local transforms and deterministic bounded variation. This scenario does not claim that a complete concert simulation, audio engine, singer controller, crowd simulator, venue model, or Renderer adapter already exists.

**Cross-repository progress:** [MicroBundleDomain's Singularity Arena fixture](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain/blob/work/soccer-domain-microbundle/docs/SINGULARITY_ARENA_PERFORMANCE.md) now demonstrates an in-memory shared venue schedule for soccer and concerts, validated event lifecycle transitions, a deterministic concert cue timeline, and individual audience-intention projection. Gestures tests now evaluate representative lead-vocal and audience-clap definitions through the real CPU evaluator. These are executable reference fixtures, not a live concert.

The pose-evaluation proof lives in [ConcertPerformanceGestureTests.cs](../tests/TheSingularityWorkshop.Gestures.Tests/ConcertPerformanceGestureTests.cs). It exercises the current public GestureDefinition and GestureEvaluator contracts directly rather than assuming a new concert-specific API.

## The proving ground

Singularity Arena is one venue with a reusable event schedule, spatial layout, and participant flow. Soccer is one event type; concerts are another. The venue can schedule an event, admit participants, assign stage and audience anchors, and publish event context. Event-specific MicroBundles provide the vocabulary and rules for that event.

The same physical Gesture model should support both a goalkeeper preparing to dive and a lead singer leaning into a chorus, without making the Gestures core know what soccer or music is.

The design target is not a crowd that appears organic because every person receives a random animation. It is a crowd whose motion is the visible consequence of many independent participants making context-sensitive choices and executing imperfect, stateful physical procedures.

> **Environment offers procedures. Participants choose intentions. Gestures evaluates physical motion. The authoritative domain decides what actually happened.**

## Event scheduling and venue ownership

Scheduling belongs to the Arena / event domain, not to the Gestures evaluator.

A first event contract needs:
- a stable event ID and event type;
- start/end time, lifecycle state, and schedule revision;
- venue and event-space anchors;
- capacity and admission policy;
- event-specific provider or MicroBundle identity/version;
- a timeline of authoritative cues and outcomes;
- independent participant assignment, location, and current procedure state.

The event lifecycle should distinguish at least **Draft**, **Scheduled**, **DoorsOpen**, **InProgress**, **Paused**, **Completed**, and **Cancelled**. Transitions must be validated by the event owner; a UI timer reaching zero is not by itself proof that a performance or match completed.

A schedule can host a soccer match, a concert, a karaoke session, a rehearsal, or another event without changing the core Gesture data contract. Event providers define their own cue vocabulary. A concert cue such as **ChorusStarted** is not interchangeable with a soccer outcome such as **GoalAwarded**, although both can trigger a new set of participant intentions.

## Karaoke: the player can be the lead performer

The participant may choose a song and role, walk to a stage mark, take a microphone, sing lead, perform backing vocals, play an instrument, or leave the stage. The selected role determines which procedures are available, but does not dictate one fixed performance.

A karaoke / performance provider could expose the following reusable procedures:

| Procedure | Example named anchors | What the Gesture describes | What another system must verify |
| --- | --- | --- | --- |
| Approach stage mark | stage-entry, performer-mark | Locomotion and arrival posture | Navigation, collision, stage access |
| Take microphone | microphone-grip, mic-stand, hand-target | Reach, grasp attempt, lift, settle | Reachability, contact, attachment/ownership |
| Sing lead phrase | microphone-mouth, head, chest, torso | Phrase-level posture, head/torso motion, hand position | Vocal/audio generation, lyrics, timing, breath model if present |
| Sing to audience | audience-focus, stage-front | Gaze/orientation and expressive torso/arm motion | Sightline and actual performer-facing target |
| Move across stage | stage-waypoint, safe-stage-region | Step and weight-shift procedure | Locomotion authority, footing, obstacles, edge safety |
| Play instrument | instrument-grip, fret/keyboard/drum anchors | Hand and body procedure for the instrument | Instrument contact, sound generation, note/event timing |
| Backing-vocal phrase | shared-cue, microphone-mouth | A role-specific phrase procedure | Audio mix and cue alignment |
| Call-and-response | audience-focus, cue-marker | Gesture that invites a response | Audience event policy and cue timing |
| Acknowledge applause | stage-front, audience-focus | Bow, wave, nod, or other selected acknowledgement | Whether applause/response is actually present |
| Recover / reset | safe-stage-region, instrument-rest | Return to a stable usable posture | Balance/contact and safe destination |

These are provider-level procedure names, not new public Gestures APIs. A procedure is parameterized by the participant's identity, current pose, chosen role, available anchors, and context. A lead singer can use the same procedure definition as another performer while retaining separate progress, variation seed, targets, and interaction outcomes.

**Voice and body are related but distinct outputs.** Gestures can describe body posture and movement. It must not pretend that a mouth pose produces a voice, that a hand movement generates a note, or that a gesture reaching its final keyframe proves the audio was synchronized. An audio/voice owner generates or plays sound and supplies timing cues; a music/performance owner determines the phrase, beat, instrument events, and whether the performer is on time.

## Audience gestures: a crowd made of participants

A spectator is not a particle that plays an animation when a global event arrives. Each audience member has independent durable identity and a current context, for example:
- seat or standing-space anchor, section, and permitted movement region;
- facing direction, sightline, distance to stage, and whether the performer is visible;
- current activity (watching, talking, singing along, clapping, filming where allowed, getting refreshments, returning to a seat);
- musical familiarity and preference, social relationships, accessibility requirements, fatigue, and personal expressiveness;
- current Gesture, progress, interruption state, and last relevant cue observed.

These are example state dimensions, not mandatory fields to add to the core library. A venue or participant provider owns which dimensions exist and how they are persisted.

Possible audience procedures include:
- settle into a seat or standing position;
- orient attention toward the performer or a screen;
- sing along with a familiar phrase;
- clap with a beat, including bounded timing offset;
- cheer, whistle, or call out where appropriate;
- raise a hand or wave;
- sway, bounce, or dance within available space;
- turn toward a friend and react socially;
- stand for a high-energy moment if venue conditions permit;
- sit back down, rest, or continue watching quietly;
- leave for concessions and navigate back;
- acknowledge the end of a song or show.

A quiet spectator is not a failed spectator. The system should preserve multiple valid reactions, including no visible reaction. Accessibility, seat constraints, obstructed views, fatigue, personal preference, and safety rules can all change the available choice set.

## How a response becomes organic

Use deterministic variation and context-driven choice, not per-frame randomness.

1. **Observe an authoritative cue.** Examples: song section changed, performer began a chorus, a musical break began, a goal was awarded, or the show ended. The event owner emits the cue with a stable ID, event time/step, and provenance.
2. **Build each participant's context.** Check whether the cue is visible/audible to that participant, their current activity, location, social context, and available procedures.
3. **Choose an intention independently.** A policy may choose sing-along, clap, cheer, dance, watch, talk, or remain still. The policy owns preference and social behavior; the Gesture evaluator does not choose the intention.
4. **Request a physical procedure.** The provider supplies the relevant definition, anchors, target, and parameters. It must check venue constraints and whether the procedure can begin from the current state.
5. **Evaluate motion over time.** Gestures evaluates a pose for the participant's own progress and stable variation seed. It does not reset every participant to the same phase or use fresh randomness every frame.
6. **Observe outcomes and continue.** The host reports success, interruption, missed contact, blocked movement, or other relevant observations. The policy can continue, adapt, or recover from actual outcomes.
7. **Retain independent state.** Reusing a definition never means sharing the same progress, pose, location, or outcome state.

A simple deterministic response delay can be derived from stable participant identity and event context. It should be bounded, reproducible, and treated as a scheduling parameter—not secretly baked into the pose evaluator. Different people then respond at different times without losing repeatability in tests or replay.

Social influence can make nearby friends more likely to join in, but it should be bounded and local. It must not cause every member of a section to switch together. The model should preserve a distribution of responses and allow nonparticipation.

## Timing and performance cues

The performance owner should publish cues in a stable event-time domain rather than relying on each client to infer a chorus from wall-clock time. A cue can include:
- event ID and monotonic cue ID;
- musical section or cue kind;
- event-time position or simulation tick;
- optional beat/bar/phrase index;
- performer or instrument role;
- whether the cue is authoritative, predicted, corrected, or cancelled.

Audio playback, lyrics, vocal synthesis, instrument sound, and beat tracking are separate concerns. If audio playback drifts or a cue is corrected, the host needs a policy for resynchronizing body motion without teleporting a performer or restarting every spectator. Gesture progress should be explicit and recoverable; it should not assume that elapsed wall-clock time is the authoritative song position.

For a first implementation, use a deterministic scripted cue timeline and a fake audio clock in tests. That is an honest integration fixture, not an audio engine. Later, connect a real audio/performance owner through a documented cue adapter.

## Shared venue, distinct event providers

A venue provides geometry and operational constraints; it does not own the meaning of every event.

- **Arena / venue provider:** stage, field, seats, aisles, safe standing zones, backstage, entry/exit, accessible routes, interaction anchors, occupancy and capacity.
- **Event scheduler:** event identity, schedule, lifecycle, doors-open policy, event-time origin, event-type provider selection.
- **Soccer provider:** match phases and authoritative match outcomes.
- **Concert provider:** song/section cues, performer roles, instrument/voice coordination, show lifecycle.
- **Participant / Digiten policy:** goals, preferences, assignments, context-sensitive choice, interruption and recovery decisions.
- **Gestures:** reusable physical procedure definitions and deterministic pose evaluation.
- **Physics / world simulation:** actual movement authority, contact, balance, collisions, occupancy, reachability, and physical outcomes.
- **Audio / music subsystem:** voice, playback, instrument sound, beat/phrase timing, mixing.
- **Renderer / Visualize:** presentation, observer-relative fidelity, and pixels.
- **WebPage:** event discovery, scheduling controls, performer setup, audience overview, and truthful status display.

FSM_API or FSM_COS may orchestrate a consumer's event lifecycle and procedure routing, but the Gestures core must remain independent of those packages. The MicroBundle composition layer can bind the relevant providers for a scheduled event; it must not turn Gestures into a venue scheduler or audio engine.

## First implementation sequence

### Milestone A — Make the concept executable without pretending it is complete
- Add a small deterministic concert cue fixture: doors open, performer takes stage, verse, chorus, instrumental break, final chorus, encore decision, show complete.
- Define stable cue records and test that replaying the same cue sequence produces the same participant intentions for the same inputs.
- Use a small audience with deliberately different preferences and roles. Include at least one quiet observer, one enthusiastic singer-along, one clapper, one friend pair, and one participant who leaves and returns.
- Reuse the current GestureDefinition / GestureEvaluator contract for a few real procedure definitions, after checking the current API; do not invent an assumed public API.
- Record which parts are real evaluated poses and which are fixture observations.

### Milestone B — Demonstrate physical procedure, not just cue-to-label mapping
- Evaluate stage approach, microphone pickup, lead-vocal posture, audience clapping, singing along, and settling back to watching.
- Add a host-side pose/anchor adapter only after its transform hierarchy and composition contract are specified.
- Add interruption cases: blocked stage path, microphone not reachable, performer loses target, participant is seated, aisle occupied, or cue arrives during another procedure.
- Keep domain outcomes explicit. A microphone pickup gesture can fail to contact; a clap gesture does not prove a synchronized audio beat; a final pose does not prove a show ended.

### Milestone C — Verify the audience
- Test stable repeatability, independent progress, bounded reaction delays, non-uniform response phases, and no global synchronized reset.
- Test cue cancellation/correction, repeated cue delivery, late arrival, leaving and returning, and participant disconnection.
- Measure the distribution of reaction types and timing. Do not call the result organic just because it looks varied; preserve the input seed, policy inputs, and generated decision trace so the behavior is inspectable.

### Milestone D — Connect to Singularity Arena
- Add a real venue/anchor model and event scheduling MicroBundle in the domain/composition layer.
- Bind the same venue to soccer and concert providers while keeping their event vocabularies separate.
- Expose the event schedule and event state in WebPage; present actual provider snapshots rather than hard-coded counts or decorative status.
- Integrate audio, Renderer, and physical simulation only through explicit contracts and tests.

## Acceptance criteria

The first proof is successful when:
- one venue can schedule distinct soccer and concert event types without coupling the Gestures core to either;
- a concert cue sequence can be replayed deterministically;
- several participants respond differently for explainable reasons and retain independent progress;
- performer and audience motion is evaluated by the real CPU Gesture evaluator, not replaced by pre-rendered decorative animation;
- blocked, unreachable, interrupted, and failed interactions are represented honestly;
- no pose completion is used as proof of audio, contact, attendance, or event completion;
- quiet, delayed, socially influenced, and nonparticipating audience members remain valid outcomes;
- every unimplemented boundary—audio generation, physical movement, collision/contact, hierarchy mapping, GPU evaluation, and live venue integration—is clearly labeled.

## The principle

The crowd should feel alive because the system knows **who each participant is, what they are attending to, what they want to do, what physical procedure they are attempting, and what actually happened**. Random motion can decorate a scene; it cannot replace that causal chain.
