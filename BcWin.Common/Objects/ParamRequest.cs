namespace BcWin.Common.Objects
{
    public class ParamRequest
    {
        public ParamRequest(string keyValue)
        {
            this.KeyValue = keyValue;
        }

        public ParamRequest(string keyName, string keyValue)
        {
            this.KeyName = keyName;
            this.KeyValue = keyValue;
        }

        public string KeyName { get; set; }
        public string KeyValue { get; set; }
    }
}
