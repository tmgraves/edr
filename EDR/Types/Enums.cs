using System;
using System.ComponentModel.DataAnnotations;

namespace EDR.Enums
{
    public enum ClassType
    {
        [Display(Name = "Class")]
        Class,
        [Display(Name = "Workshop")]
        Workshop
    }

    public enum MediaType
    {
        [Display(Name = "Picture")]
        Picture,
        [Display(Name = "Video")]
        Video
    }

    public enum EventType
    {
        [Display(Name = "Concert")]
        Concert,
        [Display(Name = "Conference")]
        Conference,
        [Display(Name = "Social")]
        Social,
        [Display(Name = "Party")]
        Party,
        [Display(Name = "Rehearsal")]
        Rehearsal,
        [Display(Name = "Workshop")]
        Workshop,
        [Display(Name = "Class")]
        Class
    }

    public enum PlaceType
    {
        [Display(Name = "Conference Center")]
        ConferenceCenter,
        [Display(Name = "Hotel")]
        Hotel,
        [Display(Name = "Nightclub")]
        Nightclub,
        [Display(Name = "Other Place")]
        OtherPlace,
        [Display(Name = "Restaurant")]
        Restaurant,
        [Display(Name = "Studio")]
        Studio,
        [Display(Name = "Theater")]
        Theater
    }

    public enum RoleName
    {
        [Display(Name = "Teacher")]
        Teacher,
        [Display(Name = "Owner")]
        Owner,
        [Display(Name = "Promoter")]
        Promoter
    }

    public enum SkillLevel
    {
        [Display(Name = "Beginner")]
        Beginner = 1,
        [Display(Name = "Beginner/Intermediate")]
        BeginnerIntermediate = 2,
        [Display(Name = "Intermediate")]
        Intermediate = 3,
        [Display(Name = "Intermediate/Advanced")]
        IntermediateAdvanced = 4,
        [Display(Name = "Advanced")]
        Advanced = 5
    }

    public enum SocialType
    {
        [Display(Name = "Social")]
        Social,
        [Display(Name = "Concert")]
        Concert,
        [Display(Name = "Conference")]
        Conference,
        [Display(Name = "OpenHouse")]
        OpenHouse,
        [Display(Name = "Party")]
        Party
    }

    public enum State
    {
        [Display(Name = "Alabama")]
        AL,

        [Display(Name = "Alaska")]
        AK,

        [Display(Name = "Arkansas")]
        AR,

        [Display(Name = "Arizona")]
        AZ,

        [Display(Name = "California")]
        CA,

        [Display(Name = "Colorado")]
        CO,

        [Display(Name = "Connecticut")]
        CT,

        [Display(Name = "D.C.")]
        DC,

        [Display(Name = "Delaware")]
        DE,

        [Display(Name = "Florida")]
        FL,

        [Display(Name = "Georgia")]
        GA,

        [Display(Name = "Hawaii")]
        HI,

        [Display(Name = "Iowa")]
        IA,

        [Display(Name = "Idaho")]
        ID,

        [Display(Name = "Illinois")]
        IL,

        [Display(Name = "Indiana")]
        IN,

        [Display(Name = "Kansas")]
        KS,

        [Display(Name = "Kentucky")]
        KY,

        [Display(Name = "Louisiana")]
        LA,

        [Display(Name = "Massachusetts")]
        MA,

        [Display(Name = "Maryland")]
        MD,

        [Display(Name = "Maine")]
        ME,

        [Display(Name = "Michigan")]
        MI,

        [Display(Name = "Minnesota")]
        MN,

        [Display(Name = "Missouri")]
        MO,

        [Display(Name = "Mississippi")]
        MS,

        [Display(Name = "Montana")]
        MT,

        [Display(Name = "North Carolina")]
        NC,

        [Display(Name = "North Dakota")]
        ND,

        [Display(Name = "Nebraska")]
        NE,

        [Display(Name = "New Hampshire")]
        NH,

        [Display(Name = "New Jersey")]
        NJ,

        [Display(Name = "New Mexico")]
        NM,

        [Display(Name = "Nevada")]
        NV,

        [Display(Name = "New York")]
        NY,

        [Display(Name = "Oklahoma")]
        OK,

        [Display(Name = "Ohio")]
        OH,

        [Display(Name = "Oregon")]
        OR,

        [Display(Name = "Pennsylvania")]
        PA,

        [Display(Name = "Rhode Island")]
        RI,

        [Display(Name = "South Carolina")]
        SC,

        [Display(Name = "South Dakota")]
        SD,

        [Display(Name = "Tennessee")]
        TN,

        [Display(Name = "Texas")]
        TX,

        [Display(Name = "Utah")]
        UT,

        [Display(Name = "Virginia")]
        VA,

        [Display(Name = "Vermont")]
        VT,

        [Display(Name = "Washington")]
        WA,

        [Display(Name = "Wisconsin")]
        WI,

        [Display(Name = "West Virginia")]
        WV,

        [Display(Name = "Wyoming")]
        WY
    }
}