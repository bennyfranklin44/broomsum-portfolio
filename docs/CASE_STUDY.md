# Broomsum: making a cleaning estimate easier to understand

**Role:** founder and product builder, using an AI-assisted development workflow.
**Delivery status:** public website live; full application in development.

## Problem

Commercial-cleaning estimates depend on building characteristics, service frequency and labor assumptions. A useful interface needs to make those inputs understandable and avoid turning typing mistakes into misleading prices.

Broomsum explores a focused experience: enter building details, see an estimate and inspect a breakdown. The live website is an initial public surface for that idea. This case study describes the delivered website and selected implementation decisions, rather than claiming outcomes from a launched application.

## Scope and approach

The website puts the calculator directly on the page and offers optional early-access signup. Desktop and mobile layouts use the same inputs and estimation behavior. The interface identifies the starting values as an example and labels the result as an estimate.

The development approach separates three responsibilities:

1. Parse and validate what the visitor enters.
2. Compute numerical results in deterministic code.
3. Render the result and communicate the state of any network request.

That separation makes important decisions testable without a browser, database or live signup service. The portfolio includes two of those components and their executable checks.

## Decision 1: distinguish incomplete input from invalid input

A numeric field is more than a final number. A visitor typing `35,000` passes through states such as `35,` and `35,0`. Treating each of those as a completed value creates confusing behavior.

The [numeric input module](../samples/NumberInput.fs) models three outcomes: `Valid`, `Incomplete` and `Invalid`. It accepts documented en-US number formats and rejects ambiguous or unsupported formats such as exponents, misplaced separators and hexadecimal input. Finalizing a field converts unfinished input into a visible problem.

The module also formats whole-dollar values without converting through a 32-bit integer, so large valid display values do not wrap into negative numbers.

## Decision 2: make success a verified state

An early-access signup should not say “success” just because a request returned a 2xx status. An empty or contradictory response may not confirm that the submission was accepted.

The [signup classifier](../samples/SignupContract.fs) requires a successful status, an explicit positive confirmation and no error array. It distinguishes a response that specifically rejects the email field from a general failure to confirm the submission. That distinction supports more accurate messages and avoids blaming the visitor for a service problem.

The public sample is a pure function. Its checks use synthetic response objects and send no real signups.

## Decision 3: make the website useful at different screen sizes

The desktop layout places the input form alongside the estimate. The mobile layout stacks the same content, with a compact estimate summary above the form. Typography, spacing and progressive disclosure help separate the essential task from optional detail.

[Desktop preview](../assets/website-desktop.jpg) · [Mobile preview](../assets/website-mobile.jpg)

## Delivered work and evidence

- A [live public website](https://broomsum.pages.dev/) with a calculator and early-access signup.
- Responsive desktop and mobile interface previews using example data.
- Extracted F# components for input validation and response classification.
- Executable checks and a GitHub Actions workflow for the selected samples.

The full application is still in development. This case study does not claim customer adoption, revenue, production operation of the complete app or field-validated estimating accuracy.

## Lessons reflected in the implementation

- Treat typing and network responses as explicit states, rather than assuming a happy path.
- Keep decision logic independent of the interface so boundary cases are straightforward to exercise.
- Use seeded examples to explain an experience without exposing customer information.
- Describe what is delivered separately from what is still being developed.
