# Microservices reference implementation - Delivery request load testing
Microsoft patterns & practices

This load test project executes performance tests on the [microservices reference implemention using Azure Service Fabric](../../Readme.md). 

This load test executes two separate scenarios: 
1. Uses a single user to deliver at 200 request/sec.
2. Loads 40 users to deliver up to 2600 request/sec. 

## Prerequisites
You need to install these tools to run the load tests:
1. Microsoft [Visual Studio](https://visualstudio.microsoft.com/vs/) Enterprise Edition. 
2. Web performance and load testing tools. For installation instructions, see [Install the Load testing component](https://docs.microsoft.com/en-us/visualstudio/test/quickstart-create-a-load-test-project?view=vs-2017#install-the-load-testing-component).
3. Azure DevOps subscription.


## Run load testing in Azure DevOps

1. Launch Visual Studio.
2. Select **Team Explorer** and connect to your Azure DevOps subscription.
3. Navigate to loadtests\Fabrikam.Ingress.Ingestion.LoadTests folder in your repo. 
4. Right-click DeliveryRequestLoadTest.loadtest and select **Open With**. The test configuration opens in **Load Test Editor**.
5. Right-click **INGEST_URL** under Run **Settings** > **Region.DeliveryRequest** > **Context Parameters** and select **Properties**.
    
    Set the value to http://{Service Fabric App Type}-{node type}.{region}.cloudapp.azure.com:{port}

6. Right-click and select **Run Load Test**.

## See Also

[Load test scenario properties](https://docs.microsoft.com/visualstudio/test/load-test-scenario-properties?view=vs-2017)
