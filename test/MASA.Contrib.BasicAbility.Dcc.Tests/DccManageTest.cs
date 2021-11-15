using MASA.Contrib.BasicAbility.Dcc.Internal;
using MASA.Utils.Caller.Core;
using System.Net;

namespace MASA.Contrib.BasicAbility.Dcc.Tests;

[TestClass]
public class DccManageTest
{
    private DccSectionOptions _dccSectionOptions;
    private JsonSerializerOptions _jsonSerializerOptions;
    private Mock<ICallerProvider> _callerProvider;
    private Mock<HttpMessageHandler> _httpMessageHandler;

    [TestInitialize]
    public void Initialize()
    {
        _dccSectionOptions = new DccSectionOptions()
        {
            Environment = "Test",
            Cluster = "Default",
            AppId = "DccTest",
            ConfigObjects = new List<string>()
            {
                "Test1"
            },
            Secret = "Secret"
        };
        _jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };
        _callerProvider = new();
    }

    [DataTestMethod]
    [DataRow("Test", "Default", "DccTest", "Brand")]
    public async Task TestUpdateAsync(string environment, string cluster, string appId, string configObject)
    {
        var brand = new Brands("Microsoft");
        _callerProvider.Setup(factory => factory.PutAsync(It.IsAny<string>(), It.IsAny<object>(), default).Result).Returns(() => new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(brand.Serialize(_jsonSerializerOptions))
        }).Verifiable();

        var manage = new ConfigurationAPIManage(_callerProvider.Object, _dccSectionOptions, null);
        await manage.UpdateAsync(environment, cluster, appId, configObject, brand);
    }

    [DataTestMethod]
    [DataRow("Test", "Default", "DccTest", "Brand")]
    public async Task TestUpdateAsyncAndError(string environment, string cluster, string appId, string configObject)
    {
        var brand = new Brands("Microsoft");

        _callerProvider.Setup(factory => factory.PutAsync(It.IsAny<string>(), It.IsAny<object>(), default).Result).Returns(() => new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.ExpectationFailed,
            Content = new StringContent("error")
        }).Verifiable();

        var manage = new ConfigurationAPIManage(_callerProvider.Object, _dccSectionOptions, null);
        await Assert.ThrowsExceptionAsync<HttpRequestException>(async () => await manage.UpdateAsync(environment, cluster, appId, configObject, brand));
    }

    [DataTestMethod]
    [DataRow("Test", "Default", "DccTest", "Brand")]
    public async Task TestUpdateAsyncAndCustomError(string environment, string cluster, string appId, string configObject)
    {
        var brand = new Brands("Microsoft");
        _callerProvider.Setup(factory => factory.PutAsync(It.IsAny<string>(), It.IsAny<object>(), default).Result).Returns(() => new HttpResponseMessage()
        {
            StatusCode = (HttpStatusCode)299,
            Content = new StringContent("custom error")
        }).Verifiable();

        var manage = new ConfigurationAPIManage(_callerProvider.Object, _dccSectionOptions, null);
        await Assert.ThrowsExceptionAsync<HttpRequestException>(async () => await manage.UpdateAsync(environment, cluster, appId, configObject, brand));
    }

    [DataTestMethod]
    [DataRow("DccTest", "Secret")]
    [DataRow("DccTest2", "Secret2")]
    [DataRow("DccTest3", "")]
    public void TestGetSecret(string appId, string secret)
    {
        var api = new CustomConfigurationAPI(_dccSectionOptions, new List<DccSectionOptions>()
        {
            new DccSectionOptions()
            {
                Environment = "Test2",
                Cluster = "Default2",
                AppId = "DccTest2",
                ConfigObjects = new List<string>()
                {
                    "Test12"
                },
                Secret = "Secret2"
            }
        });
        if (string.IsNullOrEmpty(secret))
            Assert.ThrowsException<ArgumentNullException>(() => api.GetSecret(appId));
        else
            Assert.IsTrue(api.GetSecret(appId) == secret);
    }

    [DataTestMethod]
    [DataRow("Test2", "Test2")]
    [DataRow("", "Test")]
    public void TestGetEnvironment(string environment, string outEnvironment)
    {
        var api = new CustomConfigurationAPI(_dccSectionOptions, null);
        Assert.IsTrue(api.GetEnvironment(environment) == outEnvironment);
    }

    [DataTestMethod]
    [DataRow("CustomCluster", "CustomCluster")]
    [DataRow("", "Default")]
    public void GetCluster(string cluster, string outCluster)
    {
        var api = new CustomConfigurationAPI(_dccSectionOptions, null);
        Assert.IsTrue(api.GetCluster(cluster) == outCluster);
    }

    [DataTestMethod]
    [DataRow("CustomAppid", "CustomAppid")]
    [DataRow("", "DccTest")]
    public void GetAppid(string appId, string outAppid)
    {
        var api = new CustomConfigurationAPI(_dccSectionOptions, null);
        Assert.IsTrue(api.GetAppid(appId) == outAppid);
    }

    [DataTestMethod]
    [DataRow("configObject", "configObject")]
    [DataRow("", "")]
    public void GetConfigObject(string configObject, string outConfigObject)
    {
        var api = new CustomConfigurationAPI(_dccSectionOptions, null);
        if (string.IsNullOrEmpty(configObject))
            Assert.ThrowsException<ArgumentNullException>(() => api.GetConfigObject(configObject));
        else
            Assert.IsTrue(api.GetConfigObject(configObject) == outConfigObject);
    }
}

public class CustomConfigurationAPI : ConfigurationAPIBase
{
    public CustomConfigurationAPI(DccSectionOptions defaultSectionOption, List<DccSectionOptions>? expandSectionOptions) : base(defaultSectionOption, expandSectionOptions)
    {
    }

    public string GetSecret(string appId) => base.GetSecret(appId);

    public string GetEnvironment(string environment) => base.GetEnvironment(environment);

    public string GetCluster(string cluster) => base.GetCluster(cluster);

    public string GetAppid(string appId) => base.GetAppid(appId);

    public string GetConfigObject(string configObject) => base.GetConfigObject(configObject);
}