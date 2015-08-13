using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace GoogleDirections
{
    class Program
    {
        private static string UrlFormat = "https://maps.googleapis.com/maps/api/directions/json?origin={0}&destination={1}&key={2}";

        static void Main(string[] args)
        {
            var apiKey = ConfigurationManager.AppSettings["GoogleApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine("Error: ApiKey not found!");
                return;
            }

            while (true)
            {
                Console.WriteLine("Please enter origin/destination (leave blank to exit)");
                Console.Write("Origin: ");
                var origin = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(origin))
                    break;

                Console.Write("Destination: ");
                var destination = Console.ReadLine();
                if(string.IsNullOrWhiteSpace(destination))
                    break;

                var directions = GetDirections(origin, destination, apiKey);
                if (directions != null)
                {
                    PrintDirections(directions, origin, destination);
                }

                Console.WriteLine("Press ENTER to continue...");
                Console.ReadLine();
                Console.Clear();
            }
        }

        private static void PrintDirections(Directions directions, string origin, string destination)
        {
            Console.WriteLine("Directions: {0} to {1}", origin, destination);
            Console.WriteLine(directions.copyrights);

            for (var r = 0; r < directions.Routes.Count(); r++)
            {
                var route = directions.Routes.ElementAt(r);

                Console.WriteLine();
                Console.WriteLine("Route {0}: {1} [Distiance={2}, Duration={3}]",
                    r + 1, route.summary, route.distance, route.duration);

                for (var l = 0; l < route.Legs.Count(); l++)
                {
                    var leg = route.Legs.ElementAt(l);

                    Console.WriteLine("");
                    Console.WriteLine("Leg #{0}", l + 1);

                    for (var s = 0; s < leg.Steps.Count(); s++)
                    {
                        var step = leg.Steps.ElementAt(s);

                        Console.WriteLine("[{0} - {1} - {2}]: {3}",
                            step.travel_mode, step.duration.text,
                            step.distance.text,
                            step.html_instructions);
                    }
                }
            } 
        }

        private static Directions GetDirections(string origin, string destination, string apiKey)
        {
            var url = string.Format(UrlFormat, HttpUtility.UrlEncode(origin),
                HttpUtility.UrlEncode(destination), apiKey);

            try
            {
                using (var client = new HttpClient())
                {
                    var response = client.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                        return response.Content.ReadAsAsync<Directions>().Result;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to get directions from {0} to {1}. Try again...",
                    origin, destination);
            }

            return null;
        }

        public class Directions
        {
            public IEnumerable<Route> Routes { get; set; }
            public string copyrights { get; set; }
        }

        public class Route
        {
            public string summary { get; set; }
            public ValueAndText duration { get; set; }
            public ValueAndText distance { get; set; }
            public string start_address { get; set; }
            public string end_address { get; set; }
            public IEnumerable<Leg> Legs { get; set; }
        }

        public class Leg
        {
            public IEnumerable<Step> Steps { get; set; }
        }

        public class Step
        {
            public string travel_mode { get; set; }
            public ValueAndText duration { get; set; }
            public ValueAndText distance { get; set; }
            public string html_instructions { get; set; }
        }

        public class ValueAndText
        {
            public int value { get; set; }
            public string text { get; set; }
        }
    }
}
