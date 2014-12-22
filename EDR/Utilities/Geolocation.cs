using EDR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Utilities
{
    public static class Geolocation
    {
        private class GoogleGeoCodeResponse
        {
            public string status { get; set; }
            public results[] results { get; set; }
        }

        private class results
        {
            public string formatted_address { get; set; }
            public geometry geometry { get; set; }
            public string[] types { get; set; }
            public address_component[] address_components { get; set; }
        }

        private class geometry
        {
            public string location_type { get; set; }
            public location location { get; set; }
        }

        private class location
        {
            public string lat { get; set; }
            public string lng { get; set; }
        }

        private class address_component
        {
            public string long_name { get; set; }
            public string short_name { get; set; }
            public string[] types { get; set; }
        }

        public static Address ParseAddress(string addressStr)
        {
            var address = new Address();
            var googleStr = "http://maps.google.com/maps/api/geocode/json?sensor=false&address=" + addressStr;
            var result = new System.Net.WebClient().DownloadString(googleStr);
            var resObj = JsonConvert.DeserializeObject<GoogleGeoCodeResponse>(result);

            foreach (address_component comp in resObj.results[0].address_components)
            {
                if (comp.types[0] == "postal_code")
                {
                    address.ZipCode = comp.long_name;
                }
                else if (comp.types[0] == "street_number")
                {
                    address.StreetNumber = comp.long_name;
                }
                else if (comp.types[0] == "route")
                {
                    address.StreetName = comp.long_name;
                }
                else if (comp.types[0] == "locality")
                {
                    address.City = comp.long_name;
                }
                else if (comp.types[0] == "administrative_area_level_1")
                {
                    address.State = comp.short_name;
                }
                else if (comp.types[0] == "country")
                {
                    address.Country = comp.short_name;
                }
            }

            address.Longitude = Convert.ToDouble(resObj.results[0].geometry.location.lng);
            address.Latitude = Convert.ToDouble(resObj.results[0].geometry.location.lat);

            return address;
        }
    }
}