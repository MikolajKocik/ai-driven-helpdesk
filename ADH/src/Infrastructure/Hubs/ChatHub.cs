using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ADH.Infrastructure.Hubs;

/// <summary>
/// SignalR Hub for handling real-time chat messages and system-wide notifications.
/// </summary>
public class ChatHub : Hub
{
    /// <summary>
    /// Broadcasts a message to all connected clients.
    /// </summary>
    /// <param name="user">The name of the user sending the message.</param>
    /// <param name="message">The message content.</param>
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    /// <summary>
    /// Notifies a specific user about a ticket status update.
    /// </summary>
    /// <param name="userId">The ID of the user to notify.</param>
    /// <param name="status">The new status of the ticket.</param>
    public async Task NotifyTicketUpdate(string userId, string status)
    {
        await Clients.User(userId).SendAsync("TicketUpdated", status);
    }
}
