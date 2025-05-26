using System.Net.Http;
using System.Text.Json;
using OmdbWrapperClient.Models;

var handler = new HttpClientHandler();
handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

HttpClient httpClient = new HttpClient(handler)
{
    BaseAddress = new Uri("http://localhost:5009")
};

bool exit = false;

while (!exit)
{
    Console.WriteLine("\nSelect an option:");
    Console.WriteLine("1. Search by IMDb ID");
    Console.WriteLine("2. Search by Title");
    Console.WriteLine("3. Search by Year");
    Console.WriteLine("0. Exit");
    Console.Write("Option: ");
    var input = Console.ReadLine();

    switch (input)
    {
        case "1":
            await SearchById(httpClient);
            break;
        case "2":
            await SearchByTitle(httpClient);
            break;
        case "3":
            await SearchByYear(httpClient);
            break;
        case "0":
            exit = true;
            break;
        default:
            Console.WriteLine("Invalid option");
            break;
    }
}

static async Task SearchById(HttpClient client)
{
    Console.Write("Enter IMDb ID: ");
    var id = Console.ReadLine();
    var res = await client.GetAsync($"/api/movie/id/{id}");
    await PrintMovie(res);
}

static async Task SearchByTitle(HttpClient client)
{
    Console.Write("Enter Title: ");
    var title = Console.ReadLine();
    var res = await client.GetAsync($"/api/movie/title/{title}");
    await PrintMovie(res);
}

static async Task SearchByYear(HttpClient client)
{
    Console.Write("Enter Year: ");
    var year = Console.ReadLine();
    var res = await client.GetAsync($"/api/movie/year/{year}");
    await PrintMovies(res);
}

static async Task PrintMovies(HttpResponseMessage response)
{
    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine("No movies found or error.");
        return;
    }

    var json = await response.Content.ReadAsStringAsync();
    var movies = JsonSerializer.Deserialize<List<MovieDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    if (movies == null || movies.Count == 0)
    {
        Console.WriteLine("No movies found.");
        return;
    }

    Console.WriteLine($"\nMovies found ({movies.Count}):");
    foreach (var movie in movies)
    {
        Console.WriteLine($"- {movie.Title} ({movie.Year})");
    }
}

static async Task PrintMovie(HttpResponseMessage response)
{
    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine("Movie not found.");
        return;
    }

    var json = await response.Content.ReadAsStringAsync();
    var movie = JsonSerializer.Deserialize<MovieDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    if (movie == null)
    {
        Console.WriteLine("Failed to parse movie data.");
        return;
    }

    Console.WriteLine($"\nTitle: {movie.Title}");
    Console.WriteLine($"Year: {movie.Year}");
    Console.WriteLine($"Director: {movie.Director}");
    Console.WriteLine($"Genre: {movie.Genre}");
    Console.WriteLine($"Plot: {movie.Plot}\n");
}

static async Task CreateCustomEntry(HttpClient client)
{
    Console.WriteLine("Enter details for the new movie entry.");

    Console.Write("IMDb ID: ");
    var imdbID = Console.ReadLine();

    Console.Write("Title: ");
    var title = Console.ReadLine();

    Console.Write("Year: ");
    var year = Console.ReadLine();

    Console.Write("Rated: ");
    var rated = Console.ReadLine();

    Console.Write("Released (date): ");
    var released = Console.ReadLine();

    Console.Write("Runtime: ");
    var runtime = Console.ReadLine();

    Console.Write("Genre: ");
    var genre = Console.ReadLine();

    Console.Write("Director: ");
    var director = Console.ReadLine();

    Console.Write("Plot: ");
    var plot = Console.ReadLine();

    var movie = new MovieDto
    {
        ImdbID = imdbID,
        Title = title,
        Year = year,
        Rated = rated,
        Released = released,
        Runtime = runtime,
        Genre = genre,
        Director = director,
        Plot = plot
    };

    var json = JsonSerializer.Serialize(movie);
    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

    var response = await client.PostAsync("/api/cachedentries", content);

    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("✅ Custom movie entry created successfully!");
    }
    else
    {
        Console.WriteLine($"❌ Failed to create entry: {response.StatusCode}");
    }
}

