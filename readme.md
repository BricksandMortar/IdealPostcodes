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

Ideal Postcodes will also geocode addresses using ONS and Ordanance Survey data. This information updates roughly quarterly so in certain situations information may not be available.

###Examples
Addresses generated randomly from Google Maps data

Input Address | Address 1 | Address 2 | City | County | Postcode
---- | ---- | --- | --- | --- | ---
6 Glenree, Alyth, Blairgowrie, Perth and Kinross PH11 8EA, UK | 6 Glenree | Alyth | Blairgowrie | Perthshire | PH11 8EA
63A Bridge Street, Worksop, Nottinghamshire S80 1DG, UK | 63a Bridge Street | | Worksop | Nottinghamshire | S80 1DG
South Downs National Park, 12 Fiona Close, Winchester, Hampshire SO23 0HB, UK | 12 Fiona Close | | Winchester |  Hampshire | SO23 0HB
48 Acre Lane, Carshalton, Greater London SM5 3AB, UK | 48 Acre Lane | | Carshalton | Surrey | SM5 3AB
4 Upper Achagoyle, Minard, Inveraray, Argyll and Bute PA32 8YF, UK | 4 Upper Achagoyle | Minard | Inveraray | Argyll | PA32 8YF

 would standardize to 

### Closing
If anything looks broken, flag up an issue. 

This project is licensed under the [Apache 2.0 License](http://www.apache.org/licenses/LICENSE-2.0.html).
