namespace ID.Infrastructure.Enums
{
    public enum eAppErrorCodes
    {
        UnknownError = -1
    }

    public enum eAppRoles
    {
        User = 0,
        Candidate = 1
    }

    public enum eXmlEmailTemplates
    {
        Aman = 0,
        ServiceCenter = 1
    }

    public class CookiesKeys
    {
        public static string TokenData = "TokenData";
    }

    public static class CacheKeys
    {
        public static string TemporaryTokenKey { get { return "_TemporaryTokenKey"; } }
        public static string CodeSms { get { return "_CodeSms"; } }
    }
}