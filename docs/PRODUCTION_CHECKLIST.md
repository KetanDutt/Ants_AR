# Production checklist

The project is a polished prototype, not a store-ready release as checked in. Complete and verify these items in the release environment.

## Credentials, services, and data

- [ ] Keep Google Cloud API credentials out of Git and the client binary. Use a backend proxy; configure API restrictions, per-app restrictions, quotas, billing alerts, and abuse controls.
- [ ] Rotate/restrict any key that may have been committed or copied into a public build. The TTS sample key and key logging have been removed from the working tree; Git history may still contain prior values.
- [ ] Own and monitor the configured HTTPS content endpoint, TLS certificate, content schema, availability, timeouts, and cache/rollback behavior. Or clear the URL to ship bundled content only.
- [ ] Confirm content is age-appropriate, fact-checked, localized, and safe to render/speak. Avoid HTML/TMP markup in content; the HUD renders plain text.
- [ ] Decide how content updates are versioned, cached, rolled back, and reviewed. The app currently falls back to the bundled file and does not persist remote content across launches.
- [ ] Establish privacy, retention, consent, and data-processing policies for any future analytics, user accounts, or speech service calls.

## App identity and store assets

- [ ] Replace the checked-in `com.ketandutt.antsar` application identifier with an ID registered to the actual publisher.
- [ ] Set company/publisher name, semantic version, build number, Android signing, iOS team/provisioning, platform-specific app icons, screenshots, and store descriptions.
- [ ] Revisit Android target SDK and iOS deployment target immediately before submission; platform-store requirements change over time.
- [ ] Confirm iOS camera privacy text and Android permission prompts explain the AR camera requirement.
- [ ] Verify all third-party plug-in, model, font, and Asset Store license terms permit the intended distribution.

## UX, accessibility, and reliability

- [ ] User-test instructions, font size, contrast, safe-area placement, answer affordance, feedback timing, and reset/reposition discoverability on real devices.
- [ ] Add accessible alternatives to color-only right/wrong feedback, screen-reader semantics, and localization if the audience needs them.
- [ ] Provide a clear response for unsupported devices, tracking loss, missing content, and network/TTS errors.
- [ ] Test app background/resume, camera permission denial, interruptions, low memory, device rotation, and duplicate scene loads.
- [ ] Confirm the AR scene is comfortable to use, does not encourage unsafe movement, and provides appropriate child-safety guidance for the intended audience.

## Performance and release QA

- [ ] Profile GPU/CPU, frame time, thermal throttling, battery use, memory, audio, plane count, and shader variants on low/mid/high target devices.
- [ ] Recheck dynamic scale bounds and anchor drift under long sessions and tracking interruptions.
- [ ] In XR Plug-in Management, verify Android maps to ARCore and iOS maps to ARKit, automatic loading/running is enabled, and there are no XR package/configuration warnings.
- [ ] Build from a clean checkout with the pinned Unity Editor and capture Android/iOS build logs.
- [ ] Run [TESTING.md](TESTING.md) and record device/model/OS/Editor versions and results for each release candidate.
- [ ] Configure crash reporting only after privacy/legal review, and verify release logs never contain credentials or user data.

## Known release limitations

- The remote endpoint is a legacy project URL; this repository cannot verify ownership or availability.
- TTS is disabled unless configured and should not be connected directly to a production key in the app.
- The question renderer supports exactly three answer platforms. Extra answer strings are ignored.
- The serialized content fields `WelcomeText`, `WhenAntSelectedText`, `GameTexts`, and content-driven ant skin selection are retained but currently unused.
- Unity/device builds and AR tracking cannot be proven by the repository-only Python tests.
