namespace MagicJson.Data
{
    public class MessageItem
    {
        public int Line;
        public int Position;
        public string Message;
        public string Code;

        public string GetPresentedData()
        {
            return string.Format("\r\nError\r\n\t{0}{1}", Message, string.IsNullOrWhiteSpace(Code) 
                                                    ? string.Empty 
                                                    : string.Format("\r\n\tCODE\t{0}", Code));
        }
    }
}