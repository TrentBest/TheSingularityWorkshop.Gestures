#!/usr/bin/env python3
"""Validate Gestures' SURPS sourcebook manifest and canonical files."""
from __future__ import annotations

import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / ".surps" / "participation.json"
EXPECTED_BADGES = {
    1: "rulebook-lod-1.svg",
    2: "rulebook-lod-2.svg",
    3: "rulebook-lod-3.svg",
}
errors: list[str] = []


def fail(message: str) -> None:
    errors.append(message)


def read_json(path: Path):
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        fail(f"{path.relative_to(ROOT)}: {exc}")
        return None


manifest = read_json(MANIFEST)
if not isinstance(manifest, dict):
    print("\n".join(errors))
    sys.exit(1)

if manifest.get("repository") != "TrentBest/TheSingularityWorkshop.Gestures":
    fail("manifest repository identity is incorrect")
if manifest.get("cataloguePath") != "docs/books/index.md":
    fail("cataloguePath must be docs/books/index.md")
if manifest.get("participationStatus") != "integrated-not-enrolled":
    fail("participation must remain integrated-not-enrolled until independent SURPS admission")

books = manifest.get("books")
if not isinstance(books, list) or not books:
    fail("manifest must declare at least one book")
    books = []

seen_ids: set[str] = set()
seen_covers: set[str] = set()
for entry in books:
    book_id = entry.get("id", "")
    if not book_id or book_id in seen_ids:
        fail(f"missing or duplicate book ID: {book_id!r}")
    seen_ids.add(book_id)

    for field in ("bookPath", "descriptorPath", "coverPath"):
        relative = entry.get(field, "")
        path = ROOT / relative
        if not relative or not path.is_file():
            fail(f"{book_id}: missing {field}: {relative}")
    cover = entry.get("coverPath", "")
    if cover in seen_covers:
        fail(f"{book_id}: cover must be unique: {cover}")
    seen_covers.add(cover)

    descriptor_path = ROOT / entry.get("descriptorPath", "")
    source_path = ROOT / entry.get("bookPath", "")
    descriptor = read_json(descriptor_path) if descriptor_path.is_file() else None
    if isinstance(descriptor, dict):
        for field in ("id", "title", "category", "lod", "source", "cover", "status"):
            expected = entry.get(field) if field in entry else None
            if field == "source":
                expected = entry.get("bookPath")
            if field == "cover":
                expected = entry.get("coverPath")
            if field in ("id", "title", "category", "lod", "status") and descriptor.get(field) != expected:
                fail(f"{book_id}: descriptor {field} does not match manifest")
            if field in ("source", "cover") and descriptor.get(field) != expected:
                fail(f"{book_id}: descriptor {field} does not match manifest")
        badge = str(descriptor.get("lodBadge", ""))
        expected_badge = EXPECTED_BADGES.get(entry.get("lod"))
        if not badge.endswith(expected_badge or "__invalid__"):
            fail(f"{book_id}: LOD badge does not match LOD {entry.get('lod')}")
        if descriptor.get("repository") != "https://github.com/TrentBest/TheSingularityWorkshop.Gestures":
            fail(f"{book_id}: descriptor repository URL is incorrect")

    if source_path.is_file():
        source = source_path.read_text(encoding="utf-8")
        if book_id not in source:
            fail(f"{book_id}: sourcebook does not declare its stable ID")
        if "Edition: 0.1.0" not in source:
            fail(f"{book_id}: sourcebook must declare Edition 0.1.0")

catalogue = ROOT / manifest["cataloguePath"]
if not catalogue.is_file():
    fail("catalogue file is missing")
else:
    catalogue_text = catalogue.read_text(encoding="utf-8")
    for entry in books:
        if entry.get("title", "") not in catalogue_text:
            fail(f"catalogue is missing {entry.get('title')}")

if errors:
    print("SURPS participation validation failed:")
    print("\n".join(f"- {error}" for error in errors))
    sys.exit(1)

print(f"SURPS participation validation passed: {len(books)} books, {len(seen_ids)} unique IDs, {len(seen_covers)} unique covers.")
