#!/usr/bin/env python3
"""Validate the lesson JSON consumed by the Ants AR Unity client."""

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Dict, List


PROJECT_ROOT = Path(__file__).resolve().parents[1]
DEFAULT_CONFIG = PROJECT_ROOT / "Assets" / "StreamingAssets" / "config.json"


def validate_payload(payload: Any) -> List[str]:
    """Return validation errors for a lesson payload (empty means valid)."""
    errors: List[str] = []
    if not isinstance(payload, dict):
        return ["The configuration root must be a JSON object."]

    questions = payload.get("Questions")
    if not isinstance(questions, list) or not questions:
        return ["Questions must be a non-empty array."]

    for index, question in enumerate(questions, start=1):
        prefix = f"Question {index}"
        if not isinstance(question, dict):
            errors.append(f"{prefix} must be an object.")
            continue

        prompt = question.get("question")
        if not isinstance(prompt, str) or not prompt.strip():
            errors.append(f"{prefix} needs non-empty question text.")

        answers = question.get("Answers")
        if answers is None:
            answers = question.get("answers")
        if not isinstance(answers, list) or len(answers) < 3:
            errors.append(f"{prefix} needs at least three answers.")
            continue

        choices = answers[:3]
        if any(not isinstance(choice, str) or not choice.strip() for choice in choices):
            errors.append(f"{prefix} has an empty answer choice.")
            continue

        correct = question.get("correctAnswer")
        if not isinstance(correct, str) or not correct.strip():
            # Backwards-compatible with the first version of the service schema.
            correct = prompt if isinstance(prompt, str) else ""

        normalized_choices = {choice.strip().casefold() for choice in choices}
        if not isinstance(correct, str) or correct.strip().casefold() not in normalized_choices:
            errors.append(
                f"{prefix} must set correctAnswer to one of its first three answers "
                "(or use a legacy question value that is itself a choice)."
            )

    return errors


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "config",
        nargs="?",
        type=Path,
        default=DEFAULT_CONFIG,
        help=f"lesson JSON to validate (default: {DEFAULT_CONFIG.relative_to(PROJECT_ROOT)})",
    )
    args = parser.parse_args()

    try:
        payload: Dict[str, Any] = json.loads(args.config.read_text(encoding="utf-8"))
    except OSError as exc:
        print(f"ERROR: could not read {args.config}: {exc}", file=sys.stderr)
        return 2
    except json.JSONDecodeError as exc:
        print(f"ERROR: invalid JSON at line {exc.lineno}, column {exc.colno}: {exc.msg}", file=sys.stderr)
        return 2

    errors = validate_payload(payload)
    if errors:
        for error in errors:
            print(f"ERROR: {error}", file=sys.stderr)
        return 1

    print(f"Valid lesson configuration: {args.config}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
