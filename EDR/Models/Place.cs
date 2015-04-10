﻿using EDR.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace EDR.Models
{
    public class Place : Entity
    {
        [Required(ErrorMessage = "Place Name is Required")]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        public string Address2 { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        [Range(1, 50000, ErrorMessage = "Select a State")]
        public State State { get; set; }
        [Required(ErrorMessage = "Zipcode is Required")]
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
        public bool Public { get; set; }

        public virtual ICollection<Event> Events { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Owner> Owners { get; set; }
        public virtual ICollection<Teacher> Teachers { get; set; }
        public virtual ICollection<Promoter> Promoters { get; set; }
        public ICollection<PlaceOwnerInvitation> PlaceOwnerInvitations { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}