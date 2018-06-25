using System;
using System.ComponentModel;

namespace RetrieveUserInformation
{
    class Program
    {
        /// <summary>
        /// Retrieve user information
        /// https://www.convertapi.com/doc/user
        /// </summary>
        static void Main(string[] args)
        {
            //Get your secret at https://www.convertapi.com/a
            var convertApi = new ConvertApiDotNet.ConvertApi("your api secret");
            var convertApiUser = convertApi.GetUser().Result;

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
