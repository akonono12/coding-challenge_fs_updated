// See https://aka.ms/new-console-template for more information
using System.Text.RegularExpressions;
using System.Text;
using coding_challenge_fs_updated.Infrastracture.Services;
using System.Collections.Generic;


string csharpClassDefinition = @"
            public class PersonDto
            {
                public string Name { get; set; }
                public int Age { get; set; }
                public string Gender { get; set; }
                public long? DriverLicenceNumber { get; set; }
                public List<Address> Addresses { get; set; }

                public class Address
                {
                    public int StreetNumber { get; set; }
                    public string StreetName { get; set; }
                    public string Suburb { get; set; }
                    public int PostCode { get; set; }
                }
            }";

var typeScriptConverterService = new TypeScriptConverterService();
string typescriptDefinition = typeScriptConverterService.ConvertToTypescript(csharpClassDefinition);
Console.WriteLine(typescriptDefinition);