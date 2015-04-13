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
            if (addressStr != null)
            {
                var googleStr = "https://maps.googleapis.com/maps/api/geocode/json?address=" + HttpUtility.UrlEncode(addressStr) + "&key=AIzaSyDJ-5NkuPkx9Q7PcmBMcHGVB7R5eYa0yRo";
                var result = new System.Net.WebClient().DownloadString(googleStr);
                var resObj = JsonConvert.DeserializeObject<GoogleGeoCodeResponse>(result);

                if (resObj.status != "ZERO_RESULTS")
                {
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
                    address.Street = (address.StreetNumber != null && address.StreetName != null) ? address.StreetNumber + " " + address.StreetName : null;
                }
                else
                {
                    address = null;
                }
            }
            return address;
        }

        /// <summary>
        /// The distance type to return the results in.
        /// </summary>
        public enum DistanceType { Miles, Kilometers };

        public struct Position
        {
            public double Latitude;
            public double Longitude;
        }

        /// <summary>
        /// Returns the distance in miles or kilometers of any two
        /// latitude / longitude points.
        /// </summary>
        /// <param name=”pos1″></param>
        /// <param name=”pos2″></param>
        /// <param name=”type”></param>
        /// <returns></returns>
        public static double Distance(Position pos1, Position pos2, DistanceType type)
        {
            double R = (type == DistanceType.Miles) ? 3960 : 6371;
 
            double dLat = toRadian(pos2.Latitude - pos1.Latitude);
            double dLon = toRadian(pos2.Longitude - pos1.Longitude);
 
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(toRadian(pos1.Latitude)) *Math.Cos(toRadian(pos2.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
            double d = R * c;
 
            return d;
        }
 
        /// <summary>
        /// Convert to Radians.
        /// </summary>
        /// <param name=”val”></param>
        /// <returns></returns>
        public static double toRadian(double val)
        {
            return (Math.PI / 180) * val;
        }    
    }
}