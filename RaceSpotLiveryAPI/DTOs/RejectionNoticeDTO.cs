using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RaceSpotLiveryAPI.Entities;

namespace RaceSpotLiveryAPI.DTOs
{
    public class RejectionNoticeDTO
    {
        public RejectionNoticeDTO()
        {
        }
        
        public RejectionNoticeDTO(RejectionNotice notice)
        {
            this.Id = notice.Id;
            this.LiveryId = notice.LiveryId;
            this.Status = notice.Status;
            this.Message = notice.Message;
            if (notice.Livery != null)
            {
                this.Type = notice.Livery.LiveryType.ToString();
                this.SeriesId = notice.Livery.SeriesId;
                if (notice.Livery.Series != null)
                {
                    this.SeriesName = notice.Livery.Series.Name;
                }
            }
        }
        
        public string Type { get; set; }
        public string SeriesName { get; set; }
        public Guid SeriesId { get; set; }
        public string Message { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public RejectionStatus Status { get; set; }
        public Guid LiveryId { get; set; }
        public Guid Id { get; set; }
    }
}