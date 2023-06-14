using System.Globalization;
using System.Text;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using ReactCSVPeople.Data;
using Faker;
using ReactCSVParser.Web.ViewModels;

namespace ReactCSVParser.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CSVPeopleController : ControllerBase
    {
        private readonly string _connectionString;

        public CSVPeopleController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ConStr");
        }

        [HttpPost]
        [Route("upload")]
        public void Upload(UploadViewModel viewModel)
        {
            int firstComma = viewModel.Base64File.IndexOf(',');
            string base64 = viewModel.Base64File.Substring(firstComma + 1);
            var people = ParseCsv(base64);
            var repo = new PeopleRepository(_connectionString);
            repo.Add(people);
        }

        [HttpGet]
        [Route("generate")]
        public IActionResult Generate(int amount)
        {
            var people = GenerateFakePeople(amount);
            var csv = GenerateCSV(people);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "people.csv");
        }

        [HttpGet]
        [Route("getpeople")]
        public List<Person> GetPeople()
        {
            var repo = new PeopleRepository(_connectionString);
            return repo.GetAll();
        }

        [HttpPost]
        [Route("delete")]
        public void DeleteAll()
        {
            var repo = new PeopleRepository(_connectionString);
            repo.DeleteAll();
        }

        private List<Person> GenerateFakePeople(int amount)
        {
            //var result = new List<Person>();
            //for (int i = 1; i <= amount; i++)
            //{
            //    result.Add(new Person
            //    {
            //        FirstName = Name.First(),
            //        LastName = Name.Last(),
            //        Age = RandomNumber.Next(20, 80),
            //        Address = $"{Address.StreetAddress()} {Address.StreetName()} {Address.StreetSuffix()}",
            //        Email = Internet.Email()
            //    });
            //}

            //return result;

            return Enumerable.Range(1, amount).Select(_ => new Person
            {
                FirstName = Name.First(),
                LastName = Name.Last(),
                Age = RandomNumber.Next(20, 80),
                Address = $"{Address.StreetAddress()} {Address.StreetName()} {Address.StreetSuffix()}",
                Email = Internet.Email()
            }).ToList();
        }

        private string GenerateCSV(List<Person> people)
        {
            var builder = new StringBuilder();
            using var writer = new StringWriter(builder);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(people);
            return builder.ToString();
        }

        private List<Person> ParseCsv(string base64File)
        {
            var bytes = Convert.FromBase64String(base64File);
            using var memoryStream = new MemoryStream(bytes);
            using var reader = new StreamReader(memoryStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            return csv.GetRecords<Person>().ToList();
        }
    }
}