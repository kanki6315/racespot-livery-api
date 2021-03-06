﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.Entities
{
    public class Livery
    {
        public Livery()
        {
            Id = Guid.NewGuid();
        }

        public DateTime LastUpdated { get; set; }
        public bool IsRejected { get; set; }
        public string ITeamId { get; set; }
        public string ITeamName { get; set; }
        public UploadStatus Status { get; set; }
        public LiveryType LiveryType { get; set; }
        public bool IsCustomNumber { get; set; }
        public virtual Series Series { get; set; }
        public Guid SeriesId { get; set; }
        public virtual Car Car { get; set; }
        public Guid? CarId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public string UserId { get; set; }
        public ICollection<RejectionNotice> Rejections { get; set; }
        public Guid Id { get; set; }

        public bool IsTeam()
        {
            return !String.IsNullOrEmpty(this.ITeamId);
        }
    }

    public enum LiveryType
    {
        Car,
        Suit,
        Helmet,
        [EnumMember(Value = "Spec Map")]
        SpecMap
    }

    public enum UploadStatus
    {
        WAITING, UPLOADED
    }

}
