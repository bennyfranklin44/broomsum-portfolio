# Verification

## Public sample checks

On October 22, 2026, the following command completed successfully with .NET SDK 8.0.422:

```sh
dotnet fsi tests/Run.fsx
```

**Result: 66 checks, 0 failures.**

| Area | Cases |
|---|---:|
| Numeric input formats and field boundaries | 35 |
| Finalizing a field after typing | 4 |
| Whole-dollar formatting and unsafe values | 12 |
| Signup status and response classification | 15 |

The GitHub Actions workflow executes the same script. Its run history records remote execution independently of this local result.

## Scope

These checks exercise the two F# sample modules in this repository. They do not send network requests, create real signups or connect to a database.

The website was opened and its calculator and early-access interface were inspected on October 22, 2026. The screenshots included here are desktop and mobile website captures prepared earlier in October using seeded example inputs. They contain no customer submissions.

## Limits

The sample checks are not a field-accuracy study of cleaning estimates, a complete browser regression suite or verification of a launched application. The full application remains in development.

Source files in this portfolio were extracted into an independent repository. Namespaces and contextual comments were adapted; the selected parsing, formatting and response-classification logic was retained.
