﻿using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Intellidesk.Data.Auth
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            return Task.FromResult(0);
        }
    }
}
