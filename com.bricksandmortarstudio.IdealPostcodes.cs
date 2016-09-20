using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;

using Newtonsoft.Json;
using RestSharp;

using Rock.Attribute;
using Rock.Address;
using Rock;

namespace com.bricksandmortarstudio.IdealPostcodes.Address
{
    /// <summary>
    /// The address lookup and geocoding service from <a href="https://ideal-postcodes.co.uk">Ideal Postcodes</a>
    /// </summary>
    [Description( "An address verification and geocoding service from Ideal Postcodes" )]
    [Export( typeof( VerificationComponent ) )]
    [ExportMetadata( "ComponentName", "Ideal Postcodes" )]
    [TextField( "API Key", "Your Ideal Postcodes API key (begins with ak_)", true, "", "", 2 )]
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

        public override VerificationResult Verify( Rock.Model.Location location, out string resultMsg )
        {
            resultMsg = string.Empty;
            VerificationResult result = VerificationResult.None;


            string inputKey = GetAttributeValue( "APIKey" );
            string tags;
            CreateTags( out tags );

            //Create address that encodes correctly
            string inputAddress;
            CreateInputAddress( location, out inputAddress );

            //restsharp API request
            var client = new RestClient( "https://api.ideal-postcodes.co.uk/" );
            var request = BuildRequest( inputKey, inputAddress, tags );
            var response = client.Execute( request );

            if ( response.StatusCode == HttpStatusCode.OK )
            //Create a series of vars to make decoded response accessible
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                var idealResponse = JsonConvert.DeserializeObject<RootObject>( response.Content, settings );
                var idealAddress = idealResponse.result.hits;
                if ( idealAddress.Any() )
                {
                    var address = idealAddress.FirstOrDefault();
                    resultMsg = $"Verified by Ideal Postcodes UDPRN: {address.udprn}";
                    UpdateLocation( location, address );
                    result = result | VerificationResult.Standardized;
                }
                else
                {
                    resultMsg = "No match.";
                }
            }
            else
            {
                result = VerificationResult.ConnectionError;
            }

            location.StandardizeAttemptedServiceType = "IdealPostcodes";
            location.StandardizeAttemptedDateTime = RockDateTime.Now;

            location.GeocodeAttemptedServiceType = "IdealPostcodes";
            location.GeocodeAttemptedDateTime = RockDateTime.Now;
            return result;
        }

        private IRestRequest BuildRequest( string inputKey, string inputAddress, string tags )
        {
            var request = new RestRequest( Method.GET )
            {
                RequestFormat = DataFormat.Json,
                Resource = "v1/addresses/"
            };
            request.AddParameter( "api_key", inputKey );
            request.AddParameter( "query", inputAddress );
            request.AddParameter( "limit", "1" );
            request.AddParameter( "tags", tags );
            return request;
        }

        public void CreateTags( out string tags )
        {
            tags = null;
            var version = new Version( Rock.VersionInfo.VersionInfo.GetRockSemanticVersionNumber() );
            System.Data.Odbc.OdbcConnectionStringBuilder builder = new System.Data.Odbc.OdbcConnectionStringBuilder( ConfigurationManager.ConnectionStrings["RockContext"].ConnectionString );
            object catalog;
            if ( builder.TryGetValue( "initial catalog", out catalog ) )
            {
                tags = $"{version},{catalog}";
            }
        }

        public void CreateInputAddress( Rock.Model.Location location, out string inputAddress )
        {
            var addressParts = new string[] { location.Street1, location.Street2, location.City, location.PostalCode };
            inputAddress = string.Join( " ", addressParts.Where( s => !string.IsNullOrEmpty( s ) ) );
        }

        public void UpdateLocation( Rock.Model.Location location, ResultAddress address )
        {
            location.Street1 = address.line_1;
            location.Street2 = address.line_2;
            if ( !string.IsNullOrWhiteSpace( address.dependant_locality ) && address.dependant_locality != address.line_2 )
            {
                location.City = address.dependant_locality;
            }
            else
            {
                string city = address.post_town;
                city = CultureInfo.CurrentCulture.TextInfo.ToTitleCase( city.ToLower() );
                location.City = city;
            }

            location.State = address.county;
            location.PostalCode = address.postcode;
            location.StandardizedDateTime = RockDateTime.Now;
            if ( address.latitude.HasValue && address.longitude.HasValue )
            {
                location.SetLocationPointFromLatLong( address.latitude.Value, address.longitude.Value );

            }
            location.GeocodedDateTime = RockDateTime.Now;
        }


        [SuppressMessage( "ReSharper", "InconsistentNaming" )]
        public class ResultAddress
        {
            public string dependant_locality { get; set; }
            public string postcode_type { get; set; }
            public string po_box { get; set; }
            public string post_town { get; set; }
            public string delivery_point_suffix { get; set; }
            public string double_dependant_locality { get; set; }
            public string su_organisation_indicator { get; set; }
            public double? longitude { get; set; }
            public string department_name { get; set; }
            public string district { get; set; }
            public string building_name { get; set; }
            public string dependant_thoroughfare { get; set; }
            public int? northings { get; set; }
            public string premise { get; set; }
            public string postcode_outward { get; set; }
            public string postcode_inward { get; set; }
            public string sub_building_name { get; set; }
            public int? eastings { get; set; }
            public string postcode { get; set; }
            public string country { get; set; }
            public int? udprn { get; set; }
            public string line_3 { get; set; }
            public string organisation_name { get; set; }
            public string ward { get; set; }
            public string county { get; set; }
            public string line_1 { get; set; }
            public string building_number { get; set; }
            public string thoroughfare { get; set; }
            public string line_2 { get; set; }
            public double? latitude { get; set; }
        }

        [SuppressMessage( "ReSharper", "InconsistentNaming" )]
        public class Result
        {
            public int total { get; set; }
            public int limit { get; set; }
            public int page { get; set; }
            public List<ResultAddress> hits { get; set; }
        }

        [SuppressMessage( "ReSharper", "InconsistentNaming" )]
        public class RootObject
        {
            public Result result { get; set; }
            public int code { get; set; }
            public string message { get; set; }
        }
    }
}