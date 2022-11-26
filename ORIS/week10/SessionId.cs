namespace ORIS.week10
{
    public class SessionId
    {
        public bool IsAuthorize { get; private set; }
        public int Id { get; private set; }

        public SessionId(bool isAuthorize, int id)
        {
            IsAuthorize = isAuthorize;
            Id = id;
        }
    }
}