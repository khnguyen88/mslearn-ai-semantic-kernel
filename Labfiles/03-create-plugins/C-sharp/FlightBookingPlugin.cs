using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;

public class FlightBookingPlugin
{
    private const string FilePath = "flights.json";
    private List<FlightModel> flights;

    public FlightBookingPlugin()
    {
        // Load flights from the file
        flights = LoadFlightsFromFile();
    }

    // Create a plugin function with kernel function attributes
    [KernelFunction("search_flights")]
    [Description("Searches for avaliable fligts based on the destination and depature date in the formate YYYY-MM-DD")]
    [return: Description("A list of avaliable flights")]
    public List<FlightModel> SearchFlights(string destination, string departureDate)
    {
        // Filter flights based on destination
        return flights.Where(flights =>
            flights.Destination.Equals(destination, StringComparison.OrdinalIgnoreCase) &&
            flights.DepartureDate.Equals(departureDate)
        ).ToList();
    }


    // Create a kernel function to book flights
    [KernelFunction("book_flight")]
    [Description("Books a flight based on the flight ID provided")]
    [return: Description("Booking confirmation message")]
    public string BookFlight(int flightId)
    {
        var flight = flights.FirstOrDefault(f => f.Id == flightId);

        if (flight == null)
        {
            return "Flight was not found. Please provide a valid Flight Id.";
        }

        if (flight.IsBooked)
        {
            return $"You've already booked flight {flightId}";
        }

        flight.IsBooked = true;
        SaveFlightsToFile();

        return  @$"Flight booked successfully. Airline: {flight.Airline},
                Destination: {flight.Destination},
                Departure Date: {flight.DepartureDate},
                Price: ${flight.Price}.";
    }


    private void SaveFlightsToFile()
    {
        var json = JsonSerializer.Serialize(flights, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }

    private List<FlightModel> LoadFlightsFromFile()
    {
        if (File.Exists(FilePath))
        {
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<FlightModel>>(json)!;
        }

        throw new FileNotFoundException($"The file '{FilePath}' was not found. Please provide a valid flights.json file.");
    }
}

// Flight model
public class FlightModel
{
    public int Id { get; set; }
    public required string Airline { get; set; }
    public required string Destination { get; set; }
    public required string DepartureDate { get; set; }
    public decimal Price { get; set; }
    public bool IsBooked { get; set; } = false; // Added to track booking status
}
