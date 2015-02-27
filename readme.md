# Rock Ideal Postcodes Location Service

### Intro
This is a location service for [Rock](http://rockrms.com) that verifies, standardises, and geocodes UK addresses using the [Ideal Postcodes](ideal-postcodes.co.uk) API.

The repository includes the Visual Studio solution for use with the [Rockit SDK](http://www.rockrms.com/Rock/Developer) and also the .dll file that will eventually be packaged as a Rock Store plugin.

### A Quick Explanation
This location service will pass the values (if any are present) of the address line 1, address line 2, city, state, and ZIP code fields to Ideal Postcodes. It will then ask for the best match and where values are present in the response, replace the values stored in Rock with the response values.

<b>Note:</b> The use of the county address line was deprecated in 1996 and as a result not every request will return a matching county field. For example, 10 Downing Street does not have a county according to PAF data. As a result I've decided to pass the administrative district data from the ONS if no county data is present. I may put a toggle on this behaviour in a future release.

### Closing
If anything looks broken, flag up an issue. 

This project is licensed under the [Apache 2.0 License](http://www.apache.org/licenses/LICENSE-2.0.html).
