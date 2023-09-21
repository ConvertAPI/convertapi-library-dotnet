namespace ConvertApiDotNet.Model
{
    public class ConvertApiUser
    {        
        public string Secret { get; set; }        
        public int ApiKey { get; set; }        
        public bool Active { get; set; }        
        public string FullName { get; set; }        
        public string Email { get; set; }        
        public int ConversionsTotal { get; set; }
        public int ConversionsConsumed { get; set; }
        public string Status { get; set; }
    }
}
