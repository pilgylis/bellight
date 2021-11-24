using System.Collections.Generic;

namespace UtilitiesTests.Models
{
    public class CountryCurrency
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
    }

    public class CountryLanguage
    {
        public string Iso639_1 { get; set; }
#pragma warning disable IDE1006 // Naming Styles
        public string iso639_2 { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        public string Name { get; set; }
        public string NativeName { get; set; }
    }

    public class RegionalBloc
    {
        public string Acronym { get; set; }
        public string Name { get; set; }
        public IEnumerable<string> OtherAcronyms { get; set; }
        public IEnumerable<string> OtherNames { get; set; }
    }

    public class Country
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<string> TopLevelDomain { get; set; }
        public string Alpha2Code { get; set; }
        public string Alpha3Code { get; set; }
        public IEnumerable<string> CallingCodes { get; set; }
        public string Capital { get; set; }
        public IEnumerable<string> AltSpellings { get; set; }
        public string Region { get; set; }
        public string Subregion { get; set; }
        public int? Population { get; set; }
        public IEnumerable<decimal> Latlng { get; set; }
        public string Demonym { get; set; }
        public decimal? Area { get; set; }
        public decimal? Gini { get; set; }
        public IEnumerable<string> Timezones { get; set; }
        public IEnumerable<string> Borders { get; set; }
        public string NativeName { get; set; }
        public string NumericCode { get; set; }


        public IEnumerable<CountryCurrency> Currencies { get; set; }
        public IEnumerable<CountryLanguage> Languages { get; set; }
        public IDictionary<string, string> Translations { get; set; }
        public string Flag { get; set; }
        public IEnumerable<RegionalBloc> RegionalBlocs { get; set; }
        public string Cioc { get; set; }
    }
}
