namespace ORIS.week10.Attributes
{
    public class HttpGET : Attribute
    {
        public string MethodURI;
        public HttpGET(string methodURI)
        {
            MethodURI = methodURI;
        }
    }
}