﻿using RaceSpotLiveryAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.DTOs
{
    public class UserDTO
    {
        public UserDTO()
        {
        }

        public UserDTO(ApplicationUser user)
        {
            this.Id = user.Id;
            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.IsAdmin = user.IsAdmin;
            this.IracingId = user.IracingId;
            this.EmailAddress = user.Email;
            if(user.Invite != null && user.Invite.Status == InviteStatus.SENT)
            {
                this.LastInviteSent = string.Format("{0:yyyy-MM-ddTHH:mm:ssZ}", user.Invite.LastUpdated);
            }
        }
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsAdmin { get; set; }
        public string IracingId { get; set; }
        public string EmailAddress { get; set; }
        public string LastInviteSent { get; set; }
    }
}
