namespace ORIS.week10.Attributes
{
    public class HttpController : Attribute
    {
        public string ControllerName;

        public HttpController(string controllerName)
        {
            ControllerName = controllerName;
        }
    }
}