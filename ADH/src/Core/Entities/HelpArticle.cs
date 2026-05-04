using System;
using ADH.Core.Attributes;
using Pgvector;

namespace ADH.Core.Entities;

[Auditable]
public class HelpArticle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    
    public ReadOnlyMemory<float>? Embedding { get; set; }
}
