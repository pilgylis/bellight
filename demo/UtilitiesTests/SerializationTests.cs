using Bellight.Core;
using System.Collections.Generic;
using UtilitiesTests.Fixtures;
using UtilitiesTests.Models;
using Xunit;
using Xunit.Abstractions;

namespace UtilitiesTests
{
    public class SerializationTests : IClassFixture<CountryFixture>
    {
        private readonly IEnumerable<Country> countries;
        private readonly ITestOutputHelper testOutputHelper;

        public SerializationTests(CountryFixture countryFixture, ITestOutputHelper testOutputHelper)
        {
            countries = countryFixture.Countries;
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void JsonHasContentTest()
        {
            Assert.NotNull(countries);
            Assert.NotEmpty(countries);
        }

        [Fact]
        public void SimpleJsonSerializationTest()
        {
            var json = countries.ToJson();
            Assert.NotNull(json);
            testOutputHelper.WriteLine(json);
            Assert.NotEmpty(json);
        }
    }
}