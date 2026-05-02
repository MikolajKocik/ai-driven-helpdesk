using System;

namespace ADH.Core.Attributes;

/// <summary>
/// Marks a class as auditable. Changes to entities of this type will be recorded in the audit log.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AuditableAttribute : Attribute
{
}
