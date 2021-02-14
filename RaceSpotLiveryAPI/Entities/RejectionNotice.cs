using System;

namespace RaceSpotLiveryAPI.Entities
{
    public class RejectionNotice
    {
        public RejectionNotice()
        {
            Id = Guid.NewGuid();
        }
        
        public string Message { get; set; }
        public RejectionStatus Status { get; set; }
        public Guid LiveryId { get; set; }
        public virtual Livery Livery { get; set; }
        public Guid Id { get; set; }
    }
    
    public enum RejectionStatus
    {
        Rejected,
        Updated,
        Resolved
    }
}