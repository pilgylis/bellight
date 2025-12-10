using System.Collections.Generic;

namespace UtilitiesTests.Models;

public class CountryCurrency
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
}

public class CountryLanguage
{
    public string Iso639_1 { get; set; } = string.Empty;
#pragma warning disable IDE1006 // Naming Styles
    public string iso639_2 { get; set; } = string.Empty;
#pragma warning restore IDE1006 // Naming Styles
    public string Name { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
}

public class RegionalBloc
{
    public string Acronym { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public IEnumerable<string> OtherAcronyms { get; set; } = [];
    public IEnumerable<string> OtherNames { get; set; } = [];
}

public class Country
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public IEnumerable<string> TopLevelDomain { get; set; } = [];
    public string Alpha2Code { get; set; } = string.Empty;
    public string Alpha3Code { get; set; } = string.Empty;
    public IEnumerable<string> CallingCodes { get; set; } = [];
    public string Capital { get; set; } = string.Empty;
    public IEnumerable<string> AltSpellings { get; set; } = [];
    public string Region { get; set; } = string.Empty;
    public string Subregion { get; set; } = string.Empty;
    public int? Population { get; set; }
    public IEnumerable<decimal> Latlng { get; set; } = [];
    public string Demonym { get; set; } = string.Empty;
    public decimal? Area { get; set; }
    public decimal? Gini { get; set; }
    public IEnumerable<string> Timezones { get; set; } = [];
    public IEnumerable<string> Borders { get; set; } = [];
    public string NativeName { get; set; } = string.Empty;
    public string NumericCode { get; set; } = string.Empty;

    public IEnumerable<CountryCurrency> Currencies { get; set; } = [];
    public IEnumerable<CountryLanguage> Languages { get; set; } = [];
    public IDictionary<string, string> Translations { get; set; } = new Dictionary<string, string>();
    public string Flag { get; set; } = string.Empty;
    public IEnumerable<RegionalBloc> RegionalBlocs { get; set; } = [];
    public string Cioc { get; set; } = string.Empty;
}