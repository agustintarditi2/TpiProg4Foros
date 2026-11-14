using System.Text.Json;
namespace Testing;

public class UnitTest{
    public Body ReqBody {get; set;}
    public Header ReqHeader {get; set;}
    public Func<T, bool> TestSuccess {get; set;}
    public string Name {get; set;}
    public string Method {get; set;}
    public string Route {get; set;}
    public UnitTest(
        Func<T, bool> successStatus, 
        string method, 
        string route, 
        Body body, 
        Header header, 
        string name)
    {
        Method = method;
        Route = route;
        Name = name;
        ReqBody = body;
        ReqHeader = header;
        TestSuccess = successStatus;
    }
    public bool EvaluateTest(string res){
        if (res is null)
            return false;
        obj = JsonSerializer.Deserialize(res);
        try {
            bool pass = successStatus(obj);
            return pass;
        }
        catch (Exception ex){
            Console.WriteLine("Test failed:", ex.Message);
            return false;
        }
    }
}