using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;

using Rock;
using Rock.Attribute;
using Rock.Address;
using Newtonsoft.Json;
using RestSharp;

namespace com.bricksandmortar.IdealPostcodes
{
    /// <summary>
    /// The address lookup and geocoding service from <a href="https://ideal-postcodes.co.uk">Ideal Postcodes</a>
    /// </summary>
    [Description("An address verification and geocoding service from Ideal Postcodes")]
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
                ) &&
                location.Country == "GB" &&
                (string.IsNullOrWhiteSpace(location.Street1) || string.IsNullOrWhiteSpace(location.Street2)))
            {

                string inputKey = GetAttributeValue( "APIKey" );
                var version = new Version( Rock.VersionInfo.VersionInfo.GetRockSemanticVersionNumber() );
                string tags = string.Empty;
                System.Data.Odbc.OdbcConnectionStringBuilder builder = new System.Data.Odbc.OdbcConnectionStringBuilder(ConfigurationManager.ConnectionStrings["RockContext"].ConnectionString);
                object catalog = string.Empty;
                if ( builder.TryGetValue("initial catalog", out catalog) )
                {
                   tags = string.Format("{0},{1}", version, catalog);
                }

                
                //Create address that encodes correctly
                var addressParts = new string[] 
                {location.Street1, location.Street2, location.City, location.PostalCode};  
                string inputAddress = string.Join(" ", addressParts.Where(s => !string.IsNullOrEmpty(s)));

                //restsharp API request
                var client = new RestClient("https://api.ideal-postcodes.co.uk/");
                var request = new RestRequest(Method.GET);
				request.RequestFormat = DataFormat.Json;
                request.Resource = "v1/addresses/";
                request.AddParameter("api_key", inputKey);
                request.AddParameter("query", inputAddress);
                request.AddParameter("limit", "1");
                request.AddParameter("tags", tags);
                var response = client.Execute( request );
				
                if (response.StatusCode == HttpStatusCode.OK)
                    //Create a series of vars to make decoded response accessible
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
                            if (!string.IsNullOrWhiteSpace(address.dependant_locality) && address.dependant_locality != address.line_2)
                            {
                                location.City = address.dependant_locality;
                            }
                            else
                            {
                                string city = address.post_town;
                                city = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(city.ToLower());
                                location.City = city;
                            }
                                
                            location.State = address.county;
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