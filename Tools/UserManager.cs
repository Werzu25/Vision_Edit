namespace Tools;

public class UserManager
{
    public string username { get; set; }
    public bool is_logged_in()
    {
        if (username != null || username != "")
        {
            return true;
        }
        return false;
    }
    
}