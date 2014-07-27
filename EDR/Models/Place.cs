using System.Collections.Generic;
namespace EDR.Models
{
    public abstract class Place : Entity
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }

        public virtual ICollection<Event> Events { get; set; }
    }
}