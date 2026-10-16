import math
import unittest

from motion_experiment import (
    DT_DEFAULT,
    Parameters,
    experiment,
    run_dynamic,
    sample_parameters,
    target_at,
    unit_sample,
)


class MotionExperimentTests(unittest.TestCase):
    def test_identity_samples_are_stable_and_bounded(self):
        self.assertEqual(unit_sample(1234, 0), unit_sample(1234, 0))
        self.assertNotEqual(unit_sample(1234, 0), unit_sample(1234, 1))
        for seed in range(100):
            self.assertGreaterEqual(unit_sample(seed), 0.0)
            self.assertLess(unit_sample(seed), 1.0)

    def test_profile_ranges_are_positive_and_veteran_ranges_are_narrower(self):
        green = [sample_parameters(seed, "green") for seed in range(1000)]
        veteran = [sample_parameters(seed, "veteran") for seed in range(1000)]
        for params in green + veteran:
            self.assertGreater(params.natural_frequency, 0)
            self.assertGreater(params.damping_ratio, 0)
            self.assertTrue(math.isfinite(params.natural_frequency))
            self.assertTrue(math.isfinite(params.damping_ratio))
        green_omega_width = max(p.natural_frequency for p in green) - min(
            p.natural_frequency for p in green
        )
        veteran_omega_width = max(p.natural_frequency for p in veteran) - min(
            p.natural_frequency for p in veteran
        )
        green_zeta_width = max(p.damping_ratio for p in green) - min(
            p.damping_ratio for p in green
        )
        veteran_zeta_width = max(p.damping_ratio for p in veteran) - min(
            p.damping_ratio for p in veteran
        )
        self.assertLess(veteran_omega_width, green_omega_width)
        self.assertLess(veteran_zeta_width, green_zeta_width)

    def test_target_starts_and_finishes_at_rest(self):
        self.assertEqual(target_at(0.0), (0.0, 0.0))
        self.assertEqual(target_at(0.5), (1.0, 0.0))
        self.assertEqual(target_at(1.0), (1.0, 0.0))

    def test_dynamic_run_is_repeatable_and_finite(self):
        params = Parameters(9.0, 1.0)
        first = list(run_dynamic(7, "veteran", params, DT_DEFAULT, 1.5))
        second = list(run_dynamic(7, "veteran", params, DT_DEFAULT, 1.5))
        self.assertEqual(first, second)
        for row in first:
            for value in (row.time, row.target, row.position, row.velocity,
                          row.acceleration, row.error):
                self.assertTrue(math.isfinite(value))

    def test_experiment_rejects_non_integral_duration(self):
        with self.assertRaises(ValueError):
            list(experiment(dt=0.01, duration=1.001, population=1))


if __name__ == "__main__":
    unittest.main()
