#!/usr/bin/env python3
"""Dependency-free reference experiment for the Gestures motion-model study.

This is an offline research tool, not production physics and not part of the
.NET runtime. It compares kinematic baselines with a fixed-step scalar
spring-damper tracker and emits long-form CSV for plotting or analysis.
"""
from __future__ import annotations

import argparse
import csv
import math
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable, Iterator

DT_DEFAULT = 1.0 / 240.0
DURATION_DEFAULT = 1.5
MOVE_DURATION = 0.5
MASS = 1.0


@dataclass(frozen=True)
class Parameters:
    natural_frequency: float
    damping_ratio: float


@dataclass(frozen=True)
class Sample:
    model: str
    profile: str
    seed: int
    time: float
    target: float
    position: float
    velocity: float
    acceleration: float
    error: float
    natural_frequency: float | None
    damping_ratio: float | None


def unit_sample(seed: int, lane: int = 0) -> float:
    """Stable [0, 1) sample using SplitMix64; independent of Python RNG versions."""
    mask = (1 << 64) - 1
    value = (seed + 0x9E3779B97F4A7C15 * (lane + 1)) & mask
    value = ((value ^ (value >> 30)) * 0xBF58476D1CE4E5B9) & mask
    value = ((value ^ (value >> 27)) * 0x94D049BB133111EB) & mask
    value ^= value >> 31
    return (value & mask) / float(1 << 64)


def sample_parameters(seed: int, profile: str) -> Parameters:
    """Sample stable per-identity parameters from explicit illustrative ranges."""
    if profile == "green":
        omega_low, omega_high = 7.0, 11.0
        zeta_low, zeta_high = 0.55, 1.25
    elif profile == "veteran":
        omega_low, omega_high = 8.6, 9.4
        zeta_low, zeta_high = 0.88, 1.04
    else:
        raise ValueError(f"unknown profile: {profile}")
    omega_u = unit_sample(seed, 0)
    zeta_u = unit_sample(seed, 1)
    return Parameters(
        omega_low + (omega_high - omega_low) * omega_u,
        zeta_low + (zeta_high - zeta_low) * zeta_u,
    )


def target_at(time: float) -> tuple[float, float]:
    """Smoothstep target moving from 0 to 1, then holding; returns position/rate."""
    u = min(max(time / MOVE_DURATION, 0.0), 1.0)
    target = u * u * (3.0 - 2.0 * u)
    rate = (6.0 * u * (1.0 - u) / MOVE_DURATION) if 0.0 < u < 1.0 else 0.0
    return target, rate


def run_kinematic(model: str, seed: int, time: float) -> Sample:
    target, _ = target_at(time)
    if model == "linear":
        position = min(max(time / MOVE_DURATION, 0.0), 1.0)
        velocity = 1.0 / MOVE_DURATION if 0.0 < time < MOVE_DURATION else 0.0
    elif model == "eased":
        position = target
        velocity = target_at(time)[1]
    else:
        raise ValueError(f"unknown kinematic model: {model}")
    return Sample(model, "baseline", seed, time, target, position, velocity, 0.0,
                  position - target, None, None)


def run_dynamic(seed: int, profile: str, params: Parameters, dt: float,
                duration: float) -> Iterator[Sample]:
    """Integrate m*x'' = k*(r-x) + c*(r'-x') with semi-implicit Euler."""
    stiffness = MASS * params.natural_frequency**2
    damping = 2.0 * params.damping_ratio * MASS * params.natural_frequency
    position = 0.0
    velocity = 0.0
    count = round(duration / dt)
    for index in range(count + 1):
        time = index * dt
        target, target_velocity = target_at(time)
        acceleration = (stiffness * (target - position)
                        + damping * (target_velocity - velocity)) / MASS
        yield Sample(
            f"spring-damper-{profile}", profile, seed, time, target, position,
            velocity, acceleration, position - target,
            params.natural_frequency, params.damping_ratio,
        )
        # Semi-implicit Euler: update velocity before position.
        velocity += acceleration * dt
        position += velocity * dt


def experiment(dt: float = DT_DEFAULT, duration: float = DURATION_DEFAULT,
               population: int = 16) -> Iterator[Sample]:
    if not math.isfinite(dt) or dt <= 0:
        raise ValueError("dt must be finite and positive")
    if not math.isfinite(duration) or duration <= 0:
        raise ValueError("duration must be finite and positive")
    if population < 1:
        raise ValueError("population must be at least 1")
    if abs(round(duration / dt) * dt - duration) > dt * 1e-6:
        raise ValueError("duration must be an integer multiple of dt")

    for seed in range(population):
        for model in ("linear", "eased"):
            count = round(duration / dt)
            for index in range(count + 1):
                yield run_kinematic(model, seed, index * dt)
        for profile in ("green", "veteran"):
            yield from run_dynamic(seed, profile, sample_parameters(seed, profile),
                                   dt, duration)


def write_csv(samples: Iterable[Sample], output: Path) -> None:
    output.parent.mkdir(parents=True, exist_ok=True)
    fields = list(Sample.__dataclass_fields__.keys())
    with output.open("w", newline="", encoding="utf-8") as stream:
        writer = csv.DictWriter(stream, fieldnames=fields)
        writer.writeheader()
        for sample in samples:
            writer.writerow(sample.__dict__)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path, default=Path("motion-experiment.csv"))
    parser.add_argument("--dt", type=float, default=DT_DEFAULT,
                        help="fixed timestep in seconds (default: 1/240)")
    parser.add_argument("--duration", type=float, default=DURATION_DEFAULT,
                        help="experiment duration in seconds (default: 1.5)")
    parser.add_argument("--population", type=int, default=16,
                        help="number of stable identity seeds (default: 16)")
    args = parser.parse_args()
    write_csv(experiment(args.dt, args.duration, args.population), args.output)
    print(f"Wrote {args.output} (dt={args.dt:g}s, duration={args.duration:g}s, "
          f"population={args.population})")


if __name__ == "__main__":
    main()
