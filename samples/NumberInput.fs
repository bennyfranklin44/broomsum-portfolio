namespace Broomsum.Portfolio

/// Selected website component: strict en-US numeric entry and whole-dollar display.
/// Inputs are classified as valid, incomplete while typing, or invalid.
/// This sample contains no estimating formula or service connection.
module NumberInput =

    type FieldSpec =
        { AllowDecimals: bool
          Max: float
          /// The maximum as shown to people (e.g. "10,000,000").
          MaxLabel: string }

    // Public calculator field limits. These are validation bounds, not pricing rates.
    let squareFeet = { AllowDecimals = true;  Max = 10000000.0; MaxLabel = "10,000,000" }
    let restrooms  = { AllowDecimals = false; Max = 1000.0;     MaxLabel = "1,000" }
    let ceilingFt  = { AllowDecimals = true;  Max = 300.0;      MaxLabel = "300" }

    type Problem =
        | Empty
        | Malformed
        | TooManyDecimals
        | NotWhole
        | Negative
        | TooLarge

    type Parsed =
        | Valid of float
        /// Not a number yet, but typing more could make it one ("", "35,", "35,0", ".").
        /// Never priced; shown as an error only once the person leaves the field.
        | Incomplete
        | Invalid of Problem

    let private isDigit (c: char) = c >= '0' && c <= '9'
    let private allDigits (s: string) = s |> Seq.forall isDigit

    /// Classify the text of one numeric field.
    let parse (spec: FieldSpec) (raw: string) : Parsed =
        let t = raw.Trim()
        if t = "" then Incomplete
        elif t.StartsWith "-" then Invalid Negative
        elif not (t |> Seq.forall (fun c -> isDigit c || c = ',' || c = '.')) then Invalid Malformed
        else
            let parts = t.Split '.'
            if parts.Length > 2 then Invalid Malformed
            else
                let intPart = parts.[0]
                let frac = if parts.Length = 2 then Some parts.[1] else None
                match frac with
                | Some _ when not spec.AllowDecimals -> Invalid NotWhole
                | Some f when f.Contains "," -> Invalid Malformed
                | Some f when f.Length > 2 -> Invalid TooManyDecimals
                | _ ->
                    // Thousands grouping: 1–3 leading digits, then groups of exactly 3.
                    // A short LAST group with no decimal part is still being typed.
                    let grouping =
                        if not (intPart.Contains ",") then (if allDigits intPart then Ok false else Error Malformed)
                        else
                            let groups = intPart.Split ','
                            let first = groups.[0]
                            let rest = groups |> Array.skip 1
                            let middle = rest |> Array.take (rest.Length - 1)
                            let last = rest.[rest.Length - 1]
                            if first.Length < 1 || first.Length > 3 || not (allDigits first) then Error Malformed
                            elif middle |> Array.exists (fun g -> g.Length <> 3 || not (allDigits g)) then Error Malformed
                            elif not (allDigits last) || last.Length > 3 then Error Malformed
                            elif last.Length < 3 then (if frac.IsSome then Error Malformed else Ok true)
                            else Ok false
                    match grouping with
                    | Error p -> Invalid p
                    | Ok true -> Incomplete
                    | Ok false ->
                        let digits = intPart.Replace(",", "")
                        match digits, frac with
                        | "", None -> Incomplete
                        | "", Some "" -> Incomplete
                        | _ ->
                            let whole = if digits = "" then "0" else digits
                            let text =
                                match frac with
                                | Some f when f <> "" -> whole + "." + f
                                | _ -> whole
                            let v = float text
                            if v > spec.Max then Invalid TooLarge else Valid v

    /// What leaving the field means: anything unfinished becomes a visible problem.
    let finalize (raw: string) (parsed: Parsed) : Parsed =
        match parsed with
        | Incomplete ->
            if raw.Trim() = "" then Invalid Empty else Invalid Malformed
        | other -> other

    let message (spec: FieldSpec) (problem: Problem) : string =
        match problem with
        | Empty -> "Enter a number."
        | Malformed ->
            if spec.AllowDecimals then "Use digits, with commas for thousands (35,000) and a period for decimals (12.5)."
            else "Use digits, with commas for thousands (1,000)."
        | TooManyDecimals -> "Use at most 2 decimal places. For thousands, use commas (35,000)."
        | NotWhole -> "Enter a whole number."
        | Negative -> "Enter 0 or more."
        | TooLarge -> "Enter " + spec.MaxLabel + " or less."

    /// Whole-dollar display ("$2,129"): rounds half up, groups thousands, and never
    /// goes through a 32-bit integer.
    /// Anything that is not a finite, non-negative amount below 1e15 shows "—"
    /// instead of a misleading "$0" or a negative price.
    let formatWholeDollars (v: float) : string =
        if System.Double.IsNaN v || System.Double.IsInfinity v || v < 0.0 || v >= 1e15 then "—"
        else
            let digits = sprintf "%.0f" (floor (v + 0.5))
            let grouped =
                digits
                |> Seq.rev
                |> Seq.mapi (fun i c -> if i > 0 && i % 3 = 0 then string c + "," else string c)
                |> Seq.rev
                |> String.concat ""
            "$" + grouped
