using System;
using System.ComponentModel.DataAnnotations;

namespace EDR.Enums
{
    public enum Frequency
    {
        Weekly = 1,
        Monthly = 2,
        Yearly = 3,
        Daily = 0
    }

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
        Video,
        [Display(Name = "Comment")]
        Comment
    }

    public enum EventType
    {
        [Display(Name = "Social")]
        Social,
        [Display(Name = "Class")]
        Class,
        [Display(Name = "Audition")]
        Audition,
        [Display(Name = "Performance")]
        Performance,
        [Display(Name = "Rehearsal")]
        Rehearsal
    }

    public enum MediaSource
    {
        [Display(Name = "None")]
        None,
        [Display(Name = "Facebook")]
        Facebook,
        [Display(Name = "YouTube")]
        YouTube,
        [Display(Name = "Instagram")]
        Instagram,
        [Display(Name = "Meetup")]
        Meetup,
        [Display(Name = "LinkedIn")]
        LinkedIn,
        [Display(Name = "Yelp")]
        Yelp
    }

    public enum PlaceType
    {
        [Display(Name = "Conference Center")]
        ConferenceCenter = 0,
        [Display(Name = "Hotel")]
        Hotel = 1,
        [Display(Name = "Nightclub")]
        Nightclub = 2,
        [Display(Name = "Other Place")]
        OtherPlace = 3,
        [Display(Name = "Restaurant")]
        Restaurant = 4,
        [Display(Name = "Studio")]
        Studio = 5,
        [Display(Name = "Theater")]
        Theater = 6,
        [Display(Name = "Museum")]
        Museum = 7
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
        Advanced = 5,
        [Display(Name = "Open Level")]
        Open = 6
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
        Party,
        [Display(Name = "Festival")]
        Festival
    }

    public enum State
    {
        [Display(Name = "Alabama")]
        AL = 1,

        [Display(Name = "Alaska")]
        AK = 2,

        [Display(Name = "Arkansas")]
        AR = 3,

        [Display(Name = "Arizona")]
        AZ = 4,

        [Display(Name = "California")]
        CA = 5,

        [Display(Name = "Colorado")]
        CO = 6,

        [Display(Name = "Connecticut")]
        CT = 7,

        [Display(Name = "D.C.")]
        DC = 8,

        [Display(Name = "Delaware")]
        DE = 9,

        [Display(Name = "Florida")]
        FL = 10,

        [Display(Name = "Georgia")]
        GA = 11,

        [Display(Name = "Hawaii")]
        HI = 12,

        [Display(Name = "Iowa")]
        IA = 13,

        [Display(Name = "Idaho")]
        ID = 14,

        [Display(Name = "Illinois")]
        IL = 15,

        [Display(Name = "Indiana")]
        IN = 16,

        [Display(Name = "Kansas")]
        KS = 17,

        [Display(Name = "Kentucky")]
        KY = 18,

        [Display(Name = "Louisiana")]
        LA = 19,

        [Display(Name = "Massachusetts")]
        MA = 20,

        [Display(Name = "Maryland")]
        MD = 21,

        [Display(Name = "Maine")]
        ME = 22,

        [Display(Name = "Michigan")]
        MI = 23,

        [Display(Name = "Minnesota")]
        MN = 24,

        [Display(Name = "Missouri")]
        MO = 25,

        [Display(Name = "Mississippi")]
        MS = 26,

        [Display(Name = "Montana")]
        MT = 27,

        [Display(Name = "North Carolina")]
        NC = 28,

        [Display(Name = "North Dakota")]
        ND = 29,

        [Display(Name = "Nebraska")]
        NE = 30,

        [Display(Name = "New Hampshire")]
        NH = 31,

        [Display(Name = "New Jersey")]
        NJ = 32,

        [Display(Name = "New Mexico")]
        NM = 33,

        [Display(Name = "Nevada")]
        NV = 34,

        [Display(Name = "New York")]
        NY = 35,

        [Display(Name = "Oklahoma")]
        OK = 36,

        [Display(Name = "Ohio")]
        OH = 37,

        [Display(Name = "Oregon")]
        OR = 38,

        [Display(Name = "Pennsylvania")]
        PA = 39,

        [Display(Name = "Rhode Island")]
        RI = 40,

        [Display(Name = "South Carolina")]
        SC = 41,

        [Display(Name = "South Dakota")]
        SD = 42,

        [Display(Name = "Tennessee")]
        TN = 43,

        [Display(Name = "Texas")]
        TX = 44,

        [Display(Name = "Utah")]
        UT = 45,

        [Display(Name = "Virginia")]
        VA = 46,

        [Display(Name = "Vermont")]
        VT = 47,

        [Display(Name = "Washington")]
        WA = 48,

        [Display(Name = "Wisconsin")]
        WI = 49,

        [Display(Name = "West Virginia")]
        WV = 50,

        [Display(Name = "Wyoming")]
        WY = 51
    }

    public enum UpdateType
    {
        Add,
        Edit
    }

    public enum ExternalObjectType
    {
        [Display(Name = "Group")]
        Group,
        [Display(Name = "Event")]
        Event,
        [Display(Name = "Page")]
        Page
    }

    public enum MediaTarget
    {
        User,
        Event,
        Place
    }

    public enum MusicType
    {
        [Display(Name = "DJ")]
        DJ,
        [Display(Name = "Live Band")]
        LiveBand,
        [Display(Name = "Live Band & DJ")]
        Both
    }

    public enum PaymentType
    {
        [Display(Name = "Credit Card")]
        CC,
        [Display(Name = "Check")]
        Check,
        [Display(Name = "Bill Pay")]
        BillPay,
        [Display(Name = "Direct Deposit")]
        Direct,
        [Display(Name = "Wire Transfer")]
        Wire,
        [Display(Name = "Internal Charge/Credit")]
        Internal
    }

    public enum Gender
    {
        [Display(Name = "Male/Female")]
        Both,
        [Display(Name = "Male")]
        Male,
        [Display(Name = "Female")]
        Female
    }
}