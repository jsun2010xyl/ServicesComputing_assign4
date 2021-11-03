// Name: Jingyan Sun
// Nov 15, 2020
// This service can detect the client's IP address and find out its location, then it will
// 		return the city's geographic information.
// We define 6 methods in the class "cityinfoController": Get, detectIP, convertIP,
//		coordinates, nearestCityByCoord, nearestCityByCoordXML.
// The first service provider (SP1) we consume is to detect the client's IP address. In
//		this case, it's the IP address of the GCP's server.
// SP2 is to get the city name of the IP's location.
// SP3 is to get the city's coordinates by its name.
// SP4 is to get the city's geographic information by its coordinates. Notice that SP4
//		only accepts coordinates as parameters in the requests. One cannot get a city's
//		geographic information by sending the city's name.

using System.Collections.Generic; using Microsoft.AspNetCore.Mvc; using System; 
using System.Net; 
using System.Text; 
using System.IO; 
using Newtonsoft.Json; 
using Newtonsoft.Json.Linq;

namespace cityinfoController.Controllers 
{ 
	[ApiController] 
	[Route("[controller]")] // specifies the URL pattern for a controller or action 
	public class cityinfoController : ControllerBase 
	{ 
		// write a handler for visiting the main root of the web service 
		[Route("")] // specifies an attribute route on a controller. 
		[HttpGet] // identifies an action that supports the HTTP GET method. 
		public string Get() 
		{ 
			var ip = detectIP(); 
			var city = convertIP(ip); 
			var coord = coordinates(city); //get coordinates
			var cityinfo0 = nearestCityByCoord(coord);
			return cityinfo0;
		} 

		[HttpGet] 
		[Route("ip")] 
		public string detectIP() 
		{ 
			try{
				// identifies the service endpoint for Service Provider 1 (SP1) 
				var serviceURL = "https://api.ipify.org/"; 
				// prepare the HTTP request 
				WebRequest serviceRequest = (WebRequest)WebRequest.Create(serviceURL); 
				// identify the method of HTTP request as GET 
				serviceRequest.Method = "GET"; 
				serviceRequest.ContentLength = 0; 
				serviceRequest.ContentType = "plain/text"; 
				// establish a connection and retrieve a HTTP response message 
				WebResponse serviceResponse = (WebResponse)serviceRequest.GetResponse(); 
				// read response data stream 
				Stream receiveStream = serviceResponse.GetResponseStream(); 
				// properly set the encoding as utf-8 
				Encoding encode = System.Text.Encoding.GetEncoding("utf-8"); 
				// encode the stream using utf-8 
				StreamReader readStream = new StreamReader(receiveStream, encode, true); 
				// read entire stream and store in serviceResult 
				string serviceResult = readStream.ReadToEnd(); 
				// return serviceResult as string 
				return serviceResult; }
			catch(Exception e){
				return "Error! Cannot detect the client's IP address.";
			}
		}

		[HttpGet] 
		[Route("city")] 
		public string convertIP(string IP) { 
			try {
				// create a URL that contains the service providers 2 (SP2) endpoint including the IP address 
				// this format is provided by the service provider (http://ip-api.com/json/IP-address-here) 
				var serviceURL = "http://ip-api.com/json/" + IP; 
				// prepare HTTP request 
				WebRequest serviceRequest = (WebRequest)WebRequest.Create(serviceURL); 
				// set appropriate parameters in the header (use GET method) 
				serviceRequest.Method = "GET"; 
				serviceRequest.ContentLength = 0; 
				// set contentType to JSON 
				serviceRequest.ContentType = "application/json"; 
				// establish a connection and retrieve a HTTP response message 
				WebResponse serviceResponse = (WebResponse)serviceRequest.GetResponse(); 
				Stream receiveStream = serviceResponse.GetResponseStream(); 
				Encoding encode = System.Text.Encoding.GetEncoding("utf-8"); 
				StreamReader readStream = new StreamReader(receiveStream, encode, true); 
				string serviceResult = readStream.ReadToEnd(); 
				// use the JSON Object to parse the result received in the response body 
				var json = JObject.Parse(serviceResult); 
				// select at the first level (using SelectToken method) a key called city 
				return json.SelectToken("city").ToString(); }
			catch(Exception e){
				return "Error! Cannot find out the location of the IP address.";
			}
		}

