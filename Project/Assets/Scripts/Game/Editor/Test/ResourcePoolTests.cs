using NSubstitute;
using NUnit.Framework;

using System;
using SocialPoint.Attributes;

[TestFixture]
[Category("BaseGame.Resources")]
public class ResourcePoolTests
{
    ResourcePool ResourcePool;

    [SetUp]
    public void SetUp()
    {
        ResourcePool = new ResourcePool();
        UnityEngine.Assertions.Assert.raiseExceptions = true;
    }

    [Test]
    public void Loads_From_Attr()
    {
        var data = new AttrDic();
        data.Set("food", new AttrInt(0));
        ResourcePool = new ResourcePool(data);
    }

    [Test]
    public void Converts_To_Attr()
    {
        Loads_From_Attr();
        var data = ResourcePool.ToAttr();
        Assert.That(data.AsDic.Get("food").AsValue == 0);
    }

    [Test]
    public void Request_For_UnKnown_Resource_Doesnt_Throws_Exception()
    {
        Loads_From_Attr();
        Assert.DoesNotThrow(() => {
            ResourcePool["BadID"] = 0;
        });
    }

    [Test]
    public void Add_Resource_ammount()
    {
        Loads_From_Attr();
        ResourcePool["food"] += 20;
        Assert.That(ResourcePool["food"] == 20);
        ResourcePool["food"] += 20;
        Assert.That(ResourcePool["food"] == 40);
    }

    [Test]
    public void Add_ResourcePools()
    {
        var data = new AttrDic();
        data.Set("food", new AttrInt(0));
        var rp1 = new ResourcePool(data);
        rp1["food"] += 10;
        var rp2 = new ResourcePool(data);
        rp2["food"] += 5;
        rp2["gold"] = 7;
        rp1 += rp2;
        Assert.That(rp1["food"] == 15);
        rp1 -= rp2;
        Assert.That(rp1["food"] == 10);
        Assert.Throws<UnityEngine.Assertions.AssertionException>(() => rp2 -= rp1);
    }

    [Test]
    public void Substract_Resource()
    {
        Loads_From_Attr();
        ResourcePool["food"] = 10;
        ResourcePool["food"] -= 3;
        Assert.That(ResourcePool["food"] == 7);
        ResourcePool["food"] -= 2;
        Assert.That(ResourcePool["food"] == 5);
    }

    [Test]
    public void Substract_More_Than_Remaining_Resource_Fails()
    {
        Loads_From_Attr();
        ResourcePool["food"] = 10;
        Assert.Throws<UnityEngine.Assertions.AssertionException>(() => ResourcePool["food"] -= 20);
    }

    [Test]
    public void Changes_On_Resources_Dispatch_Event()
    {
        var del = Substitute.For<ResourcePool.ResourceModifiedDelegate>();
        Loads_From_Attr();
        ResourcePool.ResourceModified += del;
        ResourcePool["food"] += 20;
        del.ReceivedWithAnyArgs(1).Invoke(Arg.Any<SocialPoint.Events.ResourceOperation>());
    }

    [TearDown]
    public void TearDown()
    {
        
    }
}
