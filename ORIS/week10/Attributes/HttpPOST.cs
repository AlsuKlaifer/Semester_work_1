namespace ORIS.week10.Attributes
{
    public class HttpPOST : Attribute
    {
        public string MethodURI;
        public HttpPOST(string methodURI)
        {
            MethodURI = methodURI;
        }
    }
}