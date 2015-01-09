using EDR.Enums;
using System.Collections.Generic;
namespace EDR.Models
{
    public class Place : Entity
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public State State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string FacebookLink { get; set; }
        public string Website { get; set; }
        public string Filename { get; set; }
        public string ThumbnailFilename { get; set; }
        public PlaceType PlaceType { get; set; }
        public string FacebookId { get; set; }

        public virtual ICollection<Event> Events { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Owner> Owners { get; set; }
        public virtual ICollection<Teacher> Teachers { get; set; }
        public virtual ICollection<Promoter> Promoters { get; set; }
    }
}