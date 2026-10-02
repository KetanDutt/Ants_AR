"""Unit tests for the standalone lesson JSON validation tool."""

import json
import unittest

from validate_lesson import DEFAULT_CONFIG, validate_payload


class LessonValidationTests(unittest.TestCase):
    def test_bundled_lesson_is_valid(self):
        payload = json.loads(DEFAULT_CONFIG.read_text(encoding="utf-8"))
        self.assertEqual(validate_payload(payload), [])

    def test_original_capitalized_answers_field_is_supported(self):
        payload = {
            "Questions": [
                {
                    "question": "Amber",
                    "Answers": ["Amber", "Blue", "Green"],
                }
            ]
        }
        self.assertEqual(validate_payload(payload), [])

    def test_correct_answer_must_be_one_of_visible_choices(self):
        payload = {
            "Questions": [
                {
                    "question": "Which is a fruit?",
                    "answers": ["Apple", "Chair", "Cloud"],
                    "correctAnswer": "Orange",
                }
            ]
        }
        self.assertTrue(any("correctAnswer" in error for error in validate_payload(payload)))

    def test_each_question_needs_three_choices(self):
        payload = {
            "Questions": [
                {
                    "question": "Pick one.",
                    "answers": ["First", "Second"],
                    "correctAnswer": "First",
                }
            ]
        }
        self.assertTrue(any("at least three" in error for error in validate_payload(payload)))

    def test_empty_question_list_is_rejected(self):
        self.assertTrue(any("non-empty" in error for error in validate_payload({"Questions": []})))


if __name__ == "__main__":
    unittest.main()
