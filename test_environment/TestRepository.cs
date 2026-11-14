
namespace Testing;

public class TestRepo{
    public List<UnitTest> Tests {get; private set;}
    
    Tests.Add(
        (T obj)=>{T.body.userId is Guid},
        "POST",
        "/User",
        Body.Create()
    )

}