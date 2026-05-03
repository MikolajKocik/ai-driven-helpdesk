using System.Collections.Generic;

namespace ADH.Application.DTOs;

public record ChatMessage(string Role, string Content);
public record ChatRequest(List<ChatMessage> Messages);
