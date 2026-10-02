# Suggested follow-up improvements

These are recommendations, not claims that they are already implemented. Prioritize work using product requirements, device testing, privacy review, and the current Unity/package constraints.

## High priority before release

1. **Move TTS behind a service.** The current vendor package sends an API key from the client. A small HTTPS backend should authenticate/limit requests, keep the Google key server-side, validate text/voice inputs, and provide operational quotas.
2. **Replace the legacy content endpoint.** Add a maintained endpoint, content version/schema version, ETag/cache policy, signed or otherwise trusted release process, and observable rollback. Preserve bundled fallback behavior.
3. **Add Unity CI.** Use the pinned Editor to compile Android/iOS targets, run edit/play-mode tests, detect missing script references, and produce signed artifacts through protected build secrets.
4. **Add device telemetry/diagnostics only with consent.** Record coarse error categories and app/device versions; avoid recording camera frames, speech text, or personal data.
5. **Add a scene-validation editor tool.** Verify required AR managers, camera/HUD references, question platform count, answer materials, animation states, input settings, and build scenes before making a player build.

## Gameplay and content

- Move from fixed three-choice prefabs to data-driven answer counts and reusable platform instances.
- Define a versioned lesson schema with explicit IDs, category/difficulty, locale, `correctAnswer`, optional explanation, feedback, and validated voice settings; remove unused legacy fields after migration.
- Add a round-complete state, replay/new-lesson flow, persistent but privacy-reviewed progress, and content-specific scoring rules.
- Let lesson content choose ant prefab/skin through a strict allow-list instead of arbitrary asset names; add accessibility options and localized prompts.
- Consider a more forgiving selection system (AR raycast/collider feedback) with a visible selected state and larger touch targets.

## UX/accessibility

- Add onboarding for camera permission and plane-scanning tips, a tracking-lost overlay, and a non-color-only correctness indicator.
- Test all status/HUD content with screen readers, dynamic type, safe areas, high contrast, one-handed use, and localization.
- Expose speech on/off, voice selection, and a clear voice-unavailable state without blocking text-only play.

## Performance and maintainability

- Profile real hardware before changing URP/quality settings; use measurements to tune shadows, MSAA, plane visuals, ant animation, and frame-rate policy.
- Pool/reuse placed worlds only if profiling shows meaningful allocation pressure; the current app creates one world at a time.
- Add structured runtime state/event boundaries between AR placement, quiz rules, presentation, and services so each part can be tested without a camera.
- Add Unity edit/play-mode tests for `Question` validation, score/progression, reset lifecycle, and TTS request correlation.
- Reassess the Unity/AR Foundation/TTS plug-in versions with an explicit migration plan; avoid upgrading packages without validating device support and serialization changes.
