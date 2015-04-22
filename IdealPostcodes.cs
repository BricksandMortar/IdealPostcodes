using System;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Web;
using System.Web;
using Rock;
using Rock.Attribute;
using Rock.Web.UI;
using Rock.Address;
using RestSharp;
using Newtonsoft.Json;

namespace Rock.Address
{
    /// <summary>
    /// The address lookup and geocoding service from <a href="https://ideal-postcodes.co.uk">Ideal Postcodes</a>
    /// </summary>
    [Description("A address verification and geocoding service from Ideal Postcodes")]
    [Export(typeof(VerificationComponent))]
    [ExportMetadata("ComponentName", "Ideal Postcodes")]
    [TextField("API Key", "Your Ideal Postcodes API Key", true, "", "", 2)]
    public class IdealPostcodes : VerificationComponent
    {
        /// <summary>
        /// Standardizes and Geocodes an address using the Ideal Postcodes service
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="reVerify">Should location be reverified even if it has already been succesfully verified</param>
        /// <param name="result">The result code unique to the service.</param>
        /// <returns>
        /// True/False value of whether the verification was successful or not
        /// </returns>
        public override bool VerifyLocation( Rock.Model.Location location, bool reVerify, out string result)
        {
            bool verified = false;
            result = string.Empty;

            // Only verify if location is valid, has not been locked, and 
            // has either never been attempted or last attempt was in last 30 secs (prev active service failed) or reverifying
            if ( location != null && 
                !(location.IsGeoPointLocked ?? false) &&  
                (
                    !location.GeocodeAttemptedDateTime.HasValue || 
                    location.GeocodeAttemptedDateTime.Value.CompareTo( RockDateTime.Now.AddSeconds(-30) ) > 0 ||
                    reVerify
                ) )
            {

                string inputKey = GetAttributeValue( "APIKey" );
                var addressParts = new string[] 
                {location.Street1, location.Street2, location.City, location.PostalCode};
                
                string inputAddress = string.Join(" ", addressParts.Where(s => !string.IsNullOrEmpty(s)));

                var client = new RestClient("https://api.ideal-postcodes.co.uk/");
                var request = new RestRequest(Method.GET);
				request.RequestFormat = DataFormat.Json;
                request.Resource = "v1/addresses/";
                request.AddParameter("api_key", inputKey);
                request.AddParameter("query", inputAddress);
                request.AddParameter("limit", "1");
                var response = client.Execute( request );
				
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var droot = JsonConvert.DeserializeObject <RootObject>(response.Content);
                    var dresult = droot.result;
                    var hitresult = dresult.hits;
                        if (hitresult.Any())
                        {
                                var address = hitresult.FirstOrDefault();
                                verified = true;
                                result = string.Format( "UDPRN: {0}", address.udprn ); 
                                location.Street1 = address.line_1;
                                location.Street2 = address.line_2;
                                location.City = address.post_town;
                                string county = address.county;
                                location.State = county;
                                location.PostalCode = address.postcode;
                                location.StandardizedDateTime = RockDateTime.Now;
                                location.SetLocationPointFromLatLong( address.latitude, address.longitude );
                                location.GeocodedDateTime = RockDateTime.Now;	
                        }
                        else
                        {
                                result = "No match.";
                                verified = false;
                        }	
                }
                else
                {
                    result = response.StatusDescription;
                }

            }

            location.StandardizeAttemptedServiceType = "IdealPostcodes";
            location.StandardizeAttemptedDateTime = RockDateTime.Now;

            location.GeocodeAttemptedServiceType = "IdealPostcodes";
            location.GeocodeAttemptedDateTime = RockDateTime.Now;

            return verified;
        }
       
    }

#pragma warning disable
    public class Hit
    {
        public string dependant_locality { get; set; }
        public string postcode_type { get; set; }
        public string po_box { get; set; }
        public string post_town { get; set; }
        public string delivery_point_suffix { get; set; }
        public string double_dependant_locality { get; set; }
        public string su_organisation_indicator { get; set; }
        public double longitude { get; set; }
        public string department_name { get; set; }
        public string district { get; set; }
        public string building_name { get; set; }
        public string dependant_thoroughfare { get; set; }
        public int northings { get; set; }
        public string premise { get; set; }
        public string postcode_outward { get; set; }
        public string postcode_inward { get; set; }
        public string sub_building_name { get; set; }
        public int eastings { get; set; }
        public string postcode { get; set; }
        public string country { get; set; }
        public int udprn { get; set; }
        public string line_3 { get; set; }
        public string organisation_name { get; set; }
        public string ward { get; set; }
        public string county { get; set; }
        public string line_1 { get; set; }
        public string building_number { get; set; }
        public string thoroughfare { get; set; }
        public string line_2 { get; set; }
        public double latitude { get; set; }
    }

    public class Result
    {
        public int total { get; set; }
        public int limit { get; set; }
        public int page { get; set; }
        public List<Hit> hits { get; set; }
    }

    public class RootObject
    {
        public Result result { get; set; }
        public int code { get; set; }
        public string message { get; set; }
    }
#pragma warning restore
    }