namespace Tools;

public class UserManager
{
    public string username { get; set; }
    public bool IsLoggedIn()
    {
        if (username != null || username != "")
        {
            return true;
        }
        return false;
    }
    
}