		[HttpGet] 
		[Route("coordinates")] 
		public string coordinates(string city) { 
			try {
				//There should be no blank in the string "city"
				city = city.Replace(" ", "");
				// create a URL that contains the service providers 3 (SP3) endpoint
				// this format is provided by the service provider
				var serviceURL = "https://api.weatherbit.io/v2.0/forecast/daily?city="
									+ city + "&key=d40c9fcdbb7f46a4ac836c97db909d66";
				
				// prepare HTTP request 
				WebRequest serviceRequest = (WebRequest)WebRequest.Create(serviceURL); 
				// set appropriate parameters in the header (use GET method) 
				serviceRequest.Method = "GET"; 
				serviceRequest.ContentLength = 0; 
				// set contentType to JSON 
				serviceRequest.ContentType = "application/json"; 
				// establish a connection and retrieve a HTTP response message 
				WebResponse serviceResponse = (WebResponse)serviceRequest.GetResponse(); 
				Stream receiveStream = serviceResponse.GetResponseStream(); 
				Encoding encode = System.Text.Encoding.GetEncoding("utf-8"); 
				StreamReader readStream = new StreamReader(receiveStream, encode, true); 
				string serviceResult = readStream.ReadToEnd(); 
				// use the JSON Object to parse the result received in the response body 
				var json = JObject.Parse(serviceResult); 
				// select at the first level (using SelectToken method) a key called lon
				string lon = json.SelectToken("lon").ToString(); 
				string lat = json.SelectToken("lat").ToString(); 
				return "lat" + lat + "lng" + lon;
				//format: lat37.3861lng-122.084
			}catch(Exception e){
				return "Error! Cannot find out the coordinates of the city.";
			}
		}

		[HttpGet] 
		[Route("cityinfojson")] 
		public string nearestCityByCoord(string coord) { 
			// There should be no "=" or "&" in the string "coord", we use try-catch to handle it
			// 		and other exceptions.
			try{
				// We transform the "coord" string's format into "lat=XXX&lng=XXX"
				if ((coord.Contains("lat")) && (coord.Contains("lng"))){
					coord=coord.Insert(3,"=");
					int i;
					for (i=4; i<coord.Length; i++){
						if (coord[i] == 'l'){
							coord=coord.Insert(i,"&");
							coord=coord.Insert(i+4,"=");
							break;
						}
					}
				}else{
					return "Wrong coordinates format!";
				}
				// create a URL that contains the service providers 4 (SP4) endpoint
				// this format is provided by the service provider
				var serviceURL = "https://api.geodatasource.com/city?key=SCB8TBZVO2ZH11MTA96Q0ABLMXXV3XKP&"
									+ coord; 
				WebRequest serviceRequest = (WebRequest)WebRequest.Create(serviceURL); 
				serviceRequest.Method = "GET"; 
				serviceRequest.ContentLength = 0; 
				serviceRequest.ContentType = "application/json"; 
				HttpWebResponse serviceResponse = (HttpWebResponse)serviceRequest.GetResponse(); 
				Stream receiveStream = serviceResponse.GetResponseStream(); 
				Encoding encode = System.Text.Encoding.GetEncoding("utf-8"); 
				StreamReader readStream = new StreamReader(receiveStream, encode, true); 
				string serviceResult = readStream.ReadToEnd(); 
				return serviceResult; }
			catch(Exception e){
				return "Error! Wrong coordinates or the coordinates' format is incorrect.";
			}
		}

		[HttpGet] 
		[Route("cityinfoxml")] 
		// This method is similar to the nearestCityByCoord method. The only difference is
		// 		that this method returns XML.
		public string nearestCityByCoordXML(string coord) { 
			// There should be no "=" or "&" in the string "coord", we use try-catch to handle it
			// 		and other exceptions.
			try {
				// We transform the "coord" string's format into "lat=XXX&lng=XXX"
				if ((coord.Contains("lat")) && (coord.Contains("lng"))){
					coord=coord.Insert(3,"=");
					int i;
					for (i=4; i<coord.Length; i++){
						if (coord[i] == 'l'){
							coord=coord.Insert(i,"&");
							coord=coord.Insert(i+4,"=");
							break;
						}
					}
				}else{
					return "Wrong coordinates format!";
				}

				// create a URL that contains the service providers 4 (SP4) endpoint
				// this format is provided by the service provider
				var serviceURL = "https://api.geodatasource.com/city?key=SCB8TBZVO2ZH11MTA96Q0ABLMXXV3XKP&format=xml&"
									+ coord; 
				WebRequest serviceRequest = (WebRequest)WebRequest.Create(serviceURL); 
				serviceRequest.Method = "GET"; 
				serviceRequest.ContentLength = 0; 
				serviceRequest.ContentType = "application/json"; 
				HttpWebResponse serviceResponse = (HttpWebResponse)serviceRequest.GetResponse(); 
				Stream receiveStream = serviceResponse.GetResponseStream(); 
				Encoding encode = System.Text.Encoding.GetEncoding("utf-8"); 
				StreamReader readStream = new StreamReader(receiveStream, encode, true); 
				string serviceResult = readStream.ReadToEnd(); 
				return serviceResult; }
			catch (Exception e){
				return "Error! Wrong coordinates or the coordinates' format is incorrect.";
			}
		}
	} 
}