# Deploy the Drone Delivery App

This reference implementation builds a Drone Delivery sample application that consists of several microservices. The microservices are deployed in an Azure Service Fabric cluster. For information about the reference implementation, see [ReadMe](README.md).

## Prerequisites
- Install PowerShell Core in Windows. For instructions, see [Installing PowerShell Core on Windows](https://docs.microsoft.com/powershell/scripting/install/installing-powershell-core-on-windows?view=powershell-6).
- Install Azure PowerShell modules. For instructions, see [Install the Azure PowerShell module](https://docs.microsoft.com/powershell/azure/install-az-ps?view=azps-2.2.0).
- 
- Clone the repo locally.

    ```
    git clone https://github.com/mspnp/microservices-reference-implementation-servicefabric.git my-local-folder

    cd my-local-folder
    ```
- If you have multiple Azure subscriptions, make sure that the subscription that you want to use for this deployment is set as default. Connect to your Azure account and use these commands to check the subscription and set the default.
    ```
    Connect-AzAccount
    Set-AzContext -SubscriptionId "<your subscription id>"
    ```
## Deploy the Azure Key Vault for storing secrets

1. Set these PowerShell variables. 

    ```
    > $keyVaultResourceGroupName  ='<your resource group name>'
    > $location='<azure region>'
    > $userIdentifier = 'user@contoso.com'
    > $keyVaultTemplate = '.\azuredeploy-keyvault.json'
    > $keyVaultName = 'myapp-kv'
    ```
2. Get the object ID of the current user or a specific user.

    ```
    $objectId = Get-AzADUser -UserPrincipalName $userIdentifier | Select-Object -ExpandProperty Id
    ```
3. Get the tenant ID for your subscription.
    ```
    $tenantId = (Get-AzContext).Subscription.TenantId
    ```
4. Create a resource group in the specified region.
    ```
    New-AzResourceGroup -Name $keyVaultResourceGroupName -Location $location
    ```
5. Deploy a Key Vault.
    ``` 
    New-AzResourceGroupDeployment -Name "kvdeploy1" -ResourceGroupName $keyVaultResourceGroupName -TemplateFile $keyVaultTemplate -keyVaultName $keyVaultName -tenantId $tenantId -objectId $objectId
    ```
## Deploy the Service Fabric cluster

1. Set these PowerShell variables. 

    ```
    > $dnsPrefix = 'myapp'
    > $keyVaultCertName = 'myapp-cluster-cert' 
    > $appResourceGroupName = 'myapp-rg'
    > $appTemplate = '.\azuredeploy.json'
    ```

2. Generate a certificate and store it in the Key Vault.
    ```
    $Policy = New-AzKeyVaultCertificatePolicy -SecretContentType "application/x-pkcs12" -SubjectName "CN=$dnsPrefix.$location.cloudapp.azure.com" -IssuerName "Self" -ValidityInMonths 6 -ReuseKeyOnRenewal

    Add-AzKeyVaultCertificate -VaultName $keyVaultName -Name $keyVaultCertName -CertificatePolicy $Policy
    ```
3. After the certificate is created, get the thumbprint of the certificate. 
    ```
    $thumbprint = Get-AzKeyVaultCertificate -VaultName $keyVaultName -Name $keyVaultCertName | Select-Object -ExpandProperty ThumbPrint
    ```

4. Create a resource group in which the Service Fabric cluster and the related resoures will be deployed.
    ```
    New-AzResourceGroup -Name $appResourceGroupName -Location $location
    ```
5. Deploy the Service Fabric cluster and related resources.
    ```
    New-AzResourceGroupDeployment -Name "sfappdep1" -ResourceGroupName $appResourceGroupName -TemplateFile $appTemplate -keyVaultName $keyVaultName -certificateName $keyVaultCertName -keyVaultResourceGroupName $keyVaultResourceGroupName -certificateThumbprint $thumbprint
    ```
## Deploy apps to the Service Fabric cluster
Follow these instructions to deploy the services to the cluster.

## Prerequisites
- [Install Visual Studio Build Tools 2017](https://visualstudio.microsoft.com/downloads/?q=visual+studio+build+tools#).
- [Nuget tool](https://docs.microsoft.com/en-us/nuget/install-nuget-client-tools)
- [JDK 8](https://www.oracle.com/technetwork/java/javase/downloads/jdk8-downloads-2133151.html) or later.
- [JRE 8](https://www.oracle.com/technetwork/java/javase/downloads/jre8-downloads-2133155.html) or later.
- [Maven build tool](https://maven.apache.org/download.cgi) for packaging guest executable as jar.
- [Service Fabric SDK and tools](https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-get-started) to enable Service Fabric Powershell cmdlets.

1. In a PowerShell Core window navigate to the root directory of the cloned repo.
2. Run this script to restore the packages required by msbuild to build the final package structure:

---
**NOTE**

Specify the full path to nuget.exe in this script. Alternatively, you can set your PATH environment variable to nuget.exe.

---

    ```
    Get-ChildItem -Path '.\src\' -Filter *.sln -Recurse -Exclude *LoadTests.sln | ForEach-Object {
        $fullName = $_.FullName
        
        # Run once to restore Service Fabric build packages
        Write-Host Running "'nuget restore' to restore Service Fabric packages on" $fullName ...
        & nuget.exe restore $fullName
        
        # Run again to restore all the nuget packages in the solution
        Write-Host Running "'nuget restore' to restore packages on" $fullName ...
        & nuget.exe restore $fullName
    }
    ```

3. Enable msbuild.
   - Install chocolatey to get the vswhere tool. 

        ```
        Set-ExecutionPolicy Bypass -Scope Process -Force; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))        
        ```
   
   - Install vswhere. 
    
        ```
        choco install vswhere --pre
        ```
   
   - Enable Visual Studio build tools by using vswhere. You need to enable msbuild every time you start a new console session. 

        ```
        $installationPath = vswhere.exe -prerelease -latest -property installationPath
        if ($installationPath -and (test-path "$installationPath\Common7\Tools\vsdevcmd.bat")) {
            & "${env:COMSPEC}" /s /c "`"$installationPath\Common7\Tools\vsdevcmd.bat`" -no_logo && set" | foreach-object {
                $name, $value = $_ -split '=', 2
                set-content env:\"$name" $value
            }
            }
        ```

4. Find the Application Insights instrumentation key. You also need the resource group name which contains your Application Insights resource.

    ```Get-AzApplicationInsights -ResourceGroupName $appResourceGroupName -Name <my-appinsights-name> | Select-Object -ExpandProperty InstrumentationKey```

5. Navigate to the dronescheduler folder.
6. Under the dronescheduler java folder, find application.properties and replace the instrumentation key:

    ```java
    # Specify the instrumentation key of your Application Insights resource.
    azure.application-insights.instrumentation-key=your-application-insights-key-here
    ```

7. Run ```mvn clean package``` to generate dronescheduler-0.0.1-SNAPSHOT.jar in the target folder.
8. Copy over the dependencies required to run your guest executable and the executable jar, to the “src\droneschedulerWrapper\Fabrikam.DroneSchedulerWrapperApp\ApplicationPackageRoot\DroneSchedulerGuestExePkg\Code” folder. These dependencies include:
    - Java runtime "jre<version>".
    - Packaged jar "dronescheduler-0.0.1-SNAPSHOT.jar".  

9.  Run msbuild on all the Service Fabric projects.
   
    ```
    Get-ChildItem -Path '.\src\' -Filter *.sfproj -Recurse | ForEach-Object {
            $fullName = $_.FullName
            Write-Host Running "'msbuild'" on $fullName
            & msbuild $fullName /p:Deterministic=true /t:Package /p:configuration="Release" /p:platform="x64" /p:VisualStudioVersion="15.0"
    }   
    ```

10. Get the keyvault URI.

    ```
    $azkeyVaultUri = get-azkeyvault -vaultname $keyVaultName | Select-Object -ExpandProperty VaultURI
    ```

11. Edit appsettings.<ASP_NET_ENVIRONMENT>.json under your projects to make sure that they contain the required settings. You might be able to use default values set in the configuration files for a sample deployment but you must set the keyvault URI in appsettings.<ASP_NET_ENVIRONMENT>.json wherever applicable. Here's a convenient script to changes those values:

    ```
    Get-ChildItem -Path '.\src\' -Filter appsettings*.json -Recurse | where {$_.DirectoryName -match "pkg"} | ForEach-Object {
        $fullName = $_.FullName
        
        Write-Host $fullName
        $jdata = Get-Content -Raw $fullName | ConvertFrom-Json
        foreach ($p in $jdata.PSObject.Properties)
        {
            Write-Host $p.Name

            if ($p.Name -eq 'AzureKeyVault'){
                $p.Value.KeyVaultUri = $azKeyVaultUri
            }
        }
        
        $jdata | ConvertTo-Json -Depth 10 | Tee-Object $fullName
    }
    ```

---
**NOTE**

Following script requires Powershell 3 or above. It does not works with Powershell Core.

---

12. Open a Powershell prompt and navigate to the root directory of the cloned repo. Run the following script to deploy all the applications and services to the Service Fabric cluster.

    ```
    $endpoint = '<my-servicefabric-cluster-endpoint>:19000'
    $thumbprint = '<certificate-thumbprint-obtained-previously>'

    # Connect to the cluster using a client certificate.
    Connect-ServiceFabricCluster -ConnectionEndpoint $endpoint `
            -KeepAliveIntervalInSec 300 `
            -X509Credential -ServerCertThumbprint $thumbprint `
            -FindType FindByThumbprint -FindValue $thumbprint `
            -StoreLocation CurrentUser -StoreName My

    Get-ChildItem -Path '.\src\' -Filter 'pkg' -Directory -Recurse | ForEach-Object {
        $deploymentPath = $_.FullName + '\Release'
        $appName = Split-Path (Split-Path $_.FullName -Parent) -Leaf
        $appType= $appName +'Type'
        $fullAppName = 'fabric:/' + $appName

        # Copy the application package to the cluster image store.
        Copy-ServiceFabricApplicationPackage $deploymentPath -ImageStoreConnectionString fabric:ImageStore -ApplicationPackagePathInImageStore $appName -ShowProgress -ShowProgressIntervalMilliseconds 500

        # Register the application type.
        Register-ServiceFabricApplicationType -ApplicationPathInImageStore $appName

        # Remove the application package to free system resources.
        Remove-ServiceFabricApplicationPackage -ImageStoreConnectionString fabric:ImageStore -ApplicationPackagePathInImageStore $appName

        # Create Service Fabric application instances
        New-ServiceFabricApplication -ApplicationName $fullAppName -ApplicationTypeName $appType -ApplicationTypeVersion 1.0.0
    }
    ```

## Deploy Azure API Management 
This reference implementation uses Azure API Management (APIM) as the API gateway (ingress).  It runs in a subnet within the same virtual network as the Service Fabric cluster. For deployment instructions, see [Integrate API Management with Service Fabric in Azure](https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-tutorial-deploy-api-management). You also need these [Azure Resource Manager templates](https://github.com/microsoft/service-fabric-scripts-and-templates/tree/master/templates/service-integration).
> The templates assume that the Service Fabric cluster resources are in the same region as the resource group.

After deploying APIM, route requests to a stateless service such as the Drone Delivery service by creating a backend policy.  

```
<policies>
    <inbound>
        <base />
        <set-backend-service backend-id="dronedelivery" sf-resolve-condition="@(context.LastError?.Reason == "BackendConnectionFailure")" sf-service-instance-name="fabric:/Fabrikam.Frontend/DeliveryRequestService" />
    </inbound>
    <backend>
        <base />
    </backend>
    <outbound>
        <base />
    </outbound>
    <on-error>
        <base />
    </on-error>
</policies>
```

For a stateful service, create a policy as shown here:
```
<policies>
    <inbound>
        <base />
        <set-backend-service backend-id="dronedelivery" sf-partition-key="@(BitConverter.ToInt64(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(context.Request.MatchedParameters["id"])),0))" sf-resolve-condition="@(context.LastError?.Reason == "BackendConnectionFailure")" sf-service-instance-name="fabric:/DroneDelivery/DeliveryService" />
    </inbound>
    <backend>
        <base />
    </backend>
    <outbound>
        <base />
    </outbound>
    <on-error>
        <base />
    </on-error>
</policies>
```
For a stateful service, specify the partition key. The Drone Delivery services uses ranged partitioning (UniformInt64Partition). To get the partition key, the policy specifies the hash algorithm used by the service. 
