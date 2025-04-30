using ConvertApiDotNet;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace RetrieveUserInformation
{
    class Program
    {
        /// <summary>
        /// Retrieve user information
        /// https://www.convertapi.com/doc/user
        /// </summary>
        static async Task Main(string[] args)
        {
            //Get your api token at https://www.convertapi.com/a
            var convertApi = new ConvertApi("api_token");
            var convertApiUser = await convertApi.GetUserAsync();

            Console.WriteLine("Secret={0}", convertApiUser.Secret);
            Console.WriteLine("ApiKey={0}", convertApiUser.ApiKey);
            Console.WriteLine("Active={0}", convertApiUser.Active);
            Console.WriteLine("FullName={0}", convertApiUser.FullName);
            Console.WriteLine("Email={0}", convertApiUser.Email);
            Console.WriteLine("ConversionsTotal={0}", convertApiUser.ConversionsTotal);
            Console.WriteLine("ConversionsConsumed={0}", convertApiUser.ConversionsConsumed);

            Console.ReadLine();
        }
    }
}
