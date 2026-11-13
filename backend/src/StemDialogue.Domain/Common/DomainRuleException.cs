namespace StemDialogue.Domain.Common;

/// <summary>
/// Raised when an operation would break a business rule (for example completing an event that has
/// not happened yet). Translated to HTTP 409 by the API.
/// </summary>
public sealed class DomainRuleException(string message) : Exception(message);
