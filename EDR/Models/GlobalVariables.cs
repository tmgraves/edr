using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public static class GlobalVariables
{
    // readonly variable
    public static double TicketRate
    {
        get
        {
            return .10;
        }
    }

    public static decimal PaymentThreshold
    {
        get
        {
            return 100;
        }
    }

    //  # of Days to allow deposit
    public static int SettlementPeriod
    {
        get
        {
            return 3;
        }
    }

    //// read-write variable
    //public static string Bar
    //{
    //    get
    //    {
    //        return HttpContext.Current.Application["Bar"] as string;
    //    }
    //    set
    //    {
    //        HttpContext.Current.Application["Bar"] = value;
    //    }
    //}
}
