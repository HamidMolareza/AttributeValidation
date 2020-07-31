using System;
using System.Collections.Generic;

namespace AttributeValidation.ResultDetails {
    public class ArgumentValidationError : ArgumentError {
        public ArgumentValidationError (List<string> errors, string? title = null,
                string? message = null, Exception? exception = null):
            base (title ?? nameof (ArgumentValidationError), message, exception : exception) {
                Errors = errors;
            }

        public List<string> Errors { get; }
    }
}