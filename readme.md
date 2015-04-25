# Rock Ideal Postcodes Location Service

### Intro
This is a location service for [Rock](http://rockrms.com) that verifies, standardises, and geocodes UK addresses using the [Ideal Postcodes](http://ideal-postcodes.co.uk) API.

The repository includes the C# source for use with the [Rockit SDK](http://www.rockrms.com/Rock/Developer). 
To download the latest release of the plugin in .dll format click [here](https://github.com/arranf/IdealPostcodesLocationService/releases/latest).

### A Quick Explanation
This location service will pass the values (if any are present) of the address line 1, address line 2, city, state, and postal code fields from Rock to Ideal Postcodes. It will then ask for the best match and if values are present in the response it will replace the values stored in Rock with the response values.

The matching is as follows

Rock Field | Ideal Postcodes Data Source
---- | ----
Address 1 | Address Line 1
Address 2 | Address Line 2
City | Post Town (Lowercase)
State | County

**Note:**
The use of the county address line was deprecated in 1996 and as a result not every request will return a matching county field. For example, 10 Downing Street does not have a county according to PAF data. 

Ideal Postcodes uses three sources for county data. 
>Normally, the postal county is returned. If this is not present, the county field will fall back to the administrative county. If the administrative county is also not present, the county field will fall back to the traditional county

Ideal Postcodes will also geocode addresses using ONS and Ordanance Survey data. This information updates roughly quarterly so in certain situations information may not be available.

### Closing
If anything looks broken, flag up an issue. 

This project is licensed under the [Apache 2.0 License](http://www.apache.org/licenses/LICENSE-2.0.html).
