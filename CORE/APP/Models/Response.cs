namespace CORE.APP.Models
{
    public abstract class Response
    {
        public virtual int Id { get; set; }

        protected Response(int id)
        {
            Id = id;
        }

        protected Response()
        {
        }
    }
}