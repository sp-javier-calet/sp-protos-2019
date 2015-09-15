using NSubstitute;
using NUnit.Framework;

using System;
using SocialPoint.Attributes;

[TestFixture]
[Category("BaseGame.Resources")]
public class ResourceManagerTests
{
    ResourceManager ResourceManager;

    [SetUp]
    public void SetUp()
    {
        ResourceManager = new ResourceManager();
        UnityEngine.Assertions.Assert.raiseExceptions = true;
    }

    [Test]
    public void Loads_From_Attr()
    {
        AttrDic data = new AttrDic();
        AttrDic product1 = new AttrDic();
        product1.Set("name", new AttrString("product 1"));
        data.Set("1", product1);
        AttrDic product2 = new AttrDic();
        product2.Set("name", new AttrString("product 2"));
        data.Set("2", product2);
        var resourcesParser = new ResourcesParser();
        ResourceManager.Resources = resourcesParser.Parse(data);

        Assert.That(ResourceManager.GetResourceByID("1").Name == "product 1");
        Assert.That(ResourceManager.GetResourceByID("2").Name == "product 2");
    }

    [Test]
    public void Converts_To_Attr()
    {
        Register_Resource_With_Ammount();
        var data = ResourceManager.ToAttr();
        Assert.That(data.AsDic.Get("TestID").AsValue == 10);
    }

    [Test]
    public void Register_Resource()
    {
        var res = new Resource("TestID", "Test");
        ResourceManager.RegisterResource(res);
        Assert.That(ResourceManager.GetResourceByID("TestID").Name == "Test");
    }

    [Test]
    public void Register_Existing_Resource_Throws_Exception()
    {
        var res = new Resource("TestID", "Test");
        ResourceManager.RegisterResource(res);
        Assert.Throws<UnityEngine.Assertions.AssertionException>(() => ResourceManager.RegisterResource(res));
    }

    [Test]
    public void Register_Resource_With_Ammount()
    {
        var res = new Resource("TestID", "Test");
        Assert.DoesNotThrow(() => ResourceManager.RegisterResource(res, 10));
    }

    [Test]
    public void Register_Resource_Negative_ammount_Throws_Exception()
    {
        var res = new Resource("TestID", "Test");
        Assert.Throws<UnityEngine.Assertions.AssertionException>(() => ResourceManager.RegisterResource(res, -1));
    }

    [Test]
    public void Request_For_Known_Resource_Return_Value()
    {
        var res = new Resource("TestID", "Test");
        ResourceManager.RegisterResource(res);
        var requestedRes = ResourceManager.GetResourceByID("TestID");
        Assert.AreEqual(res, requestedRes);
    }

    [Test]
    public void Request_For_UnKnown_Resource_Throws_Exception()
    {
        var res = new Resource("TestID", "Test");
        ResourceManager.RegisterResource(res);
        Assert.Throws<UnityEngine.Assertions.AssertionException>(() => ResourceManager.GetResourceByID("BadID"));
    }

    [Test]
    public void Add_Resource()
    {
        Register_Resource();
        ResourceManager.AddResource("TestID", 20);
        Assert.That(ResourceManager.GetResourceAmmount("TestID") == 20);
        var res = ResourceManager.GetResourceByID("TestID");
        ResourceManager.AddResource(res, 20);
        Assert.That(ResourceManager.GetResourceAmmount("TestID") == 40);
    }

    [Test]
    public void Add_Negative_Resource_Fails()
    {
        Register_Resource();
        Assert.Throws<UnityEngine.Assertions.AssertionException>(() => ResourceManager.AddResource("TestID", -20));
    }

    [Test]
    public void Substract_Resource()
    {
        Register_Resource_With_Ammount();
        ResourceManager.SubstractResource("TestID", 3);
        Assert.That(ResourceManager.GetResourceAmmount("TestID") == 7);
        var res = ResourceManager.GetResourceByID("TestID");
        ResourceManager.SubstractResource(res, 2);
        Assert.That(ResourceManager.GetResourceAmmount("TestID") == 5);
    }

    [Test]
    public void Substract_Negative_Resource_Fails()
    {
        Register_Resource_With_Ammount();
        Assert.Throws<UnityEngine.Assertions.AssertionException>(() => ResourceManager.SubstractResource("TestID", -2));
    }

    [Test]
    public void Substract_More_Than_Remaining_Resource_Fails()
    {
        Register_Resource_With_Ammount();
        Assert.Throws<UnityEngine.Assertions.AssertionException>(() => ResourceManager.SubstractResource("TestID", 20));
    }

    [Test]
    public void Changes_On_Resources_Dispatch_Event()
    {
        var del = Substitute.For<ResourceManager.ResourceModifiedDelegate>();
        Register_Resource();
        ResourceManager.ResourceModified += del;
        ResourceManager.AddResource("TestID", 20);
        del.ReceivedWithAnyArgs(1).Invoke(Arg.Any<SocialPoint.Events.ResourceOperation>());
    }

    [TearDown]
    public void TearDown()
    {
        
    }
}


