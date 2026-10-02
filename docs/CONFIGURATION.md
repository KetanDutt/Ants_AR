# Lesson configuration

## Loading order

1. The app loads `Assets/StreamingAssets/config.json` first and can be played offline.
2. If the `configUrl` field on the scene's `GameManager` is non-empty, the app makes a background HTTPS GET request. A valid result replaces the bundled lesson when safe; failures leave the bundled lesson intact.
3. If the bundle cannot be read, the app tries the remote URL. If both sources fail, the HUD displays a recoverable configuration error instead of throwing a null-reference exception.

The remote URL must be a full HTTPS URL. The current serialized scene URL is the legacy project endpoint; update or clear it if it is no longer controlled by your team. Do not use an endpoint that returns user-specific content or secrets.

## JSON contract

The root object uses the original service's PascalCase fields where applicable. `Questions` is required and must contain at least one entry.

```json
{
  "voice": {
    "language": "en-US",
    "gender": "FEMALE",
    "voice": "en-US-Wavenet-C"
  },
  "HelpText": "Welcome! Help the ant choose the right answer.",
  "WelcomeText": "Let's play a quick quiz!",
  "QuestionText": "Choose the best answer: {Question}",
  "WhenAntSelectedText": "The ant is on its way!",
  "CorrectText": "Great job! That's correct.",
  "IncorrectText": "Not quite. Let's try the next one.",
  "GameTexts": [],
  "Ants": [],
  "Questions": [
    {
      "question": "Which shape has three sides?",
      "answers": ["Triangle", "Circle", "Square"],
      "correctAnswer": "Triangle"
    }
  ]
}
```

### Required question fields

- `question`: non-empty prompt text. `{Question}` in `QuestionText` is replaced with this value.
- `answers` or legacy `Answers`: at least three non-empty strings. Only the first three are currently displayed.
- `correctAnswer`: must match one of the first three choices, case-insensitively. New content should always set it explicitly.

For compatibility with the original remote schema, when `correctAnswer` is omitted the game accepts the legacy `question` value as the correct answer **only if it exactly matches one of the first three choices**. New lessons should not rely on that legacy behavior.

Optional lesson strings may be empty. If `QuestionText` is empty, the prompt itself is displayed. If either feedback string is empty, the game uses a built-in short fallback. Voice metadata is optional; without a usable voice setting, the adapter retains its default voice configuration.

## Authoring and validation

1. Edit `Assets/StreamingAssets/config.json` using UTF-8 JSON.
2. Keep exactly three choices per question for the current scene.
3. Check that the correct choice is visible and spellings match.
4. Run:

   ```sh
   python3 Tools/validate_lesson.py
   python3 -m unittest discover -s Tools -p 'test_*.py'
   ```

The Python validator mirrors the C# runtime contract and requires no third-party packages. It does not validate Google voice names or remote endpoint availability.

## Fields currently reserved/unused

`WelcomeText`, `WhenAntSelectedText`, `GameTexts`, and the remote `Ants` descriptors are retained for compatibility with the old content feed. The current UI/gameplay does not consume them; ant visuals are selected from the `Ants` prefab array on `GameManager`. See [ROADMAP.md](ROADMAP.md) for future content-model improvements.
