using System;
using System.ComponentModel;
using ConvertApi;

namespace RetrieveUserInformation
{
    class Program
    {
        static void Main(string[] args)
        {
            //Get your secret at https://www.convertapi.com/a
            var convertApiClient = new ConvertApiClient("<Your secret here>");
            var convertApiUser = convertApiClient.GetUser().Result;

            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(convertApiUser))
            {
                var name = descriptor.Name;
                var value = descriptor.GetValue(convertApiUser);
                Console.WriteLine("{0}={1}", name, value);
            }

            Console.ReadLine();
        }
    }
}
