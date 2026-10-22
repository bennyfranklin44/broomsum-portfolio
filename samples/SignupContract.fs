namespace Broomsum.Portfolio

/// Selected website component: classify a decoded early-access response.
/// Confirmation requires both a successful HTTP status and an explicit positive body.
/// This module is pure: it does not send signups or call an external service.
module SignupContract =

    /// The reply as loosely decoded from JSON. Every field is optional.
    type SignupReply =
        { /// The reply's `ok` flag.
          Confirmed: bool option
          /// `Some fields` when an `errors` array is present: each entry's `field`
          /// value ("" when an entry names no field). `None` when there is no array.
          ErrorFields: string list option }

    type SignupFailure =
        /// The endpoint named the email field as invalid (400/422 with errors[].field = "email").
        | EmailRejected
        /// No positive confirmation: any other status, body, network failure, or timeout.
        | NotConfirmed

    /// `reply` is Error when the body was not a decodable JSON object.
    let decideSignup (status: int) (reply: Result<SignupReply, string>) : Result<unit, SignupFailure> =
        let is2xx = status >= 200 && status < 300
        let isValidationStatus = status = 400 || status = 422
        match reply with
        | Ok { Confirmed = Some true; ErrorFields = None } when is2xx -> Ok()
        | Ok { ErrorFields = Some fields } when isValidationStatus && List.contains "email" fields -> Error EmailRejected
        | Ok _ -> Error NotConfirmed
        | Error _ -> Error NotConfirmed
