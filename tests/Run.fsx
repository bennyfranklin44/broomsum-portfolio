#load "../samples/NumberInput.fs"
#load "../samples/SignupContract.fs"

open Broomsum.Portfolio

let mutable checks = 0
let mutable failures = 0

let equal label expected actual =
    checks <- checks + 1
    if actual = expected then
        printfn "PASS %s" label
    else
        failures <- failures + 1
        printfn "FAIL %s: expected %A, got %A" label expected actual

let sq = NumberInput.squareFeet
let rr = NumberInput.restrooms
let ce = NumberInput.ceilingFt

let parsingCases =
    [ "plain digits", sq, "35000", NumberInput.Valid 35000.0
      "grouped thousands", sq, "35,000", NumberInput.Valid 35000.0
      "multiple groups", sq, "1,234,567", NumberInput.Valid 1234567.0
      "outer whitespace", sq, " 20,000 ", NumberInput.Valid 20000.0
      "decimal input", sq, "1,250.75", NumberInput.Valid 1250.75
      "leading decimal", sq, ".5", NumberInput.Valid 0.5
      "trailing decimal", sq, "9.", NumberInput.Valid 9.0
      "zero", sq, "0", NumberInput.Valid 0.0
      "area maximum", sq, "10,000,000", NumberInput.Valid 10000000.0
      "area above maximum", sq, "10,000,000.01", NumberInput.Invalid NumberInput.TooLarge
      "fixture count", rr, "4", NumberInput.Valid 4.0
      "fixture maximum", rr, "1,000", NumberInput.Valid 1000.0
      "fixture above maximum", rr, "1001", NumberInput.Invalid NumberInput.TooLarge
      "fractional fixture count", rr, "4.5", NumberInput.Invalid NumberInput.NotWhole
      "ceiling decimal", ce, "12.5", NumberInput.Valid 12.5
      "ceiling maximum", ce, "300", NumberInput.Valid 300.0
      "ceiling above maximum", ce, "300.01", NumberInput.Invalid NumberInput.TooLarge
      "empty while typing", sq, "", NumberInput.Incomplete
      "partial group", sq, "35,", NumberInput.Incomplete
      "partial group one digit", sq, "35,0", NumberInput.Incomplete
      "partial group two digits", sq, "35,00", NumberInput.Incomplete
      "decimal point while typing", sq, ".", NumberInput.Incomplete
      "too many decimals", sq, "35.000", NumberInput.Invalid NumberInput.TooManyDecimals
      "incorrect group width", sq, "3,5000", NumberInput.Invalid NumberInput.Malformed
      "incorrect middle group", sq, "1,00,000", NumberInput.Invalid NumberInput.Malformed
      "leading comma", sq, ",100", NumberInput.Invalid NumberInput.Malformed
      "separator after decimal", sq, "1.2,3", NumberInput.Invalid NumberInput.Malformed
      "multiple decimal points", sq, "1.2.3", NumberInput.Invalid NumberInput.Malformed
      "negative", sq, "-1", NumberInput.Invalid NumberInput.Negative
      "exponent", sq, "1e6", NumberInput.Invalid NumberInput.Malformed
      "hexadecimal", sq, "0x10", NumberInput.Invalid NumberInput.Malformed
      "infinity", sq, "Infinity", NumberInput.Invalid NumberInput.Malformed
      "NaN text", sq, "NaN", NumberInput.Invalid NumberInput.Malformed
      "inner whitespace", sq, "35 000", NumberInput.Invalid NumberInput.Malformed
      "unit suffix", sq, "35000 sqft", NumberInput.Invalid NumberInput.Malformed ]

for label, spec, raw, expected in parsingCases do
    equal label expected (NumberInput.parse spec raw)

equal "empty on blur" (NumberInput.Invalid NumberInput.Empty)
    (NumberInput.finalize "  " (NumberInput.parse sq "  "))
equal "unfinished group on blur" (NumberInput.Invalid NumberInput.Malformed)
    (NumberInput.finalize "35," (NumberInput.parse sq "35,"))
equal "valid input on blur" (NumberInput.Valid 35000.0)
    (NumberInput.finalize "35,000" (NumberInput.parse sq "35,000"))
equal "invalid input stays invalid" (NumberInput.Invalid NumberInput.Negative)
    (NumberInput.finalize "-1" (NumberInput.parse sq "-1"))

let moneyCases =
    [ "zero display", 0.0, "$0"
      "round down", 0.4, "$0"
      "round half up", 0.5, "$1"
      "round into new group", 999.5, "$1,000"
      "large display", 1234567.0, "$1,234,567"
      "beyond 32-bit integer", 2147483648.0, "$2,147,483,648"
      "large safe value", 123456789012345.0, "$123,456,789,012,345"
      "NaN display", nan, "—"
      "positive infinity display", infinity, "—"
      "negative infinity display", -infinity, "—"
      "negative display", -1.0, "—"
      "display bound", 1e15, "—" ]

for label, amount, expected in moneyCases do
    equal label expected (NumberInput.formatWholeDollars amount)

let reply confirmed fields : Result<SignupContract.SignupReply, string> =
    Ok { Confirmed = confirmed; ErrorFields = fields }

let signupCases =
    [ "explicit confirmation", 200, reply (Some true) None, Ok ()
      "created and confirmed", 201, reply (Some true) None, Ok ()
      "missing confirmation", 200, reply None None, Error SignupContract.NotConfirmed
      "negative confirmation", 200, reply (Some false) None, Error SignupContract.NotConfirmed
      "contradictory error array", 200, reply (Some true) (Some ["email"]), Error SignupContract.NotConfirmed
      "empty error array still ambiguous", 200, reply (Some true) (Some []), Error SignupContract.NotConfirmed
      "malformed body", 200, Error "not JSON", Error SignupContract.NotConfirmed
      "empty body", 204, Error "empty", Error SignupContract.NotConfirmed
      "invalid email", 422, reply None (Some ["email"]), Error SignupContract.EmailRejected
      "email among field errors", 400, reply None (Some [""; "email"]), Error SignupContract.EmailRejected
      "form-level rejection", 400, reply None (Some [""]), Error SignupContract.NotConfirmed
      "rate limit", 429, reply None (Some ["email"]), Error SignupContract.NotConfirmed
      "server failure overrides body", 500, reply (Some true) None, Error SignupContract.NotConfirmed
      "redirect is not confirmation", 302, reply (Some true) None, Error SignupContract.NotConfirmed
      "network failure", 0, Error "network", Error SignupContract.NotConfirmed ]

for label, status, body, expected in signupCases do
    equal label expected (SignupContract.decideSignup status body)

printfn "\n%d checks, %d failures" checks failures
if failures > 0 then exit 1
