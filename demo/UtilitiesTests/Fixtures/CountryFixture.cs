using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using UtilitiesTests.Models;

namespace UtilitiesTests.Fixtures;

public class CountryFixture
{
    public IEnumerable<Country> Countries { get; private set; }

    public CountryFixture()
    {
        var json = File.ReadAllText("Data/countries.json");
        Countries = JsonSerializer.Deserialize<IEnumerable<Country>>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}